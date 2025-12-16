using System.Net;
using Avalonia.Controls.Maui.Services;
using Avalonia.Controls.Maui.Tests;
using Avalonia;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaUriImageSourceServiceTests
{
    private static readonly byte[] PngBytes = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z/C/HwAFgwJ/lkVfoQAAAABJRU5ErkJggg==");

    static AvaloniaUriImageSourceServiceTests()
    {
        // Ensure Avalonia platform services are available for Bitmap creation
        TestAppBuilder.BuildAvaloniaApp().SetupWithoutStarting();
    }

    [Fact(DisplayName = "Uses cached file for subsequent requests", Skip = "https://github.com/AvaloniaUI/Avalonia.Controls.Maui/issues/74")]
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

    [Fact(DisplayName = "Expired cache downloads again", Skip = "https://github.com/AvaloniaUI/Avalonia.Controls.Maui/issues/74")]
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
