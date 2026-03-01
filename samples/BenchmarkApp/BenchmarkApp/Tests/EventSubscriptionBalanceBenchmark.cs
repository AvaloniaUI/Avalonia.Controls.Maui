
using BenchmarkApp.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Verifies that event subscriptions are properly balanced after handler connect/disconnect cycles.
/// </summary>
/// <remarks>
/// Creates controls with gesture recognizers and event handlers, disconnects handlers,
/// and verifies all objects are collectable. Also tests that re-connecting and re-disconnecting
/// doesn't leak (verifies the balance).
/// </remarks>
[BenchmarkTest("EventSubscriptionBalance", Description = "Verifies event subscriptions are properly balanced after handler disconnect")]
public class EventSubscriptionBalanceBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        // Phase 1: Single connect/disconnect cycle
        logger.LogInformation("Phase 1: Single connect/disconnect cycle");
        var tracker1 = new LeakTracker();

        CreateControlsWithEvents(layout, tracker1, "Phase1");

        // Disconnect
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve)
            {
                ve.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();

        await LeakTracker.ForceFullGcAsync(cancellationToken);

        var result1 = tracker1.Check();
        logger.LogInformation("  Phase 1: {Collected}/{Total} collected", result1.CollectedCount, result1.TotalTracked);

        // Phase 2: Double connect/disconnect cycle (re-connect then re-disconnect)
        logger.LogInformation("Phase 2: Double connect/disconnect cycle");
        var tracker2 = new LeakTracker();

        // First connect
        CreateControlsWithEvents(layout, tracker2, "Phase2");

        // Wait for handlers to connect
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => { },
            Avalonia.Threading.DispatcherPriority.Background);

        // Disconnect all
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve)
            {
                ve.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();

        // Re-create (simulates reconnection)
        CreateControlsWithEvents(layout, tracker2, "Phase2b");

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => { },
            Avalonia.Threading.DispatcherPriority.Background);

        // Disconnect again
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve)
            {
                ve.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();

        Content = new Label { Text = "Event subscription test complete" };

        await LeakTracker.ForceFullGcAsync(cancellationToken);

        var result2 = tracker2.Check();
        logger.LogInformation("  Phase 2: {Collected}/{Total} collected", result2.CollectedCount, result2.TotalTracked);

        var metrics = new Dictionary<string, object>
        {
            ["Phase1.TotalTracked"] = result1.TotalTracked,
            ["Phase1.CollectedCount"] = result1.CollectedCount,
            ["Phase1.SurvivorCount"] = result1.Survivors.Count,
            ["Phase2.TotalTracked"] = result2.TotalTracked,
            ["Phase2.CollectedCount"] = result2.CollectedCount,
            ["Phase2.SurvivorCount"] = result2.Survivors.Count,
        };

        foreach (var (key, value) in result1.ToMetrics())
        {
            metrics[$"Phase1.{key}"] = value;
        }

        foreach (var (key, value) in result2.ToMetrics())
        {
            metrics[$"Phase2.{key}"] = value;
        }

        var reasons = new List<string>();

        if (result1.Survivors.Count > 0)
        {
            var names = string.Join(", ", result1.Survivors.Select(s => s.Name));
            reasons.Add($"Phase 1: {result1.Survivors.Count} objects leaked ({names})");
        }

        if (result2.Survivors.Count > 0)
        {
            var names = string.Join(", ", result2.Survivors.Select(s => s.Name));
            reasons.Add($"Phase 2: {result2.Survivors.Count} objects leaked ({names})");
        }

        if (reasons.Count > 0)
        {
            var reason = string.Join("; ", reasons);
            logger.LogWarning("Event subscription imbalance: {Reason}", reason);
            return BenchmarkResult.Fail(reason, metrics);
        }

        logger.LogInformation("Event subscription balance verified across both phases");
        return BenchmarkResult.Pass(metrics);
    }

    private static void CreateControlsWithEvents(VerticalStackLayout layout, LeakTracker tracker, string prefix)
    {
        // Button with click handler
        var button = new Button { Text = "Event Test" };
        button.Clicked += (_, _) => { };
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (_, _) => { };
        button.GestureRecognizers.Add(tapGesture);
        layout.Children.Add(button);
        tracker.Track($"{prefix}.Button", button);
        tracker.Track($"{prefix}.TapGesture", tapGesture);

        // Label with swipe gesture
        var label = new Label { Text = "Swipe Target" };
        var swipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeGesture.Swiped += (_, _) => { };
        label.GestureRecognizers.Add(swipeGesture);
        layout.Children.Add(label);
        tracker.Track($"{prefix}.Label", label);
        tracker.Track($"{prefix}.SwipeGesture", swipeGesture);

        // Entry with text changed and completed handlers
        var entry = new Entry { Placeholder = "Event Test" };
        entry.TextChanged += (_, _) => { };
        entry.Completed += (_, _) => { };
        var panGesture = new PanGestureRecognizer();
        entry.GestureRecognizers.Add(panGesture);
        layout.Children.Add(entry);
        tracker.Track($"{prefix}.Entry", entry);
        tracker.Track($"{prefix}.PanGesture", panGesture);
    }
}
