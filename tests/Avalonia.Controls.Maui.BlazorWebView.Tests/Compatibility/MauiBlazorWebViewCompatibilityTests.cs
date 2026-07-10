using Avalonia.Controls.Maui.BlazorWebView.Compatibility;
using Avalonia.Controls.Maui.BlazorWebView.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.BlazorWebView.Tests.Compatibility;

public class MauiBlazorWebViewCompatibilityTests
{
    [Fact]
    public void AreDeveloperToolsEnabled_UsesOfficialMauiRegistration()
    {
        var services = new ServiceCollection();
        services.AddMauiBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();

        using var provider = services.BuildServiceProvider();

        Assert.True(MauiBlazorWebViewCompatibility.AreDeveloperToolsEnabled(provider, NullLogger.Instance));
    }

    [Fact]
    public void SetContainerView_UpdatesNeutralMauiHandlerContainer()
    {
        var handler = new BlazorWebViewHandler();
        var container = new object();

        MauiBlazorWebViewCompatibility.SetContainerView(handler, container);

        Assert.Same(container, ((IViewHandler)handler).ContainerView);
    }

    [Fact]
    public void StaticContentHotReloadHooks_MatchTheCurrentMauiContract()
    {
        Assert.True(MauiBlazorWebViewCompatibility.IsStaticContentHotReloadSupported);
    }

    [Fact]
    public void TryReplaceStaticContentHotReloadResponse_ReturnsFalseWhenNoReplacementExists()
    {
        var statusCode = 404;
        Stream content = Stream.Null;
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var replaced = MauiBlazorWebViewCompatibility.TryReplaceStaticContentHotReloadResponse(
            "wwwroot",
            "http://127.0.0.1:5000/app.css",
            ref statusCode,
            ref content,
            headers,
            NullLogger.Instance);

        Assert.False(replaced);
        Assert.Same(Stream.Null, content);
    }
}
