
using System.Runtime.CompilerServices;
using BenchmarkApp.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// High-iteration soak test that creates and destroys controls 500 times to detect
/// slow memory leaks that only manifest at volume.
/// </summary>
/// <remarks>
/// Each cycle creates a Button, Label, and Entry, adds them to a layout, clears,
/// disconnects handlers, and forces GC. Memory is sampled every 50 cycles (10 samples)
/// and analyzed with linear regression.
/// Fails if growth rate exceeds 100 KB/cycle with R² > 0.7, OR total growth exceeds 50 MB.
/// </remarks>
[BenchmarkTest("HandlerCreateDestroySoak", Description = "500-cycle handler create/destroy soak test for slow leak detection")]
public class HandlerCreateDestroySoakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int totalCycles = 500;
        const int sampleInterval = 50;
        const double maxGrowthRatePerCycle = 100 * 1024; // 100 KB/cycle
        const long maxTotalGrowthBytes = 50L * 1024 * 1024; // 50 MB

        var layout = new VerticalStackLayout();
        Content = layout;

        // Establish baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(200, cancellationToken);

        var samples = new List<(double X, long Y)>();
        samples.Add((0, GC.GetTotalMemory(false)));

        for (int cycle = 1; cycle <= totalCycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CreateAndDestroyCycle(layout);

            if (cycle % sampleInterval == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                await Task.Delay(50, cancellationToken);

                var memory = GC.GetTotalMemory(false);
                samples.Add((cycle, memory));
                logger.LogInformation("  Cycle {Cycle}: {Memory:N0} bytes", cycle, memory);
            }
        }

        Content = new Label { Text = "Soak test complete" };

        var (slope, rSquared) = StatisticsHelper.ComputeLinearRegression(samples);
        var totalGrowth = samples[^1].Y - samples[0].Y;

        var metrics = new Dictionary<string, object>
        {
            ["TotalCycles"] = totalCycles,
            ["SampleCount"] = samples.Count,
            ["BaselineBytes"] = samples[0].Y,
            ["FinalBytes"] = samples[^1].Y,
            ["TotalGrowthBytes"] = totalGrowth,
            ["GrowthRatePerCycle"] = slope,
            ["GrowthRateRSquared"] = rSquared,
        };

        var reasons = new List<string>();

        if (slope > maxGrowthRatePerCycle && rSquared > 0.7)
        {
            reasons.Add($"growth rate {slope / 1024:F1} KB/cycle (R²={rSquared:F2}) exceeds {maxGrowthRatePerCycle / 1024} KB/cycle threshold");
        }

        if (totalGrowth > maxTotalGrowthBytes)
        {
            reasons.Add($"total growth {totalGrowth / (1024.0 * 1024):F1} MB exceeds {maxTotalGrowthBytes / (1024 * 1024)} MB threshold");
        }

        if (reasons.Count > 0)
        {
            var reason = string.Join("; ", reasons);
            logger.LogWarning("Handler soak leak detected: {Reason}", reason);
            return BenchmarkResult.Fail(reason, metrics);
        }

        logger.LogInformation(
            "Handler soak test passed. Growth rate: {Rate:F1} bytes/cycle (R²={R2:F2}), total: {Total:N0} bytes",
            slope, rSquared, totalGrowth);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateAndDestroyCycle(VerticalStackLayout layout)
    {
        var button = new Button { Text = "Soak" };
        var label = new Label { Text = "Soak" };
        var entry = new Entry { Placeholder = "Soak" };

        layout.Children.Add(button);
        layout.Children.Add(label);
        layout.Children.Add(entry);

        layout.Children.Clear();

        button.Handler?.DisconnectHandler();
        label.Handler?.DisconnectHandler();
        entry.Handler?.DisconnectHandler();
    }
}
