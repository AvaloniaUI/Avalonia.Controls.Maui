using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("ScrollViewScrollPerformance", Description = "Measures sustained ScrollView programmatic scrolling latency and event volume")]
public class ScrollViewScrollPerformanceBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var scrollView = new ScrollView
        {
            HeightRequest = 420,
            Content = CreateTallContent(),
        };

        var hostLayout = new Grid
        {
            Padding = new Thickness(16),
            Children =
            {
                scrollView,
            },
        };

        var scrollChangedCount = 0;
        scrollView.Scrolled += (_, _) => scrollChangedCount++;

        var metrics = new Dictionary<string, object>();
        var memoryBefore = MemorySnapshot.Capture(forceGC: true);

        try
        {
            Content = hostLayout;
            await BenchmarkUiHelpers.WaitUntilAsync(
                () => scrollView.ContentSize.Height > (scrollView.HeightRequest * 2),
                cancellationToken,
                timeoutMs: 5000,
                pollDelayMs: 20);
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 50);

            var viewportHeight = scrollView.Height > 0 ? scrollView.Height : scrollView.HeightRequest;
            var contentHeight = scrollView.ContentSize.Height;
            var maxOffset = Math.Max(0, contentHeight - viewportHeight);
            var targets = BuildScrollTargets(maxOffset);
            var latencies = new List<double>(targets.Count);

            scrollChangedCount = 0;

            foreach (var targetOffset in targets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stopwatch = Stopwatch.StartNew();
                await scrollView.ScrollToAsync(0, targetOffset, false);
                await BenchmarkUiHelpers.WaitUntilAsync(
                    () => Math.Abs(scrollView.ScrollY - targetOffset) <= 2,
                    cancellationToken,
                    timeoutMs: 3000,
                    pollDelayMs: 10);
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 10);
                stopwatch.Stop();

                latencies.Add(stopwatch.Elapsed.TotalMilliseconds);
            }

            var averageElapsedMs = latencies.Count > 0 ? latencies.Average() : 0;
            var p95ElapsedMs = BenchmarkUiHelpers.CalculatePercentile(latencies, 0.95);
            var maxElapsedMs = latencies.Count > 0 ? latencies.Max() : 0;

            metrics["ScrollTargets"] = targets.Count;
            metrics["AverageElapsedMs"] = averageElapsedMs;
            metrics["P95ElapsedMs"] = p95ElapsedMs;
            metrics["MaxElapsedMs"] = maxElapsedMs;
            metrics["ScrolledEventCount"] = scrollChangedCount;
            metrics["ViewportHeight"] = viewportHeight;
            metrics["ContentHeight"] = contentHeight;
            metrics["MaxScrollOffset"] = maxOffset;

            foreach (var (key, value) in MemorySnapshot.Capture(forceGC: true).Compare(memoryBefore).ToMetrics())
            {
                metrics[$"Scenario.{key}"] = value;
            }

            logger.LogInformation(
                "ScrollView sustained scroll: avg={AverageMs:F2}ms, p95={P95Ms:F2}ms, max={MaxMs:F2}ms, events={ScrollChangedCount}",
                averageElapsedMs,
                p95ElapsedMs,
                maxElapsedMs,
                scrollChangedCount);

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            Content = new Label { Text = "ScrollView performance benchmark complete" };
            BenchmarkUiHelpers.DisconnectElementTree(hostLayout);
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }

    private static IReadOnlyList<double> BuildScrollTargets(double maxOffset)
    {
        if (maxOffset <= 0)
        {
            return new[] { 0d };
        }

        var fractions = new[]
        {
            0.12, 0.28, 0.46, 0.64, 0.82, 0.97,
            0.58, 0.24, 0.91, 0.18, 0.74, 0.0,
        };

        return fractions
            .Select(fraction => Math.Round(maxOffset * fraction, 0))
            .Distinct()
            .Select(offset => (double)offset)
            .ToArray();
    }

    private static VerticalStackLayout CreateTallContent()
    {
        var root = new VerticalStackLayout
        {
            Spacing = 18,
            Padding = new Thickness(12),
        };

        for (int sectionIndex = 0; sectionIndex < 18; sectionIndex++)
        {
            root.Children.Add(new Label
            {
                Text = $"Section {sectionIndex + 1}",
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
            });

            for (int cardIndex = 0; cardIndex < 4; cardIndex++)
            {
                var progress = ((sectionIndex + 1) * (cardIndex + 2) % 100) / 100.0;
                var details = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto },
                    },
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto },
                    },
                    ColumnSpacing = 12,
                    RowSpacing = 6,
                };

                details.Add(new Label
                {
                    Text = $"Card {cardIndex + 1} summary for section {sectionIndex + 1}",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                });

                details.Add(new Label
                {
                    Text = $"{(int)(progress * 100)}%",
                    FontSize = 13,
                    HorizontalTextAlignment = TextAlignment.End,
                }, 1, 0);

                details.Add(new Label
                {
                    Text = "Nested content with text, actions, and progress to create a taller visual tree.",
                    FontSize = 13,
                    Opacity = 0.8,
                }, 0, 1);

                details.Add(new ProgressBar
                {
                    Progress = progress,
                    VerticalOptions = LayoutOptions.Center,
                }, 1, 1);

                root.Children.Add(new Border
                {
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                    StrokeThickness = 1,
                    Padding = new Thickness(14),
                    Content = new VerticalStackLayout
                    {
                        Spacing = 10,
                        Children =
                        {
                            details,
                            new HorizontalStackLayout
                            {
                                Spacing = 12,
                                Children =
                                {
                                    new Label { Text = $"Metric {(sectionIndex + cardIndex) % 7}", FontSize = 12 },
                                    new Label { Text = $"Value {(sectionIndex * 3) + cardIndex}", FontSize = 12 },
                                    new Label { Text = $"Trend {(cardIndex % 2 == 0 ? "Up" : "Stable")}", FontSize = 12 },
                                },
                            },
                            new Button
                            {
                                Text = $"Open section {sectionIndex + 1} card {cardIndex + 1}",
                            },
                        },
                    },
                });
            }
        }

        return root;
    }
}
