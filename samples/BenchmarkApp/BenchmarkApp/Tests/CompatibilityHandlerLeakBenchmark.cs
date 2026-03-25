using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that legacy compatibility controls (Frame, ListView, TableView, and cell types)
/// and their handlers are collected after disconnect. These use the compatibility handler
/// layer from Avalonia.Controls.Maui.Compatibility.
/// </summary>
[BenchmarkTest("CompatibilityHandlerLeak", Description = "Verifies legacy Frame, ListView, TableView and cell handlers are collected after disconnect")]
public class CompatibilityHandlerLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyFrame(trackedObjects, cancellationToken);
        await CreateAndDestroyListView(trackedObjects, cancellationToken);
        await CreateAndDestroyTableView(trackedObjects, cancellationToken);

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
            ["ControlsTested"] = 3,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["Frame.Leaked"] = leaked.Any(n => n.StartsWith("Frame")),
            ["ListView.Leaked"] = leaked.Any(n => n.StartsWith("ListView")),
            ["TableView.Leaked"] = leaked.Any(n => n.StartsWith("TableView")),
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Compatibility handler leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        logger.LogInformation(
            "All {Count} compatibility handler objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyFrame(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
#pragma warning disable CS0618 // Frame is obsolete
        var frame = new Frame
        {
            BorderColor = Colors.Gray,
            CornerRadius = 10,
            HasShadow = true,
            Padding = new Thickness(10),
            Content = new Label { Text = "Frame content" },
        };
#pragma warning restore CS0618

        Content = frame;
        trackedObjects["Frame"] = new WeakReference<object>(frame);
        trackedObjects["Frame.Content"] = new WeakReference<object>(frame.Content);

        await Task.Delay(50, cancellationToken);

        if (frame.Handler is object frameHandler)
        {
            trackedObjects["Frame.Handler"] = new WeakReference<object>(frameHandler);
        }

        frame.Handler?.DisconnectHandler();
        Content = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyListView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var items = new ObservableCollection<string>();
        for (int i = 0; i < 10; i++)
        {
            items.Add($"List Item {i}");
        }

        var listView = new ListView
        {
            ItemsSource = items,
            ItemTemplate = new DataTemplate(() =>
            {
                var textCell = new TextCell();
                textCell.SetBinding(TextCell.TextProperty, ".");
                return textCell;
            }),
        };
        listView.ItemSelected += (_, _) => { };

        Content = listView;
        trackedObjects["ListView"] = new WeakReference<object>(listView);

        await Task.Delay(100, cancellationToken);

        // Exercise selection
        if (items.Count > 0)
        {
            listView.SelectedItem = items[2];
            listView.SelectedItem = null;
        }

        if (listView.Handler is object lvHandler)
        {
            trackedObjects["ListView.Handler"] = new WeakReference<object>(lvHandler);
        }

        items.Clear();
        listView.ItemsSource = null;
        listView.Handler?.DisconnectHandler();
        Content = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyTableView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        // Create cells of each type
        var textCell = new TextCell { Text = "Text Cell", Detail = "Detail text" };
        trackedObjects["TableView.TextCell"] = new WeakReference<object>(textCell);

        var entryCell = new EntryCell { Label = "Entry", Text = "Value" };
        entryCell.Completed += (_, _) => { };
        trackedObjects["TableView.EntryCell"] = new WeakReference<object>(entryCell);

        var switchCell = new SwitchCell { Text = "Toggle", On = true };
        switchCell.OnChanged += (_, _) => { };
        trackedObjects["TableView.SwitchCell"] = new WeakReference<object>(switchCell);

        var imageCell = new ImageCell { Text = "Image Cell", Detail = "With image" };
        trackedObjects["TableView.ImageCell"] = new WeakReference<object>(imageCell);

        var viewCell = new ViewCell
        {
            View = new Label { Text = "Custom view cell" },
        };
        trackedObjects["TableView.ViewCell"] = new WeakReference<object>(viewCell);

        var tableView = new TableView
        {
            Root = new TableRoot("Settings")
            {
                new TableSection("General")
                {
                    textCell,
                    entryCell,
                },
                new TableSection("Preferences")
                {
                    switchCell,
                    imageCell,
                    viewCell,
                },
            },
        };

        Content = tableView;
        trackedObjects["TableView"] = new WeakReference<object>(tableView);

        await Task.Delay(100, cancellationToken);

        // Exercise cell interactions
        entryCell.Text = "Modified";
        switchCell.On = false;

        if (tableView.Handler is object tvHandler)
        {
            trackedObjects["TableView.Handler"] = new WeakReference<object>(tvHandler);
        }

        tableView.Handler?.DisconnectHandler();
        tableView.Root.Clear();
        Content = new Label { Text = "Compatibility test complete" };
    }
}
