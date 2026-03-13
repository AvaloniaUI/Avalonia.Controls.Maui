using System;
using System.Threading.Tasks;
using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="WebView"/>.</summary>
/// <remarks>
/// On platforms where <see cref="Avalonia.Controls.NativeWebView"/> is not supported
/// (Linux and Browser), a <see cref="PlaceholderControl"/> is shown instead.
/// </remarks>
public class WebViewHandler : ViewHandler<IWebView, Avalonia.Controls.Control>, IWebViewDelegate
{
    private WebNavigationEvent _currentNavigationEvent = WebNavigationEvent.NewPage;

    /// <summary>Property mapper for <see cref="WebViewHandler"/>.</summary>
    public static IPropertyMapper<IWebView, WebViewHandler> Mapper = new PropertyMapper<IWebView, WebViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IWebView.Source)] = MapSource,
        [nameof(IWebView.UserAgent)] = MapUserAgent,
    };

    /// <summary>Command mapper for <see cref="WebViewHandler"/>.</summary>
    public static CommandMapper<IWebView, WebViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(IWebView.GoBack)] = MapGoBack,
        [nameof(IWebView.GoForward)] = MapGoForward,
        [nameof(IWebView.Reload)] = MapReload,
        [nameof(IWebView.Eval)] = MapEval,
        [nameof(IWebView.EvaluateJavaScriptAsync)] = MapEvaluateJavaScriptAsync,
    };

    /// <summary>Initializes a new instance of <see cref="WebViewHandler"/>.</summary>
    public WebViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="WebViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public WebViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="WebViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public WebViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    private static bool IsWebViewSupported => true;

    private Avalonia.Controls.NativeWebView? NativeWebView =>
        PlatformView as Avalonia.Controls.NativeWebView;

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override Avalonia.Controls.Control CreatePlatformView()
    {
        if (IsWebViewSupported)
            return new Avalonia.Controls.NativeWebView();

        return new PlaceholderControl("WebView is not supported on this platform");
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(Avalonia.Controls.Control platformView)
    {
        base.ConnectHandler(platformView);
        if (platformView is Avalonia.Controls.NativeWebView nativeWebView)
        {
            nativeWebView.NavigationStarted += OnNavigationStarted;
            nativeWebView.NavigationCompleted += OnNavigationCompleted;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(Avalonia.Controls.Control platformView)
    {
        if (platformView is Avalonia.Controls.NativeWebView nativeWebView)
        {
            nativeWebView.NavigationStarted -= OnNavigationStarted;
            nativeWebView.NavigationCompleted -= OnNavigationCompleted;
        }

        base.DisconnectHandler(platformView);
    }

    private void OnNavigationStarted(object? sender, Avalonia.Controls.WebViewNavigationStartingEventArgs e)
    {
        if (VirtualView is null)
            return;

        var url = e.Request?.AbsoluteUri ?? string.Empty;
        var cancel = VirtualView.Navigating(_currentNavigationEvent, url);
        e.Cancel = cancel;
    }

    private void OnNavigationCompleted(object? sender, Avalonia.Controls.WebViewNavigationCompletedEventArgs e)
    {
        if (VirtualView is null || NativeWebView is not { } webView)
            return;

        var url = e.Request?.AbsoluteUri ?? string.Empty;
        var result = e.IsSuccess ? WebNavigationResult.Success : WebNavigationResult.Failure;
        VirtualView.Navigated(_currentNavigationEvent, url, result);

        webView.UpdateCanGoBackForward(VirtualView);
        _currentNavigationEvent = WebNavigationEvent.NewPage;
    }

    /// <summary>Maps the Source property to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    public static void MapSource(WebViewHandler handler, IWebView webView)
    {
        if (handler.NativeWebView is null)
            return;

        webView.Source?.Load(handler);
        handler.NativeWebView.UpdateCanGoBackForward(webView);
    }

    /// <summary>Maps the UserAgent property to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    public static void MapUserAgent(WebViewHandler handler, IWebView webView)
    {
        // UserAgent is not directly supported by NativeWebView.
    }

    /// <summary>Maps the GoBack command to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
    {
        if (handler.NativeWebView is { } webViewControl)
        {
            handler._currentNavigationEvent = WebNavigationEvent.Back;
            webViewControl.GoBack();
            webViewControl.UpdateCanGoBackForward(webView);
        }
    }

    /// <summary>Maps the GoForward command to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
    {
        if (handler.NativeWebView is { } webViewControl)
        {
            handler._currentNavigationEvent = WebNavigationEvent.Forward;
            webViewControl.GoForward();
            webViewControl.UpdateCanGoBackForward(webView);
        }
    }

    /// <summary>Maps the Reload command to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
    {
        if (handler.NativeWebView is { } webViewControl)
        {
            handler._currentNavigationEvent = WebNavigationEvent.Refresh;
            webViewControl.Refresh();
        }
    }

    /// <summary>Maps the Eval command to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    /// <param name="arg">The command argument (script string).</param>
    public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
    {
        if (handler.NativeWebView is { } webViewControl && arg is string script)
        {
            _ = webViewControl.InvokeScript(script);
        }
    }

    /// <summary>Maps the EvaluateJavaScriptAsync command to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    /// <param name="arg">The command argument (EvaluateJavaScriptAsyncRequest).</param>
    public static void MapEvaluateJavaScriptAsync(WebViewHandler handler, IWebView webView, object? arg)
    {
        if (handler.NativeWebView is { } webViewControl && arg is EvaluateJavaScriptAsyncRequest request)
        {
            EvaluateJavaScript(webViewControl, request);
        }
    }

    private static async void EvaluateJavaScript(Avalonia.Controls.NativeWebView webViewControl, EvaluateJavaScriptAsyncRequest request)
    {
        try
        {
            var result = await webViewControl.InvokeScript(request.Script);
            request.SetResult(result ?? "null");
        }
        catch (Exception ex)
        {
            request.SetException(ex);
        }
    }

    /// <inheritdoc/>
    void IWebViewDelegate.LoadHtml(string? html, string? baseUrl)
    {
        if (NativeWebView is not { } webViewControl || html is null)
            return;

        webViewControl.NavigateToString(html);
    }

    /// <inheritdoc/>
    void IWebViewDelegate.LoadUrl(string? url)
    {
        if (NativeWebView is not { } webViewControl || url is null)
            return;

        webViewControl.Navigate(new Uri(url));
    }
}
