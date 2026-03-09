

using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Verifies CollectionView items, templated views, and header/footer views are collected
/// after ItemsSource changes and handler disconnect. CollectionViewHandler subscribes to
/// multiple events in ConnectHandler which can retain references.
/// </summary>
[BenchmarkTest("CollectionViewLeak", Description = "Verifies CollectionView header/footer and items are collected after disconnect")]
public class CollectionViewLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyCollectionView(trackedObjects, cancellationToken);

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
            logger.LogWarning("CollectionView leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} CollectionView objects collected successfully",
            trackedObjects.Count);

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyCollectionView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        CreateCollectionView(trackedObjects, layout);

        // Allow handlers to connect and items to render
        await Task.Delay(100, cancellationToken);

        TearDownCollectionView(trackedObjects, layout);

        Content = new Label { Text = "CollectionView test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateCollectionView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var collectionView = new CollectionView();
        trackedObjects["CollectionView"] = new WeakReference<object>(collectionView);

        // Set up header
        var header = new VerticalStackLayout
        {
            Children = { new Label { Text = "Header" } },
        };
        collectionView.Header = header;
        trackedObjects["Header"] = new WeakReference<object>(header);

        // Set up footer
        var footer = new VerticalStackLayout
        {
            Children = { new Label { Text = "Footer" } },
        };
        collectionView.Footer = footer;
        trackedObjects["Footer"] = new WeakReference<object>(footer);

        // Set up item template
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, ".");
            return label;
        });

        // Bind to ObservableCollection
        var items = new ObservableCollection<string>();
        for (int i = 0; i < 10; i++)
        {
            items.Add($"Item {i}");
        }

        collectionView.ItemsSource = items;

        layout.Children.Add(collectionView);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDownCollectionView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        if (layout.Children.Count == 0)
            return;

        var collectionView = layout.Children[0] as CollectionView;
        if (collectionView == null)
            return;

        // Track handler before disconnect
        if (collectionView.Handler is object handler)
        {
            trackedObjects["CollectionView.Handler"] = new WeakReference<object>(handler);
        }

        // Clear items and null out sources
        if (collectionView.ItemsSource is ObservableCollection<string> items)
        {
            items.Clear();
        }

        collectionView.ItemsSource = null;
        collectionView.Header = null;
        collectionView.Footer = null;

        // Disconnect handler
        collectionView.Handler?.DisconnectHandler();
        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
    }
}
