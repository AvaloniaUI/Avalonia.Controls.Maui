using Avalonia.Controls.Maui.Platform;
using Avalonia.Markup.Xaml;
using Microsoft.Maui.Hosting;

namespace Avalonia.Controls.Maui.Tests;

public class App : MauiAvaloniaApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}