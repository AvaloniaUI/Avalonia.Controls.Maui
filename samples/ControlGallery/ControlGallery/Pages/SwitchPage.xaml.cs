namespace ControlGallery.Pages;

public partial class SwitchPage : ContentPage
{
    private int _toggleCount;

    public SwitchPage()
    {
        InitializeComponent();
    }

    private void OnBasicToggled(object? sender, ToggledEventArgs e)
    {
        _toggleCount++;
        ToggleCountLabel.Text = $"Toggled {_toggleCount} time{(_toggleCount == 1 ? "" : "s")}";

        if (sender == BasicSwitch)
        {
            BasicStateLabel.Text = e.Value ? "On" : "Off";
        }
        else
        {
            BasicOnStateLabel.Text = e.Value ? "On" : "Off";
        }
    }

    private void OnSuccessErrorToggled(object? sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            SuccessErrorLabel.Text = "✓ Success";
            SuccessErrorLabel.TextColor = Color.FromArgb("#34C759");
        }
        else
        {
            SuccessErrorLabel.Text = "✗ Error";
            SuccessErrorLabel.TextColor = Color.FromArgb("#FF3B30");
        }
    }

    private void OnEventSwitchToggled(object? sender, ToggledEventArgs e)
    {
        EventStateLabel.Text = e.Value ? "On" : "Off";
        EventLogLabel.Text = $"Last event: Toggled to {(e.Value ? "On" : "Off")}";
        EventTimestampLabel.Text = $"Timestamp: {DateTime.Now:HH:mm:ss.fff}";
    }
}