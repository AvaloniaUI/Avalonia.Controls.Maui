

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that input controls with multiple event subscriptions (Editor, SearchBar, Picker,
/// Switch, CheckBox, Slider) are collected after disconnect. These handlers have the highest
/// event subscription density and hold mutable state fields.
/// </summary>
[BenchmarkTest("InputControlLeak", Description = "Verifies input controls with event subscriptions are collected after disconnect")]
public class InputControlLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyInputControls(trackedObjects, cancellationToken);

        // Force GC multiple times with delays
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
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

        int controlsLeaked = leaked.Count(n => !n.Contains(".Handler"));
        int handlersLeaked = leaked.Count(n => n.Contains(".Handler"));

        var metrics = new Dictionary<string, object>
        {
            ["ControlsTested"] = 6,
            ["HandlersTracked"] = trackedObjects.Count(kv => kv.Key.Contains(".Handler")),
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["Editor.Leaked"] = leaked.Any(n => n.StartsWith("Editor")),
            ["SearchBar.Leaked"] = leaked.Any(n => n.StartsWith("SearchBar")),
            ["Picker.Leaked"] = leaked.Any(n => n.StartsWith("Picker")),
            ["Switch.Leaked"] = leaked.Any(n => n.StartsWith("Switch")),
            ["CheckBox.Leaked"] = leaked.Any(n => n.StartsWith("CheckBox")),
            ["Slider.Leaked"] = leaked.Any(n => n.StartsWith("Slider")),
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Input control leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} input control objects collected successfully",
            trackedObjects.Count);

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyInputControls(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        CreateInputControls(trackedObjects, layout);

        // Allow handlers to connect
        await Task.Delay(50, cancellationToken);

        // Track handlers before disconnect
        TrackHandlers(trackedObjects, layout);

        // Tear down
        TearDownInputControls(layout);

        Content = new Label { Text = "Input control test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateInputControls(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var editor = new Editor { Text = "Test text", HeightRequest = 100 };
        editor.TextChanged += (_, _) => { };
        editor.Completed += (_, _) => { };
        editor.Text = "Modified text";
        layout.Children.Add(editor);
        trackedObjects["Editor"] = new WeakReference<object>(editor);

        var searchBar = new SearchBar { Placeholder = "Search...", Text = "query" };
        searchBar.TextChanged += (_, _) => { };
        searchBar.SearchButtonPressed += (_, _) => { };
        searchBar.Text = "modified query";
        layout.Children.Add(searchBar);
        trackedObjects["SearchBar"] = new WeakReference<object>(searchBar);

        var picker = new Picker { Title = "Select item" };
        picker.Items.Add("Item 1");
        picker.Items.Add("Item 2");
        picker.Items.Add("Item 3");
        picker.SelectedIndexChanged += (_, _) => { };
        picker.SelectedIndex = 1;
        layout.Children.Add(picker);
        trackedObjects["Picker"] = new WeakReference<object>(picker);

        var switchCtrl = new Switch { IsToggled = false };
        switchCtrl.Toggled += (_, _) => { };
        switchCtrl.IsToggled = true;
        layout.Children.Add(switchCtrl);
        trackedObjects["Switch"] = new WeakReference<object>(switchCtrl);

        var checkBox = new CheckBox { IsChecked = false };
        checkBox.CheckedChanged += (_, _) => { };
        checkBox.IsChecked = true;
        layout.Children.Add(checkBox);
        trackedObjects["CheckBox"] = new WeakReference<object>(checkBox);

        var slider = new Slider { Minimum = 0, Maximum = 100, Value = 50 };
        slider.ValueChanged += (_, _) => { };
        slider.Value = 75;
        layout.Children.Add(slider);
        trackedObjects["Slider"] = new WeakReference<object>(slider);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackHandlers(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve && ve.Handler is object handler)
            {
                var typeName = ve.GetType().Name;
                trackedObjects[$"{typeName}.Handler"] = new WeakReference<object>(handler);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDownInputControls(VerticalStackLayout layout)
    {
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve)
            {
                ve.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
    }
}
