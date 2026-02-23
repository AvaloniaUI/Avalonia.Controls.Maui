using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace ControlGallery;

public partial class MauiAppStub : Application
{
    public MauiAppStub()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "Control Gallery" };
    }
}