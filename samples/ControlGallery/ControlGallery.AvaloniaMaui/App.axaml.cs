using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui.Hosting;

namespace ControlGallery.AvaloniaMaui;

public class App : MauiAvaloniaApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}