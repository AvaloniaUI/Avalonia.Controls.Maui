namespace ControlGallery.Pages;

public partial class LauncherPage : ContentPage
{
    public LauncherPage()
    {
        InitializeComponent();
    }

    async void OnCanOpenClicked(object? sender, EventArgs e)
    {
        if (!TryParseUri(LauncherEntry.Text, out var uri))
            return;

        var canOpen = await Launcher.Default.CanOpenAsync(uri);
        ResultLabel.Text = $"CanOpen: {canOpen}";
    }

    async void OnOpenClicked(object? sender, EventArgs e)
    {
        if (!TryParseUri(LauncherEntry.Text, out var uri))
            return;

        var result = await Launcher.Default.OpenAsync(uri);
        ResultLabel.Text = result ? "Opened" : "Failed to open";
    }

    async void OnTryOpenClicked(object? sender, EventArgs e)
    {
        if (!TryParseUri(LauncherEntry.Text, out var uri))
            return;

        var result = await Launcher.Default.TryOpenAsync(uri);
        ResultLabel.Text = result ? "Opened" : "URI not supported or failed";
    }

    async void OnMailtoClicked(object? sender, EventArgs e)
    {
        var result = await Launcher.Default.TryOpenAsync(new Uri("mailto:test@example.com"));
        QuickLinkResultLabel.Text = result ? "Mail client opened" : "Failed";
    }

    async void OnTelClicked(object? sender, EventArgs e)
    {
        var result = await Launcher.Default.TryOpenAsync(new Uri("tel:+1234567890"));
        QuickLinkResultLabel.Text = result ? "Dialer opened" : "Failed";
    }

    bool TryParseUri(string? text, out Uri uri)
    {
        uri = null!;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (!Uri.TryCreate(text, UriKind.Absolute, out uri!))
        {
            ResultLabel.Text = "Invalid URI";
            return false;
        }

        return true;
    }
}
