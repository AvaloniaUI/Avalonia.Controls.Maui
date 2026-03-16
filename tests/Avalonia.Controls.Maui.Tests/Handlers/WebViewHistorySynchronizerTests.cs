namespace Avalonia.Controls.Maui.Tests.Handlers;

public class WebViewHistorySynchronizerTests
{
    [Fact]
    public void MergeWithNativeStatePreservesRealBackHistory()
    {
        var state = new Avalonia.Controls.Maui.Handlers.WebViewHistoryState(
            CanGoBack: false,
            CanGoForward: false,
            Url: "https://github.com/enterprise");

        var merged = Avalonia.Controls.Maui.Handlers.WebViewHistorySynchronizer.MergeWithNativeState(
            state,
            nativeCanGoBack: true,
            nativeCanGoForward: false);

        Assert.True(merged.CanGoBack);
        Assert.False(merged.CanGoForward);
        Assert.Equal(state.Url, merged.Url);
    }

    [Fact]
    public void MergeWithNativeStateKeepsSameDocumentForwardHistory()
    {
        var state = new Avalonia.Controls.Maui.Handlers.WebViewHistoryState(
            CanGoBack: true,
            CanGoForward: true,
            Url: "https://avaloniaui.net/docs");

        var merged = Avalonia.Controls.Maui.Handlers.WebViewHistorySynchronizer.MergeWithNativeState(
            state,
            nativeCanGoBack: false,
            nativeCanGoForward: false);

        Assert.True(merged.CanGoBack);
        Assert.True(merged.CanGoForward);
        Assert.Equal(state.Url, merged.Url);
    }
}
