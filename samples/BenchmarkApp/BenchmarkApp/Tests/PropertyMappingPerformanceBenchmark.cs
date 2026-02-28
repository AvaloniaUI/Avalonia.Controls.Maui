using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Measures throughput of property updates across control types. Creates controls and applies
/// many property changes (Text, Color, Font, Size, etc.) in a tight loop to establish a
/// baseline for mapper performance regression detection.
/// </summary>
[BenchmarkTest("PropertyMappingPerformance", Description = "Measures property update throughput across control types")]
public class PropertyMappingPerformanceBenchmark : BenchmarkTestPage
{
    private const int PropertyUpdatesPerControl = 50;

    /// <inheritdoc/>
    public override Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var totalStopwatch = Stopwatch.StartNew();
        var metrics = new Dictionary<string, object>();
        int totalUpdates = 0;

        // Button property updates
        totalUpdates += MeasureButtonProperties(layout, metrics, logger, cancellationToken);

        // Label property updates
        totalUpdates += MeasureLabelProperties(layout, metrics, logger, cancellationToken);

        // Entry property updates
        totalUpdates += MeasureEntryProperties(layout, metrics, logger, cancellationToken);

        totalStopwatch.Stop();
        var memAfter = MemorySnapshot.Capture(forceGC: true);
        var memoryDelta = memAfter.Compare(memBefore);

        metrics["TotalPropertyUpdates"] = totalUpdates;
        metrics["TotalElapsedMs"] = totalStopwatch.Elapsed.TotalMilliseconds;
        metrics["AveragePerUpdateMs"] = totalUpdates > 0
            ? totalStopwatch.Elapsed.TotalMilliseconds / totalUpdates
            : 0;

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        logger.LogInformation(
            "Completed {TotalUpdates} property updates in {ElapsedMs:F2}ms ({AvgMs:F4}ms avg)",
            totalUpdates,
            totalStopwatch.Elapsed.TotalMilliseconds,
            totalStopwatch.Elapsed.TotalMilliseconds / totalUpdates);

        // Performance benchmark always passes — it's for measurement only
        return Task.FromResult(BenchmarkResult.Pass(metrics));
    }

    private static int MeasureButtonProperties(
        VerticalStackLayout layout,
        Dictionary<string, object> metrics,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var button = new Button { Text = "Perf test" };
        layout.Children.Add(button);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < PropertyUpdatesPerControl; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            button.Text = $"Button text {i}";
            button.TextColor = i % 2 == 0 ? Colors.Red : Colors.Blue;
            button.BackgroundColor = i % 2 == 0 ? Colors.LightGray : Colors.White;
            button.FontSize = 14 + (i % 10);
            button.FontAttributes = i % 3 == 0 ? FontAttributes.Bold : FontAttributes.None;
            button.Padding = new Thickness(i % 20);
            button.CornerRadius = i % 15;
            button.BorderWidth = i % 5;
            button.IsEnabled = i % 10 != 0;
        }

        sw.Stop();
        int updates = PropertyUpdatesPerControl * 9; // 9 properties per iteration
        metrics["Button.ElapsedMs"] = sw.Elapsed.TotalMilliseconds;
        metrics["Button.Updates"] = updates;
        metrics["Button.PerUpdateMs"] = sw.Elapsed.TotalMilliseconds / updates;

        logger.LogInformation(
            "Button: {Updates} property updates in {ElapsedMs:F2}ms ({PerUpdateMs:F4}ms each)",
            updates, sw.Elapsed.TotalMilliseconds, sw.Elapsed.TotalMilliseconds / updates);

        return updates;
    }

    private static int MeasureLabelProperties(
        VerticalStackLayout layout,
        Dictionary<string, object> metrics,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var label = new Label { Text = "Perf test" };
        layout.Children.Add(label);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < PropertyUpdatesPerControl; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            label.Text = $"Label text {i}";
            label.TextColor = i % 2 == 0 ? Colors.Black : Colors.DarkGray;
            label.BackgroundColor = i % 2 == 0 ? Colors.Transparent : Colors.LightYellow;
            label.FontSize = 12 + (i % 8);
            label.FontAttributes = i % 3 == 0 ? FontAttributes.Italic : FontAttributes.None;
            label.HorizontalTextAlignment = i % 2 == 0 ? TextAlignment.Start : TextAlignment.Center;
            label.LineBreakMode = i % 2 == 0 ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;
            label.MaxLines = (i % 5) + 1;
            label.TextDecorations = i % 4 == 0 ? TextDecorations.Underline : TextDecorations.None;
            label.Padding = new Thickness(i % 10);
        }

        sw.Stop();
        int updates = PropertyUpdatesPerControl * 10; // 10 properties per iteration
        metrics["Label.ElapsedMs"] = sw.Elapsed.TotalMilliseconds;
        metrics["Label.Updates"] = updates;
        metrics["Label.PerUpdateMs"] = sw.Elapsed.TotalMilliseconds / updates;

        logger.LogInformation(
            "Label: {Updates} property updates in {ElapsedMs:F2}ms ({PerUpdateMs:F4}ms each)",
            updates, sw.Elapsed.TotalMilliseconds, sw.Elapsed.TotalMilliseconds / updates);

        return updates;
    }

    private static int MeasureEntryProperties(
        VerticalStackLayout layout,
        Dictionary<string, object> metrics,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var entry = new Entry { Text = "Perf test" };
        layout.Children.Add(entry);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < PropertyUpdatesPerControl; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            entry.Text = $"Entry text {i}";
            entry.Placeholder = $"Placeholder {i}";
            entry.TextColor = i % 2 == 0 ? Colors.Black : Colors.DarkBlue;
            entry.PlaceholderColor = Colors.Gray;
            entry.BackgroundColor = i % 2 == 0 ? Colors.White : Colors.LightGray;
            entry.FontSize = 14 + (i % 6);
            entry.IsPassword = i % 10 == 0;
            entry.MaxLength = 100 + i;
            entry.HorizontalTextAlignment = i % 2 == 0 ? TextAlignment.Start : TextAlignment.End;
        }

        sw.Stop();
        int updates = PropertyUpdatesPerControl * 9; // 9 properties per iteration
        metrics["Entry.ElapsedMs"] = sw.Elapsed.TotalMilliseconds;
        metrics["Entry.Updates"] = updates;
        metrics["Entry.PerUpdateMs"] = sw.Elapsed.TotalMilliseconds / updates;

        logger.LogInformation(
            "Entry: {Updates} property updates in {ElapsedMs:F2}ms ({PerUpdateMs:F4}ms each)",
            updates, sw.Elapsed.TotalMilliseconds, sw.Elapsed.TotalMilliseconds / updates);

        return updates;
    }
}
