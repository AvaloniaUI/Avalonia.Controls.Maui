using Avalonia.Controls.Maui.BlazorWebView.Handlers;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using AvaloniaBlazorWebViewHandler = Avalonia.Controls.Maui.BlazorWebView.Handlers.BlazorWebViewHandler;
using NativeWebView = Avalonia.Controls.NativeWebView;

namespace Avalonia.Controls.Maui.BlazorWebView.Tests.Handlers;

public class BlazorWebViewHandlerTests
{
    private static readonly Uri AppBaseUri = new("http://127.0.0.1:5000/");

    [Theory]
    [InlineData("wwwroot/index.html", "wwwroot", "index.html")]
    [InlineData("wwwroot\\index.html", "wwwroot", "index.html")]
    [InlineData("index.html", "", "index.html")]
    public void GetHostPagePaths_NormalizesDirectorySeparators(
        string hostPage,
        string expectedContentRoot,
        string expectedHostPage)
    {
        var (contentRoot, relativeHostPage) = AvaloniaBlazorWebViewHandler.GetHostPagePaths(hostPage);

        Assert.Equal(expectedContentRoot, contentRoot);
        Assert.Equal(expectedHostPage, relativeHostPage);
    }

    [Fact]
    public void CreateUrlLoadingEventArgs_UsesMauiDefaultLoadingStrategy()
    {
        var appUrl = new Uri(AppBaseUri, "counter");
        var externalUrl = new Uri("https://example.com/");

        var appArgs = AvaloniaBlazorWebViewHandler.CreateUrlLoadingEventArgs(appUrl, AppBaseUri);
        var externalArgs = AvaloniaBlazorWebViewHandler.CreateUrlLoadingEventArgs(externalUrl, AppBaseUri);

        Assert.Same(appUrl, appArgs.Url);
        Assert.Equal(UrlLoadingStrategy.OpenInWebView, appArgs.UrlLoadingStrategy);
        Assert.Same(externalUrl, externalArgs.Url);
        Assert.Equal(UrlLoadingStrategy.OpenExternally, externalArgs.UrlLoadingStrategy);
    }

    [Fact]
    public async Task SetVirtualView_CreatesAvaloniaPlatformViewAndRaisesInitializingEvent()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMauiBlazorWebView();

        await using var provider = services.BuildServiceProvider();
        var context = new TestMauiContext(provider);
        var webView = new Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebView
        {
            HostPage = string.Empty,
        };
        var handler = new AvaloniaBlazorWebViewHandler();
        AvaloniaBlazorWebViewInitializingEventArgs? initializingArgs = null;
        var initializedCount = 0;

        webView.BlazorWebViewInitializing += (_, args) =>
            initializingArgs = Assert.IsType<AvaloniaBlazorWebViewInitializingEventArgs>(args);
        webView.BlazorWebViewInitialized += (_, _) => initializedCount++;

        handler.SetMauiContext(context);
        webView.Handler = handler;
        handler.SetVirtualView(webView);

        try
        {
            var platformView = Assert.IsType<NativeWebView>(((IElementHandler)handler).PlatformView);
            Assert.Same(platformView, initializingArgs?.WebView);
            Assert.Equal(0, initializedCount);
            Assert.False(await handler.TryDispatchAsync(_ => { }));
        }
        finally
        {
            ((IElementHandler)handler).DisconnectHandler();
        }

        Assert.Null(webView.Handler);
    }

    private sealed class TestMauiContext : IMauiContext
    {
        public TestMauiContext(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        public IMauiHandlersFactory Handlers => Services.GetService<IMauiHandlersFactory>()!;

        public Microsoft.Maui.Animations.IAnimationManager? AnimationManager =>
            Services.GetService<Microsoft.Maui.Animations.IAnimationManager>();
    }
}
