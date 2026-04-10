using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("CollectionViewLongScroll", Description = "Measures long-list CollectionView scrolling latency across 100, 1000, and 5000 items")]
public class CollectionViewLongScrollBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var metrics = new Dictionary<string, object>();

        foreach (var strategy in new[] { ItemSizingStrategy.MeasureAllItems, ItemSizingStrategy.MeasureFirstItem })
        {
            await MeasureScenarioAsync(100, strategy, metrics, logger, cancellationToken);
            await MeasureScenarioAsync(1000, strategy, metrics, logger, cancellationToken);
            await MeasureScenarioAsync(5000, strategy, metrics, logger, cancellationToken);
        }

        return BenchmarkResult.Pass(metrics);
    }

    private async Task MeasureScenarioAsync(
        int itemCount,
        ItemSizingStrategy itemSizingStrategy,
        Dictionary<string, object> metrics,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var strategyName = itemSizingStrategy == ItemSizingStrategy.MeasureFirstItem ? "MeasureFirst" : "MeasureAll";
        var prefix = $"Items{itemCount}.{strategyName}";
        var items = BenchmarkUiHelpers.CreateItems(itemCount);
        var collectionView = BenchmarkUiHelpers.CreateCollectionView(
            items,
            useEmptyTemplate: true,
            itemSizingStrategy: itemSizingStrategy);
        collectionView.HeightRequest = 360;

        var hostLayout = new VerticalStackLayout
        {
            Padding = new Thickness(16),
            Spacing = 8,
            Children =
            {
                new Label { Text = $"{prefix} CollectionView scroll benchmark", FontSize = 20 },
                collectionView,
            },
        };

        var scrolledEventCount = 0;
        var thresholdReachedCount = 0;
        collectionView.Scrolled += (_, _) => scrolledEventCount++;
        collectionView.RemainingItemsThresholdReached += (_, _) => thresholdReachedCount++;

        try
        {
            Content = hostLayout;
            await BenchmarkUiHelpers.WaitUntilAsync(
                () => BenchmarkUiHelpers.GetPlatformItemCount(collectionView) == itemCount,
                cancellationToken,
                timeoutMs: 15000,
                pollDelayMs: itemCount >= 1000 ? 40 : 20);

            scrolledEventCount = 0;
            thresholdReachedCount = 0;

            var targets = BuildScrollTargets(itemCount);
            var latencies = new List<double>(targets.Count);

            foreach (var targetIndex in targets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var eventCountBefore = scrolledEventCount;
                var stopwatch = Stopwatch.StartNew();
                collectionView.ScrollTo(targetIndex, -1, ScrollToPosition.MakeVisible, false);
                await BenchmarkUiHelpers.WaitUntilAsync(
                    () => scrolledEventCount > eventCountBefore,
                    cancellationToken,
                    timeoutMs: 5000,
                    pollDelayMs: itemCount >= 1000 ? 30 : 15);
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, itemCount >= 1000 ? 30 : 15);
                stopwatch.Stop();

                latencies.Add(stopwatch.Elapsed.TotalMilliseconds);
            }

            var averageElapsedMs = latencies.Count > 0 ? latencies.Average() : 0;
            var p95ElapsedMs = BenchmarkUiHelpers.CalculatePercentile(latencies, 0.95);
            var maxElapsedMs = latencies.Count > 0 ? latencies.Max() : 0;
            var platformItemCount = BenchmarkUiHelpers.GetPlatformItemCount(collectionView);

            metrics[$"{prefix}.ScrollTargets"] = targets.Count;
            metrics[$"{prefix}.AverageElapsedMs"] = averageElapsedMs;
            metrics[$"{prefix}.P95ElapsedMs"] = p95ElapsedMs;
            metrics[$"{prefix}.MaxElapsedMs"] = maxElapsedMs;
            metrics[$"{prefix}.ScrolledEventCount"] = scrolledEventCount;
            metrics[$"{prefix}.ThresholdReachedCount"] = thresholdReachedCount;
            metrics[$"{prefix}.PlatformItemCount"] = platformItemCount;
            metrics[$"{prefix}.ItemSizingStrategy"] = itemSizingStrategy.ToString();

            logger.LogInformation(
                "{Scenario}: avg={AverageMs:F2}ms, p95={P95Ms:F2}ms, max={MaxMs:F2}ms, events={ScrolledEvents}, threshold={ThresholdEvents}, platform items={PlatformItems}",
                prefix,
                averageElapsedMs,
                p95ElapsedMs,
                maxElapsedMs,
                scrolledEventCount,
                thresholdReachedCount,
                platformItemCount);
        }
        finally
        {
            collectionView.ItemsSource = null;
            Content = new Label { Text = $"{prefix} CollectionView scroll benchmark complete" };
            BenchmarkUiHelpers.DisconnectElementTree(hostLayout);
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }

    private static IReadOnlyList<int> BuildScrollTargets(int itemCount)
    {
        var fractions = new[] { 0.1, 0.32, 0.55, 0.78, 0.98, 0.52, 0.18, 0.9, 0.0 };

        return fractions
            .Select(fraction => (int)Math.Round((itemCount - 1) * fraction, MidpointRounding.AwayFromZero))
            .Distinct()
            .ToArray();
    }
}
