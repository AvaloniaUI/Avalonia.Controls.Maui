using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that WebView control and its handler are collected after disconnect.
/// WebViewHandler uses a placeholder control and has navigation event subscriptions.
/// </summary>
[BenchmarkTest("WebViewMiscLeak", Description = "Verifies WebView is collected after disconnect")]
public class WebViewMiscLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyWebViews(trackedObjects, cancellationToken);

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
            ["ControlsTested"] = 2,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["WebView.Leaked"] = leaked.Any(n => n.StartsWith("WebView")),
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("WebView leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} WebView objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyWebViews(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        CreateWebViews(trackedObjects, layout);

        // Allow handlers to connect
        await Task.Delay(50, cancellationToken);

        // Track handlers before disconnect
        TrackHandlers(trackedObjects, layout);

        // Tear down
        TearDown(layout);

        Content = new Label { Text = "WebView test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateWebViews(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        // First WebView with HTML source
        var webView1 = new WebView
        {
            HeightRequest = 200,
            Source = new HtmlWebViewSource { Html = "<html><body><p>Test</p></body></html>" },
        };
        webView1.Navigating += (_, _) => { };
        webView1.Navigated += (_, _) => { };
        layout.Children.Add(webView1);
        trackedObjects["WebView1"] = new WeakReference<object>(webView1);

        // Second WebView with URL source
        var webView2 = new WebView
        {
            HeightRequest = 200,
            Source = new UrlWebViewSource { Url = "about:blank" },
        };
        layout.Children.Add(webView2);
        trackedObjects["WebView2"] = new WeakReference<object>(webView2);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackHandlers(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        int index = 1;
        foreach (var child in layout.Children)
        {
            if (child is WebView wv && wv.Handler is object handler)
            {
                trackedObjects[$"WebView{index}.Handler"] = new WeakReference<object>(handler);
            }

            index++;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDown(VerticalStackLayout layout)
    {
        foreach (var child in layout.Children)
        {
            if (child is WebView wv)
            {
                wv.Source = null;
                wv.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
    }
}
