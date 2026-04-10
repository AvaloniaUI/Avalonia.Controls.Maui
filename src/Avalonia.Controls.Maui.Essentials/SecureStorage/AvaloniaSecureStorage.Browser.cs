using System.Runtime.Versioning;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Browser implementation of secure storage using Web Crypto API
/// encryption with localStorage persistence and an IndexedDB-backed
/// key. The browser origin remains the primary trust boundary.
/// </summary>
[SupportedOSPlatform("browser")]
public sealed partial class AvaloniaSecureStorage
{
    private partial async Task<string?> PlatformGetAsync(string key)
    {
        return await SecureStorageInterop.GetAsync(key).ConfigureAwait(false);
    }

    private partial async Task PlatformSetAsync(string key, string value)
    {
        await SecureStorageInterop.SetAsync(key, value).ConfigureAwait(false);
    }

    private partial bool PlatformRemove(string key) => SecureStorageInterop.Remove(key);

    private partial void PlatformRemoveAll() => SecureStorageInterop.RemoveAll();
}
