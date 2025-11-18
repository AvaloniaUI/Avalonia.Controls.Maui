using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Services;

public class AvaloniaUriImageSourceService : IAvaloniaImageSourceService, IImageSourceService<IUriImageSource>
{
    private readonly ILogger<AvaloniaUriImageSourceService>? _logger;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, Task<IImageSourceServiceResult<Bitmap>?>> _cache;
    private readonly SemaphoreSlim _cacheLock;

    public AvaloniaUriImageSourceService(ILogger<AvaloniaUriImageSourceService>? logger = null)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Avalonia.Controls.Maui/1.0");
        _cache = new ConcurrentDictionary<string, Task<IImageSourceServiceResult<Bitmap>?>>();
        _cacheLock = new SemaphoreSlim(1, 1);
    }

    public AvaloniaUriImageSourceService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Avalonia.Controls.Maui/1.0");
        _cache = new ConcurrentDictionary<string, Task<IImageSourceServiceResult<Bitmap>?>>();
        _cacheLock = new SemaphoreSlim(1, 1);
    }

    public Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
        IImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource is IUriImageSource uriImageSource)
        {
            return GetImageAsync(uriImageSource, scale, cancellationToken);
        }

        return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(null);
    }

    public async Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
        IUriImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource?.Uri == null)
            return null;

        try
        {
            var uri = imageSource.Uri;
            var cacheKey = uri.ToString();

            // Check if caching is enabled and if we have a cached result
            if (imageSource.CachingEnabled && _cache.TryGetValue(cacheKey, out var cachedTask))
            {
                _logger?.LogDebug("Returning cached image for URI: {Uri}", uri);
                return await cachedTask;
            }

            // Create a task for loading the image
            var loadTask = LoadImageInternalAsync(uri, cancellationToken);

            // Add to cache if caching is enabled (or get existing if another thread added it)
            if (imageSource.CachingEnabled)
            {
                var resultTask = _cache.GetOrAdd(cacheKey, loadTask);
                return await resultTask;
            }

            return await loadTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading image from URI: {Uri}", imageSource.Uri);
            throw;
        }
    }

    private async Task<IImageSourceServiceResult<Bitmap>?> LoadImageInternalAsync(Uri uri, CancellationToken cancellationToken)
    {
        // Handle different URI schemes
        if (uri.IsAbsoluteUri)
        {
            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                return await LoadFromHttpAsync(uri, cancellationToken);
            }
            else if (uri.Scheme == Uri.UriSchemeFile)
            {
                return LoadFromFile(uri);
            }
        }

        _logger?.LogWarning($"Unsupported URI scheme: {uri}");
        return null;
    }

    private async Task<IImageSourceServiceResult<Bitmap>?> LoadFromHttpAsync(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            _logger?.LogDebug($"Loading image from HTTP: {uri}");

            var stream = await _httpClient.GetStreamAsync(uri, cancellationToken).ConfigureAwait(false);
            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;
            var bitmap = new Bitmap(memoryStream);

            _logger?.LogDebug($"Successfully loaded image from HTTP: {uri}");
            return new ImageSourceServiceResult(bitmap);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to load image from HTTP: {uri}");
            throw;
        }
    }

    private IImageSourceServiceResult<Bitmap>? LoadFromFile(Uri uri)
    {
        try
        {
            var filePath = uri.LocalPath;
            if (!File.Exists(filePath))
            {
                _logger?.LogWarning($"File not found: {filePath}");
                return null;
            }

            _logger?.LogDebug($"Loading image from file: {filePath}");
            var bitmap = new Bitmap(filePath);

            _logger?.LogDebug($"Successfully loaded image from file: {filePath}");
            return new ImageSourceServiceResult(bitmap);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to load image from file: {uri}");
            throw;
        }
    }

    /// <summary>
    /// Clears the image cache
    /// </summary>
    public void ClearCache()
    {
        _logger?.LogDebug("Clearing image cache");
        _cache.Clear();
    }

    /// <summary>
    /// Removes a specific URI from the cache
    /// </summary>
    public bool RemoveFromCache(Uri uri)
    {
        var cacheKey = uri.ToString();
        var removed = _cache.TryRemove(cacheKey, out _);
        if (removed)
        {
            _logger?.LogDebug($"Removed image from cache: {uri}");
        }
        return removed;
    }
}