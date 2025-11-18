using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;

namespace Avalonia.Controls.Maui.Tests;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}