using Microsoft.Maui.ApplicationModel;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of <see cref="ILauncher"/> that opens URIs and files
/// using the platform's default handler.
/// </summary>
public partial class AvaloniaLauncher : ILauncher
{
    /// <inheritdoc/>
    public async Task<bool> CanOpenAsync(Uri uri)
    {
        try
        {
            return await PlatformCanOpenAsync(uri);
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> OpenAsync(Uri uri)
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

    /// <inheritdoc/>
    public Task<bool> OpenAsync(OpenFileRequest request)
    {
        if (request?.File?.FullPath is not { } path)
            return Task.FromResult(false);

        try
        {
            return PlatformOpenAsync(new Uri(path));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> TryOpenAsync(Uri uri)
    {
        if (!await CanOpenAsync(uri))
            return false;

        return await OpenAsync(uri);
    }

    private partial Task<bool> PlatformCanOpenAsync(Uri uri);
    private partial Task<bool> PlatformOpenAsync(Uri uri);
}
