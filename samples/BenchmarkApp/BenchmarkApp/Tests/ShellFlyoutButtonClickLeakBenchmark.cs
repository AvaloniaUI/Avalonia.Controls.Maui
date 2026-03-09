using System.Runtime.CompilerServices;
using BenchmarkApp.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that flyout item buttons and their associated ShellItems are collected after
/// the Shell handler is disconnected. Targets the inline lambda in ShellExtensions.UpdateFlyoutItems:
///   button.Click += (s, e) => handler.OnFlyoutItemSelected(item);
/// This lambda captures both the handler and the ShellItem. While the buttons themselves
/// become unreferenced after _flyoutItemButtons.Clear(), the Click event handlers are never
/// explicitly unsubscribed, which is poor hygiene and may prevent collection in edge cases
/// where buttons are retained by the Avalonia visual tree infrastructure.
/// </summary>
[BenchmarkTest("ShellFlyoutButtonClickLeak",
    Description = "Verifies flyout item buttons are collected after handler disconnect (Click event leak)")]
public class ShellFlyoutButtonClickLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var tracker = new LeakTracker();

        const int churnCycles = 5;
        const int itemsPerCycle = 3;

        await CreateShellAndChurnFlyoutItems(window, tracker, churnCycles, itemsPerCycle, logger, cancellationToken);

        await LeakTracker.ForceFullGcAsync(cancellationToken);
        await Task.Delay(50, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);
        var leakResult = tracker.Check();

        var metrics = new Dictionary<string, object>
        {
            ["ChurnCycles"] = churnCycles,
            ["ItemsPerCycle"] = itemsPerCycle,
        };

        foreach (var (key, value) in leakResult.ToMetrics())
        {
            metrics[key] = value;
        }

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leakResult.Survivors.Count > 0)
        {
            var survivorNames = string.Join(", ", leakResult.Survivors.Select(s => s.Name));
            logger.LogWarning(
                "Shell flyout button Click leak detected: {Survivors} survived out of {Total}",
                survivorNames, leakResult.TotalTracked);
            return BenchmarkResult.Fail($"Objects leaked: {survivorNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} flyout button objects collected after {Cycles} churn cycles",
            leakResult.TotalTracked, churnCycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateShellAndChurnFlyoutItems(
        Window window,
        LeakTracker tracker,
        int churnCycles,
        int itemsPerCycle,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var shell = new Shell
        {
            FlyoutBehavior = FlyoutBehavior.Flyout,
        };

        // Add a permanent home item
        shell.Items.Add(new ShellContent
        {
            Title = "Home",
            ContentTemplate = new DataTemplate(() => new ContentPage
            {
                Content = new Label { Text = "Home" },
            }),
        });

        window.Page = shell;
        await Task.Delay(100, cancellationToken);

        // Churn: add items, track them, then remove them
        for (int cycle = 0; cycle < churnCycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Add flyout items
            var addedItems = new List<FlyoutItem>();
            for (int i = 0; i < itemsPerCycle; i++)
            {
                var item = new FlyoutItem
                {
                    Title = $"C{cycle}_I{i}",
                    FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem,
                    Items =
                    {
                        new ShellContent
                        {
                            Title = $"C{cycle}_Content{i}",
                            ContentTemplate = new DataTemplate(() => new ContentPage
                            {
                                Content = new Label { Text = $"Cycle {cycle} Item {i}" },
                            }),
                        },
                    },
                };
                shell.Items.Add(item);
                addedItems.Add(item);
            }

            // Allow handlers to connect and flyout to update
            await Task.Delay(50, cancellationToken);

            // Track the items before removing
            TrackItems(tracker, addedItems, cycle);

            // Remove all added items
            foreach (var item in addedItems)
            {
                item.Handler?.DisconnectHandler();
                shell.Items.Remove(item);
            }

            await Task.Delay(30, cancellationToken);
        }

        // Track and disconnect the Shell handler itself
        TrackShellHandler(tracker, shell);
        shell.Handler?.DisconnectHandler();

        // Restore this page
        window.Page = this;
        Content = new Label { Text = "Shell flyout button click leak test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackItems(LeakTracker tracker, List<FlyoutItem> items, int cycle)
    {
        for (int i = 0; i < items.Count; i++)
        {
            tracker.Track($"Cycle{cycle}.FlyoutItem{i}", items[i]);
            if (items[i].Handler is object handler)
            {
                tracker.Track($"Cycle{cycle}.FlyoutItemHandler{i}", handler);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackShellHandler(LeakTracker tracker, Shell shell)
    {
        if (shell.Handler is object handler)
        {
            tracker.Track("Shell.Handler", handler);
        }

        tracker.Track("Shell", shell);
    }
}
