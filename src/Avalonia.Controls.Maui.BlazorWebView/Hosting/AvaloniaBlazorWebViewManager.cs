using System.Net;
using System.Text;
using Avalonia.Controls.Maui.BlazorWebView.Compatibility;
using Avalonia.Controls;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using BlazorDispatcher = Microsoft.AspNetCore.Components.Dispatcher;

namespace Avalonia.Controls.Maui.BlazorWebView.Hosting;

internal sealed class AvaloniaBlazorWebViewManager : WebViewManager
{
    private static readonly TimeSpan SendMessageDrainTimeout = TimeSpan.FromSeconds(1);

    private readonly NativeWebView _webView;
    private readonly BlazorWebViewLoopbackHost _host;
    private readonly ILogger _logger;
    private readonly string _contentRootDir;
    private readonly object _sendMessageLock = new();
    private readonly object _navigationLock = new();
    private Task _sendMessageTask = Task.CompletedTask;
    private Uri? _pendingNavigationRequest;
    private bool _isDisposing;

    public AvaloniaBlazorWebViewManager(
        NativeWebView webView,
        IServiceProvider provider,
        BlazorDispatcher dispatcher,
        BlazorWebViewLoopbackHost host,
        IFileProvider fileProvider,
        JSComponentConfigurationStore jsComponents,
        string contentRootDir,
        string hostPageRelativePath,
        ILogger logger)
        : base(provider, dispatcher, host.BaseUri, fileProvider, jsComponents, hostPageRelativePath)
    {
        _webView = webView;
        _host = host;
        _contentRootDir = contentRootDir;
        _logger = logger;
        _host.Start(HandleRequestAsync);
    }

    public Uri AppBaseUri => _host.BaseUri;

    protected override void NavigateCore(Uri absoluteUri)
    {
        lock (_navigationLock)
            _pendingNavigationRequest = absoluteUri;

        _ = ObserveNavigationAsync(Dispatcher.InvokeAsync(() => _webView.Navigate(absoluteUri)));
    }

    protected override void SendMessage(string message)
    {
        if (_isDisposing)
            return;

        var script = AvaloniaBlazorWebViewScripts.CreateDispatchMessageScript(message);
        Task queuedTask;

        lock (_sendMessageLock)
        {
            _sendMessageTask = SendMessageAsync(_sendMessageTask, script);
            queuedTask = _sendMessageTask;
        }

        _ = ObserveSendMessageAsync(queuedTask);
    }

    private async Task SendMessageAsync(Task previousTask, string script)
    {
        try
        {
            await previousTask.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "A previous Blazor WebView message could not be sent.");
        }

        if (_isDisposing)
            return;

        await Dispatcher.InvokeAsync(async () =>
        {
            if (_isDisposing)
                return;

            try
            {
                await _webView.InvokeScript(script);
            }
            catch (Exception ex)
            {
                // The page can be navigating while Blazor is tearing down. Dropping the
                // message matches WebView shutdown behavior on the native MAUI handlers.
                _logger.LogDebug(ex, "A Blazor WebView message was dropped while the page was navigating.");
            }
        }).ConfigureAwait(false);
    }

    internal void DispatchMessageReceived(Uri sourceUri, string message)
    {
        base.MessageReceived(sourceUri, message);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_isDisposing)
            return;

        _isDisposing = true;

        Task sendMessageTask;
        lock (_sendMessageLock)
            sendMessageTask = _sendMessageTask;

        try
        {
            await sendMessageTask.WaitAsync(SendMessageDrainTimeout).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            _logger.LogDebug("Timed out waiting for pending Blazor WebView messages during disposal.");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Pending Blazor WebView messages failed during disposal.");
        }

        try
        {
            await _host.DisposeAsync();
        }
        finally
        {
            await base.DisposeAsyncCore();
        }
    }

    private async Task ObserveSendMessageAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "A Blazor WebView message could not be sent.");
        }
    }

    private async Task ObserveNavigationAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to navigate the Avalonia BlazorWebView.");
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (!HttpMethods.IsGet(request.HttpMethod) && !HttpMethods.IsHead(request.HttpMethod))
        {
            response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            response.Close();
            return;
        }

        if (request.Url is null)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Close();
            return;
        }

        var requestUrl = RemovePossibleQueryString(request.Url.AbsoluteUri);
        var allowFallbackOnHostPage = ShouldFallbackToHostPage(
            AppBaseUri,
            request.Url,
            request.Headers["Accept"],
            ConsumePendingNavigationRequest(request.Url));

        var hasDefaultContent = TryGetResponseContent(
            requestUrl,
            allowFallbackOnHostPage,
            out var statusCode,
            out var statusMessage,
            out var content,
            out var headers);
        var hasHotReloadContent = MauiBlazorWebViewCompatibility.TryReplaceStaticContentHotReloadResponse(
            _contentRootDir,
            requestUrl,
            ref statusCode,
            ref content,
            headers,
            _logger);

        if (!hasDefaultContent && !hasHotReloadContent)
        {
            await WriteNotFoundResponseAsync(response);
            return;
        }

        if (hasHotReloadContent && !hasDefaultContent)
            statusMessage = "OK";

        await using (content)
        {
            response.StatusCode = statusCode;
            response.StatusDescription = statusMessage;

            foreach (var header in headers)
            {
                if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    response.ContentType = header.Value;
                else
                    response.Headers[header.Key] = header.Value;
            }

            if (content.CanSeek)
                response.ContentLength64 = content.Length;

            if (HttpMethods.IsGet(request.HttpMethod))
                await content.CopyToAsync(response.OutputStream);
        }

        response.Close();
    }

    internal static bool ShouldFallbackToHostPage(Uri appBaseUri, Uri requestUri, string? acceptHeader)
    {
        return ShouldFallbackToHostPage(appBaseUri, requestUri, acceptHeader, isPendingNavigationRequest: false);
    }

    internal static bool ShouldFallbackToHostPage(
        Uri appBaseUri,
        Uri requestUri,
        string? acceptHeader,
        bool isPendingNavigationRequest)
    {
        return appBaseUri.IsBaseOf(requestUri) &&
            (isPendingNavigationRequest || AcceptsHtml(acceptHeader));
    }

    private bool ConsumePendingNavigationRequest(Uri requestUri)
    {
        lock (_navigationLock)
        {
            if (_pendingNavigationRequest is null ||
                !AreSameNetworkRequest(_pendingNavigationRequest, requestUri))
            {
                return false;
            }

            _pendingNavigationRequest = null;
            return true;
        }
    }

    private static bool AreSameNetworkRequest(Uri left, Uri right)
    {
        return left.Scheme.Equals(right.Scheme, StringComparison.OrdinalIgnoreCase) &&
            left.Authority.Equals(right.Authority, StringComparison.OrdinalIgnoreCase) &&
            left.AbsolutePath.Equals(right.AbsolutePath, StringComparison.Ordinal) &&
            left.Query.Equals(right.Query, StringComparison.Ordinal);
    }

    private static bool AcceptsHtml(string? acceptHeader)
    {
        return acceptHeader is not null &&
            (acceptHeader.Contains("text/html", StringComparison.OrdinalIgnoreCase) ||
                acceptHeader.Contains("application/xhtml+xml", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task WriteNotFoundResponseAsync(HttpListenerResponse response)
    {
        response.StatusCode = StatusCodes.Status404NotFound;
        response.ContentType = "text/plain";

        var content = Encoding.UTF8.GetBytes("Not found");
        response.ContentLength64 = content.Length;
        await response.OutputStream.WriteAsync(content);
        response.Close();
    }

    private static string RemovePossibleQueryString(string url)
    {
        var queryStringIndex = url.IndexOf('?', StringComparison.Ordinal);
        return queryStringIndex >= 0 ? url[..queryStringIndex] : url;
    }

    private static class HttpMethods
    {
        public static bool IsGet(string method) =>
            method.Equals("GET", StringComparison.OrdinalIgnoreCase);

        public static bool IsHead(string method) =>
            method.Equals("HEAD", StringComparison.OrdinalIgnoreCase);
    }

    private static class StatusCodes
    {
        public const int Status400BadRequest = 400;
        public const int Status404NotFound = 404;
        public const int Status405MethodNotAllowed = 405;
    }
}
