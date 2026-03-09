using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Measures layout performance with nested layouts (Grid inside StackLayout inside ScrollView)
/// with many children. Times the measure/arrange passes after content changes.
/// </summary>
[BenchmarkTest("LayoutPerformance", Description = "Measures layout performance with nested layouts and many children")]
public class LayoutPerformanceBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var metrics = new Dictionary<string, object>();

        // Phase 1: Measure initial layout construction
        var constructionSw = Stopwatch.StartNew();
        var (scrollView, grid, childCount) = BuildNestedLayout();
        Content = scrollView;
        constructionSw.Stop();

        await Task.Delay(50, cancellationToken);

        metrics["ChildCount"] = childCount;
        metrics["Construction.ElapsedMs"] = constructionSw.Elapsed.TotalMilliseconds;

        logger.LogInformation(
            "Constructed nested layout with {ChildCount} children in {ElapsedMs:F2}ms",
            childCount, constructionSw.Elapsed.TotalMilliseconds);

        // Phase 2: Measure content update performance
        const int updateCycles = 10;
        var updateSw = Stopwatch.StartNew();

        for (int cycle = 0; cycle < updateCycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UpdateLayoutContent(grid, cycle);
            await Task.Delay(10, cancellationToken);
        }

        updateSw.Stop();

        metrics["UpdateCycles"] = updateCycles;
        metrics["Update.TotalElapsedMs"] = updateSw.Elapsed.TotalMilliseconds;
        metrics["Update.PerCycleMs"] = updateSw.Elapsed.TotalMilliseconds / updateCycles;

        logger.LogInformation(
            "Updated layout {Cycles} times in {ElapsedMs:F2}ms ({PerCycleMs:F2}ms per cycle)",
            updateCycles, updateSw.Elapsed.TotalMilliseconds,
            updateSw.Elapsed.TotalMilliseconds / updateCycles);

        // Phase 3: Measure adding/removing children
        const int addRemoveCycles = 20;
        var addRemoveSw = Stopwatch.StartNew();

        for (int i = 0; i < addRemoveCycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var label = new Label { Text = $"Dynamic child {i}" };
            grid.Children.Add(label);
            Grid.SetRow(label, grid.RowDefinitions.Count - 1);
        }

        // Remove them all
        for (int i = 0; i < addRemoveCycles; i++)
        {
            if (grid.Children.Count > 0)
            {
                grid.Children.RemoveAt(grid.Children.Count - 1);
            }
        }

        addRemoveSw.Stop();

        metrics["AddRemoveCycles"] = addRemoveCycles;
        metrics["AddRemove.ElapsedMs"] = addRemoveSw.Elapsed.TotalMilliseconds;

        logger.LogInformation(
            "Add/remove {Cycles} children in {ElapsedMs:F2}ms",
            addRemoveCycles, addRemoveSw.Elapsed.TotalMilliseconds);

        var memAfter = MemorySnapshot.Capture(forceGC: true);
        var memoryDelta = memAfter.Compare(memBefore);

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        // Performance benchmark always passes — it's for measurement only
        return BenchmarkResult.Pass(metrics);
    }

    private static (ScrollView ScrollView, Grid Grid, int ChildCount) BuildNestedLayout()
    {
        const int rows = 10;
        const int cols = 3;

        var grid = new Grid();

        for (int r = 0; r < rows; r++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        for (int c = 0; c < cols; c++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        int childCount = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var stack = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = $"R{r}C{c}" },
                        new Button { Text = $"Btn {r},{c}" },
                    },
                };

                Grid.SetRow(stack, r);
                Grid.SetColumn(stack, c);
                grid.Children.Add(stack);
                childCount += 3; // stack + label + button
            }
        }

        var outerStack = new VerticalStackLayout
        {
            Children =
            {
                new Label { Text = "Layout Performance Test", FontSize = 20 },
                grid,
            },
        };

        var scrollView = new ScrollView { Content = outerStack };
        childCount += 3; // scrollView + outerStack + header label

        return (scrollView, grid, childCount);
    }

    private static void UpdateLayoutContent(Grid grid, int cycle)
    {
        foreach (var child in grid.Children)
        {
            if (child is VerticalStackLayout stack)
            {
                foreach (var stackChild in stack.Children)
                {
                    if (stackChild is Label label)
                    {
                        label.Text = $"Updated {cycle}";
                    }
                    else if (stackChild is Button button)
                    {
                        button.Text = $"Cycle {cycle}";
                    }
                }
            }
        }
    }
}
