namespace ControlGallery.Pages.Essentials;

public partial class OnPlatformPage : ContentPage
{
    public OnPlatformPage()
    {
        InitializeComponent();
    }

    void OnCheckPlatformClicked(object? sender, EventArgs e)
    {
        RuntimePlatformLabel.Text = $"Platform: {DeviceInfo.Current.Platform}";
        RuntimeIdiomLabel.Text = $"Idiom: {DeviceInfo.Current.Idiom}";
    }
}
