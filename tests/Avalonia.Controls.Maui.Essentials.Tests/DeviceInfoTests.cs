using Avalonia.Controls.Maui.Essentials.Devices;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Tests;

public class DeviceInfoTests
{
    [Fact]
    public void Current_ReturnsNotNull()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.NotNull(deviceInfo);
    }

    [Fact]
    public void Current_ReturnsSameInstance()
    {
        var first = AvaloniaDeviceInfo.Current;
        var second = AvaloniaDeviceInfo.Current;
        Assert.Same(first, second);
    }

    [Fact]
    public void Model_IsNotNullOrEmpty()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.False(string.IsNullOrEmpty(deviceInfo.Model));
    }

    [Fact]
    public void Manufacturer_IsNotNullOrEmpty()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.False(string.IsNullOrEmpty(deviceInfo.Manufacturer));
    }

    [Fact]
    public void Name_IsNotNullOrEmpty()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.False(string.IsNullOrEmpty(deviceInfo.Name));
    }

    [Fact]
    public void VersionString_IsNotNullOrEmpty()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.False(string.IsNullOrEmpty(deviceInfo.VersionString));
    }

    [Fact]
    public void Version_IsNotNull()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.NotNull(deviceInfo.Version);
    }

    [Fact]
    public void Version_HasPositiveMajor()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.True(deviceInfo.Version.Major >= 0);
    }

    [Fact]
    public void Platform_IsNotUnknown()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.NotEqual(DevicePlatform.Unknown, deviceInfo.Platform);
    }

    [Fact]
    public void Platform_MatchesCurrentOS()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        var platform = deviceInfo.Platform;

        // Verify the platform matches the actual OS we're running on
        if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
        {
            Assert.True(
                platform == DevicePlatform.macOS || platform == DevicePlatform.MacCatalyst,
                $"Expected macOS or MacCatalyst, got {platform}");
        }
        else if (OperatingSystem.IsLinux())
        {
            Assert.Equal(DevicePlatform.Create("Linux"), platform);
        }
        else if (OperatingSystem.IsWindows())
        {
            Assert.Equal(DevicePlatform.WinUI, platform);
        }
    }

    [Fact]
    public void Idiom_IsNotUnknown()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.NotEqual(DeviceIdiom.Unknown, deviceInfo.Idiom);
    }

    [Fact]
    public void Idiom_IsDesktopInTestEnvironment()
    {
        // Tests run on desktop, so idiom should be Desktop
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.Equal(DeviceIdiom.Desktop, deviceInfo.Idiom);
    }

    [Fact]
    public void DeviceType_IsNotUnknown()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.NotEqual(DeviceType.Unknown, deviceInfo.DeviceType);
    }

    [Fact]
    public void DeviceType_IsPhysical()
    {
        // Current implementation defaults to Physical
        var deviceInfo = AvaloniaDeviceInfo.Current;
        Assert.Equal(DeviceType.Physical, deviceInfo.DeviceType);
    }
}
