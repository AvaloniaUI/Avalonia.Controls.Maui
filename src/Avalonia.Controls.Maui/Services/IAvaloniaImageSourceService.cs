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
    /// <summary>
    /// Retrieves an image from the specified image source service, delegating to
    /// <see cref="IAvaloniaImageSourceService"/> when the service supports Avalonia.
    /// </summary>
    /// <param name="service">The image source service to invoke.</param>
    /// <param name="imageSource">The image source describing the image to load.</param>
    /// <param name="scale">The display scale factor applied during image loading.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult"/> containing the loaded image, or <see langword="null"/>
    /// if the service does not implement <see cref="IAvaloniaImageSourceService"/>.
    /// </returns>
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

    /// <summary>
    /// Resolves the image source service for <paramref name="imageSource"/> by its image source interface.
    /// </summary>
    /// <remarks>
    /// Resolving by concrete type can return MAUI's own service (for example, <c>FileImageSourceService</c>
    /// for <c>FileImageSource</c>) instead of the Avalonia one, and is ambiguous for <c>UriImageSource</c>,
    /// which implements both <see cref="IUriImageSource"/> and <see cref="IStreamImageSource"/>.
    /// More specific interfaces are checked first.
    /// </remarks>
    internal static IImageSourceService? ResolveImageSourceService(this IImageSourceServiceProvider provider, IImageSource imageSource) =>
        provider.GetImageSourceService(imageSource switch
        {
            IFileImageSource => typeof(IFileImageSource),
            IFontImageSource => typeof(IFontImageSource),
            IUriImageSource => typeof(IUriImageSource),
            IStreamImageSource => typeof(IStreamImageSource),
            _ => imageSource.GetType()
        });
}
