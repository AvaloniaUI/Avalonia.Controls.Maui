using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("CollectionViewLoad", Description = "Measures initial CollectionView load for flat and grouped data sources")]
public class CollectionViewLoadBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var metrics = new Dictionary<string, object>();
        var memoryBefore = MemorySnapshot.Capture(forceGC: true);

        await MeasureScenarioAsync(
            "Flat100",
            BenchmarkUiHelpers.CreateCollectionView(BenchmarkUiHelpers.CreateItems(100), useEmptyTemplate: true),
            100,
            metrics,
            logger,
            cancellationToken);

        await MeasureScenarioAsync(
            "Flat1000",
            BenchmarkUiHelpers.CreateCollectionView(BenchmarkUiHelpers.CreateItems(1000), useEmptyTemplate: true),
            1000,
            metrics,
            logger,
            cancellationToken);

        await MeasureScenarioAsync(
            "Grouped500",
            BenchmarkUiHelpers.CreateCollectionView(BenchmarkUiHelpers.CreateGroups(20, 25), grouped: true, useEmptyTemplate: true),
            500,
            metrics,
            logger,
            cancellationToken);

        var memoryAfter = MemorySnapshot.Capture(forceGC: true);
        foreach (var (key, value) in memoryAfter.Compare(memoryBefore).ToMetrics())
        {
            metrics[key] = value;
        }

        return BenchmarkResult.Pass(metrics);
    }

    private async Task MeasureScenarioAsync(
        string prefix,
        CollectionView collectionView,
        int sourceCount,
        Dictionary<string, object> metrics,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        collectionView.Header = new Label { Text = $"{prefix} header", FontAttributes = FontAttributes.Bold };
        collectionView.Footer = new Label { Text = $"{prefix} footer", Opacity = 0.75 };

        var stopwatch = Stopwatch.StartNew();
        Content = collectionView;
        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 50);
        stopwatch.Stop();

        var platformItemCount = BenchmarkUiHelpers.GetPlatformItemCount(collectionView);
        metrics[$"{prefix}.ElapsedMs"] = stopwatch.Elapsed.TotalMilliseconds;
        metrics[$"{prefix}.SourceCount"] = sourceCount;
        metrics[$"{prefix}.PlatformItemCount"] = platformItemCount;

        logger.LogInformation(
            "{Scenario}: loaded {SourceCount} source items in {ElapsedMs:F2}ms; platform item count={PlatformItemCount}",
            prefix,
            sourceCount,
            stopwatch.Elapsed.TotalMilliseconds,
            platformItemCount);

        await TearDownCollectionViewAsync(collectionView, cancellationToken);
    }

    private async Task TearDownCollectionViewAsync(CollectionView collectionView, CancellationToken cancellationToken)
    {
        collectionView.ItemsSource = null;
        collectionView.Header = null;
        collectionView.Footer = null;
        Content = new Label { Text = "CollectionView load scenario complete" };
        BenchmarkUiHelpers.DisconnectElementTree(collectionView);
        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
    }
}
