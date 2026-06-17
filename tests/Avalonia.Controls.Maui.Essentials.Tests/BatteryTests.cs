using System.Reflection;
using Avalonia.Controls.Maui.Essentials.Devices;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Tests;

public class BatteryTests
{
    [Fact]
    public void Default_ReturnsNotNull()
    {
        var battery = AvaloniaBattery.Default;
        Assert.NotNull(battery);
    }

    [Fact]
    public void Default_ReturnsSameInstance()
    {
        var first = AvaloniaBattery.Default;
        var second = AvaloniaBattery.Default;
        Assert.Same(first, second);
    }

    [Fact]
    public void ChargeLevel_IsInRange()
    {
        var battery = AvaloniaBattery.Default;
        Assert.InRange(battery.ChargeLevel, 0.0, 1.0);
    }

    [Fact]
    public void State_ReturnsValidEnum()
    {
        var battery = AvaloniaBattery.Default;
        Assert.True(Enum.IsDefined(typeof(BatteryState), battery.State));
    }

    [Fact]
    public void PowerSource_ReturnsValidEnum()
    {
        var battery = AvaloniaBattery.Default;
        Assert.True(Enum.IsDefined(typeof(BatteryPowerSource), battery.PowerSource));
    }

    [Fact]
    public void EnergySaverStatus_ReturnsValidEnum()
    {
        var battery = AvaloniaBattery.Default;
        Assert.True(Enum.IsDefined(typeof(EnergySaverStatus), battery.EnergySaverStatus));
    }

    [Fact]
    public void BatteryInfoChanged_CanSubscribeAndUnsubscribe()
    {
        var battery = AvaloniaBattery.Default;
        int callCount = 0;
        
        void OnBatteryChanged(object? sender, BatteryInfoChangedEventArgs e)
        {
            callCount++;
        }

        battery.BatteryInfoChanged += OnBatteryChanged;
        battery.BatteryInfoChanged -= OnBatteryChanged;
        
        Assert.Equal(0, callCount);
    }

    [Fact]
    public void EnergySaverStatusChanged_CanSubscribeAndUnsubscribe()
    {
        var battery = AvaloniaBattery.Default;
        int callCount = 0;
        
        void OnChanged(object? sender, EnergySaverStatusChangedEventArgs e)
        {
            callCount++;
        }

        battery.EnergySaverStatusChanged += OnChanged;
        battery.EnergySaverStatusChanged -= OnChanged;
        
        Assert.Equal(0, callCount);
    }

    [Fact]
    public void Properties_AreConsistent()
    {
        var battery = AvaloniaBattery.Default;

        // If state is Full or Charging, power source should be AC
        if (battery.State == BatteryState.Charging || battery.State == BatteryState.Full)
        {
            Assert.Equal(BatteryPowerSource.AC, battery.PowerSource);
        }
    }

    [Fact]
    public void Linux_MapEnergyPerformancePreference_MapsExpectedValues()
    {
        Assert.Equal(EnergySaverStatus.On, InvokeBatteryHelper<EnergySaverStatus>("MapEnergyPerformancePreference", "power"));
        Assert.Equal(EnergySaverStatus.On, InvokeBatteryHelper<EnergySaverStatus>("MapEnergyPerformancePreference", "balance_power"));
        Assert.Equal(EnergySaverStatus.Off, InvokeBatteryHelper<EnergySaverStatus>("MapEnergyPerformancePreference", "performance"));
        Assert.Equal(EnergySaverStatus.Unknown, InvokeBatteryHelper<EnergySaverStatus>("MapEnergyPerformancePreference", "balanced"));
        Assert.Equal(EnergySaverStatus.Unknown, InvokeBatteryHelper<EnergySaverStatus>("MapEnergyPerformancePreference", null));
    }

    [Fact]
    public void Linux_MapScalingGovernor_MapsExpectedValues()
    {
        Assert.Equal(EnergySaverStatus.On, InvokeBatteryHelper<EnergySaverStatus>("MapScalingGovernor", "powersave"));
        Assert.Equal(EnergySaverStatus.On, InvokeBatteryHelper<EnergySaverStatus>("MapScalingGovernor", "conservative"));
        Assert.Equal(EnergySaverStatus.Off, InvokeBatteryHelper<EnergySaverStatus>("MapScalingGovernor", "performance"));
        Assert.Equal(EnergySaverStatus.Unknown, InvokeBatteryHelper<EnergySaverStatus>("MapScalingGovernor", "ondemand"));
        Assert.Equal(EnergySaverStatus.Unknown, InvokeBatteryHelper<EnergySaverStatus>("MapScalingGovernor", null));
    }

    [Fact]
    public void Linux_IsExternalPowerSupplyType_RecognizesNonBatteryTypes()
    {
        Assert.True(InvokeBatteryHelper<bool>("IsExternalPowerSupplyType", "Mains"));
        Assert.True(InvokeBatteryHelper<bool>("IsExternalPowerSupplyType", "USB"));
        Assert.True(InvokeBatteryHelper<bool>("IsExternalPowerSupplyType", "USB_C"));
        Assert.False(InvokeBatteryHelper<bool>("IsExternalPowerSupplyType", "Battery"));
        Assert.False(InvokeBatteryHelper<bool>("IsExternalPowerSupplyType", null));
    }

    [Fact]
    public void Linux_IsOnlineValue_ParsesExpectedStates()
    {
        Assert.True(InvokeBatteryHelper<bool>("IsOnlineValue", "1"));
        Assert.False(InvokeBatteryHelper<bool>("IsOnlineValue", "0"));
        Assert.False(InvokeBatteryHelper<bool>("IsOnlineValue", null));
    }

    private static T InvokeBatteryHelper<T>(string methodName, string? input)
    {
        var method = typeof(AvaloniaBattery).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var value = method!.Invoke(null, [input]);
        Assert.NotNull(value);
        return (T)value!;
    }
}
