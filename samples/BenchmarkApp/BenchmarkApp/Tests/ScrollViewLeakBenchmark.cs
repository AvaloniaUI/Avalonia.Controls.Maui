

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that ScrollView controls don't leak after removal and handler disconnect.
/// Targets MauiCollectionView._scrollViewer.ScrollChanged not being unsubscribed.
/// </summary>
[BenchmarkTest("ScrollViewLeak", Description = "Verifies ScrollView controls are collected after disconnect")]
public class ScrollViewLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int scrollViewCount = 10;

        // Create and destroy ScrollViews multiple times
        await CreateAndDestroyScrollViews(trackedObjects, scrollViewCount, cancellationToken);

        // Force GC multiple times with delays
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(50, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

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

        var metrics = new Dictionary<string, object>
        {
            ["ScrollViewsCreated"] = scrollViewCount,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("ScrollView leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} ScrollView objects collected successfully",
            trackedObjects.Count);

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyScrollViews(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int count,
        CancellationToken cancellationToken)
    {
        var rootLayout = new VerticalStackLayout();
        Content = rootLayout;

        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create a ScrollView with content
            CreateScrollViewIteration(trackedObjects, rootLayout, i);

            // Allow handlers to connect
            await Task.Delay(30, cancellationToken);

            // Tear down the ScrollView
            TearDownScrollView(trackedObjects, rootLayout, i);

            await Task.Delay(20, cancellationToken);
        }

        // Final cleanup
        rootLayout.Handler?.DisconnectHandler();
        Content = new Label { Text = "ScrollView test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateScrollViewIteration(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout rootLayout,
        int index)
    {
        var scrollView = new ScrollView();
        trackedObjects[$"ScrollView[{index}]"] = new WeakReference<object>(scrollView);

        var innerLayout = new VerticalStackLayout();
        trackedObjects[$"ScrollView[{index}].InnerLayout"] = new WeakReference<object>(innerLayout);

        // Add several children to the inner layout to simulate real content
        for (int j = 0; j < 5; j++)
        {
            var label = new Label { Text = $"ScrollView {index} Item {j}" };
            innerLayout.Children.Add(label);
            trackedObjects[$"ScrollView[{index}].Label[{j}]"] = new WeakReference<object>(label);
        }

        var button = new Button { Text = $"ScrollView {index} Button" };
        button.Clicked += (_, _) => { };
        innerLayout.Children.Add(button);
        trackedObjects[$"ScrollView[{index}].Button"] = new WeakReference<object>(button);

        scrollView.Content = innerLayout;
        rootLayout.Children.Add(scrollView);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDownScrollView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout rootLayout,
        int index)
    {
        if (rootLayout.Children.Count == 0)
            return;

        var scrollView = rootLayout.Children[rootLayout.Children.Count - 1] as ScrollView;
        if (scrollView == null)
            return;

        // Track the ScrollView handler
        if (scrollView.Handler is object svHandler)
        {
            trackedObjects[$"ScrollView[{index}].Handler"] = new WeakReference<object>(svHandler);
        }

        // Track inner layout handler
        if (scrollView.Content is Layout innerLayout)
        {
            if (innerLayout.Handler is object ilHandler)
            {
                trackedObjects[$"ScrollView[{index}].InnerLayout.Handler"] = new WeakReference<object>(ilHandler);
            }

            // Disconnect children handlers
            foreach (var child in innerLayout.Children)
            {
                if (child is VisualElement ve)
                {
                    ve.Handler?.DisconnectHandler();
                }
            }

            innerLayout.Children.Clear();
            innerLayout.Handler?.DisconnectHandler();
        }

        // Disconnect ScrollView
        scrollView.Content = null;
        scrollView.Handler?.DisconnectHandler();
        rootLayout.Children.Remove(scrollView);
    }
}
