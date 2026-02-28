

using System.Runtime.InteropServices.JavaScript;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Provides JavaScript interop bindings to the browser's <c>localStorage</c> API via <c>[JSImport]</c>.
/// </summary>
internal static partial class LocalStorageInterop
{
    /// <summary>
    /// Reads the value associated with the given key from <c>localStorage</c>.
    /// </summary>
    /// <param name="key">The localStorage key.</param>
    /// <returns>The stored value, or <see langword="null"/> if the key does not exist.</returns>
    [JSImport("globalThis.localStorage.getItem")]
    internal static partial string? GetItem(string key);

    /// <summary>
    /// Writes a key/value pair to <c>localStorage</c>.
    /// </summary>
    /// <param name="key">The localStorage key.</param>
    /// <param name="value">The value to store.</param>
    [JSImport("globalThis.localStorage.setItem")]
    internal static partial void SetItem(string key, string value);
}
