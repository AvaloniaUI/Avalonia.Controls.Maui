using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that rapidly pushing and popping pages on a NavigationPage doesn't leak pages or
/// their handlers. The StackNavigationManager swaps CurrentPage subscriptions on each
/// navigation — this stress-tests that path to detect subscription accumulation.
/// </summary>
[BenchmarkTest("NavigationStackDepthLeak", Description = "Verifies rapid push/pop on NavigationPage doesn't leak pages")]
public class NavigationStackDepthLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int pushPopCycles = 10;

        await PushAndPopPages(window, trackedObjects, pushPopCycles, logger, cancellationToken);

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
            ["PushPopCycles"] = pushPopCycles,
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
            logger.LogWarning("Navigation stack depth leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} objects collected after {Cycles} push/pop cycles",
            trackedObjects.Count,
            pushPopCycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task PushAndPopPages(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Create a NavigationPage as the window's page
        var rootPage = new ContentPage
        {
            Title = "Root",
            Content = new Label { Text = "Navigation root" },
        };
        var navPage = new NavigationPage(rootPage);

        window.Page = navPage;
        await Task.Delay(100, cancellationToken);

        // Rapid push/pop cycles
        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pushedPage = CreatePushedPage(trackedObjects, i);

            await navPage.PushAsync(pushedPage, animated: false);
            await Task.Delay(30, cancellationToken);

            // Track handler after it's connected
            if (pushedPage.Handler is object handler)
            {
                trackedObjects[$"Push{i}.Handler"] = new WeakReference<object>(handler);
            }

            await navPage.PopAsync(animated: false);
            await Task.Delay(30, cancellationToken);
        }

        // Also test deep stack: push 5 pages, then pop all
        for (int i = 0; i < 5; i++)
        {
            var deepPage = CreatePushedPage(trackedObjects, cycles + i);
            await navPage.PushAsync(deepPage, animated: false);
            await Task.Delay(20, cancellationToken);

            if (deepPage.Handler is object handler)
            {
                trackedObjects[$"Push{cycles + i}.Handler"] = new WeakReference<object>(handler);
            }
        }

        // Pop all 5 at once
        await navPage.PopToRootAsync(animated: false);
        await Task.Delay(50, cancellationToken);

        // Track nav page handler
        if (navPage.Handler is object navHandler)
        {
            trackedObjects["NavigationPage.Handler"] = new WeakReference<object>(navHandler);
        }

        // Track root page
        trackedObjects["RootPage"] = new WeakReference<object>(rootPage);
        if (rootPage.Handler is object rootHandler)
        {
            trackedObjects["RootPage.Handler"] = new WeakReference<object>(rootHandler);
        }

        // Disconnect navigation page
        rootPage.Handler?.DisconnectHandler();
        navPage.Handler?.DisconnectHandler();

        // Restore test page
        window.Page = this;
        Content = new Label { Text = "Navigation stack depth test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreatePushedPage(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index)
    {
        var page = new ContentPage
        {
            Title = $"Page {index}",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = $"Pushed page {index}" },
                    new Button { Text = $"Action {index}" },
                    new Entry { Placeholder = $"Input {index}" },
                },
            },
        };

        trackedObjects[$"Push{index}.Page"] = new WeakReference<object>(page);
        return page;
    }
}
