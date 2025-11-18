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

public class AvaloniaFileImageSourceService : IAvaloniaImageSourceService, IImageSourceService<IFileImageSource>
{
    private readonly ILogger<AvaloniaFileImageSourceService>? _logger;

    public AvaloniaFileImageSourceService(ILogger<AvaloniaFileImageSourceService>? logger = null)
    {
        _logger = logger;
    }

    public AvaloniaFileImageSourceService()
    {
    }

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
            var resourcePath = GetResourcePath(fileName);
            if (TryLoadFromAvaloniaResource(resourcePath, out bitmap) && bitmap != null)
            {
                _logger?.LogDebug($"Loaded image from Avalonia resource: {resourcePath}");
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

    private string GetResourcePath(string fileName)
    {
        // Remove any file extension for the resource lookup
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);

        // Try different resource path patterns
        // MAUI images are typically in the Images folder
        return $"/Images/{nameWithoutExtension}{extension}";
    }

    private bool TryLoadFromAvaloniaResource(string resourcePath, out Bitmap? bitmap)
    {
        bitmap = null;
        try
        {
            // Try to get the resource from the Avalonia resource system
            // We need to check both the current assembly and the entry assembly
            var assemblies = new[]
            {
                System.Reflection.Assembly.GetEntryAssembly(),
                System.Reflection.Assembly.GetCallingAssembly(),
                typeof(AvaloniaFileImageSourceService).Assembly
            }.Where(a => a != null).Distinct();

            foreach (var assembly in assemblies)
            {
                var assemblyName = assembly!.GetName().Name;

                // Try different URI schemes for each assembly
                var uris = new[]
                {
                    new Uri($"avares://{assemblyName}{resourcePath}"),
                    new Uri($"resm:{assemblyName}{resourcePath.Replace('/', '.')}?assembly={assemblyName}"),
                };

                foreach (var uri in uris)
                {
                    try
                    {
                        using var stream = AssetLoader.Open(uri);
                        bitmap = new Bitmap(stream);
                        return true;
                    }
                    catch
                    {
                        // Try next URI format
                    }
                }
            }

            // Also try without assembly name (for embedded resources)
            try
            {
                var uri = new Uri($"avares:///{resourcePath.TrimStart('/')}");
                using var stream = AssetLoader.Open(uri);
                bitmap = new Bitmap(stream);
                return true;
            }
            catch
            {
                // Resource not found
            }
        }
        catch (Exception)
        {
            // Resource not found
        }

        return false;
    }
}

public class ImageSourceServiceResult : IImageSourceServiceResult<Bitmap>
{
    public ImageSourceServiceResult(Bitmap value, bool resolutionDependent = false, Action? dispose = null)
    {
        Value = value;
        IsResolutionDependent = resolutionDependent;
        DisposeAction = dispose;
    }

    public Bitmap Value { get; }
    public bool IsResolutionDependent { get; }
    private Action? DisposeAction { get; }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        DisposeAction?.Invoke();
        Value?.Dispose();
    }
}
