#if BROWSER
using System;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Devices;

public partial class AvaloniaDeviceInfo
{
    partial void GetModelBrowser(ref string? v) => v = "Browser";
    partial void GetManufacturerBrowser(ref string? v) => v = GetBrowserName();
    partial void GetNameBrowser(ref string? v) => v = "Web Browser";
    partial void GetVersionStringBrowser(ref string? v) => v = GetUserAgent();
    partial void GetVersionBrowser(ref Version? v) => v = ParseBrowserVersion();
    partial void GetPlatformBrowser(ref DevicePlatform? v) => v = DevicePlatform.Create("Wasm");
    partial void GetIdiomBrowser(ref DeviceIdiom? v) => v = DeviceIdiom.Desktop;
    partial void GetDeviceTypeBrowser(ref DeviceType? v) => v = DeviceType.Physical;

    private static string GetBrowserName()
    {
        var ua = GetUserAgent().ToLowerInvariant();
        if (ua.Contains("edg")) return "Edge";
        if (ua.Contains("opr") || ua.Contains("opera")) return "Opera";
        if (ua.Contains("chrome")) return "Chrome";
        if (ua.Contains("firefox")) return "Firefox";
        if (ua.Contains("safari")) return "Safari";
        return "Unknown Browser";
    }

    private static Version ParseBrowserVersion()
    {
        try
        {
            var ua = GetUserAgent();
            // Try to extract version from known browser tokens
            // Edge: "Edg/120.0.0.0", Chrome: "Chrome/120.0.0.0", Firefox: "Firefox/121.0", Safari: "Version/17.0"
            var browserName = GetBrowserName();
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
