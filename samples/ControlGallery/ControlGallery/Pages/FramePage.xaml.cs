namespace ControlGallery.Pages;

public partial class FramePage : ContentPage
{
    public FramePage()
    {
        InitializeComponent();
    }

    private void OnCornerRadiusChanged(object? sender, ValueChangedEventArgs e)
    {
        var value = (float)e.NewValue;
        InteractiveFrame.CornerRadius = value;
        CornerRadiusValue.Text = ((int)value).ToString();
    }

    private void OnShadowToggled(object? sender, ToggledEventArgs e)
    {
        InteractiveFrame.HasShadow = e.Value;
    }

    private void OnRedBorderClicked(object? sender, EventArgs e)
    {
        InteractiveFrame.BorderColor = Colors.Red;
    }

    private void OnBlueBorderClicked(object? sender, EventArgs e)
    {
        InteractiveFrame.BorderColor = Colors.Blue;
    }

    private void OnGreenBorderClicked(object? sender, EventArgs e)
    {
        InteractiveFrame.BorderColor = Colors.Green;
    }

    private void OnNoBorderClicked(object? sender, EventArgs e)
    {
        InteractiveFrame.BorderColor = Colors.Transparent;
    }
}
