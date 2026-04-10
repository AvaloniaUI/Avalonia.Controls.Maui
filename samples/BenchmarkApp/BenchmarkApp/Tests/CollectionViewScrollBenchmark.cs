using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("CollectionViewScroll", Description = "Measures CollectionView programmatic scroll latency and scroll event volume")]
public class CollectionViewScrollBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var items = BenchmarkUiHelpers.CreateItems(500);
        var collectionView = BenchmarkUiHelpers.CreateCollectionView(items, useEmptyTemplate: true);
        collectionView.HeightRequest = 360;

        var hostLayout = new VerticalStackLayout
        {
            Padding = new Thickness(16),
            Spacing = 8,
            Children =
            {
                new Label { Text = "CollectionView scroll benchmark", FontSize = 20 },
                collectionView
            }
        };

        var scrollEventCount = 0;
        var thresholdReachedCount = 0;
        collectionView.Scrolled += (_, _) => scrollEventCount++;
        collectionView.RemainingItemsThresholdReached += (_, _) => thresholdReachedCount++;

        try
        {
            Content = hostLayout;
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 50);

            scrollEventCount = 0;
            thresholdReachedCount = 0;

            var scrollTargets = new[] { 0, 25, 75, 150, 250, 350, 450, 498, 0 };
            double totalElapsedMs = 0;
            double maxElapsedMs = 0;

            for (int i = 0; i < scrollTargets.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var targetIndex = scrollTargets[i];
                var stopwatch = Stopwatch.StartNew();
                collectionView.ScrollTo(targetIndex, -1, ScrollToPosition.MakeVisible, false);
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
                stopwatch.Stop();

                totalElapsedMs += stopwatch.Elapsed.TotalMilliseconds;
                maxElapsedMs = Math.Max(maxElapsedMs, stopwatch.Elapsed.TotalMilliseconds);
            }

            var metrics = new Dictionary<string, object>
            {
                ["ScrollTargets"] = scrollTargets.Length,
                ["AverageElapsedMs"] = totalElapsedMs / scrollTargets.Length,
                ["MaxElapsedMs"] = maxElapsedMs,
                ["ScrolledEventCount"] = scrollEventCount,
                ["ThresholdReachedCount"] = thresholdReachedCount,
                ["PlatformItemCount"] = BenchmarkUiHelpers.GetPlatformItemCount(collectionView)
            };

            logger.LogInformation(
                "CollectionView scroll: avg={AverageMs:F2}ms, max={MaxMs:F2}ms, scrolled events={ScrolledEvents}, threshold events={ThresholdEvents}",
                totalElapsedMs / scrollTargets.Length,
                maxElapsedMs,
                scrollEventCount,
                thresholdReachedCount);

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            collectionView.ItemsSource = null;
            Content = new Label { Text = "CollectionView scroll benchmark complete" };
            BenchmarkUiHelpers.DisconnectElementTree(hostLayout);
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }
}
