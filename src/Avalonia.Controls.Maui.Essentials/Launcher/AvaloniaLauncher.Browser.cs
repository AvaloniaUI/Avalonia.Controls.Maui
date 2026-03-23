namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Browser implementation that opens URIs via <c>window.open()</c> JavaScript interop.
/// </summary>
public partial class AvaloniaLauncher
{
    private partial Task<bool> PlatformCanOpenAsync(Uri uri)
    {
        var scheme = uri.Scheme.ToLowerInvariant();
        var supported = scheme is "http" or "https" or "mailto" or "tel";
        return Task.FromResult(supported);
    }

    private partial Task<bool> PlatformOpenAsync(Uri uri)
    {
        using var window = LauncherInterop.WindowOpen(uri.AbsoluteUri, "_blank");
        return Task.FromResult(window is not null);
    }
}
