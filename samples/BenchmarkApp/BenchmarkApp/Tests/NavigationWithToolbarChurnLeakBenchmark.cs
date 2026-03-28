using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Exercises rapid NavigationPage push/pop where each pushed page has different ToolbarItems
/// (some with IconImageSource). Tests StackNavigationManager.UpdateToolbarItems() subscription
/// cleanup and UpdateToolbarItemIcon() async fire-and-forget path.
/// </summary>
/// <remarks>
/// Gap: MenuAndToolbarLeakBenchmark only creates/tears down toolbar items once on a static page.
/// NavigationStackDepthLeakBenchmark pushes pages with no toolbar items. Neither exercises the
/// compound path of toolbar item subscription swapping during rapid navigation.
/// </remarks>
[BenchmarkTest("NavigationWithToolbarChurnLeak", Description = "Verifies rapid push/pop with changing ToolbarItems doesn't leak")]
public class NavigationWithToolbarChurnLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int cycles = 20;

        var (cycleMemory, cycleWorkingSet) = await PushPopWithToolbars(window, trackedObjects, cycles, logger, cancellationToken);

        // Force GC multiple times with delays
        for (int gc = 0; gc < 3; gc++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(50, cancellationToken);
        }

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);

        // Check for survivors
        var leaked = new List<string>();
        foreach (var (name, weakRef) in trackedObjects)
        {
            if (weakRef.TryGetTarget(out _))
            {
                leaked.Add(name);
            }
        }

        // Analyze per-cycle memory growth (skip first cycle as warmup)
        long avgGrowthPerCycle = 0;
        if (cycleMemory.Count > 2)
        {
            long growthAfterWarmup = cycleMemory[^1] - cycleMemory[1];
            avgGrowthPerCycle = growthAfterWarmup / (cycleMemory.Count - 2);
        }

        // Log per-cycle memory for analysis
        logger.LogInformation("Per-cycle memory (bytes after GC):");
        for (int i = 0; i < cycleMemory.Count; i++)
        {
            long delta = i > 0 ? cycleMemory[i] - cycleMemory[i - 1] : 0;
            long wsDelta = i > 0 ? cycleWorkingSet[i] - cycleWorkingSet[i - 1] : 0;
            logger.LogInformation(
                "  Cycle {Cycle}: {Memory:N0} bytes (delta: {Delta:+#,##0;-#,##0;0}), working set: {WorkingSet:N0} bytes (delta: {WsDelta:+#,##0;-#,##0;0})",
                i, cycleMemory[i], delta, cycleWorkingSet[i], wsDelta);
        }

        var metrics = new Dictionary<string, object>
        {
            ["PushPopCycles"] = cycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
            ["AvgGrowthPerCycleBytes"] = avgGrowthPerCycle,
            ["BaselineWorkingSetBytes"] = cycleWorkingSet[0],
            ["FinalCycleWorkingSetBytes"] = cycleWorkingSet[^1],
            ["TotalWorkingSetGrowthBytes"] = cycleWorkingSet[^1] - cycleWorkingSet[0],
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        bool memoryExceeded = avgGrowthPerCycle > 256 * 1024;

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Navigation+toolbar churn leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryExceeded)
        {
            logger.LogWarning("Excessive memory growth: {AvgGrowth:N0} bytes/cycle", avgGrowthPerCycle);
            return BenchmarkResult.Fail($"Avg memory growth {avgGrowthPerCycle:N0} bytes/cycle exceeds 256 KB", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} objects collected after {Cycles} cycles. Avg growth: {AvgGrowth:N0} bytes/cycle",
            trackedObjects.Count, cycles, avgGrowthPerCycle);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<(List<long> ManagedMemory, List<long> WorkingSet)> PushPopWithToolbars(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var cycleMemory = new List<long>();
        var cycleWorkingSet = new List<long>();

        var rootPage = new ContentPage
        {
            Title = "Root",
            Content = new Label { Text = "Navigation root" },
        };
        var navPage = new NavigationPage(rootPage);
        window.Page = navPage;

        await Task.Delay(100, cancellationToken);

        // Baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        cycleMemory.Add(GC.GetTotalMemory(false));
        using (var proc = Process.GetCurrentProcess())
        {
            cycleWorkingSet.Add(proc.WorkingSet64);
        }

        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Push page with 3 ToolbarItems (including one with icon)
            await PushPageWithToolbarItems(navPage, trackedObjects, i, "A", cancellationToken);

            // Push second page with different ToolbarItems while first page's icon may still be loading
            await PushPageWithToolbarItems(navPage, trackedObjects, i, "B", cancellationToken);

            // Pop both pages
            await navPage.PopAsync(animated: false);
            await Task.Delay(10, cancellationToken);
            await navPage.PopAsync(animated: false);
            await Task.Delay(10, cancellationToken);

            // Capture memory after each cycle
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            cycleMemory.Add(GC.GetTotalMemory(false));
            using (var proc = Process.GetCurrentProcess())
            {
                cycleWorkingSet.Add(proc.WorkingSet64);
            }
        }

        // Track handlers and disconnect
        if (navPage.Handler is object navHandler)
        {
            trackedObjects["NavigationPage.Handler"] = new WeakReference<object>(navHandler);
        }

        rootPage.Handler?.DisconnectHandler();
        navPage.Handler?.DisconnectHandler();

        window.Page = this;
        Content = new Label { Text = "Navigation+toolbar churn test complete" };

        return (cycleMemory, cycleWorkingSet);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task PushPageWithToolbarItems(
        NavigationPage navPage,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle,
        string variant,
        CancellationToken cancellationToken)
    {
        var page = new ContentPage
        {
            Title = $"Page {cycle}{variant}",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = $"Detail page {cycle}{variant}" },
                    new Button { Text = "Action" },
                },
            },
        };
        trackedObjects[$"Cycle{cycle}.Page{variant}"] = new WeakReference<object>(page);

        // Add ToolbarItems with different configurations
        var toolbar1 = new ToolbarItem { Text = $"Save {variant}", Order = ToolbarItemOrder.Primary };
        toolbar1.Clicked += (_, _) => { };
        page.ToolbarItems.Add(toolbar1);
        trackedObjects[$"Cycle{cycle}.Toolbar{variant}1"] = new WeakReference<object>(toolbar1);

        var toolbar2 = new ToolbarItem { Text = $"Edit {variant}", Order = ToolbarItemOrder.Primary };
        toolbar2.Clicked += (_, _) => { };
        page.ToolbarItems.Add(toolbar2);
        trackedObjects[$"Cycle{cycle}.Toolbar{variant}2"] = new WeakReference<object>(toolbar2);

        // One toolbar item with an icon to exercise UpdateToolbarItemIcon async path
        var toolbarWithIcon = new ToolbarItem
        {
            Text = $"Icon {variant}",
            Order = ToolbarItemOrder.Primary,
            IconImageSource = "dotnet_bot.png",
        };
        toolbarWithIcon.Clicked += (_, _) => { };
        page.ToolbarItems.Add(toolbarWithIcon);
        trackedObjects[$"Cycle{cycle}.Toolbar{variant}Icon"] = new WeakReference<object>(toolbarWithIcon);

        await navPage.PushAsync(page, animated: false);
        await Task.Delay(30, cancellationToken);

        // Track handler
        if (page.Handler is object handler)
        {
            trackedObjects[$"Cycle{cycle}.Page{variant}.Handler"] = new WeakReference<object>(handler);
        }
    }
}
