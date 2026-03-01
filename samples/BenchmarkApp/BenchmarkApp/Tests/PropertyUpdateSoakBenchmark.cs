
using BenchmarkApp.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Soak test that performs 5000 property updates on a Button to detect per-update
/// allocation leaks that only manifest at volume.
/// </summary>
/// <remarks>
/// Cycles through Text, TextColor, and FontSize properties. Samples memory every 500 updates
/// and uses linear regression to detect steady growth.
/// </remarks>
[BenchmarkTest("PropertyUpdateSoak", Description = "5000 property updates on a Button for per-update allocation leak detection")]
public class PropertyUpdateSoakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int totalUpdates = 5000;
        const int sampleInterval = 500;
        const double maxGrowthRatePerUpdate = 1024; // 1 KB/update
        const long maxTotalGrowthBytes = 50L * 1024 * 1024; // 50 MB

        var button = new Button { Text = "Property Soak" };
        var layout = new VerticalStackLayout();
        layout.Children.Add(button);
        Content = layout;

        await Task.Delay(200, cancellationToken);

        // Establish baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(200, cancellationToken);

        var samples = new List<(double X, long Y)>();
        samples.Add((0, GC.GetTotalMemory(false)));

        var colors = new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Orange, Colors.Purple };
        var fontSizes = new[] { 10.0, 12.0, 14.0, 16.0, 18.0, 20.0 };

        for (int i = 1; i <= totalUpdates; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Cycle through properties
            button.Text = $"Update {i}";
            button.TextColor = colors[i % colors.Length];
            button.FontSize = fontSizes[i % fontSizes.Length];

            if (i % sampleInterval == 0)
            {
                // Yield to UI thread periodically
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
                    () => { },
                    Avalonia.Threading.DispatcherPriority.Background);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                await Task.Delay(50, cancellationToken);

                var memory = GC.GetTotalMemory(false);
                samples.Add((i, memory));
                logger.LogInformation("  Update {Update}: {Memory:N0} bytes", i, memory);
            }
        }

        Content = new Label { Text = "Property update soak test complete" };

        var (slope, rSquared) = StatisticsHelper.ComputeLinearRegression(samples);
        var totalGrowth = samples[^1].Y - samples[0].Y;

        var metrics = new Dictionary<string, object>
        {
            ["TotalUpdates"] = totalUpdates,
            ["SampleCount"] = samples.Count,
            ["BaselineBytes"] = samples[0].Y,
            ["FinalBytes"] = samples[^1].Y,
            ["TotalGrowthBytes"] = totalGrowth,
            ["GrowthRatePerUpdate"] = slope,
            ["GrowthRateRSquared"] = rSquared,
        };

        var reasons = new List<string>();

        if (slope > maxGrowthRatePerUpdate && rSquared > 0.7)
        {
            reasons.Add($"growth rate {slope:F1} bytes/update (R²={rSquared:F2}) exceeds {maxGrowthRatePerUpdate} bytes/update threshold");
        }

        if (totalGrowth > maxTotalGrowthBytes)
        {
            reasons.Add($"total growth {totalGrowth / (1024.0 * 1024):F1} MB exceeds {maxTotalGrowthBytes / (1024 * 1024)} MB threshold");
        }

        if (reasons.Count > 0)
        {
            var reason = string.Join("; ", reasons);
            logger.LogWarning("Property update soak leak detected: {Reason}", reason);
            return BenchmarkResult.Fail(reason, metrics);
        }

        logger.LogInformation(
            "Property update soak test passed. Growth rate: {Rate:F1} bytes/update (R²={R2:F2}), total: {Total:N0} bytes",
            slope, rSquared, totalGrowth);
        return BenchmarkResult.Pass(metrics);
    }
}
