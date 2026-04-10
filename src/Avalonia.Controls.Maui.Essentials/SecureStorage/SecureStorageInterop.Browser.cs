using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// JavaScript interop bindings for browser-based secure storage via a JS module.
/// </summary>
[SupportedOSPlatform("browser")]
internal static partial class SecureStorageInterop
{
    const string ModuleName = "AvaloniaSecureStorage";
    const string StoragePrefix = "__avalonia_maui_securestorage_";
    static readonly object ModuleLoadLock = new();
    static bool _moduleLoaded;
    static Task? _moduleLoadTask;

    internal static bool IsModuleLoaded
    {
        get { lock (ModuleLoadLock) { return _moduleLoaded; } }
    }

    internal static Task EnsureModuleLoadedAsync()
    {
        lock (ModuleLoadLock)
        {
            if (_moduleLoaded)
                return Task.CompletedTask;

            _moduleLoadTask ??= LoadModuleAsync();
            return _moduleLoadTask;
        }
    }

    internal static async Task<string?> GetAsync(string key)
    {
        await EnsureModuleLoadedAsync().ConfigureAwait(false);
        return await GetItemAsync(key).ConfigureAwait(false);
    }

    internal static async Task SetAsync(string key, string value)
    {
        await EnsureModuleLoadedAsync().ConfigureAwait(false);
        await SetItemAsync(key, value).ConfigureAwait(false);
    }

    internal static bool Remove(string key)
    {
        var storageKey = BuildStorageKey(key);
        if (LocalStorageInterop.GetItem(storageKey) is null)
            return false;

        LocalStorageInterop.RemoveItem(storageKey);
        return true;
    }

    internal static void RemoveAll()
    {
        for (var i = LocalStorageInterop.GetLength() - 1; i >= 0; i--)
        {
            var key = LocalStorageInterop.GetKey(i);
            if (key?.StartsWith(StoragePrefix, StringComparison.Ordinal) == true)
                LocalStorageInterop.RemoveItem(key);
        }
    }

    static async Task LoadModuleAsync()
    {
        try
        {
            await JSHost.ImportAsync(ModuleName,
                "../js/securestorage-interop.js")
                .ConfigureAwait(false);

            lock (ModuleLoadLock)
            {
                _moduleLoaded = true;
            }
        }
        catch
        {
            lock (ModuleLoadLock)
            {
                _moduleLoadTask = null;
            }

            throw;
        }
    }

    static string BuildStorageKey(string key) => StoragePrefix + key;

    [JSImport("getItem", ModuleName)]
    internal static partial Task<string?> GetItemAsync(string key);

    [JSImport("setItem", ModuleName)]
    internal static partial Task SetItemAsync(string key, string value);
}
