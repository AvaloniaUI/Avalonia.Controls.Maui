using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace AlohaKit.Gallery.Browser;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder();

        builder
             .UseMauiApp<AlohaKit.Gallery.App>()
             .UseAvaloniaApp(true)
             .UseAvaloniaGraphics()
             .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Aloha.ttf", "Aloha");
            });
        var services = builder.Services;

        return builder.Build();
    }
}
