using System.Runtime.CompilerServices;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that WebView control and its handler are collected after disconnect.
/// Covers the mapped WebView surface including events, HtmlWebViewSource base URLs,
/// cookies, and custom user agents before disconnecting the handler.
/// </summary>
[BenchmarkTest("WebViewMiscLeak", Description = "Verifies WebView plus mapped state is collected after disconnect")]
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

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics, 100 * 1024 * 1024) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

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
        var cookies = new CookieContainer();
        cookies.Add(new Cookie("BenchmarkCookie", "Avalonia.Controls.Maui", "/", "example.com"));

        // First WebView exercises HtmlWebViewSource base URLs, cookie sync, and user agent mapping.
        var webView1 = new WebView
        {
            HeightRequest = 200,
            UserAgent = "BenchmarkHtmlWebView/1.0",
            Cookies = cookies,
            Source = new HtmlWebViewSource
            {
                Html = "<html><body><p>Benchmark HTML source.</p></body></html>",
                BaseUrl = "https://example.com/",
            },
        };
        webView1.Navigating += (_, _) => { };
        webView1.Navigated += (_, _) => { };
        layout.Children.Add(webView1);
        trackedObjects["WebView1"] = new WeakReference<object>(webView1);

        // Second WebView keeps URL navigation in the mix while exercising user agent mapping.
        var webView2 = new WebView
        {
            HeightRequest = 200,
            UserAgent = "BenchmarkUrlWebView/1.0",
            Source = new UrlWebViewSource { Url = "about:blank" },
        };
        webView2.Navigating += (_, _) => { };
        webView2.Navigated += (_, _) => { };
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
