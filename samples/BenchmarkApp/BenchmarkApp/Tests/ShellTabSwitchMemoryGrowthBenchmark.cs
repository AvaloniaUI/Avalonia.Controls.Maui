using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Measures per-cycle memory growth during Shell tab switching.
/// Unlike <see cref="AlohaTabBarNavigationLeakBenchmark"/> which checks WeakReference survival,
/// this test measures whether the memory floor rises with each cycle through the tabs.
/// The AlohaAI profiling showed 5-17 MB cost per tab switch that was largely never reclaimed,
/// with the post-GC memory baseline climbing after each full cycle.
/// </summary>
[BenchmarkTest("ShellTabSwitchMemoryGrowth", Description = "Measures per-cycle memory growth during Shell tab switching with threshold checks")]
public class ShellTabSwitchMemoryGrowthBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int warmupCycles = 3;
        const int measureCycles = 15;
        const long maxAvgGrowthPerCycleBytes = 512 * 1024; // 512 KB per cycle after warmup
        const long maxTotalGrowthBytes = 30 * 1024 * 1024; // 30 MB total after warmup

        var (cycleMemory, cycleWorkingSet) = await RunTabSwitchCycles(window, warmupCycles + measureCycles, logger, cancellationToken);

        // Analyze: skip warmup cycles
        var measureMemory = cycleMemory.Skip(warmupCycles).ToList();
        var measureWorkingSet = cycleWorkingSet.Skip(warmupCycles).ToList();

        if (measureMemory.Count < 2)
        {
            return BenchmarkResult.Fail("Insufficient measurement cycles", new Dictionary<string, object>());
        }

        var baselineAfterWarmup = measureMemory[0];
        var finalMemory = measureMemory[^1];
        var totalGrowth = finalMemory - baselineAfterWarmup;

        // Average growth per cycle (excluding first measurement as it's the baseline)
        long avgGrowthPerCycle = totalGrowth / (measureMemory.Count - 1);

        // Count how many cycles showed growth
        int growingCycles = 0;
        for (int i = 1; i < measureMemory.Count; i++)
        {
            if (measureMemory[i] > measureMemory[i - 1])
            {
                growingCycles++;
            }
        }

        double growthRatio = (double)growingCycles / (measureMemory.Count - 1);

        // Native working set analysis
        var wsBaselineAfterWarmup = measureWorkingSet[0];
        var wsFinalMemory = measureWorkingSet[^1];
        var wsTotalGrowth = wsFinalMemory - wsBaselineAfterWarmup;

        // Log per-cycle memory
        logger.LogInformation("Per-cycle post-GC memory (after {Warmup} warmup cycles):", warmupCycles);
        for (int i = 0; i < cycleMemory.Count; i++)
        {
            var phase = i < warmupCycles ? "warmup" : "measure";
            long delta = i > 0 ? cycleMemory[i] - cycleMemory[i - 1] : 0;
            long wsDelta = i > 0 ? cycleWorkingSet[i] - cycleWorkingSet[i - 1] : 0;
            logger.LogInformation(
                "  Cycle {Cycle} [{Phase}]: {Memory:N0} bytes (delta: {Delta:+#,##0;-#,##0;0}), WS: {WorkingSet:N0} bytes (delta: {WsDelta:+#,##0;-#,##0;0})",
                i, phase, cycleMemory[i], delta, cycleWorkingSet[i], wsDelta);
        }

        var metrics = new Dictionary<string, object>
        {
            ["WarmupCycles"] = warmupCycles,
            ["MeasureCycles"] = measureCycles,
            ["BaselineAfterWarmupBytes"] = baselineAfterWarmup,
            ["FinalMemoryBytes"] = finalMemory,
            ["TotalGrowthBytes"] = totalGrowth,
            ["AvgGrowthPerCycleBytes"] = avgGrowthPerCycle,
            ["GrowingCycleRatio"] = growthRatio,
            ["NativeWorkingSetBaselineBytes"] = wsBaselineAfterWarmup,
            ["NativeWorkingSetFinalBytes"] = wsFinalMemory,
            ["NativeWorkingSetDelta"] = wsTotalGrowth,
        };

        var reasons = new List<string>();

        if (avgGrowthPerCycle > maxAvgGrowthPerCycleBytes)
        {
            reasons.Add($"avg growth {avgGrowthPerCycle / 1024.0:F1} KB/cycle exceeds {maxAvgGrowthPerCycleBytes / 1024} KB threshold");
        }

        if (totalGrowth > maxTotalGrowthBytes)
        {
            reasons.Add($"total growth {totalGrowth / (1024.0 * 1024):F1} MB exceeds {maxTotalGrowthBytes / (1024 * 1024)} MB threshold");
        }

        if (reasons.Count > 0)
        {
            logger.LogWarning("Shell tab switch memory growth detected: {Reasons}", string.Join("; ", reasons));
            return BenchmarkResult.Fail(string.Join("; ", reasons), metrics);
        }

        if (wsTotalGrowth > 100 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {wsTotalGrowth / (1024.0 * 1024):F1} MB exceeds 100 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "Tab switch memory stable. Avg growth: {Avg:N0} bytes/cycle, total: {Total:N0} bytes over {Cycles} cycles",
            avgGrowthPerCycle, totalGrowth, measureMemory.Count - 1);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<(List<long> ManagedMemory, List<long> WorkingSet)> RunTabSwitchCycles(
        Window window,
        int totalCycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var cycleMemory = new List<long>();
        var cycleWorkingSet = new List<long>();

        var shell = CreateShellWithTabs();
        window.Page = shell;

        // Allow initial rendering
        await Task.Delay(500, cancellationToken);

        var tabBar = (TabBar)shell.Items[0];

        for (int cycle = 0; cycle < totalCycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Cycle through all tabs
            for (int tabIndex = 0; tabIndex < tabBar.Items.Count; tabIndex++)
            {
                shell.CurrentItem = tabBar.Items[tabIndex];
                await Task.Delay(50, cancellationToken);
            }

            // Return to first tab
            shell.CurrentItem = tabBar.Items[0];
            await Task.Delay(50, cancellationToken);

            // Force GC and record post-GC memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(50, cancellationToken);

            cycleMemory.Add(GC.GetTotalMemory(false));
            using (var proc = Process.GetCurrentProcess())
            {
                cycleWorkingSet.Add(proc.WorkingSet64);
            }
        }

        // Restore test page
        window.Page = this;
        Content = new Label { Text = "Tab switch test complete" };

        return (cycleMemory, cycleWorkingSet);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Shell CreateShellWithTabs()
    {
        var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Disabled };
        var tabBar = new TabBar();

        // 4 tabs with content matching AlohaAI complexity
        tabBar.Items.Add(new ShellContent
        {
            Title = "Home",
            Route = "home",
            ContentTemplate = new DataTemplate(() => CreateTabPage("Home", 6)),
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Learning",
            Route = "paths",
            ContentTemplate = new DataTemplate(() => CreateTabPage("Learning", 8)),
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Explore",
            Route = "search",
            ContentTemplate = new DataTemplate(() => CreateTabPage("Explore", 5)),
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Profile",
            Route = "profile",
            ContentTemplate = new DataTemplate(() => CreateTabPage("Profile", 4)),
        });

        shell.Items.Add(tabBar);
        return shell;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateTabPage(string title, int cardCount)
    {
        var page = new ContentPage { Title = title };
        var scrollView = new ScrollView();
        var layout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        layout.Children.Add(new Label { Text = title, FontSize = 24, FontAttributes = FontAttributes.Bold });

        for (int i = 0; i < cardCount; i++)
        {
            var card = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                StrokeThickness = 1,
                Stroke = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb("#30FFFFFF"), 0.0f),
                        new GradientStop(Color.FromArgb("#05FFFFFF"), 1.0f),
                    },
                },
                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Offset = new Point(0, 4),
                    Radius = 12,
                    Opacity = 0.3f,
                },
                Content = new VerticalStackLayout
                {
                    Spacing = 6,
                    Padding = new Thickness(14),
                    Children =
                    {
                        new Label { Text = $"{title} Card {i}", FontSize = 17, FontAttributes = FontAttributes.Bold },
                        new Label { Text = "Description text for this card", FontSize = 13 },
                        new Image { Source = "dotnet_bot.png", HeightRequest = 40, WidthRequest = 40, Aspect = Aspect.AspectFit },
                    },
                },
            };
            layout.Children.Add(card);
        }

        scrollView.Content = layout;
        page.Content = scrollView;
        return page;
    }
}
