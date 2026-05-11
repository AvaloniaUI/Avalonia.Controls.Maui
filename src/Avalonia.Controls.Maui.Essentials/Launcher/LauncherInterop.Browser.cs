using System.Runtime.InteropServices.JavaScript;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Provides JavaScript interop bindings for opening URIs in the browser.
/// </summary>
internal static partial class LauncherInterop
{
    /// <summary>
    /// Opens a URL in a new browser window/tab using <c>window.open()</c>.
    /// Returns the window object, or <see langword="null"/> if blocked by a popup blocker.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    /// <param name="target">The target window name (e.g., "_blank").</param>
    [JSImport("globalThis.window.open")]
    internal static partial JSObject? WindowOpen(string url, string target);
}
