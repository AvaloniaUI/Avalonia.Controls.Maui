
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Detects unbounded growth in the finalizer queue by repeatedly creating and discarding controls.
/// </summary>
/// <remarks>
/// Creates 20 cycles of 10 Button + 10 Label controls, clears them, forces GC, and samples
/// <c>GC.GetGCMemoryInfo().FinalizationPendingCount</c> after each cycle.
/// Fails if more than 70% of the last 10 cycles show growth AND the delta exceeds 50.
/// </remarks>
[BenchmarkTest("FinalizerQueueGrowth", Description = "Detects unbounded growth in the finalizer queue across create/discard cycles")]
public class FinalizerQueueGrowthBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int totalCycles = 20;
        const int controlsPerType = 10;
        const double maxGrowthRatio = 0.7;
        const long maxDelta = 50;

        var layout = new VerticalStackLayout();
        Content = layout;

        // Establish baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);

        var samples = new List<long>();

        for (int cycle = 0; cycle < totalCycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create controls
            for (int j = 0; j < controlsPerType; j++)
            {
                layout.Children.Add(new Button { Text = $"Btn {cycle}-{j}" });
                layout.Children.Add(new Label { Text = $"Lbl {cycle}-{j}" });
            }

            // Clear and disconnect
            foreach (var child in layout.Children)
            {
                if (child is VisualElement ve)
                {
                    ve.Handler?.DisconnectHandler();
                }
            }

            layout.Children.Clear();

            // Force GC
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(50, cancellationToken);

            var gcInfo = GC.GetGCMemoryInfo();
            samples.Add(gcInfo.FinalizationPendingCount);

            logger.LogInformation("  Cycle {Cycle}: FinalizationPendingCount = {Count}", cycle, gcInfo.FinalizationPendingCount);
        }

        Content = new Label { Text = "Finalizer queue test complete" };

        // Analyze the last 10 cycles for growth
        var recentSamples = samples.Skip(totalCycles / 2).ToList();
        int growingCount = 0;
        for (int i = 1; i < recentSamples.Count; i++)
        {
            if (recentSamples[i] > recentSamples[i - 1])
            {
                growingCount++;
            }
        }

        double growthRatio = recentSamples.Count > 1 ? (double)growingCount / (recentSamples.Count - 1) : 0;
        long totalDelta = recentSamples.Count > 0 ? recentSamples[^1] - recentSamples[0] : 0;

        var metrics = new Dictionary<string, object>
        {
            ["TotalCycles"] = totalCycles,
            ["ControlsPerCycle"] = controlsPerType * 2,
            ["FinalFinalizationPendingCount"] = samples[^1],
            ["RecentGrowthRatio"] = growthRatio,
            ["RecentDelta"] = totalDelta,
        };

        if (growthRatio > maxGrowthRatio && totalDelta > maxDelta)
        {
            var reason = $"Finalizer queue growing: {growthRatio:P0} of recent cycles show growth, delta = {totalDelta}";
            logger.LogWarning("{Reason}", reason);
            return BenchmarkResult.Fail(reason, metrics);
        }

        logger.LogInformation(
            "Finalizer queue stable. Growth ratio: {Ratio:P0}, delta: {Delta}",
            growthRatio, totalDelta);
        return BenchmarkResult.Pass(metrics);
    }
}
