using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Avalonia implementation of <see cref="IImageSourceService"/> that loads images from stream-based sources.
/// </summary>
/// <remarks>
/// If the provided stream is not seekable, the service copies its contents into a seekable
/// <see cref="MemoryStream"/> before creating the bitmap.
/// </remarks>
public partial class AvaloniaStreamImageSourceService : IAvaloniaImageSourceService, IImageSourceService<IStreamImageSource>
{
    private readonly ILogger<AvaloniaStreamImageSourceService>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaStreamImageSourceService"/> class.
    /// </summary>
    /// <param name="logger">An optional logger for diagnostic messages during image loading.</param>
    public AvaloniaStreamImageSourceService(ILogger<AvaloniaStreamImageSourceService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaStreamImageSourceService"/> class with no logger.
    /// </summary>
    public AvaloniaStreamImageSourceService()
    {
    }

    /// <summary>
    /// Attempts to load a bitmap from the specified image source by casting it to <see cref="IStreamImageSource"/>.
    /// </summary>
    /// <param name="imageSource">The image source to load. Must implement <see cref="IStreamImageSource"/> to produce a result.</param>
    /// <param name="scale">The display scale factor applied during image loading.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{Bitmap}"/> containing the loaded bitmap, or <see langword="null"/>
    /// if the source is not a stream image source.
    /// </returns>
    public Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
        IImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource is IStreamImageSource streamImageSource)
        {
            return GetImageAsync(streamImageSource, scale, cancellationToken);
        }

        return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(null);
    }

    /// <summary>
    /// Loads a bitmap by reading from the stream provided by the specified <see cref="IStreamImageSource"/>.
    /// </summary>
    /// <param name="imageSource">The stream image source from which to obtain the image data.</param>
    /// <param name="scale">The display scale factor applied during image loading.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{Bitmap}"/> containing the loaded bitmap, or <see langword="null"/>
    /// if the source is <see langword="null"/> or returns a <see langword="null"/> stream.
    /// </returns>
    public async Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
        IStreamImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource == null)
            return null;

        try
        {
            _logger?.LogDebug("Loading image from stream");

            var stream = await imageSource.GetStreamAsync(cancellationToken);
            if (stream == null)
            {
                _logger?.LogWarning("Stream image source returned null stream");
                return null;
            }

            // If the stream is not seekable, copy it to a memory stream
            if (!stream.CanSeek)
            {
                _logger?.LogDebug("Stream is not seekable, copying to memory stream");
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;
                stream.Dispose();
                stream = memoryStream;
            }

            var bitmap = new Bitmap(stream);
            _logger?.LogDebug("Successfully loaded image from stream");

            // Return result with disposal action to clean up the stream
            return new ImageSourceServiceResult(bitmap, dispose: () => stream.Dispose());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading image from stream");
            throw;
        }
    }
}