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
    private partial NetworkAccess PlatformGetNetworkAccess()
    {
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

    private partial void PlatformStartListening()
    {
        _ = StartBrowserListeningAsync();
    }

    private partial void PlatformStopListening()
    {
        if (ConnectivityInterop.IsModuleLoaded)
            ConnectivityInterop.Unsubscribe();
    }

    async Task StartBrowserListeningAsync()
    {
        await ConnectivityInterop.EnsureModuleLoadedAsync().ConfigureAwait(false);
        ConnectivityInterop.Subscribe(RaiseConnectivityChanged);
        RaiseConnectivityChanged();
    }
}
