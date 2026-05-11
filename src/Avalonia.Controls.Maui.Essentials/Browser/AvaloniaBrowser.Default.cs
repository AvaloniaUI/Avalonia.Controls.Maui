using System.Diagnostics;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Desktop implementation that opens URLs via the OS default browser.
/// </summary>
public partial class AvaloniaBrowser
{
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
