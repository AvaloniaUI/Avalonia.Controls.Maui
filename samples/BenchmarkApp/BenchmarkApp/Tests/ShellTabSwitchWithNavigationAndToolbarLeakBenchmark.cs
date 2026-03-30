using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// The closest simulation to AlohaAI's actual navigation: Shell tab switching PLUS
/// NavigationPage push/pop within tabs PLUS toolbar item changes on pushed pages.
/// Exercises Shell -> ShellSectionHandler -> StackNavigationManager -> UpdateToolbarItems
/// in combination.
/// </summary>
/// <remarks>
/// Gap: AlohaTabBarNavigationLeakBenchmark switches tabs but tabs contain static ShellContent
/// with no in-tab navigation. ShellTabSwitchMemoryGrowthBenchmark is even simpler. Neither
/// exercises the compound path of Shell tab switch + in-tab page push with toolbar items.
/// </remarks>
[BenchmarkTest("ShellTabSwitchWithNavigationAndToolbarLeak", Description = "Verifies Shell tab switching + in-tab push/pop with toolbars doesn't leak")]
public class ShellTabSwitchWithNavigationAndToolbarLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int cycles = 10;

        var (cycleMemory, cycleWorkingSet) = await RunShellTabNavigation(window, trackedObjects, cycles, logger, cancellationToken);

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

        logger.LogInformation("Per-cycle memory (bytes after GC):");
        for (int i = 0; i < cycleMemory.Count; i++)
        {
            long delta = i > 0 ? cycleMemory[i] - cycleMemory[i - 1] : 0;
            long wsDelta = i > 0 ? cycleWorkingSet[i] - cycleWorkingSet[i - 1] : 0;
            logger.LogInformation(
                "  Cycle {Cycle}: {Memory:N0} bytes (delta: {Delta:+#,##0;-#,##0;0}) | WorkingSet: {WorkingSet:N0} bytes (delta: {WsDelta:+#,##0;-#,##0;0})",
                i, cycleMemory[i], delta, cycleWorkingSet[i], wsDelta);
        }

        // Only count page leaks for the pass/fail check
        var leakedPages = leaked.Where(n => n.Contains(".Page")).ToList();

        // Analyze per-cycle working set growth (skip first cycle as warmup)
        long avgWsGrowthPerCycle = 0;
        if (cycleWorkingSet.Count > 2)
        {
            long wsGrowthAfterWarmup = cycleWorkingSet[^1] - cycleWorkingSet[1];
            avgWsGrowthPerCycle = wsGrowthAfterWarmup / (cycleWorkingSet.Count - 2);
        }

        var metrics = new Dictionary<string, object>
        {
            ["TabSwitchNavCycles"] = cycles,
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

        bool memoryExceeded = avgGrowthPerCycle > 512 * 1024;

        if (leakedPages.Count > 0)
        {
            var leakedNames = string.Join(", ", leakedPages);
            logger.LogWarning("Shell tab+nav leak detected (pages): {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Page objects leaked: {leakedNames}", metrics);
        }

        if (memoryExceeded)
        {
            logger.LogWarning("Excessive memory growth: {AvgGrowth:N0} bytes/cycle", avgGrowthPerCycle);
            return BenchmarkResult.Fail($"Avg memory growth {avgGrowthPerCycle:N0} bytes/cycle exceeds 512 KB", metrics);
        }

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        logger.LogInformation(
            "All {PageCount} pages collected after {Cycles} cycles. Avg growth: {AvgGrowth:N0} bytes/cycle",
            leakedPages.Count, cycles, avgGrowthPerCycle);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<(List<long> ManagedMemory, List<long> WorkingSet)> RunShellTabNavigation(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var cycleMemory = new List<long>();
        var cycleWorkingSet = new List<long>();

        var shell = CreateShellWithTabs();
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

        var tabBar = (TabBar)shell.Items[0];

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Switch to tab 1, push detail page with 2 ToolbarItems
            shell.CurrentItem = tabBar.Items[Math.Min(1, tabBar.Items.Count - 1)];
            await Task.Delay(20, cancellationToken);
            await PushDetailWithToolbar(shell, trackedObjects, cycle, "Tab1", 2, cancellationToken);
            await PopCurrentPage(shell, cancellationToken);

            // Switch to tab 2, push different detail page with 3 ToolbarItems
            shell.CurrentItem = tabBar.Items[Math.Min(2, tabBar.Items.Count - 1)];
            await Task.Delay(20, cancellationToken);
            await PushDetailWithToolbar(shell, trackedObjects, cycle, "Tab2", 3, cancellationToken);
            await PopCurrentPage(shell, cancellationToken);

            // Switch to tab 3, push+pop quickly
            shell.CurrentItem = tabBar.Items[Math.Min(3, tabBar.Items.Count - 1)];
            await Task.Delay(20, cancellationToken);
            await PushDetailWithToolbar(shell, trackedObjects, cycle, "Tab3", 1, cancellationToken);
            await PopCurrentPage(shell, cancellationToken);

            // Switch back to tab 0
            shell.CurrentItem = tabBar.Items[0];
            await Task.Delay(20, cancellationToken);

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
        Content = new Label { Text = "Shell tab+nav+toolbar test complete" };

        return (cycleMemory, cycleWorkingSet);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Shell CreateShellWithTabs()
    {
        var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Disabled };
        var tabBar = new TabBar();

        for (int i = 0; i < 4; i++)
        {
            var tab = new ShellContent
            {
                Title = $"Tab {i}",
                ContentTemplate = new DataTemplate(() => new ContentPage
                {
                    Title = $"Tab {i} Home",
                    Content = new Label { Text = $"Tab {i} content" },
                }),
                Route = $"tab{i}",
            };
            tabBar.Items.Add(tab);
        }

        shell.Items.Add(tabBar);
        return shell;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task PushDetailWithToolbar(
        Shell shell,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle,
        string tabLabel,
        int toolbarCount,
        CancellationToken cancellationToken)
    {
        var page = new ContentPage
        {
            Title = $"Detail {cycle}-{tabLabel}",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = $"Detail for {tabLabel}" },
                    new Entry { Placeholder = "Input" },
                },
            },
        };
        trackedObjects[$"Cycle{cycle}.{tabLabel}.Page"] = new WeakReference<object>(page);

        for (int t = 0; t < toolbarCount; t++)
        {
            var toolbar = new ToolbarItem
            {
                Text = $"Action{t}",
                Order = ToolbarItemOrder.Primary,
            };
            if (t == 0)
            {
                toolbar.IconImageSource = "dotnet_bot.png";
            }

            toolbar.Clicked += (_, _) => { };
            page.ToolbarItems.Add(toolbar);
            trackedObjects[$"Cycle{cycle}.{tabLabel}.Toolbar{t}"] = new WeakReference<object>(toolbar);
        }

        await shell.Navigation.PushAsync(page, animated: false);
        await Task.Delay(50, cancellationToken);

        if (page.Handler is object handler)
        {
            trackedObjects[$"Cycle{cycle}.{tabLabel}.Page.Handler"] = new WeakReference<object>(handler);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task PopCurrentPage(Shell shell, CancellationToken cancellationToken)
    {
        await shell.Navigation.PopAsync(animated: false);
        await Task.Delay(20, cancellationToken);
    }
}
