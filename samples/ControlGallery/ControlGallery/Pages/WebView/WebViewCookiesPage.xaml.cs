using System.Net;

namespace ControlGallery.Pages.WebView;

public partial class WebViewCookiesPage : ContentPage
{
    private const string CookieEchoUrl = "https://httpbin.org/cookies";
    private const string ExpectedCookieText = "\"MauiCookie\": \"Hmmm Cookies!\"";
    private bool _isCookieValidationPending;

    public WebViewCookiesPage()
    {
        InitializeComponent();
        CookieStatusLabel.Text = $"Press Verify Cookie. Success means the page and status show {ExpectedCookieText}.";
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
}
