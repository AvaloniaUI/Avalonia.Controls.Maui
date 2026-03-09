using System.Runtime.CompilerServices;
using BenchmarkApp.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that the ShellHandler is collected after disconnect. Targets the
/// _flyoutContentControl.AttachedToVisualTree inline lambda in ShellHandler.CreatePlatformView
/// which captures 'this' (the handler) but is never unsubscribed in DisconnectHandler.
/// This keeps the handler (and transitively the Shell VirtualView) alive as long as
/// _flyoutContentControl remains in the visual tree.
/// </summary>
[BenchmarkTest("ShellHandlerFlyoutContentLeak",
    Description = "Verifies ShellHandler is collected after disconnect (AttachedToVisualTree event leak)")]
public class ShellHandlerFlyoutContentLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var tracker = new LeakTracker();

        // Run multiple cycles to amplify the leak
        const int cycles = 3;
        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await CreateAndDisconnectShell(window, tracker, i, logger, cancellationToken);
        }

        // Restore this page as window content
        window.Page = this;
        Content = new Label { Text = "Shell handler leak test complete" };

        await LeakTracker.ForceFullGcAsync(cancellationToken);

        // Extra GC pass for good measure
        await Task.Delay(50, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);
        var leakResult = tracker.Check();

        var metrics = new Dictionary<string, object>
        {
            ["Cycles"] = cycles,
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
                "Shell handler flyout content leak detected: {Survivors} survived out of {Total}",
                survivorNames, leakResult.TotalTracked);
            return BenchmarkResult.Fail($"Objects leaked: {survivorNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} tracked objects collected after {Cycles} cycles",
            leakResult.TotalTracked, cycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task CreateAndDisconnectShell(
        Window window,
        LeakTracker tracker,
        int cycle,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Create a Shell with flyout items (triggers CreatePlatformView and the
        // AttachedToVisualTree subscription on _flyoutContentControl)
        var shell = new Shell
        {
            FlyoutBehavior = FlyoutBehavior.Flyout,
        };

        var item1 = new FlyoutItem
        {
            Title = $"Cycle{cycle}_Page1",
            FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem,
            Items =
            {
                new ShellContent
                {
                    Title = $"Cycle{cycle}_Content1",
                    ContentTemplate = new DataTemplate(() => new ContentPage
                    {
                        Content = new Label { Text = $"Cycle {cycle} Page 1" },
                    }),
                },
            },
        };

        var item2 = new FlyoutItem
        {
            Title = $"Cycle{cycle}_Page2",
            FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem,
            Items =
            {
                new ShellContent
                {
                    Title = $"Cycle{cycle}_Content2",
                    ContentTemplate = new DataTemplate(() => new ContentPage
                    {
                        Content = new Label { Text = $"Cycle {cycle} Page 2" },
                    }),
                },
            },
        };

        shell.Items.Add(item1);
        shell.Items.Add(item2);

        // Set as window page to trigger handler connection
        window.Page = shell;
        await Task.Delay(100, cancellationToken);

        // Track the handler and the shell BEFORE disconnecting
        TrackShellObjects(tracker, shell, cycle);

        // Disconnect the handler
        logger.LogInformation("Cycle {Cycle}: disconnecting Shell handler", cycle);
        shell.Handler?.DisconnectHandler();

        // Remove from window
        window.Page = new ContentPage { Content = new Label { Text = "placeholder" } };
        await Task.Delay(50, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackShellObjects(LeakTracker tracker, Shell shell, int cycle)
    {
        if (shell.Handler is object handler)
        {
            tracker.Track($"Cycle{cycle}.ShellHandler", handler);
        }

        tracker.Track($"Cycle{cycle}.Shell", shell);
    }
}
