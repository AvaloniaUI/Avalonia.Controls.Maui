namespace ControlGallery.Pages;

public partial class PageBasePage : ContentPage
{
    public PageBasePage()
    {
        InitializeComponent();
    }

    private void OnChangeBackground(object? sender, EventArgs e)
    {
        // Set a different image
        BackgroundImageSource = ImageSource.FromFile("redbug.png");
        StatusLabel.Text = "This page has a BackgroundImageSource set via FileImageSource (redbug.png).";
    }

    private void OnSetRedBackground(object? sender, EventArgs e)
    {
        Background = new SolidColorBrush(Colors.Red);
        StatusLabel.Text = "This page has a Background set to SolidColorBrush (Red).";
    }

    private void OnSetGradientBackground(object? sender, EventArgs e)
    {
        Background = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Colors.Blue, 0.0f),
                new GradientStop(Colors.Violet, 1.0f)
            }
        };
        StatusLabel.Text = "This page has a Background set to LinearGradientBrush (Blue-Violet).";
    }

    private void OnClearBackgroundImage(object? sender, EventArgs e)
    {
        BackgroundImageSource = null;
        StatusLabel.Text = "BackgroundImageSource cleared.";
    }

    private void OnClearBackground(object? sender, EventArgs e)
    {
        Background = null;
        StatusLabel.Text = "Background cleared.";
    }

}
