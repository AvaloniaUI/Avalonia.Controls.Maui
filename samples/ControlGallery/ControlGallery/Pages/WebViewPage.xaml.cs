using System.Net;

namespace ControlGallery.Pages;

public partial class WebViewPage : ContentPage
{
    private const string DefaultRemoteUrl = "https://avaloniaui.net";
    private const string AlternateRemoteUrl = "https://dotnet.microsoft.com/apps/maui";
    private const string DefaultLocalAssetPath = "WebView/index.html";
    private const string CookieEchoUrl = "https://httpbin.org/cookies";
    private const string ExpectedCookieText = "\"MauiCookie\": \"Hmmm Cookies!\"";
    private const string EventsPlaceholderText = "Navigation events will appear here.";
    private bool _isCookieValidationPending;

    public WebViewPage()
    {
        InitializeComponent();
        LoadWebStatusLabel.Text = "Load a remote URL in the sample above.";
        LocalPreviewStatusLabel.Text = "The preview above should show the packaged HTML card with its stylesheet applied.";
        NavigationStatusLabel.Text = "Open a couple of pages here, then use Back, Forward, and Reload.";
        EventsStatusLabel.Text = "Load a page or reload it to capture the navigation events.";
        EventsLogEditor.Text = EventsPlaceholderText;
        CookieStatusLabel.Text = $"Press Verify Cookie. Success means the page and status show {ExpectedCookieText}.";
        PlaygroundStatusLabel.Text = "Use the larger playground after exploring the focused samples above.";
        RefreshInlineHtml();
    }

    private void OnLoadWebAvaloniaClicked(object? sender, EventArgs e)
    {
        NavigateRemoteWebView(LoadWebWebView, DefaultRemoteUrl, WebUrlEntry, LoadWebStatusLabel, "Loading Avalonia.");
    }

    private void OnLoadWebMauiClicked(object? sender, EventArgs e)
    {
        NavigateRemoteWebView(LoadWebWebView, AlternateRemoteUrl, WebUrlEntry, LoadWebStatusLabel, "Loading the .NET MAUI site.");
    }

    private void OnLoadWebGoClicked(object? sender, EventArgs e)
    {
        var target = NormalizeRemoteUrl(WebUrlEntry.Text);
        if (target is null)
        {
            LoadWebStatusLabel.Text = "Enter a remote URL. Packaged assets belong in the Load Local sample.";
            return;
        }

        NavigateRemoteWebView(LoadWebWebView, target, WebUrlEntry, LoadWebStatusLabel, $"Loading {target}");
    }

    private void OnWebUrlEntryCompleted(object? sender, EventArgs e)
    {
        OnLoadWebGoClicked(sender, e);
    }

    private void OnLoadWebWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        LoadWebStatusLabel.Text = $"Navigated ({e.Result}): {e.Url}";
        WebUrlEntry.Text = e.Url;
    }

    private void OnLoadLocalAssetClicked(object? sender, EventArgs e)
    {
        LoadLocalPreview(LocalAssetEntry.Text);
    }

    private void OnLocalAssetEntryCompleted(object? sender, EventArgs e)
    {
        LoadLocalPreview(LocalAssetEntry.Text);
    }

    private void LoadLocalPreview(string? rawAssetPath)
    {
        var assetPath = NormalizeLocalAssetPath(rawAssetPath);
        LocalAssetEntry.Text = assetPath;
        LocalPreviewStatusLabel.Text = $"Loading packaged asset: {assetPath}";
        LocalPreviewWebView.Source = new UrlWebViewSource { Url = assetPath };
    }

    private void OnLocalPreviewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        LocalPreviewStatusLabel.Text = e.Result == WebNavigationResult.Success
            ? $"Packaged asset loaded successfully: {e.Url}"
            : $"Packaged asset navigation failed: {e.Result}";
    }

    private void OnNavigationAvaloniaClicked(object? sender, EventArgs e)
    {
        NavigateNavigationSample(DefaultRemoteUrl, "Opening Avalonia in the navigation sample.");
    }

    private void OnNavigationLocalClicked(object? sender, EventArgs e)
    {
        NavigateNavigationSample(DefaultLocalAssetPath, "Opening the packaged local asset in the navigation sample.");
    }

    private void OnNavigationBackClicked(object? sender, EventArgs e)
    {
        if (NavigationWebView.CanGoBack)
        {
            NavigationWebView.GoBack();
            return;
        }

        NavigationStatusLabel.Text = "There is no previous page in this navigation history.";
    }

    private void OnNavigationForwardClicked(object? sender, EventArgs e)
    {
        if (NavigationWebView.CanGoForward)
        {
            NavigationWebView.GoForward();
            return;
        }

        NavigationStatusLabel.Text = "There is no forward page in this navigation history.";
    }

    private void OnNavigationReloadClicked(object? sender, EventArgs e)
    {
        NavigationStatusLabel.Text = "Reloading the current navigation sample page.";
        NavigationWebView.Reload();
    }

    private void NavigateNavigationSample(string target, string statusMessage)
    {
        NavigationStatusLabel.Text = statusMessage;
        NavigationWebView.Source = new UrlWebViewSource { Url = target };
    }

    private void OnNavigationWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        NavigationStatusLabel.Text = $"Navigated ({e.Result}): {e.Url}";
    }

    private void OnEventsAvaloniaClicked(object? sender, EventArgs e)
    {
        EventsStatusLabel.Text = "Loading Avalonia in the events sample.";
        EventsWebView.Source = new UrlWebViewSource { Url = DefaultRemoteUrl };
    }

    private void OnEventsLocalClicked(object? sender, EventArgs e)
    {
        EventsStatusLabel.Text = "Loading the packaged local asset in the events sample.";
        EventsWebView.Source = new UrlWebViewSource { Url = DefaultLocalAssetPath };
    }

    private void OnEventsReloadClicked(object? sender, EventArgs e)
    {
        EventsStatusLabel.Text = "Reloading the events sample.";
        EventsWebView.Reload();
    }

    private void OnClearEventsLogClicked(object? sender, EventArgs e)
    {
        EventsLogEditor.Text = EventsPlaceholderText;
    }

    private void OnEventsWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        EventsStatusLabel.Text = $"Navigating ({e.NavigationEvent}): {e.Url}";
        AppendEventsLog($"Navigating | Event={e.NavigationEvent} | Url={e.Url}");
    }

    private void OnEventsWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        EventsStatusLabel.Text = $"Navigated ({e.NavigationEvent}, {e.Result}): {e.Url}";
        AppendEventsLog($"Navigated | Event={e.NavigationEvent} | Result={e.Result} | Url={e.Url}");
    }

    private void OnLoadCookieSampleClicked(object? sender, EventArgs e)
    {
        _isCookieValidationPending = true;
        CookieWebView.Cookies = new CookieContainer();
        CookieWebView.Cookies.Add(new Cookie("MauiCookie", "Hmmm Cookies!", "/", "httpbin.org"));
        CookieStatusLabel.Text = $"Loading {CookieEchoUrl}. Waiting for the echoed cookie response.";
        CookieWebView.Source = new UrlWebViewSource { Url = CookieEchoUrl };
    }

    private void OnResetCookieSampleClicked(object? sender, EventArgs e)
    {
        _isCookieValidationPending = false;
        CookieWebView.Cookies = new CookieContainer();
        CookieWebView.Source = new HtmlWebViewSource
        {
            Html = """
                <!DOCTYPE html>
                <html lang="en">
                <body style="font-family: Segoe UI, sans-serif; margin: 24px;">
                    <h1>Cookie sample</h1>
                    <p>Press <strong>Verify Cookie</strong> to open <code>https://httpbin.org/cookies</code>.</p>
                    <p>Success means the page shows <code>"MauiCookie": "Hmmm Cookies!"</code>.</p>
                </body>
                </html>
                """
        };
        CookieStatusLabel.Text = $"Cookie sample reset. Press Verify Cookie and look for {ExpectedCookieText}.";
    }

    private async void OnCookieWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (!_isCookieValidationPending)
            return;

        if (e.Result != WebNavigationResult.Success)
        {
            CookieStatusLabel.Text = $"Cookie sample navigation failed: {e.Result}";
            return;
        }

        try
        {
            await Task.Delay(150);
            var bodyText = await CookieWebView.EvaluateJavaScriptAsync("document.body ? document.body.innerText : ''");
            var normalizedBodyText = bodyText ?? string.Empty;

            CookieStatusLabel.Text =
                normalizedBodyText.Contains("MauiCookie", StringComparison.Ordinal) &&
                normalizedBodyText.Contains("Hmmm Cookies!", StringComparison.Ordinal)
                    ? "Cookie validated. httpbin echoed MauiCookie=Hmmm Cookies! back in the response body."
                    : $"Loaded /cookies. Validation succeeds when the page body shows {ExpectedCookieText}.";
        }
        catch
        {
            CookieStatusLabel.Text = $"Loaded /cookies. Validation succeeds when the page body shows {ExpectedCookieText}.";
        }
    }

    private void OnRefreshInlineHtmlClicked(object? sender, EventArgs e)
    {
        RefreshInlineHtml();
    }

    private void RefreshInlineHtml()
    {
        var generatedAt = DateTime.Now.ToString("HH:mm:ss");
        InlineHtmlWebView.Source = new HtmlWebViewSource
        {
            Html = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <style>
                        body {
                            margin: 0;
                            font-family: "Segoe UI", sans-serif;
                            background: linear-gradient(180deg, #fffaf0 0%, #ffe8c2 100%);
                            color: #5e3200;
                        }

                        main {
                            margin: 24px;
                            padding: 24px;
                            border-radius: 18px;
                            background: rgba(255, 255, 255, 0.92);
                            box-shadow: 0 16px 36px rgba(94, 50, 0, 0.16);
                        }

                        p {
                            line-height: 1.5;
                        }

                        code {
                            font-family: Consolas, monospace;
                            color: #8a3f00;
                        }
                    </style>
                </head>
                <body>
                    <main>
                        <p><strong>Inline HtmlWebViewSource</strong></p>
                        <h1>ControlGallery WebView</h1>
                        <p>This content was generated in code-behind and refreshed at <code>{{generatedAt}}</code>.</p>
                    </main>
                </body>
                </html>
                """
        };

        InlineHtmlStatusLabel.Text = $"Inline HTML refreshed at {generatedAt}.";
    }

    private void OnPlaygroundAvaloniaClicked(object? sender, EventArgs e)
    {
        NavigatePlayground(DefaultRemoteUrl);
    }

    private void OnPlaygroundBackClicked(object? sender, EventArgs e)
    {
        if (PlaygroundWebView.CanGoBack)
        {
            PlaygroundWebView.GoBack();
            return;
        }

        PlaygroundStatusLabel.Text = "There is no previous page in the playground history.";
    }

    private void OnPlaygroundForwardClicked(object? sender, EventArgs e)
    {
        if (PlaygroundWebView.CanGoForward)
        {
            PlaygroundWebView.GoForward();
            return;
        }

        PlaygroundStatusLabel.Text = "There is no forward page in the playground history.";
    }

    private void OnPlaygroundReloadClicked(object? sender, EventArgs e)
    {
        PlaygroundStatusLabel.Text = "Reloading the playground page.";
        PlaygroundWebView.Reload();
    }

    private void OnPlaygroundMauiClicked(object? sender, EventArgs e)
    {
        NavigatePlayground(AlternateRemoteUrl);
    }

    private void OnPlaygroundLocalClicked(object? sender, EventArgs e)
    {
        NavigatePlayground(DefaultLocalAssetPath);
    }

    private void OnPlaygroundGoClicked(object? sender, EventArgs e)
    {
        var target = NormalizeTarget(PlaygroundUrlEntry.Text);
        if (target is null)
        {
            PlaygroundStatusLabel.Text = "Enter a remote URL or a packaged .html path.";
            return;
        }

        NavigatePlayground(target);
    }

    private void OnPlaygroundUrlEntryCompleted(object? sender, EventArgs e)
    {
        OnPlaygroundGoClicked(sender, e);
    }

    private void NavigatePlayground(string target)
    {
        PlaygroundUrlEntry.Text = target;
        PlaygroundStatusLabel.Text = target.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            ? $"Loading packaged asset: {target}"
            : $"Navigating to {target}";
        PlaygroundWebView.Source = new UrlWebViewSource { Url = target };
    }

    private void OnPlaygroundWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        PlaygroundUrlEntry.Text = e.Url;
        PlaygroundStatusLabel.Text = $"Navigated ({e.Result}): {e.Url}";
    }

    private void NavigateRemoteWebView(WebView webView, string target, Entry entry, Label statusLabel, string statusMessage)
    {
        entry.Text = target;
        statusLabel.Text = statusMessage;
        webView.Source = new UrlWebViewSource { Url = target };
    }

    private void AppendEventsLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var prefix = EventsLogEditor.Text == EventsPlaceholderText ? string.Empty : Environment.NewLine;
        EventsLogEditor.Text += $"{prefix}[{timestamp}] {message}";
    }

    private static string NormalizeLocalAssetPath(string? rawAssetPath)
    {
        return string.IsNullOrWhiteSpace(rawAssetPath) ? DefaultLocalAssetPath : rawAssetPath.Trim();
    }

    private static string? NormalizeRemoteUrl(string? rawTarget)
    {
        var target = rawTarget?.Trim();
        if (string.IsNullOrEmpty(target))
            return null;

        if (!target.Contains("://", StringComparison.Ordinal) &&
            target.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            target = "https://" + target;
        }

        return target;
    }

    private static string? NormalizeTarget(string? rawTarget)
    {
        var target = rawTarget?.Trim();
        if (string.IsNullOrEmpty(target))
            return null;

        if (!target.Contains("://", StringComparison.Ordinal) &&
            target.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            return target;
        }

        if (!target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            target = "https://" + target;
        }

        return target;
    }
}
