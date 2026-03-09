using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Windows-specific stub implementation for AvaloniaUriImageSourceService.
/// Avalonia handles all image rendering directly via GetImageAsync which returns Avalonia.Media.IImage.
/// </summary>
public partial class AvaloniaUriImageSourceService
{
    /// <summary>
    /// Returns <see langword="null"/> because Avalonia handles all image rendering directly via GetImageAsync.
    /// This stub satisfies the Windows-specific IImageSourceService contract.
    /// </summary>
    /// <param name="imageSource">The image source to resolve.</param>
    /// <param name="scale">The display scale factor.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Always <see langword="null"/>.</returns>
    public Task<IImageSourceServiceResult<Microsoft.UI.Xaml.Media.ImageSource>?> GetImageSourceAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
    {
        // Avalonia handles image rendering, return null to indicate no native image
        return Task.FromResult<IImageSourceServiceResult<Microsoft.UI.Xaml.Media.ImageSource>?>(null);
    }
}
