using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using Microsoft.Maui.Networking;

namespace Avalonia.Controls.Maui.Essentials;

[UnsupportedOSPlatform("browser")]
internal static class LinuxConnectivityHelper
{
    const string ProcNetRoutePath = "/proc/net/route";
    const string ProcNetIpv6RoutePath = "/proc/net/ipv6_route";
    const string SysClassNetPath = "/sys/class/net";

    internal static bool TryGetNetworkAccess(out NetworkAccess access)
    {
        access = NetworkAccess.Unknown;

        if (!OperatingSystem.IsLinux())
            return false;

        try
        {
            var activeInterfaces = GetActiveInterfaces().ToArray();
            access = DetermineNetworkAccess(
                activeInterfaces.Select(interfaceInfo => interfaceInfo.Name),
                GetDefaultRouteInterfaces());
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    internal static bool TryGetConnectionProfiles(out IEnumerable<ConnectionProfile> profiles)
    {
        profiles = Array.Empty<ConnectionProfile>();

        if (!OperatingSystem.IsLinux())
            return false;

        try
        {
            var results = GetActiveInterfaces()
                .Select(interfaceInfo => GetConnectionProfile(interfaceInfo, SysClassNetPath))
                .Distinct()
                .ToArray();

            profiles = results.Length > 0 ? results : [ConnectionProfile.Unknown];
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    internal static NetworkAccess DetermineNetworkAccess(IEnumerable<string> activeInterfaces, ISet<string> defaultRouteInterfaces)
    {
        var active = activeInterfaces
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (active.Length == 0)
            return NetworkAccess.None;

        return active.Any(defaultRouteInterfaces.Contains)
            ? NetworkAccess.Internet
            : NetworkAccess.Local;
    }

    internal static HashSet<string> GetDefaultRouteInterfaces()
    {
        var interfaces = new HashSet<string>(StringComparer.Ordinal);

        if (File.Exists(ProcNetRoutePath))
            MergeDefaultRouteInterfaces(File.ReadAllLines(ProcNetRoutePath), interfaces);

        if (File.Exists(ProcNetIpv6RoutePath))
            MergeDefaultIpv6RouteInterfaces(File.ReadAllLines(ProcNetIpv6RoutePath), interfaces);

        return interfaces;
    }

    internal static void MergeDefaultRouteInterfaces(IEnumerable<string> routeLines, ISet<string> interfaces)
    {
        foreach (var line in routeLines.Skip(1))
        {
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
                continue;

            if (!parts[1].Equals("00000000", StringComparison.Ordinal))
                continue;

            interfaces.Add(parts[0]);
        }
    }

    internal static void MergeDefaultIpv6RouteInterfaces(IEnumerable<string> routeLines, ISet<string> interfaces)
    {
        foreach (var line in routeLines)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 10)
                continue;

            if (!parts[0].Equals(new string('0', 32), StringComparison.Ordinal) ||
                !parts[1].Equals("00000000", StringComparison.Ordinal))
            {
                continue;
            }

            interfaces.Add(parts[^1]);
        }
    }

    internal static ConnectionProfile GetConnectionProfile(LinuxInterfaceInfo interfaceInfo, string sysClassNetPath)
    {
        var interfaceRoot = Path.Combine(sysClassNetPath, interfaceInfo.Name);

        if (Directory.Exists(Path.Combine(interfaceRoot, "wireless")))
            return ConnectionProfile.WiFi;

        if (interfaceInfo.Name.StartsWith("bnep", StringComparison.OrdinalIgnoreCase))
            return ConnectionProfile.Bluetooth;

        if (interfaceInfo.Name.StartsWith("wwan", StringComparison.OrdinalIgnoreCase) ||
            interfaceInfo.Name.StartsWith("wwp", StringComparison.OrdinalIgnoreCase) ||
            interfaceInfo.Name.StartsWith("rmnet", StringComparison.OrdinalIgnoreCase))
        {
            return ConnectionProfile.Cellular;
        }

        return interfaceInfo.Type switch
        {
            NetworkInterfaceType.Wireless80211 => ConnectionProfile.WiFi,
            NetworkInterfaceType.Ethernet => ConnectionProfile.Ethernet,
            NetworkInterfaceType.GigabitEthernet => ConnectionProfile.Ethernet,
            NetworkInterfaceType.FastEthernetT => ConnectionProfile.Ethernet,
            NetworkInterfaceType.FastEthernetFx => ConnectionProfile.Ethernet,
            NetworkInterfaceType.Ethernet3Megabit => ConnectionProfile.Ethernet,
            NetworkInterfaceType.Ppp => ConnectionProfile.Cellular,
            NetworkInterfaceType.Wwanpp => ConnectionProfile.Cellular,
            NetworkInterfaceType.Wwanpp2 => ConnectionProfile.Cellular,
            _ => ConnectionProfile.Unknown
        };
    }

    static IEnumerable<LinuxInterfaceInfo> GetActiveInterfaces()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
            .Where(ni => ni.NetworkInterfaceType is not NetworkInterfaceType.Loopback and not NetworkInterfaceType.Tunnel)
            .Select(ni => new LinuxInterfaceInfo(ni.Name, ni.NetworkInterfaceType));
    }

    internal readonly record struct LinuxInterfaceInfo(string Name, NetworkInterfaceType Type);
}
