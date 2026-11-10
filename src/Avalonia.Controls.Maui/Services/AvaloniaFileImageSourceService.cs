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
    private const int DesktopResizetizerScale = 4;

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
    /// <param name="scale">
    /// The display scale. Above 1, an <c>@2x</c> or <c>@3x</c> variant of the file (for example <c>icon@2x.png</c>)
    /// is loaded when it exists, as a <see cref="ScaledBitmapImage"/> with the base image's size in
    /// device-independent units. The result is resolution-dependent when another scale could load a different file.
    /// </param>
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

            for (var variantScale = Math.Clamp((int)Math.Ceiling(scale), 1, 3); variantScale >= 2; variantScale--)
            {
                if (TryLoad(GetVariantFileName(fileName, variantScale)) is { } variant)
                {
                    return Task.FromResult<IImageSourceServiceResult<IImage>?>(
                        new ImageSourceServiceResult(new ScaledBitmapImage(variant, variantScale), resolutionDependent: true));
                }
            }

            // Above 1x the base image stands in for a missing variant; at 1x a variant suits a higher scale
            var resolutionDependent = scale > 1 || HasVariant(fileName, 2) || HasVariant(fileName, 3);

            if (TryLoad(fileName) is { } bitmap)
            {
                return Task.FromResult<IImageSourceServiceResult<IImage>?>(
                    new ImageSourceServiceResult(bitmap, resolutionDependent));
            }

            // On desktop, Resizetizer renders a resized MauiImage once, at 4x its base size
            if (TryLoad(GetVariantFileName(fileName, DesktopResizetizerScale)) is { } desktop)
            {
                return Task.FromResult<IImageSourceServiceResult<IImage>?>(
                    new ImageSourceServiceResult(new ScaledBitmapImage(desktop, DesktopResizetizerScale), resolutionDependent));
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

    private Bitmap? TryLoad(string fileName)
    {
        if (TryLoadFromAvaloniaResource(fileName, out var bitmap) && bitmap != null)
        {
            _logger?.LogDebug($"Loaded image from Avalonia resource: {fileName}");
            return bitmap;
        }

        var filePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(AppContext.BaseDirectory, fileName);
        if (File.Exists(filePath))
        {
            _logger?.LogDebug($"Loaded image from file: {filePath}");
            return new Bitmap(filePath);
        }

        return null;
    }

    private static bool HasVariant(string fileName, int scale)
    {
        var variant = GetVariantFileName(fileName, scale);
        return AvaloniaResourceHelper.TryResolveResourceUri(variant, out _)
            || File.Exists(Path.IsPathRooted(variant) ? variant : Path.Combine(AppContext.BaseDirectory, variant));
    }

    private static string GetVariantFileName(string fileName, int scale)
    {
        var extension = Path.GetExtension(fileName);
        return $"{fileName[..^extension.Length]}@{scale}x{extension}";
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
