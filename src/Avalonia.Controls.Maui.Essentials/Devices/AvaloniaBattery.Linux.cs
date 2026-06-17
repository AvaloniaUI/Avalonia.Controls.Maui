using System.IO;
using System.Diagnostics;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Devices;

public partial class AvaloniaBattery
{
    partial void GetChargeLevelLinux(ref double? v) => v = ReadChargeLevel();
    partial void GetStateLinux(ref BatteryState? v) => v = ReadBatteryState();
    partial void GetPowerSourceLinux(ref BatteryPowerSource? v) => v = ReadPowerSource();
    partial void GetEnergySaverStatusLinux(ref EnergySaverStatus? v) => v = ReadEnergySaverStatus();

    private static double ReadChargeLevel()
    {
        try
        {
            var path = FindBatteryPath("capacity");
            if (path is null) return 1.0;
            var value = File.ReadAllText(path).Trim();
            return int.TryParse(value, out var pct) ? pct / 100.0 : 1.0;
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine($"Battery Charge Level Error: {ex.Message}");
            return 1.0;
        }
    }

    private static BatteryState ReadBatteryState()
    {
        try
        {
            var path = FindBatteryPath("status");
            if (path is null) return BatteryState.NotPresent;
            var status = File.ReadAllText(path).Trim().ToLowerInvariant();
            return status switch
            {
                "charging" => BatteryState.Charging,
                "discharging" => BatteryState.Discharging,
                "full" => BatteryState.Full,
                "not charging" => BatteryState.NotCharging,
                _ => BatteryState.Unknown
            };
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine($"Battery State Error: {ex.Message}");
            return BatteryState.Unknown;
        }
    }

    private static BatteryPowerSource ReadPowerSource()
    {
        try
        {
            var acOnline = ReadAcOnline();
            if (acOnline.HasValue)
                return acOnline.Value ? BatteryPowerSource.AC : BatteryPowerSource.Battery;
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine($"Battery Power Source Error: {ex.Message}");
        }

        // Fallback: infer from battery state without caching (single read)
        var state = ReadBatteryState();
        return state switch
        {
            BatteryState.Charging or BatteryState.Full => BatteryPowerSource.AC,
            BatteryState.Discharging => BatteryPowerSource.Battery,
            BatteryState.NotPresent => BatteryPowerSource.AC,
            _ => BatteryPowerSource.Unknown
        };
    }

    /// <summary>
    /// Reads the AC adapter online status from /sys/class/power_supply.
    /// Works across distros (kernel sysfs interface).
    /// </summary>
    private static bool? ReadAcOnline()
    {
        var baseDir = "/sys/class/power_supply";
        if (!Directory.Exists(baseDir)) return null;

        var sawExternalSupply = false;
        foreach (var dir in Directory.GetDirectories(baseDir))
        {
            var typePath = Path.Combine(dir, "type");
            var onlinePath = Path.Combine(dir, "online");
            if (!File.Exists(typePath) || !File.Exists(onlinePath))
                continue;

            var type = File.ReadAllText(typePath);
            if (!IsExternalPowerSupplyType(type))
                continue;

            sawExternalSupply = true;
            if (IsOnlineValue(File.ReadAllText(onlinePath)))
                return true;
        }

        return sawExternalSupply ? false : null;
    }

    /// <summary>
    /// Attempts to detect power saving mode.
    /// Checks CPU energy performance preference (Intel) and power profiles daemon (GNOME/KDE).
    /// </summary>
    private static EnergySaverStatus ReadEnergySaverStatus()
    {
        try
        {
            // Check CPU energy performance preference (Intel HWP, available on many distros)
            var prefPath = "/sys/devices/system/cpu/cpufreq/policy0/energy_performance_preference";
            if (File.Exists(prefPath))
            {
                var pref = File.ReadAllText(prefPath);
                return MapEnergyPerformancePreference(pref);
            }

            // Check scaling governor (broader compatibility: AMD, older Intel, ARM)
            var govPath = "/sys/devices/system/cpu/cpufreq/policy0/scaling_governor";
            if (File.Exists(govPath))
            {
                var gov = File.ReadAllText(govPath);
                return MapScalingGovernor(gov);
            }
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine($"Energy Saver Status Error: {ex.Message}");
        }
        return EnergySaverStatus.Unknown;
    }

    private static bool IsExternalPowerSupplyType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return false;

        return !type.Trim().Equals("Battery", System.StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOnlineValue(string? value) => value?.Trim() == "1";

    private static EnergySaverStatus MapEnergyPerformancePreference(string? preference)
    {
        var pref = preference?.Trim().ToLowerInvariant();
        return pref switch
        {
            "power" or "balance_power" => EnergySaverStatus.On,
            "performance" or "balance_performance" => EnergySaverStatus.Off,
            _ => EnergySaverStatus.Unknown
        };
    }

    private static EnergySaverStatus MapScalingGovernor(string? governor)
    {
        var gov = governor?.Trim().ToLowerInvariant();
        return gov switch
        {
            "powersave" or "conservative" => EnergySaverStatus.On,
            "performance" => EnergySaverStatus.Off,
            _ => EnergySaverStatus.Unknown
        };
    }

    private static string? FindBatteryPath(string file)
    {
        var baseDir = "/sys/class/power_supply";
        if (!Directory.Exists(baseDir)) return null;
        foreach (var dir in Directory.GetDirectories(baseDir))
        {
            var typePath = Path.Combine(dir, "type");
            if (File.Exists(typePath) && File.ReadAllText(typePath).Trim().Equals("Battery", System.StringComparison.OrdinalIgnoreCase))
            {
                var filePath = Path.Combine(dir, file);
                if (File.Exists(filePath))
                    return filePath;
            }
        }
        return null;
    }
}
