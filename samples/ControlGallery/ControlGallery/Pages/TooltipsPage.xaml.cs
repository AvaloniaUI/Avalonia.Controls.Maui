using Microsoft.Maui.Controls;

namespace ControlGallery.Pages;

public partial class TooltipsPage : ContentPage
{
    private int _clickCount = 0;

    public TooltipsPage()
    {
        InitializeComponent();
    }

    private void OnDynamicTooltipButtonClicked(object sender, EventArgs e)
    {
        _clickCount++;
        ToolTipProperties.SetText(DynamicTooltipButton, $"Click count: {_clickCount}");
        ClickCountLabel.Text = $"Button clicked {_clickCount} time(s). Hover to see updated tooltip.";
    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        var value = (int)e.NewValue;
        ToolTipProperties.SetText(TooltipSlider, $"Value: {value}");
        SliderValueLabel.Text = $"Current value: {value}";
    }
}
