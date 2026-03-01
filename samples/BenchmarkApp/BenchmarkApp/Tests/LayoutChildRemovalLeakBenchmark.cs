using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that controls removed from a layout via Children.Remove() / Children.Clear()
/// can be garbage collected WITHOUT explicit DisconnectHandler() calls.
/// This simulates real-world usage where app code removes children from layouts
/// (e.g., dynamic tile creation/removal in games) without manually disconnecting handlers.
/// </summary>
[BenchmarkTest("LayoutChildRemovalLeak",
    Description = "Verifies controls removed from layouts without explicit DisconnectHandler can be GC'd")]
public class LayoutChildRemovalLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var weakRefs = new Dictionary<string, WeakReference<VisualElement>>();

        // Phase 1: Test Children.Remove() without DisconnectHandler
        await TestChildrenRemove(weakRefs, cancellationToken);

        // Phase 2: Test Children.Clear() without DisconnectHandler
        await TestChildrenClear(weakRefs, cancellationToken);

        // Phase 3: Test repeated add/remove cycles (simulates game tile churn)
        await TestRepeatedAddRemoveCycles(weakRefs, cancellationToken);

        // Force GC with delays to allow pending dispatcher work
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(200, cancellationToken);
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
        };

        foreach (var (name, weakRef) in weakRefs)
        {
            metrics[$"{name}.Leaked"] = weakRef.TryGetTarget(out _);
        }

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Memory leak detected: {LeakedControls}", leakedNames);
            return BenchmarkResult.Fail($"Controls leaked without DisconnectHandler: {leakedNames}", metrics);
        }

        logger.LogInformation("All {Count} controls collected successfully without explicit DisconnectHandler", weakRefs.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task TestChildrenRemove(Dictionary<string, WeakReference<VisualElement>> weakRefs, CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        var button = new Button { Text = "Remove test" };
        layout.Children.Add(button);
        weakRefs["Remove.Button"] = new WeakReference<VisualElement>(button);

        var label = new Label { Text = "Remove test" };
        layout.Children.Add(label);
        weakRefs["Remove.Label"] = new WeakReference<VisualElement>(label);

        var entry = new Entry { Placeholder = "Remove test" };
        layout.Children.Add(entry);
        weakRefs["Remove.Entry"] = new WeakReference<VisualElement>(entry);

        await Task.Delay(50, cancellationToken);

        // Remove WITHOUT calling DisconnectHandler — this is what real apps do
        layout.Children.Remove(button);
        layout.Children.Remove(label);
        layout.Children.Remove(entry);

        button = null;
        label = null;
        entry = null;

        Content = new Label { Text = "Phase 1 done" };
        layout = null;

        await Task.Delay(50, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task TestChildrenClear(Dictionary<string, WeakReference<VisualElement>> weakRefs, CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        var button = new Button { Text = "Clear test" };
        layout.Children.Add(button);
        weakRefs["Clear.Button"] = new WeakReference<VisualElement>(button);

        var label = new Label { Text = "Clear test" };
        layout.Children.Add(label);
        weakRefs["Clear.Label"] = new WeakReference<VisualElement>(label);

        await Task.Delay(50, cancellationToken);

        // Clear WITHOUT calling DisconnectHandler on individual children
        layout.Children.Clear();

        button = null;
        label = null;

        Content = new Label { Text = "Phase 2 done" };
        layout = null;

        await Task.Delay(50, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task TestRepeatedAddRemoveCycles(Dictionary<string, WeakReference<VisualElement>> weakRefs, CancellationToken cancellationToken)
    {
        var layout = new AbsoluteLayout();
        Content = layout;

        // Simulate game tile churn: create 20 controls, remove them, repeat
        for (int cycle = 0; cycle < 3; cycle++)
        {
            var controls = new List<Label>();
            for (int i = 0; i < 20; i++)
            {
                var lbl = new Label { Text = $"Tile {cycle}-{i}" };
                layout.Children.Add(lbl);
                controls.Add(lbl);

                // Track a subset for GC verification
                if (cycle == 2)
                {
                    weakRefs[$"Cycle.Label_{i}"] = new WeakReference<VisualElement>(lbl);
                }
            }

            await Task.Delay(30, cancellationToken);

            // Remove all without DisconnectHandler
            layout.Children.Clear();
            controls.Clear();
        }

        Content = new Label { Text = "Phase 3 done" };
        layout = null;

        await Task.Delay(50, cancellationToken);
    }
}
