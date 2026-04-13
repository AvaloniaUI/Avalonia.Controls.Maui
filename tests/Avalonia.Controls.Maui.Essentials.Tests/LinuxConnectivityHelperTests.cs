using System.Net.NetworkInformation;
using Avalonia.Controls.Maui.Essentials;
using Microsoft.Maui.Networking;

namespace Avalonia.Controls.Maui.Tests.Services;

public class LinuxConnectivityHelperTests
{
    [Fact]
    public void DetermineNetworkAccess_ReturnsNone_WhenNoActiveInterfaces()
    {
        var access = LinuxConnectivityHelper.DetermineNetworkAccess([], new HashSet<string>(StringComparer.Ordinal));

        Assert.Equal(NetworkAccess.None, access);
    }

    [Fact]
    public void DetermineNetworkAccess_ReturnsLocal_WhenNoDefaultRouteMatches()
    {
        var access = LinuxConnectivityHelper.DetermineNetworkAccess(
            ["eth0"],
            new HashSet<string>(StringComparer.Ordinal));

        Assert.Equal(NetworkAccess.Local, access);
    }

    [Fact]
    public void DetermineNetworkAccess_ReturnsInternet_WhenDefaultRouteMatchesActiveInterface()
    {
        var access = LinuxConnectivityHelper.DetermineNetworkAccess(
            ["eth0", "wlan0"],
            new HashSet<string>(["wlan0"], StringComparer.Ordinal));

        Assert.Equal(NetworkAccess.Internet, access);
    }

    [Fact]
    public void DetermineNetworkAccess_IgnoresBlankAndDuplicateInterfaceNames()
    {
        var access = LinuxConnectivityHelper.DetermineNetworkAccess(
            ["", "eth0", "eth0", " "],
            new HashSet<string>(["eth0"], StringComparer.Ordinal));

        Assert.Equal(NetworkAccess.Internet, access);
    }

    [Fact]
    public void MergeDefaultRouteInterfaces_AddsIpv4DefaultRouteInterfaces()
    {
        var interfaces = new HashSet<string>(StringComparer.Ordinal);

        LinuxConnectivityHelper.MergeDefaultRouteInterfaces(
            [
                "Iface\tDestination\tGateway\tFlags\tRefCnt\tUse\tMetric\tMask\tMTU\tWindow\tIRTT",
                "wlan0\t00000000\t0101A8C0\t0003\t0\t0\t600\t00000000\t0\t0\t0",
                "eth0\t0008A8C0\t00000000\t0001\t0\t0\t100\t00FFFFFF\t0\t0\t0"
            ],
            interfaces);

        Assert.Equal(["wlan0"], interfaces);
    }

    [Fact]
    public void MergeDefaultRouteInterfaces_SupportsWhitespaceSeparatedLines()
    {
        var interfaces = new HashSet<string>(StringComparer.Ordinal);

        LinuxConnectivityHelper.MergeDefaultRouteInterfaces(
            [
                "Iface Destination Gateway Flags RefCnt Use Metric Mask MTU Window IRTT",
                "eth0 00000000 0101A8C0 0003 0 0 100 00000000 0 0 0"
            ],
            interfaces);

        Assert.Equal(["eth0"], interfaces);
    }

    [Fact]
    public void MergeDefaultIpv6RouteInterfaces_AddsIpv6DefaultRouteInterfaces()
    {
        var interfaces = new HashSet<string>(StringComparer.Ordinal);

        LinuxConnectivityHelper.MergeDefaultIpv6RouteInterfaces(
            [
                "00000000000000000000000000000000 00000000 00000000000000000000000000000000 00000000 00000000000000000000000000000000 00000400 00000000 00000000 00000001 eth0",
                "20010DB8000000000000000000000000 00000040 00000000000000000000000000000000 00000000 00000000000000000000000000000000 00000100 00000000 00000000 00000001 wlan0"
            ],
            interfaces);

        Assert.Equal(["eth0"], interfaces);
    }

    [Fact]
    public void GetConnectionProfile_ReturnsWiFi_WhenWirelessDirectoryExists()
    {
        using var sysfs = new TempDirectory();
        Directory.CreateDirectory(Path.Combine(sysfs.Path, "wlan0", "wireless"));

        var profile = LinuxConnectivityHelper.GetConnectionProfile(
            new LinuxConnectivityHelper.LinuxInterfaceInfo("wlan0", NetworkInterfaceType.Ethernet),
            sysfs.Path);

        Assert.Equal(ConnectionProfile.WiFi, profile);
    }

    [Theory]
    [InlineData("bnep0", NetworkInterfaceType.Unknown, ConnectionProfile.Bluetooth)]
    [InlineData("wwan0", NetworkInterfaceType.Unknown, ConnectionProfile.Cellular)]
    [InlineData("ppp0", NetworkInterfaceType.Ppp, ConnectionProfile.Cellular)]
    [InlineData("eth0", NetworkInterfaceType.Ethernet, ConnectionProfile.Ethernet)]
    [InlineData("wlan0", NetworkInterfaceType.Wireless80211, ConnectionProfile.WiFi)]
    [InlineData("mystery0", NetworkInterfaceType.Unknown, ConnectionProfile.Unknown)]
    public void GetConnectionProfile_UsesLinuxHintsAndInterfaceType(
        string interfaceName,
        NetworkInterfaceType interfaceType,
        ConnectionProfile expected)
    {
        using var sysfs = new TempDirectory();
        Directory.CreateDirectory(Path.Combine(sysfs.Path, interfaceName));

        var profile = LinuxConnectivityHelper.GetConnectionProfile(
            new LinuxConnectivityHelper.LinuxInterfaceInfo(interfaceName, interfaceType),
            sysfs.Path);

        Assert.Equal(expected, profile);
    }

    sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
