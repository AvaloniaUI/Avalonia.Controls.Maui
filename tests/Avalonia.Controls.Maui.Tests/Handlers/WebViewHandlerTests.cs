using Avalonia.Headless.XUnit;
using AvaloniaWebViewHandler = Avalonia.Controls.Maui.Handlers.WebViewHandler;
using MauiWebView = Microsoft.Maui.Controls.WebView;
using Microsoft.Maui;
using System.Reflection;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class WebViewHandlerTests : HandlerTestBase
{
    [Fact]
    public void ProcessTerminatedSupportMatchesNativeWebViewSurface()
    {
        var nativeEvent = typeof(Avalonia.Controls.NativeWebView).GetEvent("ProcessTerminated");

        Assert.Equal(nativeEvent is not null, AvaloniaWebViewHandler.SupportsProcessTerminated);
        Assert.False(AvaloniaWebViewHandler.SupportsProcessTerminated);
    }

    [AvaloniaFact(DisplayName = "UserAgent is retained for deferred adapter setup and can reset to default")]
    public async Task UserAgentMapsAndResets()
    {
        var webView = new MauiWebView
        {
            UserAgent = "Avalonia.Controls.Maui.Tests/1.0"
        };

        var handler = await CreateHandlerAsync<AvaloniaWebViewHandler>(webView);

        await InvokeOnMainThreadAsync(() =>
        {
            Assert.Equal(webView.UserAgent, handler.DesiredUserAgent);

            webView.ClearValue(MauiWebView.UserAgentProperty);
            handler.UpdateValue(nameof(MauiWebView.UserAgent));

            Assert.Null(handler.DesiredUserAgent);
        });
    }

    [AvaloniaFact(DisplayName = "Canceled navigation resets the pending navigation event")]
    public async Task CanceledNavigationResetsPendingNavigationEvent()
    {
        var webView = new MauiWebView();
        webView.Navigating += (_, args) =>
        {
            if (args.NavigationEvent == WebNavigationEvent.Back)
                args.Cancel = true;
        };

        var handler = await CreateHandlerAsync<AvaloniaWebViewHandler>(webView);

        await InvokeOnMainThreadAsync(() =>
        {
            SetCurrentNavigationEvent(handler, WebNavigationEvent.Back);
            Assert.Equal(WebNavigationEvent.Back, handler.CurrentNavigationEvent);

            var navigationArgs = new Avalonia.Controls.WebViewNavigationStartingEventArgs
            {
                Request = new Uri("https://example.com")
            };

            handler.HandleNavigationStarted(navigationArgs);

            Assert.True(navigationArgs.Cancel);
            Assert.Equal(WebNavigationEvent.NewPage, handler.CurrentNavigationEvent);
        });
    }

    [AvaloniaFact(DisplayName = "History state messages update CanGoBack and CanGoForward")]
    public async Task HistoryStateMessagesUpdateNavigationFlags()
    {
        var webView = new MauiWebView();
        var handler = await CreateHandlerAsync<AvaloniaWebViewHandler>(webView);

        await InvokeOnMainThreadAsync(() =>
        {
            var handled = handler.HandleWebMessageReceived(new Avalonia.Controls.WebMessageReceivedEventArgs
            {
                Body = """
                    {"type":"__AvaloniaControlsMaui_HistoryState","canGoBack":true,"canGoForward":false,"url":"https://avaloniaui.net/docs"}
                    """
            });

            Assert.True(handled);
            Assert.True(webView.CanGoBack);
            Assert.False(webView.CanGoForward);
        });
    }

    [AvaloniaFact(DisplayName = "Unknown WebView messages do not change navigation flags")]
    public async Task UnknownMessagesAreIgnored()
    {
        var webView = new MauiWebView();
        var handler = await CreateHandlerAsync<AvaloniaWebViewHandler>(webView);

        await InvokeOnMainThreadAsync(() =>
        {
            Assert.False(webView.CanGoBack);
            Assert.False(webView.CanGoForward);

            var handled = handler.HandleWebMessageReceived(new Avalonia.Controls.WebMessageReceivedEventArgs
            {
                Body = """{"type":"other","canGoBack":true,"canGoForward":true}"""
            });

            Assert.False(handled);
            Assert.False(webView.CanGoBack);
            Assert.False(webView.CanGoForward);
        });
    }

    [AvaloniaFact(DisplayName = "Same-document history messages clear pending back navigation state")]
    public async Task HistoryStateMessagesResetPendingBackNavigationEvent()
    {
        var webView = new MauiWebView();
        var handler = await CreateHandlerAsync<AvaloniaWebViewHandler>(webView);

        await InvokeOnMainThreadAsync(() =>
        {
            SetCurrentNavigationEvent(handler, WebNavigationEvent.Back);

            var handled = handler.HandleWebMessageReceived(new Avalonia.Controls.WebMessageReceivedEventArgs
            {
                Body = """
                    {"type":"__AvaloniaControlsMaui_HistoryState","canGoBack":true,"canGoForward":false,"url":"https://github.com/enterprise"}
                    """
            });

            Assert.True(handled);
            Assert.Equal(WebNavigationEvent.NewPage, handler.CurrentNavigationEvent);
        });
    }

    [AvaloniaFact(DisplayName = "GoBack and GoForward reset the navigation event when history is unavailable")]
    public async Task MissingHistoryResetsBackAndForwardNavigationEvents()
    {
        var webView = new MauiWebView();
        var handler = await CreateHandlerAsync<AvaloniaWebViewHandler>(webView);

        await InvokeOnMainThreadAsync(() =>
        {
            AvaloniaWebViewHandler.MapGoBack(handler, webView, null);
            Assert.Equal(WebNavigationEvent.NewPage, handler.CurrentNavigationEvent);

            AvaloniaWebViewHandler.MapGoForward(handler, webView, null);
            Assert.Equal(WebNavigationEvent.NewPage, handler.CurrentNavigationEvent);
        });
    }

    private static void SetCurrentNavigationEvent(AvaloniaWebViewHandler handler, WebNavigationEvent navigationEvent)
    {
        typeof(AvaloniaWebViewHandler)
            .GetField("_currentNavigationEvent", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(handler, navigationEvent);
    }
}
