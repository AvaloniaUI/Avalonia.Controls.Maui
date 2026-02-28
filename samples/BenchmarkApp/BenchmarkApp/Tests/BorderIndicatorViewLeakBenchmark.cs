using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that Border (with complex stroke/shape properties) and IndicatorView (which
/// creates visual indicators dynamically) are collected after handler disconnect.
/// </summary>
[BenchmarkTest("BorderIndicatorViewLeak", Description = "Verifies Border and IndicatorView are collected after disconnect")]
public class BorderIndicatorViewLeakBenchmark : BenchmarkTestPage
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
            logger.LogWarning("Border/IndicatorView leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} Border/IndicatorView objects collected after {Iterations} iterations",
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
            var border = CreateBorder(trackedObjects, i);
            layout.Children.Add(border);

            var indicator = CreateIndicatorView(trackedObjects, i);
            layout.Children.Add(indicator);
        }

        // Track handlers
        int childIndex = 0;
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve && ve.Handler is object handler)
            {
                string typeName = ve is Border ? "Border" : "IndicatorView";
                trackedObjects[$"{typeName}{childIndex / 2}.Handler"] = new WeakReference<object>(handler);
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
        Content = new Label { Text = "Border/IndicatorView test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Border CreateBorder(Dictionary<string, WeakReference<object>> trackedObjects, int index)
    {
        var border = new Border
        {
            Stroke = Colors.Blue,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(10),
            Content = new Label { Text = $"Border content {index}" },
        };

        trackedObjects[$"Border{index}"] = new WeakReference<object>(border);
        return border;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IndicatorView CreateIndicatorView(Dictionary<string, WeakReference<object>> trackedObjects, int index)
    {
        var indicator = new IndicatorView
        {
            Count = 5,
            SelectedIndicatorColor = Colors.Blue,
            IndicatorColor = Colors.LightGray,
            Position = index % 5,
        };

        trackedObjects[$"IndicatorView{index}"] = new WeakReference<object>(indicator);
        return indicator;
    }
}
