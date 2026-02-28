using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that all 7 shape controls (Ellipse, Line, Path, Polygon, Polyline, Rectangle,
/// RoundRectangle) and their handlers are collected after disconnect. Shapes hold Fill/Stroke
/// brush references and geometry data that must be cleaned up.
/// </summary>
[BenchmarkTest("ShapeHandlerLeak", Description = "Verifies all shape controls are collected after disconnect")]
public class ShapeHandlerLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyShapes(trackedObjects, cancellationToken);

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
            ["ShapesTested"] = 7,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["Ellipse.Leaked"] = leaked.Any(n => n.StartsWith("Ellipse")),
            ["Line.Leaked"] = leaked.Any(n => n.StartsWith("Line")),
            ["Path.Leaked"] = leaked.Any(n => n.StartsWith("Path")),
            ["Polygon.Leaked"] = leaked.Any(n => n.StartsWith("Polygon")),
            ["Polyline.Leaked"] = leaked.Any(n => n.StartsWith("Polyline")),
            ["Rectangle.Leaked"] = leaked.Any(n => n.StartsWith("Rectangle")),
            ["RoundRectangle.Leaked"] = leaked.Any(n => n.StartsWith("RoundRectangle")),
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Shape handler leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} shape objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyShapes(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        CreateShapes(trackedObjects, layout);

        // Allow handlers to connect
        await Task.Delay(50, cancellationToken);

        // Track handlers before disconnect
        TrackHandlers(trackedObjects, layout);

        // Tear down
        TearDown(layout);

        Content = new Label { Text = "Shape handler test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateShapes(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var fillBrush = new SolidColorBrush(Colors.Blue);
        var strokeBrush = new SolidColorBrush(Colors.Red);

        // Ellipse
        var ellipse = new Ellipse
        {
            Fill = fillBrush,
            Stroke = strokeBrush,
            StrokeThickness = 2,
            WidthRequest = 100,
            HeightRequest = 60,
        };
        layout.Children.Add(ellipse);
        trackedObjects["Ellipse"] = new WeakReference<object>(ellipse);

        // Line
        var line = new Line
        {
            X1 = 0, Y1 = 0,
            X2 = 100, Y2 = 50,
            Stroke = strokeBrush,
            StrokeThickness = 2,
        };
        layout.Children.Add(line);
        trackedObjects["Line"] = new WeakReference<object>(line);

        // Path with PathGeometry
        var pathFigure = new PathFigure { StartPoint = new Point(0, 0) };
        pathFigure.Segments.Add(new LineSegment { Point = new Point(50, 100) });
        pathFigure.Segments.Add(new LineSegment { Point = new Point(100, 0) });
        var pathGeometry = new PathGeometry();
        pathGeometry.Figures.Add(pathFigure);
        var path = new Microsoft.Maui.Controls.Shapes.Path
        {
            Data = pathGeometry,
            Fill = fillBrush,
            Stroke = strokeBrush,
            StrokeThickness = 2,
        };
        layout.Children.Add(path);
        trackedObjects["Path"] = new WeakReference<object>(path);
        trackedObjects["Path.Geometry"] = new WeakReference<object>(pathGeometry);

        // Polygon
        var polygon = new Polygon
        {
            Points = new PointCollection
            {
                new Point(0, 50),
                new Point(50, 0),
                new Point(100, 50),
                new Point(75, 100),
                new Point(25, 100),
            },
            Fill = fillBrush,
            Stroke = strokeBrush,
            StrokeThickness = 2,
        };
        layout.Children.Add(polygon);
        trackedObjects["Polygon"] = new WeakReference<object>(polygon);

        // Polyline
        var polyline = new Polyline
        {
            Points = new PointCollection
            {
                new Point(0, 0),
                new Point(25, 50),
                new Point(50, 0),
                new Point(75, 50),
                new Point(100, 0),
            },
            Stroke = strokeBrush,
            StrokeThickness = 2,
        };
        layout.Children.Add(polyline);
        trackedObjects["Polyline"] = new WeakReference<object>(polyline);

        // Rectangle
        var rectangle = new Microsoft.Maui.Controls.Shapes.Rectangle
        {
            Fill = fillBrush,
            Stroke = strokeBrush,
            StrokeThickness = 2,
            WidthRequest = 100,
            HeightRequest = 60,
            RadiusX = 5,
            RadiusY = 5,
        };
        layout.Children.Add(rectangle);
        trackedObjects["Rectangle"] = new WeakReference<object>(rectangle);

        // RoundRectangle
        var roundRect = new RoundRectangle
        {
            CornerRadius = new CornerRadius(10, 20, 30, 40),
            WidthRequest = 100,
            HeightRequest = 60,
            Fill = fillBrush,
            Stroke = strokeBrush,
            StrokeThickness = 2,
        };
        layout.Children.Add(roundRect);
        trackedObjects["RoundRectangle"] = new WeakReference<object>(roundRect);

        // Track brushes
        trackedObjects["FillBrush"] = new WeakReference<object>(fillBrush);
        trackedObjects["StrokeBrush"] = new WeakReference<object>(strokeBrush);
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
