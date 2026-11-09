using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using IImage = Avalonia.Media.IImage;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Avalonia implementation of <see cref="IImageSourceService"/> that loads images from URI-based sources with optional disk caching.
/// </summary>
/// <remarks>
/// Supports HTTP/HTTPS and file URIs. When caching is enabled on the image source, downloaded images
/// are persisted to a temporary directory and reused until the configured cache validity period expires.
/// Concurrent requests for the same URI share one download, and each request gets a bitmap of its own.
/// </remarks>
public partial class AvaloniaUriImageSourceService : IAvaloniaImageSourceService, IImageSourceService<IUriImageSource>
{
    private readonly ILogger<AvaloniaUriImageSourceService>? _logger;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, Lazy<Task<byte[]>>> _inFlightDownloads;

    private static readonly string CacheDirectory =
        Path.Combine(Path.GetTempPath(), "AvaloniaMauiImageCache");

    /// <summary>
    /// Default constructor kept for compatibility with reflection-based activators.
    /// </summary>
    public AvaloniaUriImageSourceService()
        : this(logger: null, httpClient: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaUriImageSourceService"/> class.
    /// </summary>
    /// <param name="logger">An optional logger for diagnostic messages during image loading and caching.</param>
    /// <param name="httpClient">An optional <see cref="HttpClient"/> for downloading remote images. A default client is created if not provided.</param>
    public AvaloniaUriImageSourceService(ILogger<AvaloniaUriImageSourceService>? logger = null, HttpClient? httpClient = null)
    {
        _logger = logger;
        _httpClient = httpClient ?? new HttpClient();
        _inFlightDownloads = new ConcurrentDictionary<string, Lazy<Task<byte[]>>>();

        EnsureCacheDirectory();
    }

    /// <summary>
    /// Attempts to load a bitmap from the specified image source by casting it to <see cref="IUriImageSource"/>.
    /// </summary>
    /// <param name="imageSource">The image source to load. Must implement <see cref="IUriImageSource"/> to produce a result.</param>
    /// <param name="scale">The display scale factor applied during image loading.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{IImage}"/> containing the loaded bitmap, or <see langword="null"/>
    /// if the source is not a URI image source.
    /// </returns>
    public Task<IImageSourceServiceResult<IImage>?> GetImageAsync(
        IImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource is IUriImageSource uriImageSource)
        {
            return GetImageAsync(uriImageSource, scale, cancellationToken);
        }

        return Task.FromResult<IImageSourceServiceResult<IImage>?>(null);
    }

    /// <summary>
    /// Loads a bitmap from the URI specified by the <see cref="IUriImageSource"/>, using disk caching when enabled.
    /// </summary>
    /// <param name="imageSource">The URI image source containing the target URI and caching configuration.</param>
    /// <param name="scale">The display scale factor applied during image loading.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{IImage}"/> containing the loaded bitmap, or <see langword="null"/>
    /// if the URI is <see langword="null"/> or uses an unsupported scheme.
    /// </returns>
    public async Task<IImageSourceServiceResult<IImage>?> GetImageAsync(
        IUriImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource?.Uri == null)
            return null;

        try
        {
            var uri = imageSource.Uri;

            // Try disk cache first when enabled
            if (imageSource.CachingEnabled &&
                TryLoadFromCache(uri, imageSource.CacheValidity, out var cachedResult))
            {
                return cachedResult;
            }

            return await LoadImageInternalAsync(uri, imageSource, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading image from URI: {Uri}", imageSource.Uri);
            throw;
        }
    }

    private async Task<IImageSourceServiceResult<IImage>?> LoadImageInternalAsync(Uri uri, IUriImageSource imageSource, CancellationToken cancellationToken)
    {
        // Handle different URI schemes
        if (uri.IsAbsoluteUri)
        {
            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                return await LoadFromHttpAsync(uri, imageSource, cancellationToken);
            }
            else if (uri.Scheme == Uri.UriSchemeFile)
            {
                return LoadFromFile(uri);
            }
        }

        _logger?.LogWarning($"Unsupported URI scheme: {uri}");
        return null;
    }

    private async Task<IImageSourceServiceResult<IImage>?> LoadFromHttpAsync(Uri uri, IUriImageSource imageSource, CancellationToken cancellationToken)
    {
        try
        {
            _logger?.LogDebug($"Loading image from HTTP: {uri}");

            // Each caller decodes its own bitmap, because the loader that receives the result disposes it
            var bytes = imageSource.CachingEnabled
                ? await DownloadSharedAsync(uri).WaitAsync(cancellationToken).ConfigureAwait(false)
                : await DownloadAsync(uri, writeCache: false, cancellationToken).ConfigureAwait(false);

            await using var memoryStream = new MemoryStream(bytes);
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

    private async Task<byte[]> DownloadAsync(Uri uri, bool writeCache, CancellationToken cancellationToken)
    {
        var bytes = await _httpClient.GetByteArrayAsync(uri, cancellationToken).ConfigureAwait(false);

        if (writeCache)
        {
            TryWriteCache(uri, bytes);
        }

        return bytes;
    }

    // One download per URI in flight. No single caller cancels it, so the others still get the bytes.
    private Task<byte[]> DownloadSharedAsync(Uri uri)
    {
        var key = uri.ToString();
        Lazy<Task<byte[]>>? created = null;
        var download = _inFlightDownloads.GetOrAdd(key, _ =>
            created = new Lazy<Task<byte[]>>(() => DownloadOnceAsync(key, created!, uri)));

        return download.Value;
    }

    private async Task<byte[]> DownloadOnceAsync(string key, Lazy<Task<byte[]>> download, Uri uri)
    {
        try
        {
            return await DownloadAsync(uri, writeCache: true, CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            // Remove only this download, not a newer one for the same URI
            _inFlightDownloads.TryRemove(new KeyValuePair<string, Lazy<Task<byte[]>>>(key, download));
        }
    }

    private IImageSourceServiceResult<IImage>? LoadFromFile(Uri uri)
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
        _inFlightDownloads.Clear();
        try
        {
            if (Directory.Exists(CacheDirectory))
            {
                Directory.Delete(CacheDirectory, true);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to clear cache directory");
        }
    }

    /// <summary>
    /// Removes a specific URI from the cache
    /// </summary>
    public bool RemoveFromCache(Uri uri)
    {
        var cacheKey = uri.ToString();
        var removed = _inFlightDownloads.TryRemove(cacheKey, out _);
        if (removed)
        {
            _logger?.LogDebug($"Removed image from cache: {uri}");
        }

        var cachePath = GetCachePath(uri);
        if (File.Exists(cachePath))
        {
            try
            {
                File.Delete(cachePath);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to delete cached file for {Uri}", uri);
            }
        }

        return removed;
    }

    internal string GetCachePath(Uri uri)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(uri.ToString()));
        var safeName = BitConverter.ToString(hash).Replace("-", "");
        var ext = Path.GetExtension(uri.LocalPath);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".img";

        return Path.Combine(CacheDirectory, $"{safeName}{ext}");
    }

    private bool TryLoadFromCache(Uri uri, TimeSpan cacheValidity, out IImageSourceServiceResult<IImage>? result)
    {
        result = null;
        var cachePath = GetCachePath(uri);

        try
        {
            if (!File.Exists(cachePath))
                return false;

            var info = new FileInfo(cachePath);
            if (DateTime.UtcNow - info.LastWriteTimeUtc > cacheValidity)
            {
                File.Delete(cachePath);
                return false;
            }

            using var stream = File.OpenRead(cachePath);
            result = new ImageSourceServiceResult(new Bitmap(stream));
            _logger?.LogDebug("Loaded image from disk cache: {Uri}", uri);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to read cache for {Uri}", uri);
            return false;
        }
    }

    private void TryWriteCache(Uri uri, byte[] bytes)
    {
        try
        {
            var cachePath = GetCachePath(uri);
            EnsureCacheDirectory();
            File.WriteAllBytes(cachePath, bytes);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to write cache for {Uri}", uri);
        }
    }

    private static void EnsureCacheDirectory()
    {
        try
        {
            if (!Directory.Exists(CacheDirectory))
            {
                Directory.CreateDirectory(CacheDirectory);
            }
        }
        catch
        {
            // Ignore, cache will simply not persist
        }
    }
}
