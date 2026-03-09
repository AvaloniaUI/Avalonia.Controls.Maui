namespace ControlGallery.Pages;

public partial class ScreenshotPage : ContentPage
{
    public ScreenshotPage()
    {
        InitializeComponent();
        SupportedLabel.Text = $"IsCaptureSupported: {Screenshot.Default.IsCaptureSupported}";
    }

    private async void OnCaptureClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!Screenshot.Default.IsCaptureSupported)
            {
                DimensionsLabel.Text = "Screenshot capture is not supported.";
                return;
            }

            var result = await Screenshot.Default.CaptureAsync();
            DimensionsLabel.Text = $"Dimensions: {result.Width} x {result.Height}";

            using var stream = await result.OpenReadAsync();
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;
            ScreenshotImage.Source = ImageSource.FromStream(() => ms);
        }
        catch (Exception ex)
        {
            DimensionsLabel.Text = $"Error: {ex.Message}";
        }
    }
}
