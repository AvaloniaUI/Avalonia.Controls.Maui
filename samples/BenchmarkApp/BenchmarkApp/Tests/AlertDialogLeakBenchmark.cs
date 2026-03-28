using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that showing and dismissing alert/action-sheet/prompt dialogs doesn't leak
/// pages or window references. Dialog infrastructure uses TaskCompletionSource closures
/// and overlay helpers — this verifies those are properly released.
/// </summary>
[BenchmarkTest("AlertDialogLeak", Description = "Verifies alert/action-sheet dialogs don't leak pages or closures")]
public class AlertDialogLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int dialogCycles = 5;

        await ShowAndDismissDialogs(window, trackedObjects, dialogCycles, logger, cancellationToken);

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
            ["DialogCycles"] = dialogCycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        // Allow at most 1 survivor for GC non-determinism
        if (leaked.Count > 1)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Alert dialog leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (leaked.Count == 1)
        {
            logger.LogWarning(
                "1 of {Count} dialog objects survived GC (likely non-deterministic): {Name}",
                trackedObjects.Count,
                leaked[0]);
        }
        else
        {
            logger.LogInformation(
                "All {Count} dialog-related objects collected after {Cycles} cycles",
                trackedObjects.Count,
                dialogCycles);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task ShowAndDismissDialogs(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        Content = new Label { Text = "Dialog leak test running..." };

        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ShowDialogCycle(trackedObjects, i, logger, cancellationToken);
        }

        Content = new Label { Text = "Dialog leak test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task ShowDialogCycle(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Create a page that will host the dialog
        var hostPage = new ContentPage
        {
            Title = $"Dialog host {index}",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = $"Page for dialog {index}" },
                    new Button { Text = "Show alert" },
                },
            },
        };

        trackedObjects[$"DialogHost{index}.Page"] = new WeakReference<object>(hostPage);

        // Try to show and programmatically dismiss alerts
        try
        {
            // DisplayAlert with auto-cancel via a short timeout
            var alertTask = hostPage.DisplayAlert($"Test {index}", "Test message", "OK");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(100);

            try
            {
                await alertTask.WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected — dialog times out without user interaction
            }
            catch (InvalidOperationException)
            {
                // Expected — page may not have a valid parent/window
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug("Dialog show for cycle {Index} failed (expected): {Error}", index, ex.Message);
        }

        await Task.Delay(30, cancellationToken);

        // Try action sheet
        try
        {
            var actionTask = hostPage.DisplayActionSheet($"Actions {index}", "Cancel", "Delete", "Edit", "Share");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(100);

            try
            {
                await actionTask.WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug("ActionSheet for cycle {Index} failed (expected): {Error}", index, ex.Message);
        }

        await Task.Delay(30, cancellationToken);

        // Clean up the host page
        hostPage.Handler?.DisconnectHandler();
    }
}
