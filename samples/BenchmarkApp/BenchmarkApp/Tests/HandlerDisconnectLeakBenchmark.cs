
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that Button, Label, and Entry controls don't leak after handler disconnect.
/// </summary>
[BenchmarkTest("HandlerDisconnectLeak", Description = "Verifies controls can be GC'd after handler disconnect")]
public class HandlerDisconnectLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var weakRefs = await CreateAndTearDownControlsAsync(cancellationToken);

        // Force GC after the teardown work and queued UI cleanup have completed.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);

        // Check which controls leaked
        var leaked = new List<string>();
        foreach (var (name, weakRef) in weakRefs)
        {
            if (weakRef.TryGetTarget(out _))
            {
                leaked.Add(name);
            }
        }

        var metrics = new Dictionary<string, object>
        {
            ["ControlsTested"] = weakRefs.Count,
            ["ControlsLeaked"] = leaked.Count,
            ["Button.Leaked"] = weakRefs["Button"].TryGetTarget(out _),
            ["Label.Leaked"] = weakRefs["Label"].TryGetTarget(out _),
            ["Entry.Leaked"] = weakRefs["Entry"].TryGetTarget(out _),
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Memory leak detected: {LeakedControls}", leakedNames);
            return BenchmarkResult.Fail($"Controls leaked: {leakedNames}", metrics);
        }

        logger.LogInformation("All {Count} controls collected successfully", weakRefs.Count);

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<Dictionary<string, WeakReference<VisualElement>>> CreateAndTearDownControlsAsync(
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        var weakRefs = new Dictionary<string, WeakReference<VisualElement>>();

        var button = new Button { Text = "Leak test button" };
        layout.Children.Add(button);
        weakRefs["Button"] = new WeakReference<VisualElement>(button);

        var label = new Label { Text = "Leak test label" };
        layout.Children.Add(label);
        weakRefs["Label"] = new WeakReference<VisualElement>(label);

        var entry = new Entry { Placeholder = "Leak test entry" };
        layout.Children.Add(entry);
        weakRefs["Entry"] = new WeakReference<VisualElement>(entry);

        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken);

        layout.Children.Clear();
        button.Handler?.DisconnectHandler();
        label.Handler?.DisconnectHandler();
        entry.Handler?.DisconnectHandler();

        button = null;
        label = null;
        entry = null;

        Content = new Label { Text = "Done" };
        layout = null;

        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken);

        return weakRefs;
    }
}
