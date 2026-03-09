using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Controls.Maui.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Avalonia.Controls.Maui.SkiaSharp.Views.Services;

/// <summary>
/// Avalonia implementation of <see cref="IImageSourceService"/> that converts SkiaSharp image sources
/// to Avalonia bitmaps.
/// </summary>
public class AvaloniaSKImageSourceService : IAvaloniaImageSourceService,
    IImageSourceService<ISKImageImageSource>,
    IImageSourceService<ISKBitmapImageSource>,
    IImageSourceService<ISKPixmapImageSource>,
    IImageSourceService<ISKPictureImageSource>
{
    private readonly ILogger<AvaloniaSKImageSourceService>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaSKImageSourceService"/> class.
    /// </summary>
    /// <param name="logger">An optional logger for diagnostic messages.</param>
    public AvaloniaSKImageSourceService(ILogger<AvaloniaSKImageSourceService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaSKImageSourceService"/> class with no logger.
    /// </summary>
    public AvaloniaSKImageSourceService()
    {
    }

    /// <inheritdoc/>
    public Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
        IImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        var bitmap = imageSource switch
        {
            ISKImageImageSource img => ConvertFromSKImage(img.Image),
            ISKBitmapImageSource bmp => ConvertFromSKBitmap(bmp.Bitmap),
            ISKPixmapImageSource pix => ConvertFromSKPixmap(pix.Pixmap),
            ISKPictureImageSource pic => ConvertFromSKPicture(pic.Picture, pic.Dimensions),
            _ => null,
        };

        if (bitmap != null)
        {
            return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(
                new Avalonia.Controls.Maui.Services.ImageSourceServiceResult(bitmap));
        }

        return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(null);
    }

    private WriteableBitmap? ConvertFromSKBitmap(SKBitmap? skBitmap)
    {
        if (skBitmap == null)
            return null;

        try
        {
            var info = skBitmap.Info;
            var writeableBitmap = new WriteableBitmap(
                new PixelSize(info.Width, info.Height),
                new Vector(96, 96),
                Avalonia.Platform.PixelFormat.Bgra8888,
                Avalonia.Platform.AlphaFormat.Premul);

            // Ensure the bitmap is in Bgra8888 Premul format to match the WriteableBitmap
            SKBitmap sourceBitmap;
            if (skBitmap.ColorType != SKColorType.Bgra8888 || skBitmap.AlphaType != SKAlphaType.Premul)
            {
                sourceBitmap = new SKBitmap(info.Width, info.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                using var canvas = new SKCanvas(sourceBitmap);
                canvas.DrawBitmap(skBitmap, 0, 0);
            }
            else
            {
                sourceBitmap = skBitmap;
            }

            try
            {
                using var framebuffer = writeableBitmap.Lock();
                var srcPtr = sourceBitmap.GetPixels();
                var srcRowBytes = sourceBitmap.RowBytes;
                var dstRowBytes = framebuffer.RowBytes;
                var copyBytes = Math.Min(srcRowBytes, dstRowBytes);

                unsafe
                {
                    for (int y = 0; y < info.Height; y++)
                    {
                        Buffer.MemoryCopy(
                            (void*)(srcPtr + y * (nint)srcRowBytes),
                            (void*)(framebuffer.Address + y * dstRowBytes),
                            dstRowBytes,
                            copyBytes);
                    }
                }
            }
            finally
            {
                if (sourceBitmap != skBitmap)
                    sourceBitmap.Dispose();
            }

            return writeableBitmap;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error converting SKBitmap to WriteableBitmap");
            return null;
        }
    }

    private WriteableBitmap? ConvertFromSKImage(SKImage? skImage)
    {
        if (skImage == null)
            return null;

        try
        {
            using var skBitmap = new SKBitmap(skImage.Width, skImage.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(skBitmap);
            canvas.DrawImage(skImage, 0, 0);
            return ConvertFromSKBitmap(skBitmap);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error converting SKImage to WriteableBitmap");
            return null;
        }
    }

    private WriteableBitmap? ConvertFromSKPixmap(SKPixmap? skPixmap)
    {
        if (skPixmap == null)
            return null;

        try
        {
            using var skBitmap = new SKBitmap();
            skBitmap.InstallPixels(skPixmap);
            return ConvertFromSKBitmap(skBitmap);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error converting SKPixmap to WriteableBitmap");
            return null;
        }
    }

    private WriteableBitmap? ConvertFromSKPicture(SKPicture? skPicture, SKSizeI dimensions)
    {
        if (skPicture == null)
            return null;

        try
        {
            var width = dimensions.Width > 0 ? dimensions.Width : (int)skPicture.CullRect.Width;
            var height = dimensions.Height > 0 ? dimensions.Height : (int)skPicture.CullRect.Height;

            if (width <= 0 || height <= 0)
                return null;

            using var skBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(skBitmap);
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(skPicture);
            return ConvertFromSKBitmap(skBitmap);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error converting SKPicture to WriteableBitmap");
            return null;
        }
    }
}
