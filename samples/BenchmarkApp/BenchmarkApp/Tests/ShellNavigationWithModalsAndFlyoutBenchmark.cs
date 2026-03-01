using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Full navigation subsystem compound test: Shell with flyout + tab switching + in-tab push/pop
/// + modal pages on top. Exercises ShellHandler, ShellItemHandler, ShellSectionHandler,
/// StackNavigationManager, AND MauiAvaloniaWindow modal stack simultaneously.
/// </summary>
/// <remarks>
/// Gap: No existing test combines Shell navigation + modals + flyout. ShellNavigationLeakBenchmark
/// has no modals. ModalPageLeakBenchmark has no Shell. ShellFlyoutItemChurnLeakBenchmark has no
/// modals or in-tab navigation.
/// </remarks>
[BenchmarkTest("ShellNavWithModalsAndFlyoutLeak", Description = "Verifies Shell + flyout + in-tab nav + modals compound navigation doesn't leak")]
public class ShellNavigationWithModalsAndFlyoutBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int cycles = 8;

        var (cycleMemory, cycleWorkingSet) = await RunCompoundNavigation(window, trackedObjects, cycles, logger, cancellationToken);

        // Force GC multiple times with delays
        for (int gc = 0; gc < 3; gc++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(100, cancellationToken);
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

        // Only page leaks are critical
        var leakedPages = leaked.Where(n => n.Contains(".Page") || n.Contains(".Modal")).ToList();

        // Analyze per-cycle memory growth
        long avgGrowthPerCycle = 0;
        if (cycleMemory.Count > 2)
        {
            long growthAfterWarmup = cycleMemory[^1] - cycleMemory[1];
            avgGrowthPerCycle = growthAfterWarmup / (cycleMemory.Count - 2);
        }

        logger.LogInformation("Per-cycle memory (bytes after GC):");
        for (int i = 0; i < cycleMemory.Count; i++)
        {
            long delta = i > 0 ? cycleMemory[i] - cycleMemory[i - 1] : 0;
            long wsDelta = i > 0 ? cycleWorkingSet[i] - cycleWorkingSet[i - 1] : 0;
            logger.LogInformation(
                "  Cycle {Cycle}: {Memory:N0} bytes (delta: {Delta:+#,##0;-#,##0;0}) | WorkingSet: {WorkingSet:N0} bytes (delta: {WsDelta:+#,##0;-#,##0;0})",
                i, cycleMemory[i], delta, cycleWorkingSet[i], wsDelta);
        }

        // Analyze per-cycle working set growth (skip first cycle as warmup)
        long avgWsGrowthPerCycle = 0;
        if (cycleWorkingSet.Count > 2)
        {
            long wsGrowthAfterWarmup = cycleWorkingSet[^1] - cycleWorkingSet[1];
            avgWsGrowthPerCycle = wsGrowthAfterWarmup / (cycleWorkingSet.Count - 2);
        }

        var metrics = new Dictionary<string, object>
        {
            ["CompoundNavCycles"] = cycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["PagesLeaked"] = leakedPages.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
            ["AvgGrowthPerCycleBytes"] = avgGrowthPerCycle,
            ["AvgWorkingSetGrowthPerCycleBytes"] = avgWsGrowthPerCycle,
            ["WorkingSetStartBytes"] = cycleWorkingSet.Count > 0 ? cycleWorkingSet[0] : 0,
            ["WorkingSetEndBytes"] = cycleWorkingSet.Count > 0 ? cycleWorkingSet[^1] : 0,
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        bool memoryExceeded = avgGrowthPerCycle > 1024 * 1024;

        if (leakedPages.Count > 0)
        {
            var leakedNames = string.Join(", ", leakedPages);
            logger.LogWarning("Shell+modals+flyout compound leak (pages): {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Page objects leaked: {leakedNames}", metrics);
        }

        if (memoryExceeded)
        {
            logger.LogWarning("Excessive memory growth: {AvgGrowth:N0} bytes/cycle", avgGrowthPerCycle);
            return BenchmarkResult.Fail($"Avg memory growth {avgGrowthPerCycle:N0} bytes/cycle exceeds 1 MB", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} page objects collected after {Cycles} compound navigation cycles. Avg growth: {AvgGrowth:N0} bytes/cycle",
            trackedObjects.Count, cycles, avgGrowthPerCycle);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<(List<long> ManagedMemory, List<long> WorkingSet)> RunCompoundNavigation(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var cycleMemory = new List<long>();
        var cycleWorkingSet = new List<long>();

        var shell = CreateShellWithFlyout();
        window.Page = shell;
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

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await RunSingleCompoundCycle(shell, trackedObjects, cycle, logger, cancellationToken);

            // Capture memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            cycleMemory.Add(GC.GetTotalMemory(false));
            using (var proc = Process.GetCurrentProcess())
            {
                cycleWorkingSet.Add(proc.WorkingSet64);
            }
        }

        // Teardown
        if (shell.Handler is object shellHandler)
        {
            trackedObjects["Shell.Handler"] = new WeakReference<object>(shellHandler);
        }

        shell.Handler?.DisconnectHandler();
        window.Page = this;
        Content = new Label { Text = "Shell+modals+flyout test complete" };

        return (cycleMemory, cycleWorkingSet);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task RunSingleCompoundCycle(
        Shell shell,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Switch to tab 0 and push detail page with ToolbarItems
        shell.CurrentItem = shell.Items[0];
        await Task.Delay(20, cancellationToken);

        var detail1 = CreateDetailPage(trackedObjects, cycle, "Detail1");
        detail1.ToolbarItems.Add(new ToolbarItem { Text = "Edit" });
        detail1.ToolbarItems.Add(new ToolbarItem { Text = "Delete", IconImageSource = "dotnet_bot.png" });

        try
        {
            await shell.Navigation.PushAsync(detail1, animated: false);
            await Task.Delay(30, cancellationToken);

            if (detail1.Handler is object d1Handler)
            {
                trackedObjects[$"Cycle{cycle}.Detail1.Handler"] = new WeakReference<object>(d1Handler);
            }

            // Push a modal on top of Shell navigation
            var modal = CreateModalPage(trackedObjects, cycle);
            await shell.Navigation.PushModalAsync(modal, animated: false);
            await Task.Delay(100, cancellationToken);

            // Dismiss modal
            await shell.Navigation.PopModalAsync(animated: false);
            await Task.Delay(30, cancellationToken);
            modal.Handler?.DisconnectHandler();

            // Pop detail
            await shell.Navigation.PopAsync(animated: false);
            await Task.Delay(20, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Cycle {Cycle} tab0 nav failed: {Error}", cycle, ex.Message);
        }

        // Switch to tab 1 and toggle flyout
        if (shell.Items.Count > 1)
        {
            shell.CurrentItem = shell.Items[1];
            await Task.Delay(20, cancellationToken);

            // Open/close flyout
            shell.FlyoutIsPresented = true;
            await Task.Delay(50, cancellationToken);
            shell.FlyoutIsPresented = false;
            await Task.Delay(20, cancellationToken);
        }

        // Switch to tab 2, push+pop detail
        if (shell.Items.Count > 2)
        {
            shell.CurrentItem = shell.Items[2];
            await Task.Delay(20, cancellationToken);

            var detail2 = CreateDetailPage(trackedObjects, cycle, "Detail2");
            try
            {
                await shell.Navigation.PushAsync(detail2, animated: false);
                await Task.Delay(30, cancellationToken);

                if (detail2.Handler is object d2Handler)
                {
                    trackedObjects[$"Cycle{cycle}.Detail2.Handler"] = new WeakReference<object>(d2Handler);
                }

                await shell.Navigation.PopAsync(animated: false);
                await Task.Delay(20, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Cycle {Cycle} tab2 nav failed: {Error}", cycle, ex.Message);
            }
        }

        // Switch back to tab 0
        shell.CurrentItem = shell.Items[0];
        await Task.Delay(20, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Shell CreateShellWithFlyout()
    {
        var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Flyout };

        for (int i = 0; i < 3; i++)
        {
            var shellItem = new FlyoutItem
            {
                Title = $"Section {i}",
                FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem,
                Items =
                {
                    new ShellContent
                    {
                        Title = $"Tab {i}",
                        ContentTemplate = new DataTemplate(() => new ContentPage
                        {
                            Title = $"Section {i} Home",
                            Content = new VerticalStackLayout
                            {
                                Children =
                                {
                                    new Label { Text = $"Section {i} content" },
                                    new Button { Text = "Navigate" },
                                },
                            },
                        }),
                        Route = $"section{i}",
                    },
                },
            };
            shell.Items.Add(shellItem);
        }

        return shell;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateDetailPage(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle,
        string name)
    {
        var page = new ContentPage
        {
            Title = $"{name} (cycle {cycle})",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = $"Detail page: {name}" },
                    new Entry { Placeholder = "Input" },
                    new Button { Text = "Action" },
                },
            },
        };
        trackedObjects[$"Cycle{cycle}.{name}.Page"] = new WeakReference<object>(page);
        return page;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateModalPage(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle)
    {
        var page = new ContentPage
        {
            Title = $"Modal (cycle {cycle})",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = $"Modal page for cycle {cycle}", FontSize = 20 },
                    new Label { Text = "This modal tests compound navigation" },
                    new Button { Text = "Dismiss" },
                    new Entry { Placeholder = "Modal input" },
                },
            },
        };
        trackedObjects[$"Cycle{cycle}.Modal.Page"] = new WeakReference<object>(page);
        return page;
    }
}
