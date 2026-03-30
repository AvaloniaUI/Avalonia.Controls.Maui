

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that Image controls don't leak after source changes and handler disconnect.
/// Targets ImageHandler missing ConnectHandler/DisconnectHandler overrides and _loadCts not being cancelled.
/// </summary>
[BenchmarkTest("ImageSourceLeak", Description = "Verifies Image controls are collected after source changes and disconnect")]
public class ImageSourceLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int imageCount = 20;

        // Create images, change sources, disconnect
        await CreateAndTrackImages(trackedObjects, imageCount, cancellationToken);

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
            ["ImagesCreated"] = imageCount,
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
            logger.LogWarning("Image source leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} Image objects collected successfully",
            trackedObjects.Count);

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndTrackImages(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int count,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        // Create images and track them
        CreateImageBatch(trackedObjects, layout, count);

        // Allow handlers to connect and image loading to begin
        await Task.Delay(50, cancellationToken);

        // Change sources to simulate real-world usage (e.g., scrolling a list with images)
        ChangeImageSources(layout);

        await Task.Delay(50, cancellationToken);

        // Disconnect and tear down
        DisconnectAndClear(trackedObjects, layout);

        Content = new Label { Text = "Images cleared" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateImageBatch(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout,
        int count)
    {
        for (int i = 0; i < count; i++)
        {
            var image = new Image
            {
                WidthRequest = 50,
                HeightRequest = 50,
                Aspect = Aspect.AspectFit,
            };

            layout.Children.Add(image);
            trackedObjects[$"Image[{i}]"] = new WeakReference<object>(image);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ChangeImageSources(VerticalStackLayout layout)
    {
        // Clear sources to simulate images going off-screen
        foreach (var child in layout.Children)
        {
            if (child is Image image)
            {
                image.Source = null;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DisconnectAndClear(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        // Track handlers before disconnecting
        int index = 0;
        foreach (var child in layout.Children)
        {
            if (child is Image image && image.Handler is object handler)
            {
                trackedObjects[$"Image[{index}].Handler"] = new WeakReference<object>(handler);
            }

            index++;
        }

        // Disconnect handlers
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve)
            {
                ve.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
    }
}
