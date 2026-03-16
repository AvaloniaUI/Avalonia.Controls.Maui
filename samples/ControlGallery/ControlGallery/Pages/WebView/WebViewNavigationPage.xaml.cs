namespace ControlGallery.Pages.WebView;

public partial class WebViewNavigationPage : ContentPage
{
    private const string DefaultRemoteUrl = "https://avaloniaui.net";
    private const string DefaultLocalAssetPath = "WebView/index.html";

    public WebViewNavigationPage()
    {
        InitializeComponent();
        NavigationStatusLabel.Text = "Open a couple of pages here, then use Back, Forward, and Reload.";
    }

    private void OnNavigationAvaloniaClicked(object? sender, EventArgs e)
    {
        NavigationStatusLabel.Text = "Opening Avalonia in the navigation sample.";
        NavigationWebView.Source = new UrlWebViewSource { Url = DefaultRemoteUrl };
    }

    private void OnNavigationLocalClicked(object? sender, EventArgs e)
    {
        NavigationStatusLabel.Text = "Opening the packaged local asset in the navigation sample.";
        NavigationWebView.Source = new UrlWebViewSource { Url = DefaultLocalAssetPath };
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

    private void OnNavigationWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        NavigationStatusLabel.Text = $"Navigated ({e.Result}): {e.Url}";
    }
}
