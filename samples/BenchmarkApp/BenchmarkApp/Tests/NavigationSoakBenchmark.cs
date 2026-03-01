
using System.Runtime.CompilerServices;
using BenchmarkApp.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Soak test that performs 200 push/pop navigation cycles on a NavigationPage
/// to detect memory leaks from navigation stack management.
/// </summary>
/// <remarks>
/// Samples memory every 20 cycles and uses linear regression to detect steady growth.
/// Fails if growth rate exceeds 100 KB/cycle with R² > 0.7, OR total growth exceeds 50 MB.
/// </remarks>
[BenchmarkTest("NavigationSoak", Description = "200 push/pop navigation cycles for leak detection")]
public class NavigationSoakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int totalCycles = 200;
        const int sampleInterval = 20;
        const double maxGrowthRatePerCycle = 100 * 1024; // 100 KB/cycle
        const long maxTotalGrowthBytes = 50L * 1024 * 1024; // 50 MB

        var rootPage = new ContentPage { Title = "Root", Content = new Label { Text = "Navigation Root" } };
        var navPage = new NavigationPage(rootPage);
        window.Page = navPage;

        await Task.Delay(500, cancellationToken);

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

            await PushPopCycle(navPage, cycle, cancellationToken);

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

        // Restore test page
        window.Page = this;
        Content = new Label { Text = "Navigation soak test complete" };

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
            logger.LogWarning("Navigation soak leak detected: {Reason}", reason);
            return BenchmarkResult.Fail(reason, metrics);
        }

        logger.LogInformation(
            "Navigation soak test passed. Growth rate: {Rate:F1} bytes/cycle (R²={R2:F2}), total: {Total:N0} bytes",
            slope, rSquared, totalGrowth);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task PushPopCycle(NavigationPage navPage, int cycle, CancellationToken cancellationToken)
    {
        var page = new ContentPage
        {
            Title = $"Page {cycle}",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = $"Navigation page {cycle}" },
                    new Button { Text = "Action" },
                    new Entry { Placeholder = "Input" },
                },
            },
        };

        await navPage.PushAsync(page);
        await Task.Delay(30, cancellationToken);
        await navPage.PopAsync();
        await Task.Delay(30, cancellationToken);
    }
}
