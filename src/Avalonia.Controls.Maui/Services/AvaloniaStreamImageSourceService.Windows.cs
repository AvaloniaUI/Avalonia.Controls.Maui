using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Windows-specific stub implementation for AvaloniaStreamImageSourceService.
/// Avalonia handles all image rendering directly via GetImageAsync which returns Avalonia.Media.IImage.
/// </summary>
public partial class AvaloniaStreamImageSourceService
{
    public Task<IImageSourceServiceResult<Microsoft.UI.Xaml.Media.ImageSource>?> GetImageSourceAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IImageSourceServiceResult<Microsoft.UI.Xaml.Media.ImageSource>?>(null);
    }
}
