namespace ControlGallery.Pages;

public partial class BrowserPage : ContentPage
{
    public BrowserPage()
    {
        InitializeComponent();
    }

    async void OnOpenClicked(object? sender, EventArgs e)
    {
        var url = UrlEntry.Text;
        if (string.IsNullOrWhiteSpace(url))
            return;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            ResultLabel.Text = "Invalid URL";
            return;
        }

        var result = await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        ResultLabel.Text = result ? "Opened" : "Failed to open";
    }

    async void OnAvaloniaClicked(object? sender, EventArgs e)
    {
        await Browser.Default.OpenAsync(new Uri("https://avaloniaui.net"), BrowserLaunchMode.SystemPreferred);
    }

    async void OnGitHubClicked(object? sender, EventArgs e)
    {
        await Browser.Default.OpenAsync(new Uri("https://github.com/AvaloniaUI"), BrowserLaunchMode.SystemPreferred);
    }

    async void OnMauiDocsClicked(object? sender, EventArgs e)
    {
        await Browser.Default.OpenAsync(new Uri("https://learn.microsoft.com/dotnet/maui"), BrowserLaunchMode.SystemPreferred);
    }
}
