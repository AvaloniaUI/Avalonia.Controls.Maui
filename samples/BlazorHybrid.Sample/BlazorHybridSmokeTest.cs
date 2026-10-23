using Avalonia.Controls;
using Avalonia.Controls.Maui.BlazorWebView;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;
using AvaloniaDispatcher = Avalonia.Threading.Dispatcher;

namespace BlazorHybrid.Sample;

internal sealed class BlazorHybridSmokeTest
{
    private const string EnabledEnvironmentVariable = "AVALONIA_MAUI_BLAZOR_SMOKE_TEST";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);
    private readonly BlazorWebView _blazorWebView;
    private NativeWebView? _nativeWebView;
    private int _completed;

    private BlazorHybridSmokeTest(BlazorWebView blazorWebView)
    {
        _blazorWebView = blazorWebView;
        _blazorWebView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;
        _ = FailOnTimeoutAsync();
    }

    public static void AttachIfEnabled(BlazorWebView blazorWebView)
    {
        if (string.Equals(
            Environment.GetEnvironmentVariable(EnabledEnvironmentVariable),
            "1",
            StringComparison.Ordinal))
        {
            _ = new BlazorHybridSmokeTest(blazorWebView);
        }
    }

    private void OnBlazorWebViewInitialized(object? sender, BlazorWebViewInitializedEventArgs e)
    {
        _blazorWebView.BlazorWebViewInitialized -= OnBlazorWebViewInitialized;

        if (e is not AvaloniaBlazorWebViewInitializedEventArgs avaloniaArgs)
        {
            Complete(false, "The Avalonia BlazorWebView initialized event arguments were not provided.");
            return;
        }

        _nativeWebView = avaloniaArgs.WebView;
        _nativeWebView.NavigationCompleted += OnNavigationCompleted;
    }

    private void OnNavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess || e.Request is null)
        {
            Complete(false, $"Navigation failed: {e.Request}");
            return;
        }

        if (!e.Request.AbsolutePath.Equals("/interop", StringComparison.Ordinal))
        {
            _nativeWebView!.Navigate(new Uri(e.Request, "/interop"));
            return;
        }

        _nativeWebView!.NavigationCompleted -= OnNavigationCompleted;
        _ = VerifyInteropAsync(_nativeWebView);
    }

    private async Task VerifyInteropAsync(NativeWebView webView)
    {
        try
        {
            await WaitForScriptResultAsync(
                webView,
                "document.getElementById('run-interop') !== null",
                result => result.Contains("true", StringComparison.OrdinalIgnoreCase));

            await InvokeScriptAsync(webView, "document.getElementById('run-interop').click()");

            await WaitForScriptResultAsync(
                webView,
                "document.body.innerText",
                result =>
                    result.Contains("echo:ping", StringComparison.Ordinal) &&
                    result.Contains("callback:ok", StringComparison.Ordinal));

            Complete(true, "Blazor rendered and JS/.NET interop completed.");
        }
        catch (Exception ex)
        {
            Complete(false, ex.Message);
        }
    }

    private static async Task WaitForScriptResultAsync(
        NativeWebView webView,
        string script,
        Func<string, bool> predicate)
    {
        var deadline = DateTime.UtcNow + Timeout;

        while (DateTime.UtcNow < deadline)
        {
            var result = await InvokeScriptAsync(webView, script);
            if (result is not null && predicate(result))
                return;

            await Task.Delay(100);
        }

        throw new TimeoutException($"Timed out waiting for script result: {script}");
    }

    private static Task<string?> InvokeScriptAsync(NativeWebView webView, string script)
    {
        if (AvaloniaDispatcher.UIThread.CheckAccess())
            return webView.InvokeScript(script);

        return AvaloniaDispatcher.UIThread.InvokeAsync(() => webView.InvokeScript(script));
    }

    private async Task FailOnTimeoutAsync()
    {
        await Task.Delay(Timeout);
        Complete(false, "Timed out waiting for the Blazor WebView smoke test.");
    }

    private void Complete(bool success, string message)
    {
        if (Interlocked.Exchange(ref _completed, 1) != 0)
            return;

        if (_nativeWebView is not null)
            _nativeWebView.NavigationCompleted -= OnNavigationCompleted;

        var prefix = success
            ? "BLAZOR_HYBRID_SMOKE_TEST_PASSED"
            : "BLAZOR_HYBRID_SMOKE_TEST_FAILED";
        var output = $"{prefix}: {message}";

        if (success)
            Console.WriteLine(output);
        else
            Console.Error.WriteLine(output);

        Environment.ExitCode = success ? 0 : 1;
        AvaloniaDispatcher.UIThread.Post(() => Application.Current?.Quit());
    }
}
