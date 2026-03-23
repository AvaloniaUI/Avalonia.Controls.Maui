using System.Runtime.InteropServices.JavaScript;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Provides JavaScript interop bindings to the browser's <c>navigator.clipboard</c> API via <c>[JSImport]</c>.
/// </summary>
internal static partial class ClipboardInterop
{
    /// <summary>
    /// Reads text from the browser clipboard using <c>navigator.clipboard.readText()</c>.
    /// </summary>
    /// <returns>The text content on the clipboard, or <see langword="null"/> if unavailable.</returns>
    [JSImport("globalThis.navigator.clipboard.readText")]
    internal static partial Task<string?> ReadTextAsync();

    /// <summary>
    /// Writes text to the browser clipboard using <c>navigator.clipboard.writeText()</c>.
    /// </summary>
    /// <param name="text">The text to write to the clipboard.</param>
    [JSImport("globalThis.navigator.clipboard.writeText")]
    internal static partial Task WriteTextAsync(string text);
}
