using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests the disconnect → reconnect → disconnect lifecycle. Some handlers may not properly
/// clean up when reconnected after a prior disconnect. Creates controls, disconnects handlers,
/// forces reconnection by re-adding to layout, then disconnects again and verifies all collected.
/// </summary>
[BenchmarkTest("HandlerReconnectionLeak", Description = "Verifies disconnect/reconnect/disconnect lifecycle doesn't leak")]
public class HandlerReconnectionLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int cycles = 5;

        await ReconnectionCycles(trackedObjects, cycles, cancellationToken);

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
            ["ReconnectionCycles"] = cycles,
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
            logger.LogWarning("Handler reconnection leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} objects collected after {Cycles} reconnection cycles",
            trackedObjects.Count,
            cycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task ReconnectionCycles(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await RunSingleCycle(layout, trackedObjects, i, cancellationToken);
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
        Content = new Label { Text = "Handler reconnection test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task RunSingleCycle(
        VerticalStackLayout layout,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index,
        CancellationToken cancellationToken)
    {
        // Create controls
        var button = new Button { Text = $"Reconnect button {index}" };
        var entry = new Entry { Placeholder = $"Reconnect entry {index}" };
        var label = new Label { Text = $"Reconnect label {index}" };
        var editor = new Editor { Text = $"Reconnect editor {index}" };

        trackedObjects[$"Cycle{index}.Button"] = new WeakReference<object>(button);
        trackedObjects[$"Cycle{index}.Entry"] = new WeakReference<object>(entry);
        trackedObjects[$"Cycle{index}.Label"] = new WeakReference<object>(label);
        trackedObjects[$"Cycle{index}.Editor"] = new WeakReference<object>(editor);

        // Phase 1: Add to layout (triggers handler connect)
        layout.Children.Add(button);
        layout.Children.Add(entry);
        layout.Children.Add(label);
        layout.Children.Add(editor);
        await Task.Delay(30, cancellationToken);

        // Track first-generation handlers
        if (button.Handler is object h1)
        {
            trackedObjects[$"Cycle{index}.Button.Handler1"] = new WeakReference<object>(h1);
        }

        if (entry.Handler is object h2)
        {
            trackedObjects[$"Cycle{index}.Entry.Handler1"] = new WeakReference<object>(h2);
        }

        // Phase 2: Disconnect handlers
        button.Handler?.DisconnectHandler();
        entry.Handler?.DisconnectHandler();
        label.Handler?.DisconnectHandler();
        editor.Handler?.DisconnectHandler();

        // Phase 3: Remove and re-add to trigger reconnection
        layout.Children.Clear();
        layout.Children.Add(button);
        layout.Children.Add(entry);
        layout.Children.Add(label);
        layout.Children.Add(editor);
        await Task.Delay(30, cancellationToken);

        // Track second-generation handlers
        if (button.Handler is object h3)
        {
            trackedObjects[$"Cycle{index}.Button.Handler2"] = new WeakReference<object>(h3);
        }

        if (entry.Handler is object h4)
        {
            trackedObjects[$"Cycle{index}.Entry.Handler2"] = new WeakReference<object>(h4);
        }

        // Phase 4: Final disconnect
        button.Handler?.DisconnectHandler();
        entry.Handler?.DisconnectHandler();
        label.Handler?.DisconnectHandler();
        editor.Handler?.DisconnectHandler();

        layout.Children.Clear();
        await Task.Delay(20, cancellationToken);
    }
}
