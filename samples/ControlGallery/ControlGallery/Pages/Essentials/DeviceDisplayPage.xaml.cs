using Microsoft.Maui.Devices;
using Avalonia.Controls.Maui.Essentials.Devices;

namespace ControlGallery.Pages.Essentials;

public partial class DeviceDisplayPage : ContentPage
{
    private readonly IDeviceDisplay _deviceDisplay;

    public DeviceDisplayPage()
    {
        InitializeComponent();

        _deviceDisplay = new AvaloniaDeviceDisplay();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateDisplayInfo();
        _deviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _deviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
    }

    private void UpdateDisplayInfo()
    {
        var info = _deviceDisplay.MainDisplayInfo;
        WidthLabel.Text = info.Width.ToString();
        HeightLabel.Text = info.Height.ToString();
        DensityLabel.Text = info.Density.ToString();
        OrientationLabel.Text = info.Orientation.ToString();
        RotationLabel.Text = info.Rotation.ToString();
        RefreshRateLabel.Text = info.RefreshRate.ToString();
        
        KeepScreenOnSwitch.IsToggled = _deviceDisplay.KeepScreenOn;
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateDisplayInfo);
    }

    private void OnKeepScreenOnToggled(object sender, ToggledEventArgs e)
    {
        _deviceDisplay.KeepScreenOn = e.Value;
    }
}
