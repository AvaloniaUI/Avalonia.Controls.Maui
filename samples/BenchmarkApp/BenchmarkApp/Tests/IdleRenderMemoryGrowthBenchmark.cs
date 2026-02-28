using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests whether memory grows continuously while a rendered page sits idle.
/// The AlohaAI profiling showed ~400 MB growth in 60 seconds with zero user interaction,
/// pointing to render pipeline resources (Avalonia composition visuals, Skia surfaces,
/// byte[] render buffers) being created every frame and never released.
/// </summary>
[BenchmarkTest("IdleRenderMemoryGrowth", Description = "Detects continuous memory growth while a rendered page is idle (no interaction)")]
public class IdleRenderMemoryGrowthBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int sampleCount = 10;
        const int sampleIntervalMs = 1000;
        const long maxAcceptableGrowthBytes = 20 * 1024 * 1024; // 20 MB over the observation period
        const double maxGrowthRatePerSecond = 2 * 1024 * 1024; // 2 MB/s steady-state growth is suspicious

        // Build a representative page with varied controls (labels, images, borders, gradients, shadows)
        BuildContentPage();

        // Allow initial rendering and layout to stabilize
        await Task.Delay(2000, cancellationToken);

        // Force GC to establish a clean baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(200, cancellationToken);

        var baselineMemory = GC.GetTotalMemory(false);
        long baselineWorkingSet;
        using (var proc = Process.GetCurrentProcess())
        {
            baselineWorkingSet = proc.WorkingSet64;
        }

        var samples = new List<(double ElapsedSeconds, long MemoryBytes)>();
        var workingSetSamples = new List<(double ElapsedSeconds, long WorkingSetBytes)>();
        samples.Add((0, baselineMemory));
        workingSetSamples.Add((0, baselineWorkingSet));

        logger.LogInformation("Baseline memory: {Memory:N0} bytes, working set: {WorkingSet:N0} bytes", baselineMemory, baselineWorkingSet);
        logger.LogInformation("Observing idle memory for {Duration}s...", sampleCount * sampleIntervalMs / 1000);

        // Sample memory over time while the page sits idle
        for (int i = 0; i < sampleCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(sampleIntervalMs, cancellationToken);

            var currentMemory = GC.GetTotalMemory(false);
            long currentWorkingSet;
            using (var proc = Process.GetCurrentProcess())
            {
                currentWorkingSet = proc.WorkingSet64;
            }

            var elapsed = (i + 1) * sampleIntervalMs / 1000.0;
            samples.Add((elapsed, currentMemory));
            workingSetSamples.Add((elapsed, currentWorkingSet));

            var delta = currentMemory - baselineMemory;
            var wsDelta = currentWorkingSet - baselineWorkingSet;
            logger.LogInformation(
                "  {Elapsed:F0}s: {Memory:N0} bytes (delta: {Delta:+#,##0;-#,##0;0}), WS: {WorkingSet:N0} bytes (delta: {WsDelta:+#,##0;-#,##0;0})",
                elapsed, currentMemory, delta, currentWorkingSet, wsDelta);
        }

        // Force GC and take a post-GC measurement to distinguish retained vs transient
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var postGcMemory = GC.GetTotalMemory(false);
        var postGcGrowth = postGcMemory - baselineMemory;
        long postGcWorkingSet;
        using (var proc = Process.GetCurrentProcess())
        {
            postGcWorkingSet = proc.WorkingSet64;
        }

        var workingSetGrowth = postGcWorkingSet - baselineWorkingSet;

        logger.LogInformation("Post-GC memory: {Memory:N0} bytes (growth from baseline: {Growth:+#,##0;-#,##0;0})", postGcMemory, postGcGrowth);
        logger.LogInformation("Post-GC working set: {WorkingSet:N0} bytes (growth from baseline: {WsGrowth:+#,##0;-#,##0;0})", postGcWorkingSet, workingSetGrowth);

        // Linear regression on samples to compute growth rate
        var (slopePerSecond, rSquared) = ComputeLinearRegression(samples);

        // Clean up
        Content = new Label { Text = "Idle test complete" };

        var metrics = new Dictionary<string, object>
        {
            ["SampleCount"] = sampleCount,
            ["SampleIntervalMs"] = sampleIntervalMs,
            ["BaselineMemoryBytes"] = baselineMemory,
            ["FinalMemoryBytes"] = samples[^1].MemoryBytes,
            ["PostGcMemoryBytes"] = postGcMemory,
            ["RawGrowthBytes"] = samples[^1].MemoryBytes - baselineMemory,
            ["PostGcGrowthBytes"] = postGcGrowth,
            ["GrowthRatePerSecond"] = slopePerSecond,
            ["GrowthRateRSquared"] = rSquared,
            ["BaselineWorkingSetBytes"] = baselineWorkingSet,
            ["PostGcWorkingSetBytes"] = postGcWorkingSet,
            ["NativeWorkingSetDelta"] = workingSetGrowth,
        };

        var reasons = new List<string>();

        if (postGcGrowth > maxAcceptableGrowthBytes)
        {
            reasons.Add($"post-GC growth {postGcGrowth / (1024.0 * 1024):F1} MB exceeds {maxAcceptableGrowthBytes / (1024 * 1024)} MB threshold");
        }

        if (slopePerSecond > maxGrowthRatePerSecond && rSquared > 0.6)
        {
            reasons.Add($"steady growth rate {slopePerSecond / (1024.0 * 1024):F2} MB/s (R²={rSquared:F2}) exceeds {maxGrowthRatePerSecond / (1024 * 1024)} MB/s threshold");
        }

        if (reasons.Count > 0)
        {
            logger.LogWarning("Idle render leak detected: {Reasons}", string.Join("; ", reasons));
            return BenchmarkResult.Fail(string.Join("; ", reasons), metrics);
        }

        if (workingSetGrowth > 100 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {workingSetGrowth / (1024.0 * 1024):F1} MB exceeds 100 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "No idle render leak. Post-GC growth: {Growth:N0} bytes, rate: {Rate:F2} bytes/s (R²={R2:F2})",
            postGcGrowth, slopePerSecond, rSquared);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void BuildContentPage()
    {
        var scrollView = new ScrollView();
        var layout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        // Header with gradient background (produces composition layer + gradient brush resources)
        var headerGrid = new Grid { HeightRequest = 200 };
        headerGrid.Children.Add(new Image
        {
            Source = "dotnet_bot.png",
            Aspect = Aspect.AspectFill,
            InputTransparent = true,
        });
        headerGrid.Children.Add(new BoxView
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0.0f),
                    new GradientStop(Color.FromArgb("#80000000"), 0.5f),
                    new GradientStop(Color.FromArgb("#FF1A1A2E"), 1.0f),
                },
            },
        });
        layout.Children.Add(headerGrid);

        // Multiple cards with shadows and borders (produces render target surfaces)
        for (int i = 0; i < 6; i++)
        {
            var card = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Stroke = Colors.Transparent,
                Padding = new Thickness(16),
                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Offset = new Point(0, 4),
                    Radius = 12,
                    Opacity = 0.3f,
                },
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Label { Text = $"Card {i}", FontSize = 18, FontAttributes = FontAttributes.Bold },
                        new Label { Text = $"Description text for card {i} with enough content to be realistic", FontSize = 14 },
                        new Image
                        {
                            Source = "dotnet_bot.png",
                            HeightRequest = 60,
                            WidthRequest = 60,
                            Aspect = Aspect.AspectFit,
                        },
                        new ProgressBar { Progress = (i + 1) / 7.0 },
                    },
                },
            };
            layout.Children.Add(card);
        }

        // Labels to increase visual tree depth
        for (int i = 0; i < 10; i++)
        {
            layout.Children.Add(new Label { Text = $"Item {i}", FontSize = 14 });
        }

        scrollView.Content = layout;
        Content = scrollView;
    }

    private static (double Slope, double RSquared) ComputeLinearRegression(List<(double X, long Y)> samples)
    {
        int n = samples.Count;
        if (n < 3)
        {
            return (0, 0);
        }

        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;
        for (int i = 0; i < n; i++)
        {
            double x = samples[i].X;
            double y = samples[i].Y;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
            sumY2 += y * y;
        }

        var denominator = (n * sumX2) - (sumX * sumX);
        if (denominator == 0)
        {
            return (0, 0);
        }

        var slope = ((n * sumXY) - (sumX * sumY)) / denominator;
        var intercept = (sumY - (slope * sumX)) / n;

        var yMean = sumY / n;
        var ssTot = sumY2 - (n * yMean * yMean);

        double ssRes = 0;
        for (int i = 0; i < n; i++)
        {
            double predicted = intercept + (slope * samples[i].X);
            double residual = samples[i].Y - predicted;
            ssRes += residual * residual;
        }

        var rSquared = ssTot > 0 ? 1.0 - (ssRes / ssTot) : 0;
        return (slope, rSquared);
    }
}
