using System.Diagnostics;
using Avalonia.Controls.Maui;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace WeatherTwentyOne;

public partial class MauiAppStub : Application
{
    public MauiAppStub()
    {
        InitializeComponent();
    }

    // protected override Window CreateWindow(IActivationState? activationState)
    // {
    //     return new Window(new AppShell());
    // }

    async void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync($"///settings");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"err: {ex.Message}");
        }
    }
}