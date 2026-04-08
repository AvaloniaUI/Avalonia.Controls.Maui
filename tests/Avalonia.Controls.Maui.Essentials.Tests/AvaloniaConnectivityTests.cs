using Avalonia.Controls.Maui.Essentials;
using Microsoft.Maui.Networking;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaConnectivityTests
{
    [Fact]
    public void AvaloniaConnectivity_Implements_IConnectivity()
    {
        var sut = new AvaloniaConnectivity();

        Assert.IsAssignableFrom<IConnectivity>(sut);
    }

    [Fact]
    public void NetworkAccess_ReturnsValidEnumValue()
    {
        var sut = new AvaloniaConnectivity();

        var access = sut.NetworkAccess;

        Assert.True(Enum.IsDefined(typeof(NetworkAccess), access));
    }

    [Fact]
    public void ConnectionProfiles_ReturnsNonNull()
    {
        var sut = new AvaloniaConnectivity();

        var profiles = sut.ConnectionProfiles;

        Assert.NotNull(profiles);
    }

    [Fact]
    public void ConnectionProfiles_ContainsDistinctValues()
    {
        var sut = new AvaloniaConnectivity();

        var profiles = sut.ConnectionProfiles.ToList();

        Assert.Equal(profiles.Distinct().Count(), profiles.Count);
    }

    [Fact]
    public void ConnectivityChanged_CanSubscribeAndUnsubscribe()
    {
        var sut = new AvaloniaConnectivity();
        void handler(object? s, ConnectivityChangedEventArgs e) { }

        sut.ConnectivityChanged += handler;
        sut.ConnectivityChanged -= handler;
    }

    [Fact]
    public void ConnectivityChanged_MultipleSubscribers_DoNotThrow()
    {
        var sut = new AvaloniaConnectivity();
        void handler1(object? s, ConnectivityChangedEventArgs e) { }
        void handler2(object? s, ConnectivityChangedEventArgs e) { }

        sut.ConnectivityChanged += handler1;
        sut.ConnectivityChanged += handler2;
        sut.ConnectivityChanged -= handler1;
        sut.ConnectivityChanged -= handler2;
    }

    [Fact]
    public void NetworkAccess_ConsistentAcrossMultipleCalls()
    {
        var sut = new AvaloniaConnectivity();

        var first = sut.NetworkAccess;
        var second = sut.NetworkAccess;

        Assert.Equal(first, second);
    }

    [Fact]
    public void ConnectionProfiles_AllValuesAreValidEnum()
    {
        var sut = new AvaloniaConnectivity();

        var profiles = sut.ConnectionProfiles;

        foreach (var profile in profiles)
        {
            Assert.True(Enum.IsDefined(typeof(ConnectionProfile), profile));
        }
    }
}
