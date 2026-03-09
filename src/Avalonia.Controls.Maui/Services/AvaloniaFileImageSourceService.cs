using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;

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
    /// An <see cref="IImageSourceServiceResult{Bitmap}"/> containing the loaded bitmap, or <see langword="null"/>
    /// if the source is not a file image source.
    /// </returns>
    public Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
        IImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource is IFileImageSource fileImageSource)
        {
            return GetImageAsync(fileImageSource, scale, cancellationToken);
        }

        return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(null);
    }

    /// <summary>
    /// Loads a bitmap from the specified file image source, first trying Avalonia resources and then the filesystem.
    /// </summary>
    /// <param name="imageSource">The file image source containing the file name or path to load.</param>
    /// <param name="scale">The display scale factor applied during image loading.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{Bitmap}"/> containing the loaded bitmap, or <see langword="null"/>
    /// if the file could not be found.
    /// </returns>
    public Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
        IFileImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource == null || string.IsNullOrEmpty(imageSource.File))
            return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(null);

        try
        {
            var fileName = imageSource.File;
            Bitmap? bitmap = null;

            // First try to load as an Avalonia resource
            if (TryLoadFromAvaloniaResource(fileName, out bitmap) && bitmap != null)
            {
                _logger?.LogDebug($"Loaded image from Avalonia resource: {fileName}");
                return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(
                    new ImageSourceServiceResult(bitmap));
            }

            // Try to load from file system
            var filePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(AppContext.BaseDirectory, fileName);
            if (File.Exists(filePath))
            {
                bitmap = new Bitmap(filePath);
                _logger?.LogDebug($"Loaded image from file: {filePath}");
                return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(
                    new ImageSourceServiceResult(bitmap));
            }

            _logger?.LogWarning($"Could not find image: {fileName}");
            return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(null);
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

/// <summary>
/// Wraps an Avalonia <see cref="Bitmap"/> as an <see cref="IImageSourceServiceResult{Bitmap}"/>.
/// </summary>
/// <remarks>
/// This type manages the lifetime of the bitmap and an optional disposal action, ensuring
/// resources are released when the result is no longer needed.
/// </remarks>
public class ImageSourceServiceResult : IImageSourceServiceResult<Bitmap>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSourceServiceResult"/> class.
    /// </summary>
    /// <param name="value">The loaded <see cref="Bitmap"/> instance.</param>
    /// <param name="resolutionDependent">Indicates whether the image varies with display resolution.</param>
    /// <param name="dispose">An optional callback invoked during disposal for additional cleanup.</param>
    public ImageSourceServiceResult(Bitmap value, bool resolutionDependent = false, Action? dispose = null)
    {
        Value = value;
        IsResolutionDependent = resolutionDependent;
        DisposeAction = dispose;
    }

    /// <summary>
    /// Gets the loaded <see cref="Bitmap"/> image.
    /// </summary>
    public Bitmap Value { get; }

    /// <summary>
    /// Gets a value indicating whether the image is resolution-dependent.
    /// </summary>
    public bool IsResolutionDependent { get; }
    private Action? DisposeAction { get; }

    /// <summary>
    /// Gets a value indicating whether this result has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Disposes the bitmap and invokes any additional cleanup action.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        DisposeAction?.Invoke();
        Value?.Dispose();
    }
}
