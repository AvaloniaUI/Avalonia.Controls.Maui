using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that rapidly changing CarouselView's ItemsSource and Position doesn't leak items
/// or handlers. MauiCarouselView subscribes to CollectionChanged on each new source and
/// must properly unsubscribe from the old one.
/// </summary>
[BenchmarkTest("CarouselViewSourceChurnLeak", Description = "Verifies CarouselView handles rapid ItemsSource changes without leaking")]
public class CarouselViewSourceChurnLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int churnCycles = 8;

        await ChurnCarouselViewSource(trackedObjects, churnCycles, cancellationToken);

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
            ["ChurnCycles"] = churnCycles,
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
            logger.LogWarning("CarouselView source churn leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} CarouselView churn objects collected after {Cycles} cycles",
            trackedObjects.Count,
            churnCycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task ChurnCarouselViewSource(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int churnCycles,
        CancellationToken cancellationToken)
    {
        var carouselView = new CarouselView
        {
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 24,
                };
                label.SetBinding(Label.TextProperty, ".");
                return label;
            }),
            HeightRequest = 200,
        };

        Content = carouselView;
        await Task.Delay(50, cancellationToken);

        // Rapidly swap ItemsSource and change Position
        for (int cycle = 0; cycle < churnCycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set new source
            var items = new ObservableCollection<string>();
            for (int i = 0; i < 5; i++)
            {
                items.Add($"Cycle{cycle}_Slide{i}");
            }

            trackedObjects[$"Cycle{cycle}.ItemsSource"] = new WeakReference<object>(items);
            carouselView.ItemsSource = items;
            await Task.Delay(30, cancellationToken);

            // Change position to exercise scroll/layout paths
            if (items.Count > 2)
            {
                carouselView.Position = 2;
                await Task.Delay(20, cancellationToken);
            }

            // Add items while connected
            items.Add($"Cycle{cycle}_Extra");
            await Task.Delay(20, cancellationToken);

            // Remove items while connected
            if (items.Count > 3)
            {
                items.RemoveAt(items.Count - 1);
            }

            await Task.Delay(20, cancellationToken);

            // Clear and null out
            items.Clear();
            carouselView.ItemsSource = null;
            await Task.Delay(20, cancellationToken);
        }

        // Track handler
        if (carouselView.Handler is object handler)
        {
            trackedObjects["CarouselView.Handler"] = new WeakReference<object>(handler);
        }

        trackedObjects["CarouselView"] = new WeakReference<object>(carouselView);

        // Disconnect
        carouselView.Handler?.DisconnectHandler();
        Content = new Label { Text = "CarouselView source churn test complete" };
    }
}
