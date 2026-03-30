using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that creating dispatcher timers, subscribing to Tick, starting, stopping, and
/// nulling them allows GC. The AvaloniaDispatcherTimer wraps Avalonia's DispatcherTimer
/// and subscribes to its Tick event — this verifies cleanup.
/// </summary>
[BenchmarkTest("DispatcherTimerLeak", Description = "Verifies DispatcherTimer instances are collected after stop and disposal")]
public class DispatcherTimerLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int timerCount = 10;

        await CreateAndStopTimers(trackedObjects, timerCount, logger, cancellationToken);

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
            ["TimerCount"] = timerCount,
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
            logger.LogWarning("DispatcherTimer leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        logger.LogInformation(
            "All {Count} timer objects collected after {TimerCount} timers",
            trackedObjects.Count,
            timerCount);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndStopTimers(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int count,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        Content = new Label { Text = "Timer leak test running..." };

        var dispatcher = Dispatcher;
        if (dispatcher is null)
        {
            logger.LogWarning("No dispatcher available, attempting to get from application");
            dispatcher = Application.Current?.Dispatcher;
        }

        if (dispatcher is null)
        {
            logger.LogWarning("No dispatcher available for timer test, skipping timer creation");
            Content = new Label { Text = "Timer test skipped (no dispatcher)" };
            return;
        }

        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CreateStartAndStopTimer(trackedObjects, dispatcher, i);
        }

        // Allow any pending tick callbacks to drain
        await Task.Delay(200, cancellationToken);

        Content = new Label { Text = "Timer leak test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateStartAndStopTimer(
        Dictionary<string, WeakReference<object>> trackedObjects,
        IDispatcher dispatcher,
        int index)
    {
        var timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(50);

        int tickCount = 0;
        timer.Tick += (_, _) => tickCount++;

        trackedObjects[$"Timer{index}"] = new WeakReference<object>(timer);

        timer.Start();
        timer.Stop();
    }
}
