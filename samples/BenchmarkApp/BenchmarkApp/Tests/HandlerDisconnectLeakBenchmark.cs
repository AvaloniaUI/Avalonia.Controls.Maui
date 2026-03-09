

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

        var layout = new VerticalStackLayout();
        Content = layout;

        var weakRefs = new Dictionary<string, WeakReference<VisualElement>>();

        // Create and connect each control type
        var button = new Button { Text = "Leak test button" };
        layout.Children.Add(button);
        weakRefs["Button"] = new WeakReference<VisualElement>(button);

        var label = new Label { Text = "Leak test label" };
        layout.Children.Add(label);
        weakRefs["Label"] = new WeakReference<VisualElement>(label);

        var entry = new Entry { Placeholder = "Leak test entry" };
        layout.Children.Add(entry);
        weakRefs["Entry"] = new WeakReference<VisualElement>(entry);

        // Remove from layout and disconnect handlers
        layout.Children.Clear();
        button.Handler?.DisconnectHandler();
        label.Handler?.DisconnectHandler();
        entry.Handler?.DisconnectHandler();

        // Null local references
        button = null;
        label = null;
        entry = null;

        // Replace content and null layout
        Content = new Label { Text = "Done" };
        layout = null;

        // Force GC with delay to allow pending dispatcher work
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
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

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        return BenchmarkResult.Pass(metrics);
    }
}
