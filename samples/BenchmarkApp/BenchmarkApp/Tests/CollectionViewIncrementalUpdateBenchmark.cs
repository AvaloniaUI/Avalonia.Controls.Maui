using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("CollectionViewIncrementalUpdate", Description = "Measures incremental CollectionView add, replace, remove, and reset operations")]
public class CollectionViewIncrementalUpdateBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int initialCount = 200;
        const int deltaCount = 100;

        var items = BenchmarkUiHelpers.CreateItems(initialCount);
        var collectionView = BenchmarkUiHelpers.CreateCollectionView(items, useEmptyTemplate: true);
        var metrics = new Dictionary<string, object>();
        var memoryBefore = MemorySnapshot.Capture(forceGC: true);

        try
        {
            Content = collectionView;
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 40);

            var addStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < deltaCount; i++)
            {
                items.Add(new CollectionViewBenchmarkItem(initialCount + i, $"Added {i}", $"Add detail {i % 10}"));
            }

            addStopwatch.Stop();
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 30);
            metrics["Add.ElapsedMs"] = addStopwatch.Elapsed.TotalMilliseconds;
            metrics["Add.ItemCount"] = items.Count;
            metrics["Add.PlatformItemCount"] = BenchmarkUiHelpers.GetPlatformItemCount(collectionView);

            var replaceStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < deltaCount; i++)
            {
                items[i] = new CollectionViewBenchmarkItem(i, $"Replaced {i}", $"Replace detail {i % 10}");
            }

            replaceStopwatch.Stop();
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 30);
            metrics["Replace.ElapsedMs"] = replaceStopwatch.Elapsed.TotalMilliseconds;
            metrics["Replace.ItemCount"] = items.Count;
            metrics["Replace.PlatformItemCount"] = BenchmarkUiHelpers.GetPlatformItemCount(collectionView);

            var removeStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < deltaCount; i++)
            {
                items.RemoveAt(items.Count - 1);
            }

            removeStopwatch.Stop();
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 30);
            metrics["Remove.ElapsedMs"] = removeStopwatch.Elapsed.TotalMilliseconds;
            metrics["Remove.ItemCount"] = items.Count;
            metrics["Remove.PlatformItemCount"] = BenchmarkUiHelpers.GetPlatformItemCount(collectionView);

            var resetStopwatch = Stopwatch.StartNew();
            items.Clear();
            foreach (var item in BenchmarkUiHelpers.CreateItems(initialCount))
            {
                items.Add(item);
            }

            resetStopwatch.Stop();
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 30);
            metrics["Reset.ElapsedMs"] = resetStopwatch.Elapsed.TotalMilliseconds;
            metrics["Reset.ItemCount"] = items.Count;
            metrics["Reset.PlatformItemCount"] = BenchmarkUiHelpers.GetPlatformItemCount(collectionView);

            logger.LogInformation(
                "CollectionView incremental updates: add={AddMs:F2}ms, replace={ReplaceMs:F2}ms, remove={RemoveMs:F2}ms, reset={ResetMs:F2}ms",
                addStopwatch.Elapsed.TotalMilliseconds,
                replaceStopwatch.Elapsed.TotalMilliseconds,
                removeStopwatch.Elapsed.TotalMilliseconds,
                resetStopwatch.Elapsed.TotalMilliseconds);

            foreach (var (key, value) in MemorySnapshot.Capture(forceGC: true).Compare(memoryBefore).ToMetrics())
            {
                metrics[key] = value;
            }

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            collectionView.ItemsSource = null;
            Content = new Label { Text = "CollectionView incremental update benchmark complete" };
            BenchmarkUiHelpers.DisconnectElementTree(collectionView);
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }
}
