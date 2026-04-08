using System.Net.NetworkInformation;
using Microsoft.Maui.Networking;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Desktop implementation of connectivity monitoring using <see cref="System.Net.NetworkInformation"/>.
/// </summary>
public sealed partial class AvaloniaConnectivity
{
    private partial NetworkAccess PlatformGetNetworkAccess()
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
            return NetworkAccess.None;

        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var ni in interfaces)
        {
            if (ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
            {
                return NetworkAccess.Internet;
            }
        }

        return NetworkAccess.None;
    }

    private partial IEnumerable<ConnectionProfile> PlatformGetConnectionProfiles()
    {
        var profiles = new HashSet<ConnectionProfile>();
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var ni in interfaces)
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            if (ni.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
                continue;

            var profile = ni.NetworkInterfaceType switch
            {
                NetworkInterfaceType.Wireless80211 => ConnectionProfile.WiFi,
                NetworkInterfaceType.Ethernet => ConnectionProfile.Ethernet,
                NetworkInterfaceType.GigabitEthernet => ConnectionProfile.Ethernet,
                NetworkInterfaceType.FastEthernetT => ConnectionProfile.Ethernet,
                NetworkInterfaceType.FastEthernetFx => ConnectionProfile.Ethernet,
                NetworkInterfaceType.Ethernet3Megabit => ConnectionProfile.Ethernet,
                NetworkInterfaceType.Wwanpp => ConnectionProfile.Cellular,
                NetworkInterfaceType.Wwanpp2 => ConnectionProfile.Cellular,
                _ => ConnectionProfile.Unknown
            };

            profiles.Add(profile);
        }

        return profiles;
    }

    private partial void PlatformStartListening()
    {
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
    }

    private partial void PlatformStopListening()
    {
        NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
        NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
    }

    void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e) =>
        RaiseConnectivityChanged();

    void OnNetworkAddressChanged(object? sender, EventArgs e) =>
        RaiseConnectivityChanged();
}
