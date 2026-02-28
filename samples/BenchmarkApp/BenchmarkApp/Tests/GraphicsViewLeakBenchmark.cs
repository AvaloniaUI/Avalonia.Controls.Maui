using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that GraphicsView with a custom IDrawable is collected after handler disconnect.
/// The GraphicsViewHandler has explicit Connect/Disconnect for Drawable resources — this
/// verifies those are properly released.
/// </summary>
[BenchmarkTest("GraphicsViewLeak", Description = "Verifies GraphicsView + IDrawable are collected after disconnect")]
public class GraphicsViewLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int iterations = 5;

        await BuildAndTearDown(trackedObjects, iterations, cancellationToken);

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
            logger.LogWarning("GraphicsView leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} GraphicsView objects collected after {Iterations} iterations",
            trackedObjects.Count,
            iterations);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task BuildAndTearDown(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int iterations,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        for (int i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (graphicsView, drawable) = CreateGraphicsView(i);
            trackedObjects[$"GraphicsView{i}"] = new WeakReference<object>(graphicsView);
            trackedObjects[$"Drawable{i}"] = new WeakReference<object>(drawable);

            layout.Children.Add(graphicsView);
            await Task.Delay(30, cancellationToken);

            // Track handler
            if (graphicsView.Handler is object handler)
            {
                trackedObjects[$"GraphicsView{i}.Handler"] = new WeakReference<object>(handler);
            }

            // Trigger invalidation
            graphicsView.Invalidate();
            await Task.Delay(20, cancellationToken);

            // Remove and disconnect
            layout.Children.Remove(graphicsView);
            graphicsView.Handler?.DisconnectHandler();
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
        Content = new Label { Text = "GraphicsView test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (GraphicsView View, IDrawable Drawable) CreateGraphicsView(int index)
    {
        var drawable = new TestDrawable(index);
        var graphicsView = new GraphicsView
        {
            Drawable = drawable,
            HeightRequest = 100,
            WidthRequest = 200,
        };

        return (graphicsView, drawable);
    }

    private sealed class TestDrawable(int index) : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Blue;
            canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);
            canvas.FontColor = Colors.White;
            canvas.DrawString($"View {index}", dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
