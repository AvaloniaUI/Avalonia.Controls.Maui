#if BROWSER
using System;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Devices;

public partial class AvaloniaDeviceInfo
{
    private static readonly Lazy<string> _cachedUserAgent = new(GetUserAgent);
    private static readonly Lazy<string> _cachedBrowserName = new(() => ParseBrowserName(_cachedUserAgent.Value));

    partial void GetModelBrowser(ref string? v) => v = "Browser";
    partial void GetManufacturerBrowser(ref string? v) => v = _cachedBrowserName.Value;
    partial void GetNameBrowser(ref string? v) => v = "Web Browser";
    partial void GetVersionStringBrowser(ref string? v) => v = _cachedUserAgent.Value;
    partial void GetVersionBrowser(ref Version? v) => v = ParseBrowserVersion();
    partial void GetPlatformBrowser(ref DevicePlatform? v) => v = DevicePlatform.Create("Wasm");
    partial void GetIdiomBrowser(ref DeviceIdiom? v) => v = GetIdiomFromUserAgent();
    partial void GetDeviceTypeBrowser(ref DeviceType? v) => v = DeviceType.Physical;

    private static string ParseBrowserName(string ua)
    {
        var lower = ua.ToLowerInvariant();
        if (lower.Contains("edg")) return "Edge";
        if (lower.Contains("opr") || lower.Contains("opera")) return "Opera";
        if (lower.Contains("chrome")) return "Chrome";
        if (lower.Contains("firefox")) return "Firefox";
        if (lower.Contains("safari")) return "Safari";
        return "Unknown Browser";
    }

    private static DeviceIdiom GetIdiomFromUserAgent()
    {
        var ua = _cachedUserAgent.Value.ToLowerInvariant();
        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
            return DeviceIdiom.Phone;
        if (ua.Contains("tablet") || ua.Contains("ipad"))
            return DeviceIdiom.Tablet;
        return DeviceIdiom.Desktop;
    }

    private static Version ParseBrowserVersion()
    {
        try
        {
            var ua = _cachedUserAgent.Value;
            var browserName = _cachedBrowserName.Value;
            var token = browserName switch
            {
                "Edge" => "Edg/",
                "Opera" => "OPR/",
                "Chrome" => "Chrome/",
                "Firefox" => "Firefox/",
                "Safari" => "Version/",
                _ => null
            };

            if (token != null)
            {
                var idx = ua.IndexOf(token, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var versionStart = idx + token.Length;
                    var versionEnd = ua.IndexOfAny([' ', ';', ')'], versionStart);
                    var versionStr = versionEnd > 0
                        ? ua[versionStart..versionEnd]
                        : ua[versionStart..];

                    if (Version.TryParse(versionStr, out var version))
                        return version;
                }
            }
        }
        catch { }

        return new Version(1, 0);
    }

    [JSImport("globalThis.navigator.userAgent")]
    internal static partial string GetUserAgent();
}
#endif
