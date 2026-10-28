namespace ControlGallery.Pages;

public partial class StackLayoutPage : ContentPage
{
    public StackLayoutPage()
    {
        InitializeComponent();
    }

    private void OnMarginSliderValueChanged(object? sender, ValueChangedEventArgs e)
    {
        var margin = Math.Round(e.NewValue);
        MarginLabel.Margin = new Thickness(margin, 5);
        MarginValueLabel.Text = $"Horizontal margin: {margin}";
    }
}
