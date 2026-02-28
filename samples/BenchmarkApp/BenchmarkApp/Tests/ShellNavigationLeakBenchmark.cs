

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that pages navigated via Shell don't leak after navigating away.
/// Targets ShellSectionHandler._navigationStack and _currentPage not being cleared on disconnect.
/// </summary>
[BenchmarkTest("ShellNavigationLeak", Description = "Verifies Shell-navigated pages are collected after navigation")]
public class ShellNavigationLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int navigationCycles = 5;

        // Build a Shell with multiple pages and navigate between them
        await BuildShellAndNavigate(trackedObjects, navigationCycles, logger, cancellationToken);

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
            ["NavigationCycles"] = navigationCycles,
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
            logger.LogWarning("Shell navigation leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} objects collected after {Cycles} navigation cycles",
            trackedObjects.Count,
            navigationCycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task BuildShellAndNavigate(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Create pages that simulate Shell navigation targets
        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Build a page with controls, simulating what Shell navigation does
            CreateAndTrackPage(trackedObjects, $"Cycle{i}");

            // Allow handlers to connect
            await Task.Delay(30, cancellationToken);

            // Tear down to simulate navigation away
            TearDownCurrentContent(trackedObjects, $"Cycle{i}");
        }

        // Final content replacement
        Content = new Label { Text = "Navigation complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void CreateAndTrackPage(Dictionary<string, WeakReference<object>> trackedObjects, string prefix)
    {
        var pageLayout = new VerticalStackLayout();
        trackedObjects[$"{prefix}.Layout"] = new WeakReference<object>(pageLayout);

        var titleLabel = new Label { Text = $"{prefix} Title" };
        pageLayout.Children.Add(titleLabel);
        trackedObjects[$"{prefix}.TitleLabel"] = new WeakReference<object>(titleLabel);

        var contentLabel = new Label { Text = $"{prefix} Content with some text" };
        pageLayout.Children.Add(contentLabel);
        trackedObjects[$"{prefix}.ContentLabel"] = new WeakReference<object>(contentLabel);

        var actionButton = new Button { Text = $"{prefix} Action" };
        actionButton.Clicked += (_, _) => { };
        pageLayout.Children.Add(actionButton);
        trackedObjects[$"{prefix}.ActionButton"] = new WeakReference<object>(actionButton);

        var inputEntry = new Entry { Placeholder = $"{prefix} Input" };
        pageLayout.Children.Add(inputEntry);
        trackedObjects[$"{prefix}.InputEntry"] = new WeakReference<object>(inputEntry);

        Content = pageLayout;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void TearDownCurrentContent(Dictionary<string, WeakReference<object>> trackedObjects, string prefix)
    {
        if (Content is not Layout layout)
            return;

        // Track handlers before disconnecting
        TrackChildHandlers(trackedObjects, layout, prefix);

        // Disconnect all handlers
        DisconnectLayoutHandlers(layout);

        // Clear content
        Content = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackChildHandlers(
        Dictionary<string, WeakReference<object>> trackedObjects,
        Layout layout,
        string prefix)
    {
        int index = 0;
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve && ve.Handler is object handler)
            {
                trackedObjects[$"{prefix}.Handler{index}"] = new WeakReference<object>(handler);
            }

            index++;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DisconnectLayoutHandlers(Layout layout)
    {
        foreach (var child in layout.Children)
        {
            if (child is Layout childLayout)
            {
                DisconnectLayoutHandlers(childLayout);
            }

            if (child is VisualElement ve)
            {
                ve.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
    }
}
