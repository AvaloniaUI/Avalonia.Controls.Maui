

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

    /// <summary>
    /// Removes the value associated with the given key from <c>localStorage</c>.
    /// </summary>
    /// <param name="key">The localStorage key.</param>
    [JSImport("globalThis.localStorage.removeItem")]
    internal static partial void RemoveItem(string key);

    /// <summary>
    /// Reads the key at a given index from <c>localStorage</c>.
    /// </summary>
    /// <param name="index">The zero-based key index.</param>
    /// <returns>The localStorage key, or <see langword="null"/> if the index is out of range.</returns>
    [JSImport("globalThis.localStorage.key")]
    internal static partial string? GetKey(int index);

    /// <summary>
    /// Gets the current number of items stored in <c>localStorage</c>.
    /// </summary>
    /// <returns>The number of items stored.</returns>
    [JSImport("globalThis.localStorage.length")]
    internal static partial int GetLength();
}
