using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Extended image source service interface for Avalonia platform
/// </summary>
public interface IAvaloniaImageSourceService : IImageSourceService
{
    /// <summary>
    /// Gets an Avalonia Bitmap from the image source.
    /// On iOS/MacCatalyst, this hides the platform-specific GetImageAsync.
    /// </summary>
#if IOS || MACCATALYST
    new Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
#else
    Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
#endif
        IImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Extension methods to provide unified access to image services
/// </summary>
public static class ImageSourceServiceExtensions
{
    public static async Task<IImageSourceServiceResult?> GetImageAsync(
        this IImageSourceService service,
        IImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (service is IAvaloniaImageSourceService avaloniaService)
        {
            return await avaloniaService.GetImageAsync(imageSource, scale, cancellationToken);
        }

        // Fallback for platform-specific services
        return null;
    }
}
