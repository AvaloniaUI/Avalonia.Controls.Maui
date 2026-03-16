namespace ControlGallery.Pages.WebView;

public partial class WebViewLoadWebPage : ContentPage
{
    private const string DefaultRemoteUrl = "https://avaloniaui.net";
    private const string AlternateRemoteUrl = "https://dotnet.microsoft.com/apps/maui";

    public WebViewLoadWebPage()
    {
        InitializeComponent();
        LoadWebStatusLabel.Text = "Load a remote URL in the sample above.";
    }

    private void OnLoadWebAvaloniaClicked(object? sender, EventArgs e)
    {
        NavigateRemoteWebView(DefaultRemoteUrl, "Loading Avalonia.");
    }

    private void OnLoadWebMauiClicked(object? sender, EventArgs e)
    {
        NavigateRemoteWebView(AlternateRemoteUrl, "Loading the .NET MAUI site.");
    }

    private void OnLoadWebGoClicked(object? sender, EventArgs e)
    {
        var target = NormalizeRemoteUrl(WebUrlEntry.Text);
        if (target is null)
        {
            LoadWebStatusLabel.Text = "Enter a remote URL. Packaged assets belong in the Load Local sample.";
            return;
        }

        NavigateRemoteWebView(target, $"Loading {target}");
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

    private void NavigateRemoteWebView(string target, string statusMessage)
    {
        WebUrlEntry.Text = target;
        LoadWebStatusLabel.Text = statusMessage;
        LoadWebWebView.Source = new UrlWebViewSource { Url = target };
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
}
