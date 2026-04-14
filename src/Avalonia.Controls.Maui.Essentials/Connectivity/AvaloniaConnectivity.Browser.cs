using System.Runtime.Versioning;
using Microsoft.Maui.Networking;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Browser implementation of connectivity monitoring using the Navigator Online API
/// and the Network Information API.
/// </summary>
[SupportedOSPlatform("browser")]
public sealed partial class AvaloniaConnectivity
{
    int _browserSubscriptionVersion;
    bool _browserSubscribed;

    private partial NetworkAccess PlatformGetNetworkAccess()
    {
        EnsureBrowserModuleLoaded();

        if (!ConnectivityInterop.IsModuleLoaded)
            return NetworkAccess.Unknown;

        // Browser online/offline is a best-effort signal. Treat online as Internet for
        // compatibility with IConnectivity expectations, but it does not prove external reachability.
        return ConnectivityInterop.IsOnline()
            ? NetworkAccess.Internet
            : NetworkAccess.None;
    }

    private partial IEnumerable<ConnectionProfile> PlatformGetConnectionProfiles()
    {
        EnsureBrowserModuleLoaded();

        if (!ConnectivityInterop.IsModuleLoaded)
            return [ConnectionProfile.Unknown];

        var type = ConnectivityInterop.GetConnectionType();
        var profile = type switch
        {
            "wifi" => ConnectionProfile.WiFi,
            "ethernet" => ConnectionProfile.Ethernet,
            "cellular" => ConnectionProfile.Cellular,
            "bluetooth" => ConnectionProfile.Bluetooth,
            _ => ConnectionProfile.Unknown
        };

        return [profile];
    }

    private partial void PlatformInitialize()
    {
        EnsureBrowserModuleLoaded();
    }

    private partial void PlatformStartListening()
    {
        var version = Interlocked.Increment(ref _browserSubscriptionVersion);
        _ = StartBrowserListeningAsync(version);
    }

    private partial void PlatformStopListening()
    {
        Interlocked.Increment(ref _browserSubscriptionVersion);

        lock (_eventLock)
        {
            if (ConnectivityInterop.IsModuleLoaded && _browserSubscribed)
            {
                ConnectivityInterop.Unsubscribe();
                _browserSubscribed = false;
            }
        }
    }

    void EnsureBrowserModuleLoaded()
    {
        _ = ConnectivityInterop.EnsureModuleLoadedAsync();
    }

    async Task StartBrowserListeningAsync(int version)
    {
        await ConnectivityInterop.EnsureModuleLoadedAsync().ConfigureAwait(false);

        lock (_eventLock)
        {
            if (!_isListening || version != _browserSubscriptionVersion)
                return;

            if (_browserSubscribed)
                ConnectivityInterop.Unsubscribe();

            ConnectivityInterop.Subscribe(RaiseConnectivityChanged);
            _browserSubscribed = true;
        }
        RaiseConnectivityChanged();
    }
}
