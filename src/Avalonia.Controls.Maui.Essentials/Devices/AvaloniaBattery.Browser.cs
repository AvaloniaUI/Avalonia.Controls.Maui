#if BROWSER
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Devices;

public partial class AvaloniaBattery
{
    private bool _isInitialized;

    partial void GetChargeLevelBrowser(ref double? v) => v = _isInitialized ? GetBatteryLevel() : 1.0;
    partial void GetStateBrowser(ref BatteryState? v) => v = _isInitialized ? (GetBatteryCharging() ? BatteryState.Charging : BatteryState.Discharging) : BatteryState.Unknown;
    partial void GetPowerSourceBrowser(ref BatteryPowerSource? v) => v = _isInitialized ? (GetBatteryCharging() ? BatteryPowerSource.AC : BatteryPowerSource.Battery) : BatteryPowerSource.Unknown;
    partial void GetEnergySaverStatusBrowser(ref EnergySaverStatus? v) => v = EnergySaverStatus.Off;

    public AvaloniaBattery()
    {
        if (OperatingSystem.IsBrowser())
        {
            if (!_isInitialized)
            {
                _ = InitializeAsync();
            }
        }
    }

    private async Task InitializeAsync()
    {
        if (await InitBattery())
        {
            _isInitialized = true;
            SubscribeBatteryEvents(OnBatteryChanged);
        }
    }

    [JSExport]
    internal static void OnBatteryChanged()
    {
        if (AvaloniaBattery.Default is AvaloniaBattery battery)
        {
            battery.OnBatteryInfoChanged(new BatteryInfoChangedEventArgs(battery.ChargeLevel, battery.State, battery.PowerSource));
        }
    }

    [JSImport("initBattery", "./Devices/AvaloniaBattery.Browser.js")]
    internal static partial Task<bool> InitBattery();

    [JSImport("getBatteryLevel", "./Devices/AvaloniaBattery.Browser.js")]
    internal static partial double GetBatteryLevel();

    [JSImport("getBatteryCharging", "./Devices/AvaloniaBattery.Browser.js")]
    internal static partial bool GetBatteryCharging();

    [JSImport("subscribeBatteryEvents", "./Devices/AvaloniaBattery.Browser.js")]
    internal static partial void SubscribeBatteryEvents([JSMarshalAs<JSType.Function>] Action callback);
}
#endif
