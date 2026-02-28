using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that SwipeView controls with SwipeItems are collected after handler disconnect.
/// SwipeViewHandler subscribes to SwipeStarted, SwipeChanging, SwipeEnded, and ExecuteRequested
/// events in ConnectHandler — this verifies those subscriptions are properly cleaned up.
/// </summary>
[BenchmarkTest("SwipeViewLeak", Description = "Verifies SwipeView + SwipeItems are collected after disconnect")]
public class SwipeViewLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int iterations = 5;

        BuildAndTearDown(trackedObjects, iterations);

        await Task.Delay(50, cancellationToken);

        // Force GC multiple times with delays
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
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
            ["Iterations"] = iterations,
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
            logger.LogWarning("SwipeView leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} SwipeView objects collected after {Iterations} iterations",
            trackedObjects.Count,
            iterations);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void BuildAndTearDown(Dictionary<string, WeakReference<object>> trackedObjects, int iterations)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        for (int i = 0; i < iterations; i++)
        {
            var swipeView = CreateSwipeView(trackedObjects, i);
            layout.Children.Add(swipeView);
        }

        // Track handlers after connection
        int childIndex = 0;
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve && ve.Handler is object handler)
            {
                trackedObjects[$"SwipeView{childIndex}.Handler"] = new WeakReference<object>(handler);
            }

            childIndex++;
        }

        // Disconnect all
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve)
            {
                ve.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
        Content = new Label { Text = "SwipeView test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static SwipeView CreateSwipeView(Dictionary<string, WeakReference<object>> trackedObjects, int index)
    {
        var leftItems = new SwipeItems
        {
            new SwipeItem
            {
                Text = $"Delete {index}",
                BackgroundColor = Colors.Red,
            },
            new SwipeItem
            {
                Text = $"Archive {index}",
                BackgroundColor = Colors.Blue,
            },
        };

        var rightItems = new SwipeItems
        {
            new SwipeItem
            {
                Text = $"Flag {index}",
                BackgroundColor = Colors.Orange,
            },
        };

        var swipeView = new SwipeView
        {
            LeftItems = leftItems,
            RightItems = rightItems,
            Content = new Label { Text = $"Swipeable item {index}", Padding = new Thickness(20) },
        };

        trackedObjects[$"SwipeView{index}"] = new WeakReference<object>(swipeView);
        trackedObjects[$"SwipeView{index}.LeftItems"] = new WeakReference<object>(leftItems);
        trackedObjects[$"SwipeView{index}.RightItems"] = new WeakReference<object>(rightItems);

        return swipeView;
    }
}
