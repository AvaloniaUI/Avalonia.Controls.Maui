using System;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Devices;

/// <summary>
/// Avalonia implementation of <see cref="IDeviceInfo"/> that provides hardware and platform
/// information across Linux, browser, and native MAUI platforms.
/// </summary>
public partial class AvaloniaDeviceInfo : IDeviceInfo
{
    private static readonly Lazy<AvaloniaDeviceInfo> _current = new(() => new AvaloniaDeviceInfo());

    /// <summary>
    /// Provides the default implementation for static usage of this API.
    /// </summary>
    public static IDeviceInfo Current => _current.Value;

    /// <inheritdoc/>
    public string Model => GetPlatformModel() ?? "Unknown";

    /// <inheritdoc/>
    public string Manufacturer => GetPlatformManufacturer() ?? "Unknown";

    /// <inheritdoc/>
    public string Name => GetPlatformName() ?? Environment.MachineName;

    /// <inheritdoc/>
    public string VersionString => GetPlatformVersionString() ?? Environment.OSVersion.VersionString;

    /// <inheritdoc/>
    public Version Version => GetPlatformVersion() ?? Environment.OSVersion.Version;

    /// <inheritdoc/>
    public DevicePlatform Platform => GetPlatformPlatform();

    /// <inheritdoc/>
    public DeviceIdiom Idiom => GetPlatformIdiom();

    /// <inheritdoc/>
    public DeviceType DeviceType => GetPlatformDeviceType();

    private string? GetPlatformModel()
    {
        string? v = null;
        if (OperatingSystem.IsLinux())
            GetModelLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetModelBrowser(ref v);
        return v;
    }

    private string? GetPlatformManufacturer()
    {
        string? v = null;
        if (OperatingSystem.IsLinux())
            GetManufacturerLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetManufacturerBrowser(ref v);
        return v;
    }

    private string? GetPlatformName()
    {
        string? v = null;
        if (OperatingSystem.IsLinux())
            GetNameLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetNameBrowser(ref v);
        return v;
    }

    private string? GetPlatformVersionString()
    {
        string? v = null;
        if (OperatingSystem.IsLinux())
            GetVersionStringLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetVersionStringBrowser(ref v);
        return v;
    }

    private Version? GetPlatformVersion()
    {
        Version? v = null;
        if (OperatingSystem.IsLinux())
            GetVersionLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetVersionBrowser(ref v);
        return v;
    }

    private DevicePlatform GetPlatformPlatform()
    {
        DevicePlatform? v = null;
        if (OperatingSystem.IsLinux())
            GetPlatformLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetPlatformBrowser(ref v);

        if (v == null || v == DevicePlatform.Unknown)
            return GetDefaultPlatform();

        return v.Value;
    }

    private DeviceIdiom GetPlatformIdiom()
    {
        DeviceIdiom? v = null;
        if (OperatingSystem.IsLinux())
            GetIdiomLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetIdiomBrowser(ref v);

        if (v == null || v == DeviceIdiom.Unknown)
            return DeviceIdiom.Desktop;

        return v.Value;
    }

    private DeviceType GetPlatformDeviceType()
    {
        DeviceType? v = null;
        if (OperatingSystem.IsLinux())
            GetDeviceTypeLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetDeviceTypeBrowser(ref v);

        if (v == null || v == DeviceType.Unknown)
            return DeviceType.Physical;

        return v.Value;
    }

    partial void GetModelLinux(ref string? v);
    partial void GetManufacturerLinux(ref string? v);
    partial void GetNameLinux(ref string? v);
    partial void GetVersionStringLinux(ref string? v);
    partial void GetVersionLinux(ref Version? v);
    partial void GetPlatformLinux(ref DevicePlatform? v);
    partial void GetIdiomLinux(ref DeviceIdiom? v);
    partial void GetDeviceTypeLinux(ref DeviceType? v);

    partial void GetModelBrowser(ref string? v);
    partial void GetManufacturerBrowser(ref string? v);
    partial void GetNameBrowser(ref string? v);
    partial void GetVersionStringBrowser(ref string? v);
    partial void GetVersionBrowser(ref Version? v);
    partial void GetPlatformBrowser(ref DevicePlatform? v);
    partial void GetIdiomBrowser(ref DeviceIdiom? v);
    partial void GetDeviceTypeBrowser(ref DeviceType? v);

    private DevicePlatform GetDefaultPlatform()
    {
        if (OperatingSystem.IsLinux()) return DevicePlatform.Create("Linux");
        if (OperatingSystem.IsBrowser()) return DevicePlatform.Create("Wasm");
        if (OperatingSystem.IsAndroid()) return DevicePlatform.Android;
        if (OperatingSystem.IsIOS()) return DevicePlatform.iOS;
        if (OperatingSystem.IsMacCatalyst()) return DevicePlatform.MacCatalyst;
        if (OperatingSystem.IsMacOS()) return DevicePlatform.macOS;
        if (OperatingSystem.IsWindows()) return DevicePlatform.WinUI;
        return DevicePlatform.Unknown;
    }
}
