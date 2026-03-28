using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests GestureManager._singleTapCts leak when controls with both single-tap AND double-tap
/// recognizers are destroyed while the 300ms delay timer is pending. The Task.Delay(300).ContinueWith()
/// continuation captures the view and recognizer references.
/// </summary>
/// <remarks>
/// Gap: GestureRecognizerLeakBenchmark uses one recognizer type per control and never triggers
/// the double-tap delay codepath (requires both single AND double-tap on same control).
/// </remarks>
[BenchmarkTest("GestureDoubleTapCancellationLeak", Description = "Verifies controls with single+double tap recognizers don't leak on rapid destroy")]
public class GestureDoubleTapCancellationLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int controlCount = 10;
        const int cycles = 15;

        await CreateAndDestroyDoubleTapControls(trackedObjects, controlCount, cycles, cancellationToken);

        // Wait past the 300ms double-tap delay window
        await Task.Delay(400, cancellationToken);

        // Force GC multiple times with delays
        for (int gc = 0; gc < 3; gc++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(50, cancellationToken);
        }

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
            ["ControlCount"] = controlCount,
            ["Cycles"] = cycles,
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
            logger.LogWarning("Gesture double-tap cancellation leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} gesture objects collected after {Cycles} cycles with {Controls} controls",
            trackedObjects.Count, cycles, controlCount);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyDoubleTapControls(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int controlCount,
        int cycles,
        CancellationToken cancellationToken)
    {
        var outerLayout = new VerticalStackLayout();
        Content = outerLayout;

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create controls with both single-tap AND double-tap recognizers
            var controls = new List<Border>();
            for (int i = 0; i < controlCount; i++)
            {
                var border = CreateDoubleTapBorder(trackedObjects, cycle, i);
                controls.Add(border);
                outerLayout.Children.Add(border);
            }

            // Allow handlers to connect
            await Task.Delay(50, cancellationToken);

            // Remove and disconnect WITHOUT waiting 300ms (during the delay window)
            foreach (var border in controls)
            {
                outerLayout.Children.Remove(border);
                border.GestureRecognizers.Clear();
                border.Handler?.DisconnectHandler();
            }

            controls.Clear();
        }

        outerLayout.Children.Clear();
        outerLayout.Handler?.DisconnectHandler();
        Content = new Label { Text = "Gesture double-tap cancellation test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Border CreateDoubleTapBorder(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle,
        int index)
    {
        var border = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(12),
            Content = new Label { Text = $"Tap target {cycle}-{index}" },
        };

        // Single-tap recognizer (taps=1)
        var singleTap = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
        singleTap.Tapped += (_, _) => { };
        border.GestureRecognizers.Add(singleTap);

        // Double-tap recognizer (taps=2) — this triggers the 300ms delay codepath in GestureManager
        var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTap.Tapped += (_, _) => { };
        border.GestureRecognizers.Add(doubleTap);

        trackedObjects[$"Cycle{cycle}.Border{index}"] = new WeakReference<object>(border);
        trackedObjects[$"Cycle{cycle}.SingleTap{index}"] = new WeakReference<object>(singleTap);
        trackedObjects[$"Cycle{cycle}.DoubleTap{index}"] = new WeakReference<object>(doubleTap);

        return border;
    }
}
