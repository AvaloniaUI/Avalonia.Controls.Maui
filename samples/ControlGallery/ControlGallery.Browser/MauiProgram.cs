using Avalonia.Controls.Maui.Compatibility;
using Avalonia.Controls.Maui.Essentials;

namespace ControlGallery;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<ControlGallery.MauiAppStub>()
            .UseAvaloniaApp(useSingleViewLifetime: true)
            .UseAvaloniaCompatibility()
            .UseAvaloniaEssentials()
            .UseAvaloniaGraphics()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("bpg-banner-webfont.ttf", "BgpBanner");
            });

        return builder.Build();
    }
}
