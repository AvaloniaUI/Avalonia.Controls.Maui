using Avalonia.Controls;
using Microsoft.AspNetCore.Components.WebView;

namespace Avalonia.Controls.Maui.BlazorWebView;

/// <summary>
/// Provides access to the Avalonia WebView before its native adapter is initialized.
/// </summary>
public sealed class AvaloniaBlazorWebViewInitializingEventArgs : BlazorWebViewInitializingEventArgs
{
    internal AvaloniaBlazorWebViewInitializingEventArgs(NativeWebView nativeWebView)
    {
        WebView = nativeWebView;
    }

    /// <summary>
    /// Gets the Avalonia WebView that will host Blazor content.
    /// </summary>
    public NativeWebView WebView { get; }
}

/// <summary>
/// Provides access to the Avalonia WebView after its native adapter is initialized.
/// </summary>
public sealed class AvaloniaBlazorWebViewInitializedEventArgs : BlazorWebViewInitializedEventArgs
{
    internal AvaloniaBlazorWebViewInitializedEventArgs(NativeWebView nativeWebView)
    {
        WebView = nativeWebView;
    }

    /// <summary>
    /// Gets the initialized Avalonia WebView.
    /// </summary>
    public NativeWebView WebView { get; }
}
