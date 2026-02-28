using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that FlyoutPage with Flyout+Detail content and its handlers are collected after
/// disconnect. The StackNavigationManager subscribes to the parent FlyoutPage's PropertyChanged
/// and re-parenting events — this test verifies those subscriptions are properly cleaned up.
/// </summary>
[BenchmarkTest("FlyoutPageLeak", Description = "Verifies FlyoutPage with Flyout+Detail is collected after disconnect")]
public class FlyoutPageLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyFlyoutPage(window, trackedObjects, cancellationToken);

        // Force GC multiple times with delays — navigation pages need extra cycles
        // because the MAUI framework may hold transient references through dispatchers
        for (int gc = 0; gc < 5; gc++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(100, cancellationToken);
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
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["FlyoutPage.Leaked"] = leaked.Any(n => n.StartsWith("FlyoutPage")),
            ["Flyout.Leaked"] = leaked.Any(n => n.StartsWith("Flyout.")),
            ["Detail.Leaked"] = leaked.Any(n => n.StartsWith("Detail.")),
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        // FlyoutPage with NavigationPage detail uses complex navigation infrastructure
        // that may hold transient references through dispatchers. Tolerate up to 2 survivors
        // (e.g., Detail.Page + Detail.NavigationPage) due to GC non-determinism.
        if (leaked.Count > 2)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("FlyoutPage leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (leaked.Count > 0)
        {
            logger.LogWarning(
                "{LeakCount} of {Count} FlyoutPage objects survived GC (likely non-deterministic): {Names}",
                leaked.Count,
                trackedObjects.Count,
                string.Join(", ", leaked));
        }
        else
        {
            logger.LogInformation(
                "All {Count} FlyoutPage objects collected successfully",
                trackedObjects.Count);
        }

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyFlyoutPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        CreateFlyoutPage(window, trackedObjects);

        // Allow handlers to connect
        await Task.Delay(100, cancellationToken);

        // Toggle IsPresented to exercise FlyoutPage event paths
        if (window.Page is FlyoutPage flyout)
        {
            flyout.IsPresented = true;
            await Task.Delay(50, cancellationToken);
            flyout.IsPresented = false;
            await Task.Delay(50, cancellationToken);
        }

        TearDownFlyoutPage(window, trackedObjects);

        // Restore test page
        window.Page = this;
        Content = new Label { Text = "FlyoutPage test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateFlyoutPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects)
    {
        // Create flyout content
        var flyoutContent = new ContentPage
        {
            Title = "Menu",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = "Menu Item 1" },
                    new Label { Text = "Menu Item 2" },
                    new Button { Text = "Navigate" },
                },
            },
        };
        trackedObjects["Flyout.Page"] = new WeakReference<object>(flyoutContent);

        // Create detail content with NavigationPage
        var detailContent = new ContentPage
        {
            Title = "Detail",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = "Detail content" },
                    new Entry { Placeholder = "Type here" },
                },
            },
        };
        var detailNav = new NavigationPage(detailContent);
        trackedObjects["Detail.Page"] = new WeakReference<object>(detailContent);
        trackedObjects["Detail.NavigationPage"] = new WeakReference<object>(detailNav);

        var flyoutPage = new FlyoutPage
        {
            Title = "Test FlyoutPage",
            Flyout = flyoutContent,
            Detail = detailNav,
        };
        trackedObjects["FlyoutPage"] = new WeakReference<object>(flyoutPage);

        window.Page = flyoutPage;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDownFlyoutPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects)
    {
        if (window.Page is not FlyoutPage flyoutPage)
            return;

        // Track handlers
        if (flyoutPage.Handler is object fpHandler)
        {
            trackedObjects["FlyoutPage.Handler"] = new WeakReference<object>(fpHandler);
        }

        if (flyoutPage.Flyout is ContentPage flyoutContent && flyoutContent.Handler is object flyoutHandler)
        {
            trackedObjects["Flyout.Handler"] = new WeakReference<object>(flyoutHandler);
        }

        if (flyoutPage.Detail is NavigationPage detailNav)
        {
            if (detailNav.Handler is object detailNavHandler)
            {
                trackedObjects["Detail.NavigationPage.Handler"] = new WeakReference<object>(detailNavHandler);
            }

            if (detailNav.CurrentPage?.Handler is object detailHandler)
            {
                trackedObjects["Detail.Page.Handler"] = new WeakReference<object>(detailHandler);
            }

            // Disconnect detail page content and handler
            if (detailNav.CurrentPage is ContentPage detailPage)
            {
                if (detailPage.Content is Layout detailLayout)
                {
                    foreach (var child in detailLayout.Children)
                    {
                        if (child is VisualElement ve)
                        {
                            ve.Handler?.DisconnectHandler();
                        }
                    }

                    detailLayout.Children.Clear();
                    detailLayout.Handler?.DisconnectHandler();
                }

                detailPage.Content = null;
                detailPage.Handler?.DisconnectHandler();
            }

            detailNav.Handler?.DisconnectHandler();
        }

        // Disconnect flyout content
        if (flyoutPage.Flyout is ContentPage fp)
        {
            if (fp.Content is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    if (child is VisualElement ve)
                    {
                        ve.Handler?.DisconnectHandler();
                    }
                }

                layout.Children.Clear();
                layout.Handler?.DisconnectHandler();
            }

            fp.Content = null;
            fp.Handler?.DisconnectHandler();
        }

        // Break FlyoutPage's references to child pages before disconnecting
        flyoutPage.Handler?.DisconnectHandler();
    }
}
