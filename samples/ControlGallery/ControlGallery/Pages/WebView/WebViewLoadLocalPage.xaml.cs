namespace ControlGallery.Pages.WebView;

public partial class WebViewLoadLocalPage : ContentPage
{
    private const string DefaultLocalAssetPath = "WebView/index.html";

    public WebViewLoadLocalPage()
    {
        InitializeComponent();
        LocalPreviewStatusLabel.Text = "The preview above should show the packaged HTML card with its stylesheet applied.";
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
        var assetPath = string.IsNullOrWhiteSpace(rawAssetPath) ? DefaultLocalAssetPath : rawAssetPath.Trim();
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
}
