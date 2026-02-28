using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that Shell SearchHandler objects and their associated ShellSearchControl are collected
/// after the Shell is disconnected. SearchHandler subscribes to PropertyChanged and focus events
/// which must be properly cleaned up.
/// </summary>
[BenchmarkTest("ShellSearchHandlerLeak", Description = "Verifies Shell SearchHandler is collected after Shell disconnect")]
public class ShellSearchHandlerLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyShellWithSearch(window, trackedObjects, cancellationToken);

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
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["Shell.Leaked"] = leaked.Any(n => n.StartsWith("Shell")),
            ["SearchHandler.Leaked"] = leaked.Any(n => n.Contains("SearchHandler")),
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Shell SearchHandler leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} Shell SearchHandler objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyShellWithSearch(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        CreateShellWithSearch(window, trackedObjects);

        // Allow handlers to connect
        await Task.Delay(100, cancellationToken);

        // Exercise the search handler by changing its query
        if (window.Page is Shell shell)
        {
            // Navigate the Shell hierarchy: ShellItem -> ShellSection -> ShellContent
            foreach (var shellItem in shell.Items)
            {
                foreach (var section in shellItem.Items)
                {
                    foreach (var sc in section.Items)
                    {
                        if (sc.Content is ContentPage page)
                        {
                            var searchHandler = Shell.GetSearchHandler(page);
                            if (searchHandler != null)
                            {
                                searchHandler.Query = "test query";
                                await Task.Delay(30, cancellationToken);
                                searchHandler.Query = "another query";
                                await Task.Delay(30, cancellationToken);
                                searchHandler.Query = string.Empty;
                            }
                        }
                    }
                }
            }
        }

        TearDownShell(window, trackedObjects);

        // Restore test page
        window.Page = this;
        Content = new Label { Text = "Shell SearchHandler test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateShellWithSearch(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects)
    {
        var searchHandler = new SearchHandler
        {
            Placeholder = "Search...",
            ShowsResults = true,
            IsSearchEnabled = true,
        };
        trackedObjects["SearchHandler"] = new WeakReference<object>(searchHandler);

        var searchPage = new ContentPage
        {
            Title = "Searchable Page",
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = "Page with SearchHandler" },
                    new Label { Text = "Content below search" },
                },
            },
        };
        Shell.SetSearchHandler(searchPage, searchHandler);
        trackedObjects["Shell.SearchPage"] = new WeakReference<object>(searchPage);

        var shell = new Shell();
        shell.Items.Add(new ShellContent
        {
            Title = "Search",
            Content = searchPage,
        });

        trackedObjects["Shell"] = new WeakReference<object>(shell);

        window.Page = shell;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDownShell(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects)
    {
        if (window.Page is not Shell shell)
            return;

        if (shell.Handler is object shellHandler)
        {
            trackedObjects["Shell.Handler"] = new WeakReference<object>(shellHandler);
        }

        // Clear search handler from pages before disconnect
        foreach (var shellItem in shell.Items)
        {
            foreach (var section in shellItem.Items)
            {
                foreach (var sc in section.Items)
                {
                    if (sc.Content is ContentPage page)
                    {
                        Shell.SetSearchHandler(page, null);
                        page.Handler?.DisconnectHandler();
                    }
                }
            }
        }

        shell.Handler?.DisconnectHandler();
    }
}
