using Avalonia.Controls.Maui.Handlers;
using Avalonia.Headless.XUnit;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class WebViewSourceResolverTests
{
    private static readonly Uri RootAssetUri =
        new($"avares://{typeof(WebViewSourceResolverTests).Assembly.GetName().Name}/");

    [AvaloniaFact(DisplayName = "Resolves packaged html files to html content with a base uri")]
    public void ResolvesPackagedHtmlFiles()
    {
        var resolved = WebViewSourceResolver.TryResolve(
            "WebView/local.html",
            out var source,
            typeof(WebViewSourceResolverTests).Assembly);

        Assert.True(resolved);
        Assert.True(source.IsHtmlContent);
        Assert.Null(source.Uri);
        Assert.Equal(new Uri(RootAssetUri, "WebView/"), source.BaseUri);
        Assert.Contains("Relative stylesheet", source.Html);
    }

    [AvaloniaFact(DisplayName = "Relative base urls map to avares asset uris")]
    public void ResolvesRelativeBaseUrls()
    {
        var baseUri = WebViewSourceResolver.ResolveBaseUri(
            "WebView/",
            typeof(WebViewSourceResolverTests).Assembly);

        Assert.Equal(new Uri(RootAssetUri, "WebView/"), baseUri);
    }
}
