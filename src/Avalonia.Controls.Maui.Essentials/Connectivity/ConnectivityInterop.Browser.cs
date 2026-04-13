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
    const string ModuleSource = """
        let _callback = null;

        export function isOnline() {
            return globalThis.navigator.onLine;
        }

        export function getConnectionType() {
            const conn = getConnection();
            return conn?.type ?? "unknown";
        }

        export function subscribe(callback) {
            _callback = callback;
            globalThis.addEventListener("online", onStatusChange);
            globalThis.addEventListener("offline", onStatusChange);

            const conn = getConnection();
            if (conn) {
                conn.addEventListener("change", onStatusChange);
            }
        }

        export function unsubscribe() {
            globalThis.removeEventListener("online", onStatusChange);
            globalThis.removeEventListener("offline", onStatusChange);

            const conn = getConnection();
            if (conn) {
                conn.removeEventListener("change", onStatusChange);
            }
            _callback = null;
        }

        function onStatusChange() {
            if (_callback) {
                _callback();
            }
        }

        function getConnection() {
            return globalThis.navigator.connection ||
                globalThis.navigator.mozConnection ||
                globalThis.navigator.webkitConnection;
        }
        """;
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
            var moduleUrl = "data:text/javascript;charset=utf-8," +
                System.Uri.EscapeDataString(ModuleSource);

            await JSHost.ImportAsync(ModuleName,
                moduleUrl)
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
