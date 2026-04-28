using System.Net;
using System.Reflection;
using Avalonia.Controls.Maui.Services;
using Avalonia.Controls.Maui.Tests;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

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

    [Fact(DisplayName = "Default URI image service sets no User-Agent")]
    public void DefaultRequestSendsNoUserAgent()
    {
        using var client = new HttpClient(new CountingHandler(PngBytes));

        _ = new AvaloniaUriImageSourceService(null, client);

        Assert.Equal(string.Empty, client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact(DisplayName = "Configured User-Agent is applied to URI image service")]
    public void ConfiguredUserAgentIsSent()
    {
        using var client = new HttpClient(new CountingHandler(PngBytes));
        var options = new AvaloniaUriImageSourceServiceOptions
        {
            UserAgent = "Avalonia.Controls.Maui.Tests/1.0"
        };

        _ = new AvaloniaUriImageSourceService(null, client, options);

        Assert.Equal("Avalonia.Controls.Maui.Tests/1.0", client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact(DisplayName = "Existing HttpClient User-Agent is preserved when not configured")]
    public void ExistingHttpClientUserAgentIsPreservedWhenNotConfigured()
    {
        using var client = new HttpClient(new CountingHandler(PngBytes));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Existing.Client/2.0");

        _ = new AvaloniaUriImageSourceService(null, client);

        Assert.Equal("Existing.Client/2.0", client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact(DisplayName = "Configured User-Agent replaces existing HttpClient User-Agent")]
    public void ConfiguredUserAgentReplacesExistingHttpClientUserAgent()
    {
        using var client = new HttpClient(new CountingHandler(PngBytes));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Existing.Client/2.0");
        var options = new AvaloniaUriImageSourceServiceOptions
        {
            UserAgent = "Configured.Client/3.0"
        };

        _ = new AvaloniaUriImageSourceService(null, client, options);

        Assert.Equal("Configured.Client/3.0", client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact(DisplayName = "Blank configured User-Agent preserves existing HttpClient User-Agent")]
    public void BlankConfiguredUserAgentPreservesExistingHttpClientUserAgent()
    {
        using var client = new HttpClient(new CountingHandler(PngBytes));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Existing.Client/2.0");
        var options = new AvaloniaUriImageSourceServiceOptions
        {
            UserAgent = " "
        };

        _ = new AvaloniaUriImageSourceService(null, client, options);

        Assert.Equal("Existing.Client/2.0", client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact(DisplayName = "App builder registers configured URI image User-Agent")]
    public void ConfigureAvaloniaUriImageSourceServiceRegistersUserAgent()
    {
        var builder = MauiApp.CreateBuilder();
        builder.ConfigureAvaloniaUriImageSourceService(options =>
        {
            options.UserAgent = "Configured.App/3.0";
        });

        using var provider = builder.Services.BuildServiceProvider();
        var options = provider.GetRequiredService<AvaloniaUriImageSourceServiceOptions>();

        Assert.Equal("Configured.App/3.0", options.UserAgent);
    }

    [Fact(DisplayName = "Configured URI image options replace default registration")]
    public void ConfigureAvaloniaUriImageSourceServiceReplacesDefaultOptionsRegistration()
    {
        var builder = MauiApp.CreateBuilder();
        builder.ConfigureImageSources();
        builder.ConfigureAvaloniaUriImageSourceService(options =>
        {
            options.UserAgent = "Configured.App/3.0";
        });

        var registrations = builder.Services
            .Where(descriptor => descriptor.ServiceType == typeof(AvaloniaUriImageSourceServiceOptions))
            .ToList();

        using var provider = builder.Services.BuildServiceProvider();
        var options = provider.GetServices<AvaloniaUriImageSourceServiceOptions>().ToList();

        Assert.Single(registrations);
        Assert.Single(options);
        Assert.Equal("Configured.App/3.0", options[0].UserAgent);
    }

    [Fact(DisplayName = "Default image source registration applies configured URI image User-Agent")]
    public void ConfigureImageSourcesAppliesConfiguredUriImageSourceUserAgent()
    {
        var builder = MauiApp.CreateBuilder();
        builder.ConfigureImageSources();
        builder.ConfigureAvaloniaUriImageSourceService(options =>
        {
            options.UserAgent = "Configured.Images/4.0";
        });

        using var app = builder.Build();
        var imageSourceServiceProvider = app.Services.GetRequiredService<IImageSourceServiceProvider>();
        var service = Assert.IsType<AvaloniaUriImageSourceService>(
            imageSourceServiceProvider.GetImageSourceService(typeof(IUriImageSource)));
        var httpClient = GetHttpClient(service);

        Assert.Equal("Configured.Images/4.0", httpClient.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact(DisplayName = "Default image source registration uses registered HttpClient")]
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

    private static HttpClient GetHttpClient(AvaloniaUriImageSourceService service)
    {
        var field = typeof(AvaloniaUriImageSourceService).GetField(
            "_httpClient",
            BindingFlags.Instance | BindingFlags.NonPublic);

        return Assert.IsType<HttpClient>(field?.GetValue(service));
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
