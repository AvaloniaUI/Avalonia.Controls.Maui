namespace ControlGallery.Pages;

public partial class WebViewPage : ContentPage
{
    public WebViewPage()
    {
        InitializeComponent();
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        if (MainWebView.CanGoBack)
            MainWebView.GoBack();
    }

    private void OnForwardClicked(object? sender, EventArgs e)
    {
        if (MainWebView.CanGoForward)
            MainWebView.GoForward();
    }

    private void OnReloadClicked(object? sender, EventArgs e)
    {
        MainWebView.Reload();
    }

    private void OnGoClicked(object? sender, EventArgs e)
    {
        NavigateToUrl();
    }

    private void OnUrlEntryCompleted(object? sender, EventArgs e)
    {
        NavigateToUrl();
    }

    private void NavigateToUrl()
    {
        var url = UrlEntry.Text?.Trim();
        if (string.IsNullOrEmpty(url))
            return;

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        MainWebView.Source = new UrlWebViewSource { Url = url };
    }

    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        UrlEntry.Text = e.Url;
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        BackButton.IsEnabled = MainWebView.CanGoBack;
        ForwardButton.IsEnabled = MainWebView.CanGoForward;
        UrlEntry.Text = e.Url;
    }
}
