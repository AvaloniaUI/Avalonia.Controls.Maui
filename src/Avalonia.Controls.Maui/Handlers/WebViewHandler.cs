using System.Collections.Generic;
using System;
using System.Net;
using System.Reflection;
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
    private static readonly EventInfo? NativeProcessTerminatedEvent =
        typeof(Avalonia.Controls.NativeWebView).GetEvent("ProcessTerminated");

    private WebNavigationEvent _currentNavigationEvent = WebNavigationEvent.NewPage;
    private IWebViewSource? _pendingSource;
    private string? _desiredUserAgent;

    /// <summary>
    /// Gets a value indicating whether the current Avalonia WebView backend exposes a process-termination signal.
    /// MAUI's <see cref="Microsoft.Maui.IWebView.ProcessTerminated(Microsoft.Maui.WebProcessTerminatedEventArgs)"/> callback cannot be raised
    /// until Avalonia surfaces an equivalent native event.
    /// </summary>
    public static bool SupportsProcessTerminated => NativeProcessTerminatedEvent is not null;

    /// <summary>Property mapper for <see cref="WebViewHandler"/>.</summary>
    public static IPropertyMapper<IWebView, WebViewHandler> Mapper = new PropertyMapper<IWebView, WebViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IWebView.Source)] = MapSource,
        [nameof(IWebView.Cookies)] = MapCookies,
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
            nativeWebView.WebMessageReceived += OnWebMessageReceived;
            nativeWebView.AdapterCreated += OnAdapterCreated;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(Avalonia.Controls.Control platformView)
    {
        if (platformView is Avalonia.Controls.NativeWebView nativeWebView)
        {
            nativeWebView.NavigationStarted -= OnNavigationStarted;
            nativeWebView.NavigationCompleted -= OnNavigationCompleted;
            nativeWebView.WebMessageReceived -= OnWebMessageReceived;
            nativeWebView.AdapterCreated -= OnAdapterCreated;
        }

        base.DisconnectHandler(platformView);
    }

    private void OnAdapterCreated(object? sender, Avalonia.Controls.WebViewAdapterEventArgs e)
    {
        if (VirtualView is null || NativeWebView is not { } nativeWebView)
            return;

        nativeWebView.UpdateUserAgent(VirtualView);

        if (_pendingSource is null)
            return;

        var source = _pendingSource;
        _pendingSource = null;
        LoadSource(VirtualView, source);
    }

    private void OnNavigationStarted(object? sender, Avalonia.Controls.WebViewNavigationStartingEventArgs e)
        => HandleNavigationStarted(e);

    private void OnWebMessageReceived(object? sender, Avalonia.Controls.WebMessageReceivedEventArgs e)
        => HandleWebMessageReceived(e);

    internal WebNavigationEvent CurrentNavigationEvent => _currentNavigationEvent;

    internal string? DesiredUserAgent => _desiredUserAgent;

    internal bool HandleWebMessageReceived(Avalonia.Controls.WebMessageReceivedEventArgs e)
    {
        if (VirtualView is null || !WebViewHistorySynchronizer.TryParseMessage(e.Body, out var state))
            return false;

        var mergedState = WebViewHistorySynchronizer.MergeWithNativeState(
            state,
            NativeWebView?.CanGoBack == true,
            NativeWebView?.CanGoForward == true);

        VirtualView.CanGoBack = mergedState.CanGoBack;
        VirtualView.CanGoForward = mergedState.CanGoForward;

        if (_currentNavigationEvent is WebNavigationEvent.Back or WebNavigationEvent.Forward)
            _currentNavigationEvent = WebNavigationEvent.NewPage;

        return true;
    }

    internal void HandleNavigationStarted(Avalonia.Controls.WebViewNavigationStartingEventArgs e)
    {
        if (VirtualView is null)
            return;

        var url = e.Request?.AbsoluteUri ?? string.Empty;
        var cancel = VirtualView.Navigating(_currentNavigationEvent, url);
        e.Cancel = cancel;

        if (cancel)
            _currentNavigationEvent = WebNavigationEvent.NewPage;
    }

    private async void OnNavigationCompleted(object? sender, Avalonia.Controls.WebViewNavigationCompletedEventArgs e)
    {
        await HandleNavigationCompletedAsync(e);
    }

    internal async Task HandleNavigationCompletedAsync(Avalonia.Controls.WebViewNavigationCompletedEventArgs e)
    {
        if (VirtualView is null || NativeWebView is not { } webView)
            return;

        var url = e.Request?.AbsoluteUri ?? string.Empty;
        var result = e.IsSuccess ? WebNavigationResult.Success : WebNavigationResult.Failure;

        try
        {
            await SyncCookiesToVirtualViewAsync(VirtualView, e.Request);
        }
        catch
        {
            // Keep navigation reporting resilient even if cookie sync fails.
        }

        VirtualView.Navigated(_currentNavigationEvent, url, result);

        webView.UpdateCanGoBackForward(VirtualView);
        _ = EnsureHistoryTrackingAsync(webView);
        _currentNavigationEvent = WebNavigationEvent.NewPage;
    }

    /// <summary>Maps the Source property to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    public static void MapSource(WebViewHandler handler, IWebView webView)
    {
        if (handler.NativeWebView is null)
            return;

        if (!handler.IsAdapterReady)
        {
            handler._pendingSource = webView.Source;
            return;
        }

        handler._pendingSource = null;
        handler.LoadSource(webView, webView.Source);
    }

    /// <summary>Maps the Cookies property to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    public static void MapCookies(WebViewHandler handler, IWebView webView)
    {
        if (handler.NativeWebView is null)
            return;

        _ = handler.SyncCookiesToPlatformAsync(webView, handler.GetCurrentNavigationUri());
    }

    /// <summary>Maps the UserAgent property to the platform view.</summary>
    /// <param name="handler">The handler for the WebView.</param>
    /// <param name="webView">The virtual view.</param>
    public static void MapUserAgent(WebViewHandler handler, IWebView webView)
    {
        handler.ApplyUserAgent(webView.UserAgent);
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
            var navigated = webViewControl.GoBack();
            webViewControl.UpdateCanGoBackForward(webView);

            if (!navigated)
                handler._currentNavigationEvent = WebNavigationEvent.NewPage;
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
            var navigated = webViewControl.GoForward();
            webViewControl.UpdateCanGoBackForward(webView);

            if (!navigated)
                handler._currentNavigationEvent = WebNavigationEvent.NewPage;
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
            _ = handler.ReloadAsync(webViewControl, webView);
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
            webViewControl.Eval(script);
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
        if (html is null)
            return;

        _ = NavigateToHtmlAsync(html, WebViewSourceResolver.ResolveBaseUri(baseUrl));
    }

    /// <inheritdoc/>
    void IWebViewDelegate.LoadUrl(string? url)
    {
        if (url is null)
            return;

        if (!WebViewSourceResolver.TryResolve(url, out var source))
            return;

        NavigateToResolvedSourceAsync(source);
    }

    private bool IsAdapterReady => NativeWebView?.TryGetPlatformHandle() is not null;

    private Uri? GetCurrentNavigationUri()
    {
        return NativeWebView?.GetAbsoluteSourceUri();
    }

    private void LoadSource(IWebView webView, IWebViewSource? source)
    {
        NativeWebView?.UpdateSource(webView, this, source);
    }

    private async Task ReloadAsync(Avalonia.Controls.NativeWebView webViewControl, IWebView webView)
    {
        await SyncCookiesToPlatformAsync(webView, GetCurrentNavigationUri());
        webViewControl.Refresh();
    }

    private static async Task EnsureHistoryTrackingAsync(Avalonia.Controls.NativeWebView webViewControl)
    {
        try
        {
            await webViewControl.InvokeScript(WebViewHistorySynchronizer.InjectionScript);
        }
        catch
        {
            // Some pages may reject or delay script execution; keep navigation usable without history sync.
        }
    }

    private void ApplyUserAgent(string? userAgent)
    {
        _desiredUserAgent = userAgent;

        if (NativeWebView is not { } webViewControl)
            return;

        if (VirtualView is not null)
        {
            webViewControl.UpdateUserAgent(VirtualView);
            return;
        }

        webViewControl.UpdateUserAgent(userAgent);
    }

    private async void NavigateToResolvedSourceAsync(ResolvedWebViewSource source)
    {
        if (NativeWebView is not { } webViewControl || VirtualView is null)
            return;

        if (source.IsHtmlContent)
        {
            await NavigateToHtmlAsync(source.Html!, source.BaseUri);
            return;
        }

        if (source.Uri is not { } uri)
            return;

        await SyncCookiesToPlatformAsync(VirtualView, uri);

        if (NativeWebView == webViewControl)
            webViewControl.Navigate(uri);
    }

    private async Task NavigateToHtmlAsync(string html, Uri? baseUri)
    {
        if (NativeWebView is not { } webViewControl || VirtualView is null)
            return;

        await SyncCookiesToPlatformAsync(VirtualView, baseUri);

        if (NativeWebView == webViewControl)
            webViewControl.NavigateToString(html, baseUri);
    }

    private async Task SyncCookiesToPlatformAsync(IWebView webView, Uri? uri)
    {
        if (TryGetCookieStore() is not { } cookieStore)
            return;

        await WebViewCookieSynchronizer.SyncToPlatformAsync(cookieStore, webView.Cookies, uri);
    }

    private async Task SyncCookiesToVirtualViewAsync(IWebView webView, Uri? uri)
    {
        if (TryGetCookieStore() is not { } cookieStore)
            return;

        await WebViewCookieSynchronizer.SyncToVirtualViewAsync(cookieStore, webView.Cookies, uri);
    }

    private IWebViewCookieStore? TryGetCookieStore()
    {
        if (NativeWebView?.TryGetCookieManager() is not { } cookieManager)
            return null;

        return new NativeWebViewCookieStore(cookieManager);
    }

    private sealed class NativeWebViewCookieStore : IWebViewCookieStore
    {
        private readonly Avalonia.Controls.NativeWebViewCookieManager _cookieManager;

        public NativeWebViewCookieStore(Avalonia.Controls.NativeWebViewCookieManager cookieManager)
        {
            _cookieManager = cookieManager;
        }

        public void AddOrUpdateCookie(Cookie cookie)
        {
            _cookieManager.AddOrUpdateCookie(cookie);
        }

        public void DeleteCookie(string name, string domain, string path)
        {
            _cookieManager.DeleteCookie(name, domain, path);
        }

        public Task<IReadOnlyList<Cookie>> GetCookiesAsync()
        {
            return _cookieManager.GetCookiesAsync();
        }
    }
}
