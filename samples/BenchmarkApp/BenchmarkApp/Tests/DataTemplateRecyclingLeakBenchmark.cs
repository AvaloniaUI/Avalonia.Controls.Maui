using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that CollectionView with complex DataTemplates properly releases templated views
/// when the ItemsSource is rapidly replaced. Exercises the template creation and teardown
/// paths to detect slow leaks from accumulated template instances.
/// </summary>
[BenchmarkTest("DataTemplateRecyclingLeak", Description = "Verifies CollectionView template recycling doesn't leak on rapid source changes")]
public class DataTemplateRecyclingLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int churnCycles = 8;

        await ChurnCollectionViewSource(trackedObjects, churnCycles, cancellationToken);

        // Force GC multiple times with delays
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(50, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);

        // Check for survivors
        var leaked = new List<string>();
        foreach (var (name, weakRef) in trackedObjects)
        {
            if (weakRef.TryGetTarget(out _))
            {
                leaked.Add(name);
            }
        }

        var metrics = new Dictionary<string, object>
        {
            ["ChurnCycles"] = churnCycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("DataTemplate recycling leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} DataTemplate objects collected after {Cycles} churn cycles",
            trackedObjects.Count,
            churnCycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task ChurnCollectionViewSource(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int churnCycles,
        CancellationToken cancellationToken)
    {
        var collectionView = new CollectionView
        {
            ItemTemplate = new DataTemplate(() =>
            {
                // Complex template with multiple controls
                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition(new GridLength(50)),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto),
                    },
                    Padding = new Thickness(8),
                };

                var avatar = new BoxView
                {
                    WidthRequest = 40,
                    HeightRequest = 40,
                    CornerRadius = 20,
                    Color = Colors.Blue,
                };
                grid.Add(avatar, 0, 0);

                var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
                nameLabel.SetBinding(Label.TextProperty, "Name");

                var detailLabel = new Label { FontSize = 12, TextColor = Colors.Gray };
                detailLabel.SetBinding(Label.TextProperty, "Detail");

                var stack = new VerticalStackLayout();
                stack.Children.Add(nameLabel);
                stack.Children.Add(detailLabel);
                grid.Add(stack, 1, 0);

                var actionButton = new Button { Text = ">" };
                grid.Add(actionButton, 2, 0);

                return grid;
            }),
        };

        Content = collectionView;
        await Task.Delay(50, cancellationToken);

        // Rapidly replace ItemsSource
        for (int cycle = 0; cycle < churnCycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var items = CreateItemsSource(cycle);
            trackedObjects[$"Cycle{cycle}.ItemsSource"] = new WeakReference<object>(items);

            collectionView.ItemsSource = items;
            await Task.Delay(50, cancellationToken);

            // Null out source to release
            collectionView.ItemsSource = null;
            await Task.Delay(20, cancellationToken);
        }

        // Track handler
        if (collectionView.Handler is object handler)
        {
            trackedObjects["CollectionView.Handler"] = new WeakReference<object>(handler);
        }

        trackedObjects["CollectionView"] = new WeakReference<object>(collectionView);

        // Disconnect
        collectionView.Handler?.DisconnectHandler();
        Content = new Label { Text = "DataTemplate recycling test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ObservableCollection<ItemData> CreateItemsSource(int cycle)
    {
        var items = new ObservableCollection<ItemData>();
        for (int i = 0; i < 20; i++)
        {
            items.Add(new ItemData
            {
                Name = $"Cycle{cycle}_Item{i}",
                Detail = $"Detail for item {i} in cycle {cycle}",
            });
        }

        return items;
    }

    private sealed class ItemData
    {
        public string Name { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }
}
