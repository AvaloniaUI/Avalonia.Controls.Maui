using System.Net;
using Avalonia.Controls.Maui.BlazorWebView.Hosting;
using Avalonia.Controls;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using BlazorDispatcher = Microsoft.AspNetCore.Components.Dispatcher;

namespace Avalonia.Controls.Maui.BlazorWebView.Tests.Hosting;

public class AvaloniaBlazorWebViewManagerTests
{
    private static readonly Uri AppBaseUri = new("http://127.0.0.1:5000/");

    [Theory]
    [InlineData("weather", null, false, false)]
    [InlineData("weather", null, true, true)]
    [InlineData("weather/today", "*/*", false, false)]
    [InlineData("weather/today", "*/*", true, true)]
    [InlineData("users/jane.doe", "text/html,application/xhtml+xml", false, true)]
    [InlineData("users/jane.doe", "application/xhtml+xml", false, true)]
    [InlineData("_framework/blazor.webview.js", "application/javascript", false, false)]
    [InlineData("css/site.css", "*/*", false, false)]
    [InlineData("api/report.json", "application/json", false, false)]
    public void ShouldFallbackToHostPage_OnlyFallsBackForAppDocumentRoutes(
        string relativeUri,
        string? acceptHeader,
        bool isPendingNavigationRequest,
        bool expected)
    {
        Assert.Equal(
            expected,
            AvaloniaBlazorWebViewManager.ShouldFallbackToHostPage(
                AppBaseUri,
                new Uri(AppBaseUri, relativeUri),
                acceptHeader,
                isPendingNavigationRequest));
    }

    [Fact]
    public void ShouldFallbackToHostPage_RejectsExternalUris()
    {
        Assert.False(AvaloniaBlazorWebViewManager.ShouldFallbackToHostPage(
            AppBaseUri,
            new Uri("https://example.com/users/jane.doe"),
            "text/html",
            isPendingNavigationRequest: true));
    }

    [Fact]
    public async Task ContentHost_ServesStaticAssetsWithContentType()
    {
        await using var fixture = CreateFixture();
        using var client = new HttpClient { BaseAddress = fixture.BaseUri };

        var response = await client.GetAsync("app.js");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/javascript", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("console.log('loaded');", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ContentHost_HeadReturnsHeadersWithoutBody()
    {
        await using var fixture = CreateFixture();
        using var client = new HttpClient { BaseAddress = fixture.BaseUri };
        using var request = new HttpRequestMessage(HttpMethod.Head, "index.html");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task ContentHost_FallsBackToHostPageForDocumentRoute()
    {
        await using var fixture = CreateFixture();
        using var client = new HttpClient { BaseAddress = fixture.BaseUri };
        using var request = new HttpRequestMessage(HttpMethod.Get, "counter");
        request.Headers.Accept.ParseAdd("text/html");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("<html><body id=\"app\"></body></html>", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ContentHost_DoesNotFallbackForMissingAsset()
    {
        await using var fixture = CreateFixture();
        using var client = new HttpClient { BaseAddress = fixture.BaseUri };

        var response = await client.GetAsync("missing.json");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ContentHost_RejectsUnsupportedMethods()
    {
        await using var fixture = CreateFixture();
        using var client = new HttpClient { BaseAddress = fixture.BaseUri };

        var response = await client.PostAsync("index.html", new StringContent(string.Empty));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    private static ManagerFixture CreateFixture()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMauiBlazorWebView();

        var provider = services.BuildServiceProvider();
        var host = BlazorWebViewLoopbackHost.Create();
        var files = new MemoryFileProvider(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["index.html"] = "<html><body id=\"app\"></body></html>",
            ["app.js"] = "console.log('loaded');",
        });

        var manager = new AvaloniaBlazorWebViewManager(
            new NativeWebView(),
            provider,
            BlazorDispatcher.CreateDefault(),
            host,
            files,
            new JSComponentConfigurationStore(),
            string.Empty,
            "index.html",
            NullLogger.Instance);

        return new ManagerFixture(provider, manager, host.BaseUri);
    }

    private sealed class ManagerFixture : IAsyncDisposable
    {
        private readonly ServiceProvider _services;
        private readonly AvaloniaBlazorWebViewManager _manager;

        public ManagerFixture(ServiceProvider services, AvaloniaBlazorWebViewManager manager, Uri baseUri)
        {
            _services = services;
            _manager = manager;
            BaseUri = baseUri;
        }

        public Uri BaseUri { get; }

        public async ValueTask DisposeAsync()
        {
            await _manager.DisposeAsync();
            await _services.DisposeAsync();
        }
    }

    private sealed class MemoryFileProvider : IFileProvider
    {
        private readonly IReadOnlyDictionary<string, byte[]> _files;

        public MemoryFileProvider(IReadOnlyDictionary<string, string> files)
        {
            _files = files.ToDictionary(
                pair => pair.Key,
                pair => System.Text.Encoding.UTF8.GetBytes(pair.Value),
                StringComparer.Ordinal);
        }

        public IDirectoryContents GetDirectoryContents(string subpath) =>
            NotFoundDirectoryContents.Singleton;

        public IFileInfo GetFileInfo(string subpath)
        {
            var path = subpath.Replace('\\', '/').TrimStart('/');
            return _files.TryGetValue(path, out var content)
                ? new MemoryFileInfo(path, content)
                : new NotFoundFileInfo(Path.GetFileName(path));
        }

        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;
    }

    private sealed class MemoryFileInfo : IFileInfo
    {
        private readonly byte[] _content;

        public MemoryFileInfo(string name, byte[] content)
        {
            Name = name;
            _content = content;
        }

        public bool Exists => true;
        public long Length => _content.Length;
        public string? PhysicalPath => null;
        public string Name { get; }
        public DateTimeOffset LastModified => DateTimeOffset.MinValue;
        public bool IsDirectory => false;
        public Stream CreateReadStream() => new MemoryStream(_content, writable: false);
    }
}
