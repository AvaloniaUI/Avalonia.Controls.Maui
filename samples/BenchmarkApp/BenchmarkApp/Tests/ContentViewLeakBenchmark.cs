using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that ContentView and Border controls with nested content are collected after disconnect.
/// Exercises content swapping to verify child reference cleanup in ContentPresenter paths.
/// </summary>
[BenchmarkTest("ContentViewLeak", Description = "Verifies ContentView and Border with content swapping are collected after disconnect")]
public class ContentViewLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyContentViews(trackedObjects, cancellationToken);

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
            ["ControlsTested"] = 2,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["ContentView.Leaked"] = leaked.Any(n => n.StartsWith("ContentView")),
            ["Border.Leaked"] = leaked.Any(n => n.StartsWith("Border")),
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("ContentView leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        logger.LogInformation(
            "All {Count} ContentView objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyContentViews(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        CreateContentViews(trackedObjects, layout);

        // Allow handlers to connect
        await Task.Delay(50, cancellationToken);

        // Swap content to exercise content change paths
        SwapContent(trackedObjects, layout);

        await Task.Delay(50, cancellationToken);

        // Track handlers before disconnect
        TrackHandlers(trackedObjects, layout);

        // Tear down
        TearDown(layout);

        Content = new Label { Text = "ContentView test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateContentViews(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        // ContentView with nested Label content
        var contentView = new ContentView();
        var innerLabel = new Label { Text = "ContentView child" };
        contentView.Content = innerLabel;
        layout.Children.Add(contentView);
        trackedObjects["ContentView"] = new WeakReference<object>(contentView);
        trackedObjects["ContentView.OriginalChild"] = new WeakReference<object>(innerLabel);

        // Border with Label content and stroke/shape
        var border = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            Stroke = Colors.Gray,
            StrokeThickness = 2,
            Padding = new Thickness(10),
            Content = new Label { Text = "Border child" },
        };
        layout.Children.Add(border);
        trackedObjects["Border"] = new WeakReference<object>(border);
        trackedObjects["Border.OriginalChild"] = new WeakReference<object>(border.Content);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SwapContent(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        // Swap ContentView content
        if (layout.Children[0] is ContentView cv)
        {
            cv.Content = null;
            var newLabel = new Label { Text = "Swapped content" };
            cv.Content = newLabel;
            trackedObjects["ContentView.SwappedChild"] = new WeakReference<object>(newLabel);
            // Set to null to release
            cv.Content = null;
        }

        // Swap Border content
        if (layout.Children[1] is Border border)
        {
            border.Content = null;
            var newLabel = new Label { Text = "Swapped border content" };
            border.Content = newLabel;
            trackedObjects["Border.SwappedChild"] = new WeakReference<object>(newLabel);
            border.Content = null;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackHandlers(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve && ve.Handler is object handler)
            {
                var typeName = ve.GetType().Name;
                trackedObjects[$"{typeName}.Handler"] = new WeakReference<object>(handler);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDown(VerticalStackLayout layout)
    {
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
