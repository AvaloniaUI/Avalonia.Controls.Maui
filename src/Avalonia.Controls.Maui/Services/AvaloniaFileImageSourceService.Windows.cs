using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Windows-specific stub implementation for AvaloniaFileImageSourceService.
/// Avalonia handles all image rendering directly via GetImageAsync which returns Avalonia.Media.IImage.
/// </summary>
public partial class AvaloniaFileImageSourceService
{
    public Task<IImageSourceServiceResult<Microsoft.UI.Xaml.Media.ImageSource>?> GetImageSourceAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
    {
        // Avalonia handles image rendering, return null to indicate no native image
        return Task.FromResult<IImageSourceServiceResult<Microsoft.UI.Xaml.Media.ImageSource>?>(null);
    }
}
