using Microsoft.Maui.Devices;
using Avalonia.Controls.Maui.Essentials.Devices;

namespace ControlGallery.Pages.Essentials;

public partial class BatteryPage : ContentPage
{
    public BatteryPage()
    {
        InitializeComponent();
        LoadBatteryInfo();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AvaloniaBattery.Default.BatteryInfoChanged += OnBatteryInfoChanged;
        AvaloniaBattery.Default.EnergySaverStatusChanged += OnEnergySaverStatusChanged;
        LoadBatteryInfo();
    }

    private void LoadBatteryInfo()
    {
        var battery = AvaloniaBattery.Default;
        LevelLabel.Text = $"{(battery.ChargeLevel * 100):F0}%";
        LevelProgressBar.Progress = battery.ChargeLevel;
        StateLabel.Text = battery.State.ToString();
        PowerSourceLabel.Text = battery.PowerSource.ToString();
        EnergySaverLabel.Text = battery.EnergySaverStatus.ToString();
    }

    private void OnBatteryInfoChanged(object? sender, BatteryInfoChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LoadBatteryInfo();
        });
    }

    private void OnEnergySaverStatusChanged(object? sender, EnergySaverStatusChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LoadBatteryInfo();
        });
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadBatteryInfo();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        AvaloniaBattery.Default.BatteryInfoChanged -= OnBatteryInfoChanged;
        AvaloniaBattery.Default.EnergySaverStatusChanged -= OnEnergySaverStatusChanged;
    }
}
