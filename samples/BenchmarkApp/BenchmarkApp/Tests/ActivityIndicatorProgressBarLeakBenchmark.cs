using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that ActivityIndicator and ProgressBar controls and their handlers are collected
/// after disconnect. These are simple visual controls but verify animation/rendering state cleanup.
/// </summary>
[BenchmarkTest("ActivityIndicatorProgressBarLeak", Description = "Verifies ActivityIndicator and ProgressBar are collected after disconnect")]
public class ActivityIndicatorProgressBarLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyControls(trackedObjects, cancellationToken);

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

        var metrics = new Dictionary<string, object>
        {
            ["ControlsTested"] = 2,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["ActivityIndicator.Leaked"] = leaked.Any(n => n.StartsWith("ActivityIndicator")),
            ["ProgressBar.Leaked"] = leaked.Any(n => n.StartsWith("ProgressBar")),
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("ActivityIndicator/ProgressBar leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} ActivityIndicator/ProgressBar objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyControls(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        CreateControls(trackedObjects, layout);

        // Allow handlers to connect
        await Task.Delay(50, cancellationToken);

        // Track handlers before disconnect
        TrackHandlers(trackedObjects, layout);

        // Tear down
        TearDown(layout);

        Content = new Label { Text = "ActivityIndicator/ProgressBar test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateControls(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var activityIndicator = new ActivityIndicator
        {
            IsRunning = true,
            Color = Colors.Blue,
        };
        layout.Children.Add(activityIndicator);
        trackedObjects["ActivityIndicator"] = new WeakReference<object>(activityIndicator);

        // Toggle state to exercise property change handlers
        activityIndicator.IsRunning = false;
        activityIndicator.IsRunning = true;

        var progressBar = new ProgressBar
        {
            Progress = 0.5,
            ProgressColor = Colors.Green,
        };
        layout.Children.Add(progressBar);
        trackedObjects["ProgressBar"] = new WeakReference<object>(progressBar);

        // Change progress to exercise update path
        progressBar.Progress = 0.75;
        progressBar.Progress = 1.0;
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
    private static void TearDown(VerticalStackLayout layout)
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
