using System.Runtime.CompilerServices;
using BenchmarkApp.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that NavigationPage and its pages are collected after being replaced.
/// Validates that the NavigationViewHandler, StackNavigationManager, and NavigationView
/// properly clean up event subscriptions and references during disconnect.
/// </summary>
[BenchmarkTest("NavigationViewDetachLeak",
    Description = "Verifies NavigationPage is collected after replacement and handler disconnect")]
public class NavigationViewDetachLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var tracker = new LeakTracker();

        const int cycles = 5;

        await CreateAndDestroyNavigationPages(window, tracker, cycles, logger, cancellationToken);

        // Restore this test page and allow pending UI work to complete
        window.Page = this;
        Content = new Label { Text = "NavigationView detach leak test complete" };
        await Task.Delay(100, cancellationToken);

        // Force GC aggressively - the async state machines from CreateNavigationPageCycle
        // need to be collected first (they hold locals like navPage, rootPage)
        await LeakTracker.ForceFullGcAsync(cancellationToken);
        await Task.Delay(100, cancellationToken);
        await LeakTracker.ForceFullGcAsync(cancellationToken);

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
                "NavigationView detach leak detected: {Survivors} survived out of {Total}",
                survivorNames, leakResult.TotalTracked);
            return BenchmarkResult.Fail($"Objects leaked: {survivorNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} NavigationPage objects collected after {Cycles} cycles",
            leakResult.TotalTracked, cycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task CreateAndDestroyNavigationPages(
        Window window,
        LeakTracker tracker,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await CreateNavigationPageCycle(window, tracker, i, logger, cancellationToken);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task CreateNavigationPageCycle(
        Window window,
        LeakTracker tracker,
        int cycle,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Build and connect in a separate non-inlined method so locals
        // don't root objects on this async state machine
        var refs = BuildAndConnect(window, tracker, cycle);
        await Task.Delay(80, cancellationToken);

        // Push a child page
        var childRefs = PushChildPage(refs.NavPage, tracker, cycle);
        await Task.Delay(50, cancellationToken);

        // Track objects before teardown
        TrackNavigationObjects(tracker, refs.NavPage, cycle);

        // Pop child page
        await refs.NavPage.PopAsync();
        await Task.Delay(30, cancellationToken);

        // Replace window page FIRST (lets MAUI handle internal cleanup),
        // then disconnect the handler
        window.Page = new ContentPage { Content = new Label { Text = "placeholder" } };
        await Task.Delay(30, cancellationToken);

        // Now disconnect the handler (navPage is no longer window.Page)
        refs.NavPage.Handler?.DisconnectHandler();
        await Task.Delay(30, cancellationToken);

        logger.LogInformation("Cycle {Cycle}: NavigationPage disconnected and detached", cycle);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (NavigationPage NavPage, ContentPage RootPage) BuildAndConnect(
        Window window,
        LeakTracker tracker,
        int cycle)
    {
        var rootPage = new ContentPage
        {
            Title = $"Cycle{cycle}_Root",
            Content = new VerticalStackLayout
            {
                new Label { Text = $"Root page cycle {cycle}" },
                new Button { Text = "Navigate" },
            },
        };

        var navPage = new NavigationPage(rootPage);

        tracker.Track($"Cycle{cycle}.RootPage", rootPage);
        tracker.Track($"Cycle{cycle}.NavigationPage", navPage);

        // Set as window page to connect handlers
        window.Page = navPage;

        return (navPage, rootPage);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage PushChildPage(
        NavigationPage navPage,
        LeakTracker tracker,
        int cycle)
    {
        var childPage = new ContentPage
        {
            Title = $"Cycle{cycle}_Child",
            Content = new VerticalStackLayout
            {
                new Label { Text = $"Child page cycle {cycle}" },
                new Entry { Placeholder = "Some input" },
            },
            ToolbarItems =
            {
                new ToolbarItem { Text = "Action1" },
                new ToolbarItem { Text = "Action2" },
                new ToolbarItem { Text = "Action3" },
                new ToolbarItem { Text = "Overflow1" },
            },
        };

        tracker.Track($"Cycle{cycle}.ChildPage", childPage);

        // PushAsync is called synchronously here; the await happens in the caller
        _ = navPage.PushAsync(childPage);

        return childPage;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackNavigationObjects(
        LeakTracker tracker,
        NavigationPage navPage,
        int cycle)
    {
        if (navPage.Handler is object navHandler)
        {
            tracker.Track($"Cycle{cycle}.NavigationPageHandler", navHandler);
        }

        // Track root page handler
        if (navPage.RootPage?.Handler is object rootHandler)
        {
            tracker.Track($"Cycle{cycle}.RootPageHandler", rootHandler);
        }

        // Track child page handler (current page on top of stack)
        if (navPage.CurrentPage?.Handler is object childHandler && navPage.CurrentPage != navPage.RootPage)
        {
            tracker.Track($"Cycle{cycle}.ChildPageHandler", childHandler);
        }
    }
}
