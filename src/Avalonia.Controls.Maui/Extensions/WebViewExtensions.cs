using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>Extension methods for mapping MAUI WebView properties to Avalonia's NativeWebView.</summary>
internal static class WebViewExtensions
{
    /// <summary>Updates the CanGoBack and CanGoForward properties on the virtual view from the platform view.</summary>
    /// <param name="platformView">The Avalonia NativeWebView.</param>
    /// <param name="webView">The MAUI virtual view.</param>
    public static void UpdateCanGoBackForward(this Avalonia.Controls.NativeWebView platformView, IWebView webView)
    {
        webView.CanGoBack = platformView.CanGoBack;
        webView.CanGoForward = platformView.CanGoForward;
    }
}
