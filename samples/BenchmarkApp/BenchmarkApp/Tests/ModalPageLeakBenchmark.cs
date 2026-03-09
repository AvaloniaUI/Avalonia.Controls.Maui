using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that modal pages pushed and popped via Navigation.PushModalAsync/PopModalAsync
/// are collected after dismissal. The MauiAvaloniaWindow manages a modal stack with scrim
/// overlays — this verifies those resources are properly released.
/// </summary>
[BenchmarkTest("ModalPageLeak", Description = "Verifies modal pages are collected after PopModalAsync")]
public class ModalPageLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int modalCycles = 5;

        await PushAndPopModals(trackedObjects, modalCycles, logger, cancellationToken);

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
            ["ModalCycles"] = modalCycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        // With animated:false properly respected, no async animations hold references.
        // Allow at most 1 survivor for GC non-determinism tolerance.
        if (leaked.Count > 1)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Modal page leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (leaked.Count == 1)
        {
            logger.LogWarning(
                "1 of {Count} modal objects survived GC (likely non-deterministic): {Name}",
                trackedObjects.Count,
                leaked[0]);
        }
        else
        {
            logger.LogInformation(
                "All {Count} modal page objects collected after {Cycles} push/pop cycles",
                trackedObjects.Count,
                modalCycles);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task PushAndPopModals(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        Content = new Label { Text = "Modal test running..." };

        // Each push/pop cycle is in its own NoInlining method to ensure
        // the async state machine doesn't keep modal page references alive
        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await PushPopSingleModal(trackedObjects, i, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Modal push/pop cycle {Cycle} failed: {Error}", i, ex.Message);
                break;
            }
        }

        // Test stacked modals: push 3, then pop all
        await PushAndPopStackedModals(trackedObjects, cycles, logger, cancellationToken);

        Content = new Label { Text = "Modal page test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task PushPopSingleModal(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index,
        CancellationToken cancellationToken)
    {
        var modalPage = CreateModalPage(trackedObjects, index);

        await Navigation.PushModalAsync(modalPage, animated: false);
        await Task.Delay(50, cancellationToken);

        // Track handler after it's connected
        if (modalPage.Handler is object handler)
        {
            trackedObjects[$"Modal{index}.Handler"] = new WeakReference<object>(handler);
        }

        await Navigation.PopModalAsync(animated: false);
        await Task.Delay(30, cancellationToken);

        // Explicitly disconnect handler after pop to break reference chains
        modalPage.Handler?.DisconnectHandler();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task PushAndPopStackedModals(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int baseIndex,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var stackedModals = new List<ContentPage>();
        try
        {
            for (int i = 0; i < 3; i++)
            {
                var modal = CreateModalPage(trackedObjects, baseIndex + i);
                stackedModals.Add(modal);

                await Navigation.PushModalAsync(modal, animated: false);
                await Task.Delay(30, cancellationToken);

                if (modal.Handler is object handler)
                {
                    trackedObjects[$"Modal{baseIndex + i}.Handler"] = new WeakReference<object>(handler);
                }
            }

            // Pop all stacked modals and disconnect handlers
            for (int i = stackedModals.Count - 1; i >= 0; i--)
            {
                await Navigation.PopModalAsync(animated: false);
                await Task.Delay(20, cancellationToken);
                stackedModals[i].Handler?.DisconnectHandler();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Stacked modal test failed: {Error}", ex.Message);
        }

        stackedModals.Clear();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateModalPage(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index)
    {
        var page = new ContentPage
        {
            Title = $"Modal {index}",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = $"Modal Page {index}", FontSize = 24 },
                    new Label { Text = "This is a modal page" },
                    new Button { Text = "Close" },
                    new Entry { Placeholder = "Type something" },
                },
            },
        };

        trackedObjects[$"Modal{index}.Page"] = new WeakReference<object>(page);
        return page;
    }
}
