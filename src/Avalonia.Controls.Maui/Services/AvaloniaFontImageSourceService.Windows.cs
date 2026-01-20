using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Windows-specific stub implementation for AvaloniaFontImageSourceService.
/// Avalonia handles all image rendering directly via GetImageAsync which returns Avalonia.Media.IImage.
/// </summary>
public partial class AvaloniaFontImageSourceService
{
    public Task<IImageSourceServiceResult<Microsoft.UI.Xaml.Media.ImageSource>?> GetImageSourceAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IImageSourceServiceResult<Microsoft.UI.Xaml.Media.ImageSource>?>(null);
    }
}
