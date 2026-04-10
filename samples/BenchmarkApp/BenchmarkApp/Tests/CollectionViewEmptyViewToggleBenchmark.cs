using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("CollectionViewEmptyViewToggle", Description = "Measures CollectionView cost when alternating between empty and populated sources")]
public class CollectionViewEmptyViewToggleBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int fullItemCount = 120;
        const int cycles = 20;

        var emptyItems = BenchmarkUiHelpers.CreateItems(0);
        var fullItems = BenchmarkUiHelpers.CreateItems(fullItemCount);
        var collectionView = BenchmarkUiHelpers.CreateCollectionView(emptyItems, useEmptyTemplate: true);
        var metrics = new Dictionary<string, object>();
        var memoryBefore = MemorySnapshot.Capture(forceGC: true);

        try
        {
            Content = collectionView;
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 40);

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < cycles; i++)
            {
                collectionView.ItemsSource = fullItems;
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 15);

                collectionView.ItemsSource = emptyItems;
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 15);
            }

            stopwatch.Stop();

            collectionView.ItemsSource = fullItems;
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
            var fullPlatformItemCount = BenchmarkUiHelpers.GetPlatformItemCount(collectionView);

            collectionView.ItemsSource = emptyItems;
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
            var emptyPlatformItemCount = BenchmarkUiHelpers.GetPlatformItemCount(collectionView);

            metrics["Cycles"] = cycles;
            metrics["Transitions"] = cycles * 2;
            metrics["ElapsedMs"] = stopwatch.Elapsed.TotalMilliseconds;
            metrics["PerTransitionMs"] = stopwatch.Elapsed.TotalMilliseconds / (cycles * 2);
            metrics["Full.PlatformItemCount"] = fullPlatformItemCount;
            metrics["Empty.PlatformItemCount"] = emptyPlatformItemCount;

            foreach (var (key, value) in MemorySnapshot.Capture(forceGC: true).Compare(memoryBefore).ToMetrics())
            {
                metrics[key] = value;
            }

            logger.LogInformation(
                "CollectionView empty/full toggle: {Transitions} transitions in {ElapsedMs:F2}ms ({PerTransitionMs:F2}ms per transition)",
                cycles * 2,
                stopwatch.Elapsed.TotalMilliseconds,
                stopwatch.Elapsed.TotalMilliseconds / (cycles * 2));

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            collectionView.ItemsSource = null;
            Content = new Label { Text = "CollectionView empty toggle benchmark complete" };
            BenchmarkUiHelpers.DisconnectElementTree(collectionView);
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }
}
