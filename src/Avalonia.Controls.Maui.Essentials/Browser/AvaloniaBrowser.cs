using Microsoft.Maui.ApplicationModel;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of <see cref="IBrowser"/> that opens URLs in the system's default web browser.
/// </summary>
public partial class AvaloniaBrowser : IBrowser
{
    /// <inheritdoc/>
    public async Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options)
    {
        try
        {
            return await PlatformOpenAsync(uri);
        }
        catch
        {
            return false;
        }
    }

    private partial Task<bool> PlatformOpenAsync(Uri uri);
}
