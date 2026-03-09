
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Measures the cost of rapid layout-triggering property changes to establish a performance baseline.
/// </summary>
/// <remarks>
/// Rapidly changes Width, Height, Padding, and Margin properties that trigger layout recalculation.
/// Compares individual property changes vs batch changes. Always passes (measurement baseline).
/// </remarks>
[BenchmarkTest("LayoutThrashing", Description = "Measures layout performance under rapid property change load")]
public class LayoutThrashingBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int iterations = 1000;

        var layout = new VerticalStackLayout();
        var target = new Grid { WidthRequest = 100, HeightRequest = 100 };
        target.Children.Add(new BoxView { Color = Colors.Blue });
        layout.Children.Add(target);
        Content = layout;

        await Task.Delay(200, cancellationToken);

        // Measure individual property changes
        var individualStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            target.WidthRequest = 50 + (i % 100);
            target.HeightRequest = 50 + (i % 100);
            target.Margin = new Thickness(i % 20);
            target.Padding = new Thickness(i % 10);
        }

        individualStopwatch.Stop();
        var individualMs = individualStopwatch.Elapsed.TotalMilliseconds;

        // Yield for layout to settle
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => { },
            Avalonia.Threading.DispatcherPriority.Background);

        // Measure batch property changes (using BatchBegin/BatchCommit)
        var batchStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            target.BatchBegin();
            target.WidthRequest = 50 + (i % 100);
            target.HeightRequest = 50 + (i % 100);
            target.Margin = new Thickness(i % 20);
            target.Padding = new Thickness(i % 10);
            target.BatchCommit();
        }

        batchStopwatch.Stop();
        var batchMs = batchStopwatch.Elapsed.TotalMilliseconds;

        // Yield for layout to settle
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => { },
            Avalonia.Threading.DispatcherPriority.Background);

        Content = new Label { Text = "Layout thrashing test complete" };

        var metrics = new Dictionary<string, object>
        {
            ["Iterations"] = iterations,
            ["IndividualElapsedMs"] = individualMs,
            ["IndividualPerIterationMs"] = individualMs / iterations,
            ["BatchElapsedMs"] = batchMs,
            ["BatchPerIterationMs"] = batchMs / iterations,
            ["BatchSpeedup"] = individualMs > 0 ? individualMs / batchMs : 0,
        };

        logger.LogInformation(
            "Layout thrashing: Individual={IndividualMs:F1}ms ({PerInd:F3}ms/iter), Batch={BatchMs:F1}ms ({PerBatch:F3}ms/iter), Speedup={Speedup:F2}x",
            individualMs, individualMs / iterations, batchMs, batchMs / iterations,
            individualMs > 0 ? individualMs / batchMs : 0);

        // Always passes - this is a measurement baseline
        return BenchmarkResult.Pass(metrics);
    }
}
