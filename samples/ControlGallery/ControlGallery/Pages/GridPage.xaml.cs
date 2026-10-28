namespace ControlGallery.Pages;

public partial class GridPage : ContentPage
{
    public GridPage()
    {
        InitializeComponent();
    }

    private void OnOverlayInputToggled(object? sender, ToggledEventArgs e)
    {
        InputOverlay.InputTransparent = InputTransparentSwitch.IsToggled;
        InputOverlay.CascadeInputTransparent = CascadeSwitch.IsToggled;
    }

    private void OnInputTargetClicked(object? sender, EventArgs e)
    {
        if (sender is Button button)
            InputTargetLabel.Text = $"Last clicked: {button.Text}";
    }
}
