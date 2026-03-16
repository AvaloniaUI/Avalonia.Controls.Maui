namespace ControlGallery.Pages.WebView;

public partial class WebViewPlaygroundPage : ContentPage
{
    private const string DefaultRemoteUrl = "https://avaloniaui.net";
    private const string AlternateRemoteUrl = "https://dotnet.microsoft.com/apps/maui";
    private const string DefaultLocalAssetPath = "WebView/index.html";

    public WebViewPlaygroundPage()
    {
        InitializeComponent();
        PlaygroundStatusLabel.Text = "Use the playground to try remote URLs or packaged assets.";
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
