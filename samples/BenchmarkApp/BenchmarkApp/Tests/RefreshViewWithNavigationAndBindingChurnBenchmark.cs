using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests RefreshView actively refreshing (IsRefreshing=true) when page is popped, combined with
/// BindingContext changes during refresh. Exercises _currentRefreshDeferral and
/// Visualizer.TemplateApplied subscription racing with handler disconnect.
/// </summary>
/// <remarks>
/// Gap: RefreshViewLeakBenchmark toggles IsRefreshing in a flat layout and never navigates.
/// It never has an active refresh at the moment of disconnect.
/// </remarks>
[BenchmarkTest("RefreshViewWithNavAndBindingChurnLeak", Description = "Verifies RefreshView isn't leaked when actively refreshing during navigation pop")]
public class RefreshViewWithNavigationAndBindingChurnBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int cycles = 10;

        var (cycleMemory, cycleWorkingSet) = await PushPopRefreshPages(window, trackedObjects, cycles, logger, cancellationToken);

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

        // Only count RefreshView leaks for pass/fail
        var leakedRefreshViews = leaked.Where(n => n.Contains("RefreshView") && !n.Contains("Handler")).ToList();

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
            ["PushPopCycles"] = cycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["RefreshViewsLeaked"] = leakedRefreshViews.Count,
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

        bool memoryExceeded = avgGrowthPerCycle > 256 * 1024;

        if (leakedRefreshViews.Count > 0)
        {
            var leakedNames = string.Join(", ", leakedRefreshViews);
            logger.LogWarning("RefreshView+navigation leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"RefreshView objects leaked: {leakedNames}", metrics);
        }

        if (memoryExceeded)
        {
            logger.LogWarning("Excessive memory growth: {AvgGrowth:N0} bytes/cycle", avgGrowthPerCycle);
            return BenchmarkResult.Fail($"Avg memory growth {avgGrowthPerCycle:N0} bytes/cycle exceeds 256 KB", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} RefreshView objects collected after {Cycles} cycles. Avg growth: {AvgGrowth:N0} bytes/cycle",
            leakedRefreshViews.Count, cycles, avgGrowthPerCycle);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<(List<long> ManagedMemory, List<long> WorkingSet)> PushPopRefreshPages(
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
            Content = new Label { Text = "RefreshView navigation root" },
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

            await PushPopSingleRefreshPage(navPage, trackedObjects, i, cancellationToken);

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
        if (navPage.Handler is object navHandler)
        {
            trackedObjects["NavigationPage.Handler"] = new WeakReference<object>(navHandler);
        }

        rootPage.Handler?.DisconnectHandler();
        navPage.Handler?.DisconnectHandler();

        window.Page = this;
        Content = new Label { Text = "RefreshView+navigation test complete" };

        return (cycleMemory, cycleWorkingSet);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task PushPopSingleRefreshPage(
        NavigationPage navPage,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index,
        CancellationToken cancellationToken)
    {
        var viewModel1 = new RefreshViewModel { Status = $"Loading {index}..." };
        var viewModel2 = new RefreshViewModel { Status = $"Updated {index}" };
        trackedObjects[$"Cycle{index}.ViewModel1"] = new WeakReference<object>(viewModel1);
        trackedObjects[$"Cycle{index}.ViewModel2"] = new WeakReference<object>(viewModel2);

        var contentLayout = new VerticalStackLayout { Spacing = 8 };
        var statusLabel = new Label();
        statusLabel.SetBinding(Label.TextProperty, nameof(RefreshViewModel.Status));
        contentLayout.Children.Add(statusLabel);

        for (int j = 0; j < 5; j++)
        {
            var itemLabel = new Label { Text = $"Item {j}" };
            contentLayout.Children.Add(itemLabel);
        }

        var refreshView = new RefreshView
        {
            Content = contentLayout,
            RefreshColor = Colors.Blue,
        };
        trackedObjects[$"Cycle{index}.RefreshView"] = new WeakReference<object>(refreshView);
        trackedObjects[$"Cycle{index}.ContentLayout"] = new WeakReference<object>(contentLayout);

        var page = new ContentPage
        {
            Title = $"Refresh Page {index}",
            Content = refreshView,
            BindingContext = viewModel1,
        };
        trackedObjects[$"Cycle{index}.Page"] = new WeakReference<object>(page);

        await navPage.PushAsync(page, animated: false);
        await Task.Delay(30, cancellationToken);

        if (refreshView.Handler is object rvHandler)
        {
            trackedObjects[$"Cycle{index}.RefreshView.Handler"] = new WeakReference<object>(rvHandler);
        }

        // Set IsRefreshing=true to activate refresh
        refreshView.IsRefreshing = true;

        // Immediately change BindingContext while refreshing
        page.BindingContext = viewModel2;

        // Pop page WITHOUT waiting for refresh completion
        await navPage.PopAsync(animated: false);
        await Task.Delay(10, cancellationToken);
    }

    private class RefreshViewModel : BindableObject
    {
        public static readonly BindableProperty StatusProperty =
            BindableProperty.Create(nameof(Status), typeof(string), typeof(RefreshViewModel), string.Empty);

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }
    }
}
