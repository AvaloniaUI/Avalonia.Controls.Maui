using System.Diagnostics;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Desktop implementation that opens URIs via the OS default handler.
/// </summary>
public partial class AvaloniaLauncher
{
    private partial Task<bool> PlatformCanOpenAsync(Uri uri)
    {
        var scheme = uri.Scheme.ToLowerInvariant();
        var supported = scheme is "http" or "https" or "mailto" or "file" or "tel";
        return Task.FromResult(supported);
    }

    private partial Task<bool> PlatformOpenAsync(Uri uri)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = uri.AbsoluteUri,
            UseShellExecute = true
        });
        return Task.FromResult(process is not null);
    }
}
