using Microsoft.Maui.Storage;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of <see cref="ISecureStorage"/> with platform-specific
/// backends for desktop (AES-GCM file encryption) and browser (Web Crypto API with
/// an IndexedDB-backed key).
/// </summary>
public sealed partial class AvaloniaSecureStorage : ISecureStorage
{
    /// <inheritdoc/>
    public Task<string?> GetAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        return PlatformGetAsync(key);
    }

    /// <inheritdoc/>
    public Task SetAsync(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return PlatformSetAsync(key, value);
    }

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        return PlatformRemove(key);
    }

    /// <inheritdoc/>
    public void RemoveAll() => PlatformRemoveAll();

    private partial Task<string?> PlatformGetAsync(string key);
    private partial Task PlatformSetAsync(string key, string value);
    private partial bool PlatformRemove(string key);
    private partial void PlatformRemoveAll();
}
