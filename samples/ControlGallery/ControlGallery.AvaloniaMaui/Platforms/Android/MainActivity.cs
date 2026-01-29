using Android.App;
using Android.Content.PM;
using Avalonia;

namespace ControlGallery.AvaloniaMaui;

[Activity(Name = "com.Avalonia.ControlCatalog.MainActivity", Label = "ControlCatalog.AvaloniaMaui", MainLauncher = true, Exported = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : Avalonia.Android.AvaloniaMainActivity
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}