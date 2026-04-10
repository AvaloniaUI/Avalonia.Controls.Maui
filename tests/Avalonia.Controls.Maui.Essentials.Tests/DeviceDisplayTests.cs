using System.Reflection;
using Avalonia.Controls.Maui.Essentials.Devices;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Tests;

public class DeviceDisplayTests
{
    [Fact]
    public void MainDisplayInfo_HasPositiveWidth()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        var info = deviceDisplay.MainDisplayInfo;
        Assert.True(info.Width > 0);
    }

    [Fact]
    public void MainDisplayInfo_HasPositiveHeight()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        var info = deviceDisplay.MainDisplayInfo;
        Assert.True(info.Height > 0);
    }

    [Fact]
    public void MainDisplayInfo_HasPositiveDensity()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        var info = deviceDisplay.MainDisplayInfo;
        Assert.True(info.Density > 0);
    }

    [Fact]
    public void MainDisplayInfo_HasPositiveRefreshRate()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        var info = deviceDisplay.MainDisplayInfo;
        Assert.True(info.RefreshRate > 0);
    }

    [Fact]
    public void MainDisplayInfo_HasValidOrientation()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        var info = deviceDisplay.MainDisplayInfo;
        Assert.True(Enum.IsDefined(typeof(DisplayOrientation), info.Orientation));
    }

    [Fact]
    public void MainDisplayInfo_HasValidRotation()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        var info = deviceDisplay.MainDisplayInfo;
        Assert.True(Enum.IsDefined(typeof(DisplayRotation), info.Rotation));
    }

    [Fact]
    public void MainDisplayInfo_ReturnsDefaultWhenNoApp()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        var info = deviceDisplay.MainDisplayInfo;
        
        // Without an Avalonia app running, falls back to default 1920x1080
        Assert.Equal(1920, info.Width);
        Assert.Equal(1080, info.Height);
        Assert.Equal(1.0, info.Density);
        Assert.Equal(DisplayOrientation.Landscape, info.Orientation);
        Assert.Equal(DisplayRotation.Rotation0, info.Rotation);
        Assert.Equal(60.0f, info.RefreshRate);
    }

    [Fact]
    public void KeepScreenOn_DefaultsToFalse()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        Assert.False(deviceDisplay.KeepScreenOn);
    }

    [Fact]
    public void KeepScreenOn_CanSetAndGet()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        deviceDisplay.KeepScreenOn = true;
        
        Assert.True(deviceDisplay.KeepScreenOn);
        
        deviceDisplay.KeepScreenOn = false;
        Assert.False(deviceDisplay.KeepScreenOn);
    }

    [Fact]
    public void KeepScreenOn_SettingSameValue_IsIdempotent()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        deviceDisplay.KeepScreenOn = true;
        deviceDisplay.KeepScreenOn = true; // Should not throw or have side effects
        
        Assert.True(deviceDisplay.KeepScreenOn);
    }

    [Fact]
    public void MainDisplayInfoChanged_CanSubscribeAndUnsubscribe()
    {
        var deviceDisplay = new AvaloniaDeviceDisplay();
        int callCount = 0;

        void OnChanged(object? sender, DisplayInfoChangedEventArgs e)
        {
            callCount++;
        }

        deviceDisplay.MainDisplayInfoChanged += OnChanged;
        deviceDisplay.MainDisplayInfoChanged -= OnChanged;

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void MultipleInstances_AreIndependent()
    {
        var display1 = new AvaloniaDeviceDisplay();
        var display2 = new AvaloniaDeviceDisplay();

        display1.KeepScreenOn = true;
        
        Assert.True(display1.KeepScreenOn);
        Assert.False(display2.KeepScreenOn);
    }

    [Fact]
    public void Linux_TryParseDbusCookie_ParsesExpectedCookie()
    {
        var method = GetDeviceDisplayHelper("TryParseDbusCookie");
        var args = new object?[] { "(uint32 42,)", 0u };
        var parsed = (bool)method.Invoke(null, args)!;

        Assert.True(parsed);
        Assert.Equal((uint)42, (uint)args[1]!);
    }

    [Fact]
    public void Linux_TryParseDbusCookie_ReturnsFalseForInvalidOutput()
    {
        var method = GetDeviceDisplayHelper("TryParseDbusCookie");
        var args = new object?[] { "(boolean true,)", 0u };
        var parsed = (bool)method.Invoke(null, args)!;

        Assert.False(parsed);
        Assert.Equal((uint)0, (uint)args[1]!);
    }

    [Fact]
    public void Linux_TryParsePortalRequestPath_ParsesExpectedPath()
    {
        var method = GetDeviceDisplayHelper("TryParsePortalRequestPath");
        var args = new object?[] { "(objectpath '/org/freedesktop/portal/desktop/request/1_1/foo',)", string.Empty };
        var parsed = (bool)method.Invoke(null, args)!;

        Assert.True(parsed);
        Assert.Equal("/org/freedesktop/portal/desktop/request/1_1/foo", (string)args[1]!);
    }

    private static MethodInfo GetDeviceDisplayHelper(string methodName)
    {
        var method = typeof(AvaloniaDeviceDisplay).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return method!;
    }
}
