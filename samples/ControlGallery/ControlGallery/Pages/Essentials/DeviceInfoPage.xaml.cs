using Microsoft.Maui.Devices;
using Avalonia.Controls.Maui.Essentials.Devices;

namespace ControlGallery.Pages.Essentials;

public partial class DeviceInfoPage : ContentPage
{
    public DeviceInfoPage()
    {
        InitializeComponent();
        LoadDeviceInfo();
    }

    private void LoadDeviceInfo()
    {
        var deviceInfo = AvaloniaDeviceInfo.Current;
        ModelLabel.Text = deviceInfo.Model;
        ManufacturerLabel.Text = deviceInfo.Manufacturer;
        NameLabel.Text = deviceInfo.Name;
        VersionLabel.Text = deviceInfo.VersionString;
        PlatformLabel.Text = deviceInfo.Platform.ToString();
        IdiomLabel.Text = deviceInfo.Idiom.ToString();
        DeviceTypeLabel.Text = deviceInfo.DeviceType.ToString();
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadDeviceInfo();
    }
}
