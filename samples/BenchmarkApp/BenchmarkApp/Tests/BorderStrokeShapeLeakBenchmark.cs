using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that Border controls with RoundRectangle StrokeShape can be collected
/// after removal from a layout, both with and without explicit DisconnectHandler.
/// Inspired by the 2048Game GC dump pattern where RoundRectangle instances accumulated
/// over sustained tile churn during attract mode.
/// </summary>
[BenchmarkTest("BorderStrokeShapeLeak",
    Description = "Verifies Border and its RoundRectangle StrokeShape can be GC'd after removal from layout")]
public class BorderStrokeShapeLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var borderWeakRefs = new Dictionary<string, WeakReference<VisualElement>>();
        var shapeWeakRefs = new Dictionary<string, WeakReference<object>>();

        // Phase 1: Single border with RoundRectangle, removed without DisconnectHandler
        await TestSingleBorderRemoval(borderWeakRefs, shapeWeakRefs, cancellationToken);

        // Phase 2: Multiple borders simulating game tile churn (like 2048Game)
        await TestTileChurn(borderWeakRefs, shapeWeakRefs, cancellationToken);

        // Phase 3: Single border with explicit DisconnectHandler (control case)
        await TestWithExplicitDisconnect(borderWeakRefs, shapeWeakRefs, cancellationToken);

        // Force GC with delays
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(200, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Check which objects leaked
        var leakedBorders = new List<string>();
        foreach (var (name, weakRef) in borderWeakRefs)
        {
            if (weakRef.TryGetTarget(out _))
                leakedBorders.Add(name);
        }

        var leakedShapes = new List<string>();
        foreach (var (name, weakRef) in shapeWeakRefs)
        {
            if (weakRef.TryGetTarget(out _))
                leakedShapes.Add(name);
        }

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);

        var metrics = new Dictionary<string, object>
        {
            ["BordersTested"] = borderWeakRefs.Count,
            ["BordersLeaked"] = leakedBorders.Count,
            ["ShapesTested"] = shapeWeakRefs.Count,
            ["ShapesLeaked"] = leakedShapes.Count,
        };

        foreach (var (name, weakRef) in borderWeakRefs)
            metrics[$"{name}.Leaked"] = weakRef.TryGetTarget(out _);

        foreach (var (name, weakRef) in shapeWeakRefs)
            metrics[$"{name}.Leaked"] = weakRef.TryGetTarget(out _);

        foreach (var (key, value) in memoryDelta.ToMetrics())
            metrics[key] = value;

        var failures = new List<string>();

        if (leakedBorders.Count > 0)
            failures.Add($"Borders leaked: {string.Join(", ", leakedBorders)}");

        if (leakedShapes.Count > 0)
            failures.Add($"StrokeShapes leaked: {string.Join(", ", leakedShapes)}");

        if (failures.Count > 0)
        {
            var failureMsg = string.Join("; ", failures);
            logger.LogWarning("Memory leak detected: {Failures}", failureMsg);
            return BenchmarkResult.Fail(failureMsg, metrics);
        }

        logger.LogInformation(
            "All {BorderCount} borders and {ShapeCount} stroke shapes collected successfully",
            borderWeakRefs.Count, shapeWeakRefs.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task TestSingleBorderRemoval(
        Dictionary<string, WeakReference<VisualElement>> borderWeakRefs,
        Dictionary<string, WeakReference<object>> shapeWeakRefs,
        CancellationToken cancellationToken)
    {
        var layout = new AbsoluteLayout();
        Content = layout;

        var roundRect = new RoundRectangle { CornerRadius = 5 };
        var border = new Border
        {
            HeightRequest = 70,
            WidthRequest = 70,
            StrokeShape = roundRect,
            Stroke = Colors.Transparent,
            BackgroundColor = Colors.Orange,
            Content = new Label { Text = "2048", HorizontalOptions = LayoutOptions.Center }
        };

        layout.Children.Add(border);
        borderWeakRefs["Single.Border"] = new WeakReference<VisualElement>(border);
        shapeWeakRefs["Single.RoundRect"] = new WeakReference<object>(roundRect);

        await Task.Delay(50, cancellationToken);

        // Remove without DisconnectHandler — the pattern that leaks in 2048Game
        layout.Children.Remove(border);

        border = null;
        roundRect = null;

        Content = new Label { Text = "Phase 1 done" };
        layout = null;

        await Task.Delay(50, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task TestTileChurn(
        Dictionary<string, WeakReference<VisualElement>> borderWeakRefs,
        Dictionary<string, WeakReference<object>> shapeWeakRefs,
        CancellationToken cancellationToken)
    {
        var layout = new AbsoluteLayout();
        Content = layout;

        // Simulate 2048Game tile lifecycle: create tiles, move them, merge (remove),
        // create new tiles — repeat for multiple rounds
        for (int round = 0; round < 5; round++)
        {
            var tiles = new List<Border>();

            // Create 4 tiles (like a new game)
            for (int i = 0; i < 4; i++)
            {
                var roundRect = new RoundRectangle { CornerRadius = 5 };
                var tile = new Border
                {
                    HeightRequest = 70,
                    WidthRequest = 70,
                    StrokeShape = roundRect,
                    Stroke = Colors.Transparent,
                    BackgroundColor = Colors.Coral,
                    Content = new Label
                    {
                        Text = ((i + 1) * 2).ToString(),
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };

                layout.Children.Add(tile);
                tiles.Add(tile);

                // Track the last round's objects for GC verification
                if (round == 4)
                {
                    borderWeakRefs[$"Churn.Border_{i}"] = new WeakReference<VisualElement>(tile);
                    shapeWeakRefs[$"Churn.RoundRect_{i}"] = new WeakReference<object>(roundRect);
                }
            }

            await Task.Delay(30, cancellationToken);

            // "Merge" — remove tiles without DisconnectHandler (like 2048Game does)
            layout.Children.Clear();
            tiles.Clear();
        }

        Content = new Label { Text = "Phase 2 done" };
        layout = null;

        await Task.Delay(50, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task TestWithExplicitDisconnect(
        Dictionary<string, WeakReference<VisualElement>> borderWeakRefs,
        Dictionary<string, WeakReference<object>> shapeWeakRefs,
        CancellationToken cancellationToken)
    {
        var layout = new AbsoluteLayout();
        Content = layout;

        var roundRect = new RoundRectangle { CornerRadius = 5 };
        var border = new Border
        {
            HeightRequest = 70,
            WidthRequest = 70,
            StrokeShape = roundRect,
            Stroke = Colors.Transparent,
            BackgroundColor = Colors.Green,
            Content = new Label { Text = "OK" }
        };

        layout.Children.Add(border);
        borderWeakRefs["Explicit.Border"] = new WeakReference<VisualElement>(border);
        shapeWeakRefs["Explicit.RoundRect"] = new WeakReference<object>(roundRect);

        await Task.Delay(50, cancellationToken);

        // Remove WITH explicit DisconnectHandler — this should always work
        layout.Children.Remove(border);
        border.Handler?.DisconnectHandler();

        border = null;
        roundRect = null;

        Content = new Label { Text = "Phase 3 done" };
        layout = null;

        await Task.Delay(50, cancellationToken);
    }
}
