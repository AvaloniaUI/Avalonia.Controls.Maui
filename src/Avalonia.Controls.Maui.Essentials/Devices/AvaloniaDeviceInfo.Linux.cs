using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Devices;

public partial class AvaloniaDeviceInfo
{
    private readonly Lazy<(string model, string manufacturer)> _hwInfo = new(ReadHardwareInfo);
    private static readonly Lazy<Dictionary<string, string>> _osRelease = new(ReadOsRelease);

    partial void GetModelLinux(ref string? v) => v = _hwInfo.Value.model;
    partial void GetManufacturerLinux(ref string? v) => v = _hwInfo.Value.manufacturer;
    partial void GetNameLinux(ref string? v) => v = Environment.MachineName;
    partial void GetVersionStringLinux(ref string? v) => v = ReadDistroVersion() ?? Environment.OSVersion.VersionString;
    partial void GetVersionLinux(ref Version? v) => v = Environment.OSVersion.Version;
    partial void GetPlatformLinux(ref DevicePlatform? v) => v = DevicePlatform.Create("Linux");
    partial void GetIdiomLinux(ref DeviceIdiom? v) => v = DeviceIdiom.Desktop;
    partial void GetDeviceTypeLinux(ref DeviceType? v) => v = DeviceType.Physical;

    private static (string model, string manufacturer) ReadHardwareInfo()
    {
        // Use DMI tables (x86/x64 systems: Ubuntu, Fedora, Arch, etc.)
        var model = ReadFileOrDefault("/sys/devices/virtual/dmi/id/product_name", null);
        var manufacturer = ReadFileOrDefault("/sys/devices/virtual/dmi/id/sys_vendor", null);

        // Use device-tree for ARM devices (Raspberry Pi, Pine64, etc.)
        if (string.IsNullOrWhiteSpace(model))
            model = ReadFileOrDefault("/sys/firmware/devicetree/base/model", "Linux Device");

        if (string.IsNullOrWhiteSpace(manufacturer))
            manufacturer = ReadDistroId() ?? "Unknown";

        return (model!.Trim(), manufacturer.Trim());
    }

    /// <summary>
    /// Reads the distro ID from /etc/os-release (e.g. "ubuntu", "fedora", "arch").
    /// Used as a manufacturer fallback when DMI vendor is unavailable (ARM, containers).
    /// </summary>
    private static string? ReadDistroId()
    {
        return _osRelease.Value.GetValueOrDefault("ID");
    }

    /// <summary>
    /// Reads PRETTY_NAME from /etc/os-release for a descriptive version string.
    /// Example: "Ubuntu 24.04 LTS", "Fedora Linux 40", "Arch Linux".
    /// </summary>
    private static string? ReadDistroVersion()
    {
        return _osRelease.Value.GetValueOrDefault("PRETTY_NAME");
    }

    private static Dictionary<string, string> ReadOsRelease()
    {
        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (!File.Exists("/etc/os-release"))
                return data;

            foreach (var line in File.ReadLines("/etc/os-release"))
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Trim('"', '\'');
                    data[key] = value;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OS Release Reading Error: {ex.Message}");
        }
        return data;
    }

    private static string? ReadFileOrDefault(string path, string? defaultValue)
    {
        try
        {
            var content = File.ReadAllText(path).Trim();
            return string.IsNullOrWhiteSpace(content) ? defaultValue : content;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"File Reading Error ({path}): {ex.Message}");
            return defaultValue;
        }
    }
}
