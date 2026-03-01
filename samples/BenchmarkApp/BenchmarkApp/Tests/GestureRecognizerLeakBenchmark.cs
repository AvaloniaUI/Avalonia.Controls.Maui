

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that controls with gesture recognizers don't leak after removal and handler disconnect.
/// Gesture recognizers create bidirectional references; a missed cleanup leaks the entire control tree.
/// </summary>
[BenchmarkTest("GestureRecognizerLeak", Description = "Verifies controls with gesture recognizers are collected after disconnect")]
public class GestureRecognizerLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyGestureControls(trackedObjects, cancellationToken);

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

        // Count gesture-specific leaks
        int tapLeaked = leaked.Count(n => n.Contains("Tap"));
        int swipeLeaked = leaked.Count(n => n.Contains("Swipe"));
        int panLeaked = leaked.Count(n => n.Contains("Pan"));
        int dragLeaked = leaked.Count(n => n.Contains("Drag"));
        int dropLeaked = leaked.Count(n => n.Contains("Drop"));
        int pinchLeaked = leaked.Count(n => n.Contains("Pinch"));
        int pointerLeaked = leaked.Count(n => n.Contains("Pointer"));

        var metrics = new Dictionary<string, object>
        {
            ["ControlsTested"] = 7,
            ["GestureRecognizersTested"] = 7,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["Tap.Leaked"] = tapLeaked > 0,
            ["Swipe.Leaked"] = swipeLeaked > 0,
            ["Pan.Leaked"] = panLeaked > 0,
            ["Drag.Leaked"] = dragLeaked > 0,
            ["Drop.Leaked"] = dropLeaked > 0,
            ["Pinch.Leaked"] = pinchLeaked > 0,
            ["Pointer.Leaked"] = pointerLeaked > 0,
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Gesture recognizer leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} gesture recognizer objects collected successfully",
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
    private async Task CreateAndDestroyGestureControls(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        // Create controls with gesture recognizers
        CreateTapGestureControl(trackedObjects, layout);
        CreateSwipeGestureControl(trackedObjects, layout);
        CreatePanGestureControl(trackedObjects, layout);
        CreateDragGestureControl(trackedObjects, layout);
        CreateDropGestureControl(trackedObjects, layout);
        CreatePinchGestureControl(trackedObjects, layout);
        CreatePointerGestureControl(trackedObjects, layout);

        // Allow handlers to connect
        await Task.Delay(50, cancellationToken);

        // Tear down
        TearDownGestureControls(layout);

        Content = new Label { Text = "Gesture recognizer test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateTapGestureControl(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var label = new Label { Text = "Tap me" };
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (_, _) => { };
        label.GestureRecognizers.Add(tapGesture);

        trackedObjects["TapLabel"] = new WeakReference<object>(label);
        trackedObjects["TapGestureRecognizer"] = new WeakReference<object>(tapGesture);

        layout.Children.Add(label);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateSwipeGestureControl(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var label = new Label { Text = "Swipe me" };
        var swipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeGesture.Swiped += (_, _) => { };
        label.GestureRecognizers.Add(swipeGesture);

        trackedObjects["SwipeLabel"] = new WeakReference<object>(label);
        trackedObjects["SwipeGestureRecognizer"] = new WeakReference<object>(swipeGesture);

        layout.Children.Add(label);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreatePanGestureControl(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var label = new Label { Text = "Pan me", HeightRequest = 100 };
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += (_, _) => { };
        label.GestureRecognizers.Add(panGesture);

        trackedObjects["PanLabel"] = new WeakReference<object>(label);
        trackedObjects["PanGestureRecognizer"] = new WeakReference<object>(panGesture);

        layout.Children.Add(label);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateDragGestureControl(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var label = new Label { Text = "Drag me" };
        var dragGesture = new DragGestureRecognizer();
        dragGesture.DragStarting += (_, _) => { };
        dragGesture.DropCompleted += (_, _) => { };
        label.GestureRecognizers.Add(dragGesture);

        trackedObjects["DragLabel"] = new WeakReference<object>(label);
        trackedObjects["DragGestureRecognizer"] = new WeakReference<object>(dragGesture);

        layout.Children.Add(label);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateDropGestureControl(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var label = new Label { Text = "Drop here" };
        var dropGesture = new DropGestureRecognizer();
        dropGesture.DragOver += (_, _) => { };
        dropGesture.DragLeave += (_, _) => { };
        dropGesture.Drop += (_, _) => { };
        label.GestureRecognizers.Add(dropGesture);

        trackedObjects["DropLabel"] = new WeakReference<object>(label);
        trackedObjects["DropGestureRecognizer"] = new WeakReference<object>(dropGesture);

        layout.Children.Add(label);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreatePinchGestureControl(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var label = new Label { Text = "Pinch me", HeightRequest = 100 };
        var pinchGesture = new PinchGestureRecognizer();
        pinchGesture.PinchUpdated += (_, _) => { };
        label.GestureRecognizers.Add(pinchGesture);

        trackedObjects["PinchLabel"] = new WeakReference<object>(label);
        trackedObjects["PinchGestureRecognizer"] = new WeakReference<object>(pinchGesture);

        layout.Children.Add(label);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreatePointerGestureControl(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var label = new Label { Text = "Pointer me" };
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += (_, _) => { };
        pointerGesture.PointerExited += (_, _) => { };
        pointerGesture.PointerMoved += (_, _) => { };
        label.GestureRecognizers.Add(pointerGesture);

        trackedObjects["PointerLabel"] = new WeakReference<object>(label);
        trackedObjects["PointerGestureRecognizer"] = new WeakReference<object>(pointerGesture);

        layout.Children.Add(label);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDownGestureControls(VerticalStackLayout layout)
    {
        foreach (var child in layout.Children)
        {
            if (child is View view)
            {
                view.GestureRecognizers.Clear();
                view.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
    }
}
