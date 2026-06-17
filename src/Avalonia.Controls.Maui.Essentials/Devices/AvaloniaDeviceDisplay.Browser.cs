#if BROWSER
using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace Avalonia.Controls.Maui.Essentials.Devices;

public partial class AvaloniaDeviceDisplay
{
    private JSObject? _wakeLockSentinel;

    partial void UpdateKeepScreenOnBrowser(bool value)
    {
        _ = UpdateWakeLockAsync(value);
    }

    private async Task UpdateWakeLockAsync(bool value)
    {
        try
        {
            if (value)
            {
                if (_wakeLockSentinel == null)
                {
                    _wakeLockSentinel = await RequestWakeLock();
                }
            }
            else
            {
                if (_wakeLockSentinel != null)
                {
                    await ReleaseWakeLock(_wakeLockSentinel);
                    _wakeLockSentinel.Dispose();
                    _wakeLockSentinel = null;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Wake Lock Error: {ex.Message}");
        }
    }

    [JSImport("requestWakeLock", "./Devices/AvaloniaDeviceDisplay.Browser.js")]
    internal static partial Task<JSObject?> RequestWakeLock();

    [JSImport("releaseWakeLock", "./Devices/AvaloniaDeviceDisplay.Browser.js")]
    internal static partial Task ReleaseWakeLock(JSObject? sentinel);
}
#endif
