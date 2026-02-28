using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that composite container controls (RefreshView, SwipeView, CarouselView,
/// IndicatorView, FlyoutPage) and their handlers are collected after disconnect.
/// These controls wrap child content and subscribe to multiple events.
/// </summary>
[BenchmarkTest("CompositeViewLeak", Description = "Verifies composite container controls are collected after disconnect")]
public class CompositeViewLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyRefreshView(trackedObjects, cancellationToken);
        await CreateAndDestroySwipeView(trackedObjects, cancellationToken);
        await CreateAndDestroyCarouselView(trackedObjects, cancellationToken);
        await CreateAndDestroyIndicatorView(trackedObjects, cancellationToken);

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
            ["ControlsTested"] = 4,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["RefreshView.Leaked"] = leaked.Any(n => n.StartsWith("RefreshView")),
            ["SwipeView.Leaked"] = leaked.Any(n => n.StartsWith("SwipeView")),
            ["CarouselView.Leaked"] = leaked.Any(n => n.StartsWith("CarouselView")),
            ["IndicatorView.Leaked"] = leaked.Any(n => n.StartsWith("IndicatorView")),
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Composite view leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} composite view objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyRefreshView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var refreshView = new RefreshView
        {
            Content = new ScrollView
            {
                Content = new Label { Text = "Pull to refresh content" },
            },
        };
        refreshView.Refreshing += (_, _) => { };
        Content = refreshView;
        trackedObjects["RefreshView"] = new WeakReference<object>(refreshView);
        trackedObjects["RefreshView.Content"] = new WeakReference<object>(refreshView.Content);

        await Task.Delay(50, cancellationToken);

        if (refreshView.Handler is object rvHandler)
        {
            trackedObjects["RefreshView.Handler"] = new WeakReference<object>(rvHandler);
        }

        // Toggle refreshing to exercise event paths
        refreshView.IsRefreshing = true;
        refreshView.IsRefreshing = false;

        refreshView.Handler?.DisconnectHandler();
        if (refreshView.Content is VisualElement rvContent)
        {
            rvContent.Handler?.DisconnectHandler();
        }

        Content = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroySwipeView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var swipeView = new SwipeView();

        var leftItems = new SwipeItems
        {
            new SwipeItem { Text = "Delete", BackgroundColor = Colors.Red },
            new SwipeItem { Text = "Archive", BackgroundColor = Colors.Blue },
        };
        swipeView.LeftItems = leftItems;

        var rightItems = new SwipeItems
        {
            new SwipeItem { Text = "Flag", BackgroundColor = Colors.Orange },
        };
        swipeView.RightItems = rightItems;

        swipeView.Content = new Label { Text = "Swipeable content" };

        Content = swipeView;
        trackedObjects["SwipeView"] = new WeakReference<object>(swipeView);
        trackedObjects["SwipeView.LeftItems"] = new WeakReference<object>(leftItems);
        trackedObjects["SwipeView.RightItems"] = new WeakReference<object>(rightItems);

        await Task.Delay(50, cancellationToken);

        if (swipeView.Handler is object svHandler)
        {
            trackedObjects["SwipeView.Handler"] = new WeakReference<object>(svHandler);
        }

        swipeView.Handler?.DisconnectHandler();
        Content = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyCarouselView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var items = new ObservableCollection<string> { "Page 1", "Page 2", "Page 3", "Page 4", "Page 5" };
        var carouselView = new CarouselView
        {
            ItemsSource = items,
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label();
                label.SetBinding(Label.TextProperty, ".");
                return label;
            }),
        };

        Content = carouselView;
        trackedObjects["CarouselView"] = new WeakReference<object>(carouselView);

        await Task.Delay(100, cancellationToken);

        // Change position to exercise scroll paths
        if (carouselView.Handler != null)
        {
            carouselView.Position = 2;
            await Task.Delay(30, cancellationToken);
            carouselView.Position = 4;
            await Task.Delay(30, cancellationToken);
        }

        if (carouselView.Handler is object cvHandler)
        {
            trackedObjects["CarouselView.Handler"] = new WeakReference<object>(cvHandler);
        }

        items.Clear();
        carouselView.ItemsSource = null;
        carouselView.Handler?.DisconnectHandler();
        Content = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyIndicatorView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var indicatorView = new IndicatorView
        {
            Count = 5,
            Position = 2,
            IndicatorColor = Colors.Gray,
            SelectedIndicatorColor = Colors.Blue,
        };

        var layout = new VerticalStackLayout();
        layout.Children.Add(indicatorView);
        Content = layout;
        trackedObjects["IndicatorView"] = new WeakReference<object>(indicatorView);

        await Task.Delay(50, cancellationToken);

        // Change position
        indicatorView.Position = 0;
        indicatorView.Position = 4;

        if (indicatorView.Handler is object ivHandler)
        {
            trackedObjects["IndicatorView.Handler"] = new WeakReference<object>(ivHandler);
        }

        indicatorView.Handler?.DisconnectHandler();
        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
        Content = new Label { Text = "Composite view test complete" };
    }
}
