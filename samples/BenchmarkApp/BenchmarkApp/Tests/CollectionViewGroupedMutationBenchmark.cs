using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("CollectionViewGroupedMutation", Description = "Measures grouped CollectionView mutation latency, item-source churn, and container churn")]
public class CollectionViewGroupedMutationBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int groupCount = 8;
        const int itemsPerGroup = 25;

        var groups = BenchmarkUiHelpers.CreateGroups(groupCount, itemsPerGroup);
        var collectionView = BenchmarkUiHelpers.CreateCollectionView(groups, grouped: true, useEmptyTemplate: true);
        var metrics = new Dictionary<string, object>();

        try
        {
            Content = collectionView;
            await BenchmarkUiHelpers.WaitForRealizedPlatformItemsAsync(
                collectionView,
                () => GetExpectedPlatformItemCount(groups),
                cancellationToken,
                timeoutMs: 8000,
                pollDelayMs: 25);

            await MeasureMutationAsync(
                "WithinGroupAdd",
                collectionView,
                () => groups[0].Add(new CollectionViewBenchmarkItem(9001, "Added inside group", "Within group add")),
                () => GetExpectedPlatformItemCount(groups),
                metrics,
                logger,
                cancellationToken);

            await MeasureMutationAsync(
                "WithinGroupReplace",
                collectionView,
                () => groups[0][5] = new CollectionViewBenchmarkItem(9002, "Replaced inside group", "Within group replace"),
                () => GetExpectedPlatformItemCount(groups),
                metrics,
                logger,
                cancellationToken);

            await MeasureMutationAsync(
                "WithinGroupRemove",
                collectionView,
                () => groups[0].RemoveAt(0),
                () => GetExpectedPlatformItemCount(groups),
                metrics,
                logger,
                cancellationToken);

            await MeasureMutationAsync(
                "MoveAcrossGroups",
                collectionView,
                () =>
                {
                    var moved = groups[1][0];
                    groups[1].RemoveAt(0);
                    groups[2].Add(new CollectionViewBenchmarkItem(moved.Id, $"{moved.Title} moved", moved.Detail));
                },
                () => GetExpectedPlatformItemCount(groups),
                metrics,
                logger,
                cancellationToken);

            await MeasureMutationAsync(
                "GroupAdd",
                collectionView,
                () => groups.Insert(2, new CollectionViewBenchmarkGroup("Inserted Group", BenchmarkUiHelpers.CreateItems(12, 12000))),
                () => GetExpectedPlatformItemCount(groups),
                metrics,
                logger,
                cancellationToken);

            await MeasureMutationAsync(
                "GroupRemove",
                collectionView,
                () => groups.RemoveAt(groups.Count - 1),
                () => GetExpectedPlatformItemCount(groups),
                metrics,
                logger,
                cancellationToken);

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            collectionView.ItemsSource = null;
            Content = new Label { Text = "Grouped CollectionView mutation benchmark complete" };
            BenchmarkUiHelpers.DisconnectElementTree(collectionView);
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }

    private static async Task MeasureMutationAsync(
        string prefix,
        CollectionView collectionView,
        Action mutation,
        Func<int> expectedPlatformItemCount,
        Dictionary<string, object> metrics,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var beforeItemsSource = BenchmarkUiHelpers.GetPlatformItemsSource(collectionView);
        var beforeContainers = BenchmarkUiHelpers.GetPlatformContainers(collectionView);
        var beforePlatformItemCount = BenchmarkUiHelpers.GetPlatformItemCount(collectionView);

        var stopwatch = Stopwatch.StartNew();
        mutation();
        await BenchmarkUiHelpers.WaitForRealizedPlatformItemsAsync(
            collectionView,
            expectedPlatformItemCount,
            cancellationToken,
            timeoutMs: 8000,
            pollDelayMs: 20);
        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        stopwatch.Stop();

        var afterItemsSource = BenchmarkUiHelpers.GetPlatformItemsSource(collectionView);
        var afterContainers = BenchmarkUiHelpers.GetPlatformContainers(collectionView);
        var afterPlatformItemCount = BenchmarkUiHelpers.GetPlatformItemCount(collectionView);
        var retainedCount = BenchmarkUiHelpers.CountRetainedReferences(beforeContainers, afterContainers);
        var removedCount = Math.Max(0, beforeContainers.Count - retainedCount);
        var addedCount = Math.Max(0, afterContainers.Count - retainedCount);
        var churnCount = removedCount + addedCount;

        metrics[$"{prefix}.ElapsedMs"] = stopwatch.Elapsed.TotalMilliseconds;
        metrics[$"{prefix}.ItemsSourceReassigned"] = !ReferenceEquals(beforeItemsSource, afterItemsSource);
        metrics[$"{prefix}.PlatformItemCountBefore"] = beforePlatformItemCount;
        metrics[$"{prefix}.PlatformItemCountAfter"] = afterPlatformItemCount;
        metrics[$"{prefix}.ContainerRetainedCount"] = retainedCount;
        metrics[$"{prefix}.ContainerRemovedCount"] = removedCount;
        metrics[$"{prefix}.ContainerAddedCount"] = addedCount;
        metrics[$"{prefix}.ContainerChurnCount"] = churnCount;
        metrics[$"{prefix}.ContainerRetentionRatio"] =
            beforeContainers.Count > 0 ? retainedCount / (double)beforeContainers.Count : 0;

        logger.LogInformation(
            "{Scenario}: elapsed={ElapsedMs:F2}ms, itemsSourceReassigned={ItemsSourceReassigned}, retained={RetainedCount}, removed={RemovedCount}, added={AddedCount}",
            prefix,
            stopwatch.Elapsed.TotalMilliseconds,
            !ReferenceEquals(beforeItemsSource, afterItemsSource),
            retainedCount,
            removedCount,
            addedCount);
    }

    private static int GetExpectedPlatformItemCount(IEnumerable<CollectionViewBenchmarkGroup> groups)
    {
        return groups.Sum(group => group.Count) + groups.Count();
    }
}
