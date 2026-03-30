

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Verifies that TabbedPage and its child pages are collected after handler disconnect.
/// TabbedPageHandler subscribes to PagesChanged and SelectionChanged events and manages
/// tab items, making it a complex handler to clean up correctly.
/// </summary>
[BenchmarkTest("TabbedPageLeak", Description = "Verifies TabbedPage and child pages are collected after disconnect")]
public class TabbedPageLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyTabbedPage(window, trackedObjects, cancellationToken);

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

        int pagesCreated = 3;
        int childControlsTracked = trackedObjects.Count(kv =>
            kv.Key.Contains(".Label") || kv.Key.Contains(".Button"));

        var metrics = new Dictionary<string, object>
        {
            ["PagesCreated"] = pagesCreated,
            ["ChildControlsTracked"] = childControlsTracked,
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
            logger.LogWarning("TabbedPage leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} TabbedPage objects collected successfully",
            trackedObjects.Count);

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyTabbedPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        // Swap the window's page to a TabbedPage
        CreateTabbedPage(window, trackedObjects);

        // Allow handlers to connect
        await Task.Delay(100, cancellationToken);

        TearDownTabbedPage(window, trackedObjects);

        // Restore this test page as the window's page
        window.Page = this;
        Content = new Label { Text = "TabbedPage test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateTabbedPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects)
    {
        var tabbedPage = new TabbedPage();
        trackedObjects["TabbedPage"] = new WeakReference<object>(tabbedPage);

        for (int i = 0; i < 3; i++)
        {
            var pageLabel = new Label { Text = $"Tab {i} Content" };
            var pageButton = new Button { Text = $"Tab {i} Action" };
            pageButton.Clicked += (_, _) => { };

            var pageLayout = new VerticalStackLayout
            {
                Children = { pageLabel, pageButton },
            };

            var childPage = new ContentPage
            {
                Title = $"Tab {i}",
                Content = pageLayout,
            };

            trackedObjects[$"Page[{i}]"] = new WeakReference<object>(childPage);
            trackedObjects[$"Page[{i}].Label"] = new WeakReference<object>(pageLabel);
            trackedObjects[$"Page[{i}].Button"] = new WeakReference<object>(pageButton);
            trackedObjects[$"Page[{i}].Layout"] = new WeakReference<object>(pageLayout);

            tabbedPage.Children.Add(childPage);
        }

        window.Page = tabbedPage;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDownTabbedPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects)
    {
        if (window.Page is not TabbedPage tabbedPage)
            return;

        // Track handler before disconnect
        if (tabbedPage.Handler is object handler)
        {
            trackedObjects["TabbedPage.Handler"] = new WeakReference<object>(handler);
        }

        // Disconnect child page handlers
        foreach (var child in tabbedPage.Children)
        {
            if (child is ContentPage contentPage)
            {
                if (contentPage.Content is Layout childLayout)
                {
                    foreach (var layoutChild in childLayout.Children)
                    {
                        if (layoutChild is VisualElement ve)
                        {
                            ve.Handler?.DisconnectHandler();
                        }
                    }

                    childLayout.Children.Clear();
                    childLayout.Handler?.DisconnectHandler();
                }

                contentPage.Handler?.DisconnectHandler();
            }
        }

        tabbedPage.Children.Clear();
        tabbedPage.Handler?.DisconnectHandler();
    }
}
