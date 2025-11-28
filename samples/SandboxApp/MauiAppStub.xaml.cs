using Avalonia.Controls.Maui;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SandboxApp;

public partial class MauiAppStub : Application
{
    public MauiAppStub()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Width = 800, Height = 600 };
    }
}