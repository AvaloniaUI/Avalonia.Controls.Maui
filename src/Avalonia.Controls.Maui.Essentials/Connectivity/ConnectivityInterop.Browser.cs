using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// JavaScript interop bindings for the browser Connectivity API via a JS module.
/// </summary>
[SupportedOSPlatform("browser")]
internal static partial class ConnectivityInterop
{
    const string ModuleName = "AvaloniaConnectivity";
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

    static async Task LoadModuleAsync()
    {
        try
        {
            await JSHost.ImportAsync(ModuleName,
                "../js/connectivity-interop.js")
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

    [JSImport("isOnline", ModuleName)]
    internal static partial bool IsOnline();

    [JSImport("getConnectionType", ModuleName)]
    internal static partial string GetConnectionType();

    [JSImport("subscribe", ModuleName)]
    internal static partial void Subscribe([JSMarshalAs<JSType.Function>] Action callback);

    [JSImport("unsubscribe", ModuleName)]
    internal static partial void Unsubscribe();
}
