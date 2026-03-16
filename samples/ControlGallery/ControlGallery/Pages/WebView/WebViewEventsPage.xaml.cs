namespace ControlGallery.Pages.WebView;

public partial class WebViewEventsPage : ContentPage
{
    private const string DefaultRemoteUrl = "https://avaloniaui.net";
    private const string DefaultLocalAssetPath = "WebView/index.html";
    private const string EventsPlaceholderText = "Navigation events will appear here.";

    public WebViewEventsPage()
    {
        InitializeComponent();
        EventsStatusLabel.Text = "Load a page or reload it to capture the navigation events.";
        EventsLogEditor.Text = EventsPlaceholderText;
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

    private void AppendEventsLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var prefix = EventsLogEditor.Text == EventsPlaceholderText ? string.Empty : Environment.NewLine;
        EventsLogEditor.Text += $"{prefix}[{timestamp}] {message}";
    }
}
