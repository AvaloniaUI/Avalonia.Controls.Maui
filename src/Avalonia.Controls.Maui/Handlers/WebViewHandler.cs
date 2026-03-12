using System;
using System.Threading.Tasks;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="WebView"/>.</summary>
public class WebViewHandler : ViewHandler<IWebView, Avalonia.Controls.NativeWebView>, IWebViewDelegate
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

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override Avalonia.Controls.NativeWebView CreatePlatformView()
    {
        return new Avalonia.Controls.NativeWebView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(Avalonia.Controls.NativeWebView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.NavigationStarted += OnNavigationStarted;
        platformView.NavigationCompleted += OnNavigationCompleted;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(Avalonia.Controls.NativeWebView platformView)
    {
        platformView.NavigationStarted -= OnNavigationStarted;
        platformView.NavigationCompleted -= OnNavigationCompleted;
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
        if (VirtualView is null || PlatformView is null)
            return;

        var url = e.Request?.AbsoluteUri ?? string.Empty;
        var result = e.IsSuccess ? WebNavigationResult.Success : WebNavigationResult.Failure;
        VirtualView.Navigated(_currentNavigationEvent, url, result);

        PlatformView.UpdateCanGoBackForward(VirtualView);
        _currentNavigationEvent = WebNavigationEvent.NewPage;
    }

    /// <summary>Maps the Source property to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    public static void MapSource(WebViewHandler handler, IWebView webView)
    {
        webView.Source?.Load(handler);
        handler.PlatformView?.UpdateCanGoBackForward(webView);
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
        if (handler.PlatformView is { } platformView)
        {
            handler._currentNavigationEvent = WebNavigationEvent.Back;
            platformView.GoBack();
            platformView.UpdateCanGoBackForward(webView);
        }
    }

    /// <summary>Maps the GoForward command to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
    {
        if (handler.PlatformView is { } platformView)
        {
            handler._currentNavigationEvent = WebNavigationEvent.Forward;
            platformView.GoForward();
            platformView.UpdateCanGoBackForward(webView);
        }
    }

    /// <summary>Maps the Reload command to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
    {
        if (handler.PlatformView is { } platformView)
        {
            handler._currentNavigationEvent = WebNavigationEvent.Refresh;
            platformView.Refresh();
        }
    }

    /// <summary>Maps the Eval command to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    /// <param name="arg">The command argument (script string).</param>
    public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
    {
        if (handler.PlatformView is { } platformView && arg is string script)
        {
            _ = platformView.InvokeScript(script);
        }
    }

    /// <summary>Maps the EvaluateJavaScriptAsync command to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    /// <param name="arg">The command argument (EvaluateJavaScriptAsyncRequest).</param>
    public static void MapEvaluateJavaScriptAsync(WebViewHandler handler, IWebView webView, object? arg)
    {
        if (handler.PlatformView is { } platformView && arg is EvaluateJavaScriptAsyncRequest request)
        {
            EvaluateJavaScript(platformView, request);
        }
    }

    private static async void EvaluateJavaScript(Avalonia.Controls.NativeWebView platformView, EvaluateJavaScriptAsyncRequest request)
    {
        try
        {
            var result = await platformView.InvokeScript(request.Script);
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
        if (PlatformView is null || html is null)
            return;

        PlatformView.NavigateToString(html);
    }

    /// <inheritdoc/>
    void IWebViewDelegate.LoadUrl(string? url)
    {
        if (PlatformView is null || url is null)
            return;

        PlatformView.Navigate(new Uri(url));
    }
}
