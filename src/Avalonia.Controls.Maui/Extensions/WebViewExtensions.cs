using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>Extension methods for mapping MAUI WebView properties to Avalonia's NativeWebView.</summary>
internal static class WebViewExtensions
{
    /// <summary>Updates the platform WebView user agent from the virtual view.</summary>
    /// <param name="platformView">The Avalonia NativeWebView.</param>
    /// <param name="webView">The MAUI virtual view.</param>
    public static void UpdateUserAgent(this Avalonia.Controls.NativeWebView platformView, IWebView webView)
    {
        platformView.UserAgent = webView.UserAgent;
    }

    /// <summary>Updates the platform WebView user agent directly.</summary>
    /// <param name="platformView">The Avalonia NativeWebView.</param>
    /// <param name="userAgent">The desired user agent string.</param>
    public static void UpdateUserAgent(this Avalonia.Controls.NativeWebView platformView, string? userAgent)
    {
        platformView.UserAgent = userAgent;
    }

    /// <summary>Loads the provided source into the platform WebView and updates navigation state.</summary>
    /// <param name="platformView">The Avalonia NativeWebView.</param>
    /// <param name="webView">The MAUI virtual view.</param>
    /// <param name="webViewDelegate">The delegate used to translate MAUI sources to platform navigation.</param>
    /// <param name="source">The source to load.</param>
    public static void UpdateSource(this Avalonia.Controls.NativeWebView platformView, IWebView webView, IWebViewDelegate webViewDelegate, IWebViewSource? source)
    {
        source?.Load(webViewDelegate);
        platformView.UpdateCanGoBackForward(webView);
    }

    /// <summary>Updates the CanGoBack and CanGoForward properties on the virtual view from the platform view.</summary>
    /// <param name="platformView">The Avalonia NativeWebView.</param>
    /// <param name="webView">The MAUI virtual view.</param>
    public static void UpdateCanGoBackForward(this Avalonia.Controls.NativeWebView platformView, IWebView webView)
    {
        webView.CanGoBack = platformView.CanGoBack;
        webView.CanGoForward = platformView.CanGoForward;
    }

    /// <summary>Gets the current absolute source URI if the WebView is currently on an absolute URL.</summary>
    /// <param name="platformView">The Avalonia NativeWebView.</param>
    /// <returns>The current absolute URI, or <c>null</c> when unavailable.</returns>
    public static Uri? GetAbsoluteSourceUri(this Avalonia.Controls.NativeWebView platformView)
    {
        var source = platformView.Source;
        return source is { IsAbsoluteUri: true } ? source : null;
    }

    /// <summary>Executes JavaScript without waiting for the result.</summary>
    /// <param name="platformView">The Avalonia NativeWebView.</param>
    /// <param name="script">The script to execute.</param>
    public static void Eval(this Avalonia.Controls.NativeWebView platformView, string script)
    {
        _ = platformView.InvokeScript(script);
    }
}
