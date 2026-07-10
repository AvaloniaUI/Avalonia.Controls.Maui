using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Avalonia.Controls.Maui.BlazorWebView.Hosting;

internal sealed class BlazorWebViewLoopbackHost : IAsyncDisposable
{
    private static readonly TimeSpan DefaultRequestDrainTimeout = TimeSpan.FromSeconds(2);

    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly object _requestTasksLock = new();
    private readonly HashSet<Task> _requestTasks = [];
    private readonly TimeSpan _requestDrainTimeout;
    private readonly ILogger _logger;
    private Func<HttpListenerContext, Task>? _requestHandler;
    private Task? _listenTask;
    private int _disposed;

    private BlazorWebViewLoopbackHost(int port, TimeSpan requestDrainTimeout, ILogger logger)
    {
        BaseUri = new Uri($"http://127.0.0.1:{port}/");
        _requestDrainTimeout = requestDrainTimeout;
        _logger = logger;
        _listener = new HttpListener();
        _listener.Prefixes.Add(BaseUri.AbsoluteUri);
    }

    public Uri BaseUri { get; }

    public static BlazorWebViewLoopbackHost Create(ILogger? logger = null)
    {
        return Create(DefaultRequestDrainTimeout, logger);
    }

    internal static BlazorWebViewLoopbackHost Create(TimeSpan requestDrainTimeout, ILogger? logger = null)
    {
        const int maxAttempts = 10;

        if (requestDrainTimeout < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(requestDrainTimeout), requestDrainTimeout, "The request drain timeout must be non-negative.");

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var host = new BlazorWebViewLoopbackHost(
                GetAvailableLoopbackPort(),
                requestDrainTimeout,
                logger ?? NullLogger.Instance);

            try
            {
                host._listener.Start();
                return host;
            }
            catch (HttpListenerException)
            {
                host._listener.Close();
            }
        }

        throw new InvalidOperationException("Unable to bind a loopback HTTP listener for BlazorWebView.");
    }

    public void Start(Func<HttpListenerContext, Task> requestHandler)
    {
        ArgumentNullException.ThrowIfNull(requestHandler);

        if (_requestHandler is not null)
            throw new InvalidOperationException("The loopback host has already been started.");

        _requestHandler = requestHandler;
        _listenTask = ListenAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        _cancellationTokenSource.Cancel();
        _listener.Close();

        try
        {
            if (_listenTask is not null)
                await _listenTask.ConfigureAwait(false);

            Task[] requestTasks;
            lock (_requestTasksLock)
                requestTasks = _requestTasks.ToArray();

            if (requestTasks.Length > 0)
                await WaitForActiveRequestsAsync(requestTasks).ConfigureAwait(false);
        }
        finally
        {
            _cancellationTokenSource.Dispose();
        }
    }

    private async Task WaitForActiveRequestsAsync(Task[] requestTasks)
    {
        try
        {
            await Task.WhenAll(requestTasks).WaitAsync(_requestDrainTimeout).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            _logger.LogDebug(
                "Timed out after {Timeout} waiting for {RequestCount} active Blazor WebView content requests.",
                _requestDrainTimeout,
                requestTasks.Length);
        }
    }

    private async Task ListenAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested && _listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (HttpListenerException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                return;
            }

            TrackRequestTask(Task.Run(() => HandleContextAsync(context)));
        }
    }

    private async Task HandleContextAsync(HttpListenerContext context)
    {
        try
        {
            if (_requestHandler is null)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.Close();
                return;
            }

            await _requestHandler(context);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "An unhandled error occurred while serving Blazor WebView content.");

            try
            {
                if (context.Response.OutputStream.CanWrite)
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
            catch
            {
            }

            try
            {
                context.Response.Close();
            }
            catch
            {
            }
        }
    }

    private void TrackRequestTask(Task task)
    {
        lock (_requestTasksLock)
            _requestTasks.Add(task);

        _ = task.ContinueWith(
            completedTask =>
            {
                _ = completedTask.Exception;

                lock (_requestTasksLock)
                    _requestTasks.Remove(completedTask);
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private static int GetAvailableLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private static class StatusCodes
    {
        public const int Status500InternalServerError = 500;
        public const int Status503ServiceUnavailable = 503;
    }
}
