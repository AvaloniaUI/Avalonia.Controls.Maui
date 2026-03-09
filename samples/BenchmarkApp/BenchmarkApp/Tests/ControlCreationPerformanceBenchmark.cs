

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Measures creation time across multiple control types to establish baselines and detect regressions.
/// Creates 50 instances of each of 10 control types (500 total).
/// </summary>
[BenchmarkTest("ControlCreationPerformance", Description = "Measures creation time across 10 control types (500 controls total)")]
public class ControlCreationPerformanceBenchmark : BenchmarkTestPage
{
    private static readonly (string Name, Func<View> Factory)[] ControlFactories =
    [
        ("Button", () => new Button { Text = "Test" }),
        ("Label", () => new Label { Text = "Test" }),
        ("Entry", () => new Entry { Placeholder = "Test" }),
        ("Editor", () => new Editor { Text = "Test" }),
        ("Switch", () => new Microsoft.Maui.Controls.Switch { IsToggled = false }),
        ("Picker", () => new Picker { Title = "Test" }),
        ("CheckBox", () => new CheckBox { IsChecked = false }),
        ("Slider", () => new Slider { Minimum = 0, Maximum = 100, Value = 50 }),
        ("ProgressBar", () => new ProgressBar { Progress = 0.5 }),
        ("Image", () => new Image()),
    ];

    /// <inheritdoc/>
    public override Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int instancesPerType = 50;

        var layout = new VerticalStackLayout();
        Content = layout;

        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var totalStopwatch = Stopwatch.StartNew();
        var metrics = new Dictionary<string, object>();
        int totalControls = 0;

        foreach (var (name, factory) in ControlFactories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < instancesPerType; i++)
            {
                var control = factory();
                layout.Children.Add(control);
            }

            sw.Stop();
            totalControls += instancesPerType;

            var elapsedMs = sw.Elapsed.TotalMilliseconds;
            var perControlMs = elapsedMs / instancesPerType;

            metrics[$"{name}.ElapsedMs"] = elapsedMs;
            metrics[$"{name}.PerControlMs"] = perControlMs;

            logger.LogInformation(
                "{ControlType}: {Count} controls in {ElapsedMs:F2}ms ({PerControlMs:F3}ms each)",
                name, instancesPerType, elapsedMs, perControlMs);
        }

        totalStopwatch.Stop();
        var memAfter = MemorySnapshot.Capture(forceGC: true);
        var memoryDelta = memAfter.Compare(memBefore);

        metrics["TotalControls"] = totalControls;
        metrics["TotalElapsedMs"] = totalStopwatch.Elapsed.TotalMilliseconds;
        metrics["AveragePerControlMs"] = totalStopwatch.Elapsed.TotalMilliseconds / totalControls;

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        logger.LogInformation(
            "Created {TotalControls} controls across {TypeCount} types in {ElapsedMs:F2}ms ({AvgMs:F3}ms avg)",
            totalControls,
            ControlFactories.Length,
            totalStopwatch.Elapsed.TotalMilliseconds,
            totalStopwatch.Elapsed.TotalMilliseconds / totalControls);

        // Performance benchmark always passes — it's for measurement only
        return Task.FromResult(BenchmarkResult.Pass(metrics));
    }
}
