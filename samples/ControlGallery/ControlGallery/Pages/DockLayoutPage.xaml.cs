namespace ControlGallery.Pages;

public partial class DockLayoutPage : ContentPage
{
    public DockLayoutPage()
    {
        InitializeComponent();
    }

    private void OnExpandLastChildToggled(object? sender, ToggledEventArgs e)
    {
        DemoDock.ShouldExpandLastChild = e.Value;
    }

    private void OnSpacingChanged(object? sender, ValueChangedEventArgs e)
    {
        var spacing = Math.Round(e.NewValue);
        SpacingDock.HorizontalSpacing = spacing;
        SpacingDock.VerticalSpacing = spacing;
        SpacingLabel.Text = $"Spacing: {spacing}";
    }
}
