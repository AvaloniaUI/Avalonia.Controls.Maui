using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using IImage = Avalonia.Media.IImage;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Avalonia implementation of <see cref="IImageSourceService"/> that loads images from file-based sources.
/// </summary>
/// <remarks>
/// This service resolves images by first attempting to locate them as Avalonia embedded resources
/// and then falling back to the local filesystem.
/// </remarks>
public partial class AvaloniaFileImageSourceService : IAvaloniaImageSourceService, IImageSourceService<IFileImageSource>
{
    private readonly ILogger<AvaloniaFileImageSourceService>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaFileImageSourceService"/> class.
    /// </summary>
    /// <param name="logger">An optional logger for diagnostic messages during image loading.</param>
    public AvaloniaFileImageSourceService(ILogger<AvaloniaFileImageSourceService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaFileImageSourceService"/> class with no logger.
    /// </summary>
    public AvaloniaFileImageSourceService()
    {
    }

    /// <summary>
    /// Attempts to load a bitmap from the specified image source by casting it to <see cref="IFileImageSource"/>.
    /// </summary>
    /// <param name="imageSource">The image source to load. Must implement <see cref="IFileImageSource"/> to produce a result.</param>
    /// <param name="scale">The display scale factor applied during image loading.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{IImage}"/> containing the loaded bitmap, or <see langword="null"/>
    /// if the source is not a file image source.
    /// </returns>
    public Task<IImageSourceServiceResult<IImage>?> GetImageAsync(
        IImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource is IFileImageSource fileImageSource)
        {
            return GetImageAsync(fileImageSource, scale, cancellationToken);
        }

        return Task.FromResult<IImageSourceServiceResult<IImage>?>(null);
    }

    /// <summary>
    /// Loads a bitmap from the specified file image source, first trying Avalonia resources and then the filesystem.
    /// </summary>
    /// <param name="imageSource">The file image source containing the file name or path to load.</param>
    /// <param name="scale">The display scale factor applied during image loading.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{IImage}"/> containing the loaded bitmap, or <see langword="null"/>
    /// if the file could not be found.
    /// </returns>
    public Task<IImageSourceServiceResult<IImage>?> GetImageAsync(
        IFileImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource == null || string.IsNullOrEmpty(imageSource.File))
            return Task.FromResult<IImageSourceServiceResult<IImage>?>(null);

        try
        {
            var fileName = imageSource.File;
            Bitmap? bitmap = null;

            // First try to load as an Avalonia resource
            if (TryLoadFromAvaloniaResource(fileName, out bitmap) && bitmap != null)
            {
                _logger?.LogDebug($"Loaded image from Avalonia resource: {fileName}");
                return Task.FromResult<IImageSourceServiceResult<IImage>?>(
                    new ImageSourceServiceResult(bitmap));
            }

            // Try to load from file system
            var filePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(AppContext.BaseDirectory, fileName);
            if (File.Exists(filePath))
            {
                bitmap = new Bitmap(filePath);
                _logger?.LogDebug($"Loaded image from file: {filePath}");
                return Task.FromResult<IImageSourceServiceResult<IImage>?>(
                    new ImageSourceServiceResult(bitmap));
            }

            _logger?.LogWarning($"Could not find image: {fileName}");
            return Task.FromResult<IImageSourceServiceResult<IImage>?>(null);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error loading image: {imageSource.File}");
            throw;
        }
    }

    private bool TryLoadFromAvaloniaResource(string fileName, out Bitmap? bitmap)
    {
        bitmap = null;
        if (!AvaloniaResourceHelper.TryResolveResourceUri(fileName, out var uri) || uri == null)
            return false;

        try
        {
            using var stream = AssetLoader.Open(uri);
            bitmap = new Bitmap(stream);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, $"Resource URI resolved but failed to load bitmap: {uri}");
            return false;
        }
    }
}
