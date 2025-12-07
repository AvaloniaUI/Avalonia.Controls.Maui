using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace MauiPlanets;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder();

        builder
             .UseMauiApp<MauiAppStub>()
             .UseAvaloniaApp(true)
             .UseAvaloniaGraphics()
             .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Montserrat-Medium.ttf", "RegularFont");
                fonts.AddFont("Montserrat-SemiBold.ttf", "MediumFont");
                fonts.AddFont("Montserrat-Bold.ttf", "BoldFont");
            });
        var services = builder.Services;

        return builder.Build();
    }
}
