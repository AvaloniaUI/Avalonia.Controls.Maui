using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui;
using UIKit;

namespace Avalonia.Controls.Maui.Services;

public partial class AvaloniaStreamImageSourceService
{
    /// <summary>
    /// Platform-specific implementation for iOS/MacCatalyst.
    /// Returns null as Avalonia handles the actual image rendering.
    /// </summary>
    Task<IImageSourceServiceResult<UIImage>?> IImageSourceService.GetImageAsync(IImageSource imageSource, float scale, CancellationToken cancellationToken)
    {
        // Avalonia handles image rendering, return null to indicate no native image
        return Task.FromResult<IImageSourceServiceResult<UIImage>?>(null);
    }
}
