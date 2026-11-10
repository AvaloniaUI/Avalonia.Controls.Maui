using System.Net;
using System.Reflection;
using Avalonia.Controls.Maui.Services;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaUriImageSourceServiceTests
{
    // A valid 1x1 PNG; the service decodes with the real Skia backend, so the
    // payload must be a genuinely decodable image.
    private static readonly byte[] PngBytes = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR4nGP4z8DwHwAFAAH/iZk9HQAAAABJRU5ErkJggg==");

    [AvaloniaFact(DisplayName = "Default URI image service sets no User-Agent")]
    public void DefaultRequestSendsNoUserAgent()
    {
        using var client = new HttpClient(new CountingHandler(PngBytes));

        _ = new AvaloniaUriImageSourceService(null, client);

        Assert.Equal(string.Empty, client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [AvaloniaFact(DisplayName = "URI image service preserves supplied HttpClient User-Agent")]
    public void SuppliedHttpClientUserAgentIsPreserved()
    {
        using var client = new HttpClient(new CountingHandler(PngBytes));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Existing.Client/2.0");

        _ = new AvaloniaUriImageSourceService(null, client);

        Assert.Equal("Existing.Client/2.0", client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [AvaloniaFact(DisplayName = "Default image source registration sets no URI image User-Agent")]
    public void ConfigureImageSourcesSetsNoUriImageSourceUserAgent()
    {
        var builder = MauiApp.CreateBuilder();
        builder.ConfigureImageSources();

        using var app = builder.Build();
        var imageSourceServiceProvider = app.Services.GetRequiredService<IImageSourceServiceProvider>();
        var service = Assert.IsType<AvaloniaUriImageSourceService>(
            imageSourceServiceProvider.GetImageSourceService(typeof(IUriImageSource)));
        var httpClient = GetHttpClient(service);

        Assert.Equal(string.Empty, httpClient.DefaultRequestHeaders.UserAgent.ToString());
    }

    [AvaloniaFact(DisplayName = "Default image source registration uses registered HttpClient")]
    public void ConfigureImageSourcesUsesRegisteredHttpClient()
    {
        var builder = MauiApp.CreateBuilder();
        using var client = new HttpClient(new CountingHandler(PngBytes));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Registered.Client/5.0");

        builder.Services.AddSingleton(client);
        builder.ConfigureImageSources();

        using var app = builder.Build();
        var imageSourceServiceProvider = app.Services.GetRequiredService<IImageSourceServiceProvider>();
        var service = Assert.IsType<AvaloniaUriImageSourceService>(
            imageSourceServiceProvider.GetImageSourceService(typeof(IUriImageSource)));
        var httpClient = GetHttpClient(service);

        Assert.Same(client, httpClient);
        Assert.Equal("Registered.Client/5.0", httpClient.DefaultRequestHeaders.UserAgent.ToString());
    }

    [AvaloniaFact(DisplayName = "Uses cached file for subsequent requests")]
    public async Task UsesCachedFile()
    {
        var handler = new CountingHandler(PngBytes);
        var client = new HttpClient(handler);
        var service = new AvaloniaUriImageSourceService(null, client);
        var source = new UriImageSource
        {
            Uri = new Uri("http://example.com/image.png"),
            CacheValidity = TimeSpan.FromMinutes(5),
            CachingEnabled = true
        };

        DeleteCacheIfExists(source.Uri);

        try
        {
            var result1 = await service.GetImageAsync(source);
            Assert.NotNull(result1?.Value);
            Assert.Equal(1, handler.RequestCount);

            var cachePath = GetCachePath(source.Uri);
            Assert.True(File.Exists(cachePath));

            var service2 = new AvaloniaUriImageSourceService(null, client);
            var result2 = await service2.GetImageAsync(source);
            Assert.NotNull(result2?.Value);

            // Should be served from disk, not via HTTP
            Assert.Equal(1, handler.RequestCount);
        }
        finally
        {
            service.ClearCache();
        }
    }

    [AvaloniaFact(DisplayName = "Expired cache downloads again")]
    public async Task ExpiredCacheDownloadsAgain()
    {
        var handler = new CountingHandler(PngBytes);
        var client = new HttpClient(handler);
        var service = new AvaloniaUriImageSourceService(null, client);
        var source = new UriImageSource
        {
            Uri = new Uri("http://example.com/image.png"),
            CacheValidity = TimeSpan.FromSeconds(1),
            CachingEnabled = true
        };

        DeleteCacheIfExists(source.Uri);

        try
        {
            await service.GetImageAsync(source);
            Assert.Equal(1, handler.RequestCount);

            var cachePath = GetCachePath(source.Uri);
            Assert.True(File.Exists(cachePath));

            File.SetLastWriteTimeUtc(cachePath, DateTime.UtcNow - TimeSpan.FromSeconds(5));

            var service2 = new AvaloniaUriImageSourceService(null, client);
            await service2.GetImageAsync(source);

            Assert.Equal(2, handler.RequestCount);
        }
        finally
        {
            service.ClearCache();
        }
    }

    private static string GetCachePath(Uri uri)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(uri.ToString()));
        var safeName = BitConverter.ToString(hash).Replace("-", "");
        var ext = Path.GetExtension(uri.LocalPath);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".img";

        return Path.Combine(Path.GetTempPath(), "AvaloniaMauiImageCache", $"{safeName}{ext}");
    }

    private static void DeleteCacheIfExists(Uri uri)
    {
        var path = GetCachePath(uri);
        if (File.Exists(path))
        {
            try { File.Delete(path); } catch { }
        }
    }

    private static HttpClient GetHttpClient(AvaloniaUriImageSourceService service)
    {
        var field = typeof(AvaloniaUriImageSourceService).GetField(
            "_httpClient",
            BindingFlags.Instance | BindingFlags.NonPublic);

        return Assert.IsType<HttpClient>(field?.GetValue(service));
    }

    [AvaloniaFact(DisplayName = "Concurrent loads of one URI share a download and get bitmaps of their own")]
    public async Task ConcurrentLoadsShareDownloadNotBitmap()
    {
        var handler = new GatedHandler(CreatePng());
        var service = new AvaloniaUriImageSourceService(null, new HttpClient(handler));
        var source = new UriImageSource { Uri = new Uri($"http://example.com/{Guid.NewGuid():N}.png"), CachingEnabled = true };

        try
        {
            var first = service.GetImageAsync(source);
            var second = service.GetImageAsync(source);
            handler.Release();

            using var firstResult = await first;
            using var secondResult = await second;

            Assert.Equal(1, handler.RequestCount);
            Assert.NotSame(firstResult!.Value, secondResult!.Value);

            // Disposing one control's image leaves the other's usable
            firstResult.Dispose();
            Assert.Equal(new PixelSize(1, 1), Assert.IsAssignableFrom<Avalonia.Media.Imaging.Bitmap>(secondResult.Value).PixelSize);
        }
        finally
        {
            File.Delete(service.GetCachePath(source.Uri));
        }
    }

    [AvaloniaFact(DisplayName = "A cancelled load does not cancel another load of the same URI")]
    public async Task CancelledLoadDoesNotCancelSharedDownload()
    {
        var handler = new GatedHandler(CreatePng());
        var service = new AvaloniaUriImageSourceService(null, new HttpClient(handler));
        var source = new UriImageSource { Uri = new Uri($"http://example.com/{Guid.NewGuid():N}.png"), CachingEnabled = true };
        using var cancellation = new CancellationTokenSource();

        try
        {
            var cancelled = service.GetImageAsync(source, 1, cancellation.Token);
            var other = service.GetImageAsync(source);

            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelled);

            handler.Release();
            using var result = await other;

            Assert.NotNull(result?.Value);
            Assert.Equal(1, handler.RequestCount);
        }
        finally
        {
            File.Delete(service.GetCachePath(source.Uri));
        }
    }

    static byte[] CreatePng()
    {
        using var bitmap = new Avalonia.Media.Imaging.RenderTargetBitmap(new PixelSize(1, 1));
        using var stream = new MemoryStream();
        bitmap.Save(stream);
        return stream.ToArray();
    }

    private sealed class GatedHandler : HttpMessageHandler
    {
        private readonly byte[] _payload;
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _requestCount;

        public GatedHandler(byte[] payload) => _payload = payload;

        public int RequestCount => _requestCount;

        public void Release() => _gate.TrySetResult();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _requestCount);
            await _gate.Task.WaitAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(_payload) };
        }
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        private readonly byte[] _payload;

        public int RequestCount { get; private set; }

        public CountingHandler(byte[] payload)
        {
            _payload = payload;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(_payload)
            };
            return Task.FromResult(response);
        }
    }
}
