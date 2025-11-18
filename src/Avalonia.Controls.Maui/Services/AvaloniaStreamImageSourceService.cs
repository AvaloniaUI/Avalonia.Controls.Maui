using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Services;

public class AvaloniaStreamImageSourceService : IAvaloniaImageSourceService, IImageSourceService<IStreamImageSource>
{
    private readonly ILogger<AvaloniaStreamImageSourceService>? _logger;

    public AvaloniaStreamImageSourceService(ILogger<AvaloniaStreamImageSourceService>? logger = null)
    {
        _logger = logger;
    }

    public AvaloniaStreamImageSourceService()
    {
    }

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