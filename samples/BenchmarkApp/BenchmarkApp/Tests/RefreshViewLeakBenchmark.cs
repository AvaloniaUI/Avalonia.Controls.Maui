using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that RefreshView controls are collected after handler disconnect.
/// RefreshViewHandler subscribes to RefreshRequested, PropertyChanged, and TemplateApplied
/// events — this verifies those subscriptions are properly cleaned up across multiple
/// refresh cycles.
/// </summary>
[BenchmarkTest("RefreshViewLeak", Description = "Verifies RefreshView is collected after refresh cycles and disconnect")]
public class RefreshViewLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int iterations = 5;
        const int refreshCyclesPerIteration = 3;

        await BuildRefreshAndTearDown(trackedObjects, iterations, refreshCyclesPerIteration, cancellationToken);

        // Force GC multiple times with delays
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
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
            ["Iterations"] = iterations,
            ["RefreshCyclesPerIteration"] = refreshCyclesPerIteration,
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
            logger.LogWarning("RefreshView leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        logger.LogInformation(
            "All {Count} RefreshView objects collected after {Iterations} iterations",
            trackedObjects.Count,
            iterations);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task BuildRefreshAndTearDown(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int iterations,
        int refreshCycles,
        CancellationToken cancellationToken)
    {
        var outerLayout = new VerticalStackLayout();
        Content = outerLayout;

        for (int i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var refreshView = CreateRefreshView(trackedObjects, i);
            outerLayout.Children.Add(refreshView);

            // Allow handler to connect
            await Task.Delay(30, cancellationToken);

            // Track handler
            if (refreshView.Handler is object handler)
            {
                trackedObjects[$"RefreshView{i}.Handler"] = new WeakReference<object>(handler);
            }

            // Simulate refresh cycles
            for (int r = 0; r < refreshCycles; r++)
            {
                refreshView.IsRefreshing = true;
                await Task.Delay(20, cancellationToken);
                refreshView.IsRefreshing = false;
                await Task.Delay(20, cancellationToken);
            }

            // Remove and disconnect
            outerLayout.Children.Remove(refreshView);
            refreshView.Handler?.DisconnectHandler();
        }

        outerLayout.Children.Clear();
        outerLayout.Handler?.DisconnectHandler();
        Content = new Label { Text = "RefreshView test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static RefreshView CreateRefreshView(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index)
    {
        var innerContent = new VerticalStackLayout
        {
            Children =
            {
                new Label { Text = $"Refresh content {index}" },
                new Label { Text = "Pull to refresh" },
            },
        };

        var refreshView = new RefreshView
        {
            Content = innerContent,
            RefreshColor = Colors.Blue,
        };

        trackedObjects[$"RefreshView{index}"] = new WeakReference<object>(refreshView);
        trackedObjects[$"RefreshView{index}.Content"] = new WeakReference<object>(innerContent);

        return refreshView;
    }
}
