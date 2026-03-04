using Avalonia.Controls;
using Avalonia.Controls.Maui.Compatibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Avalonia.Controls.Maui.Essentials;
using ControlGallery.Views;
using ControlGallery.Handlers;

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
            .ConfigureMauiHandlers(handlers =>
             {
                 handlers.AddHandler<CounterView, CounterViewHandler>();
             })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("bpg-banner-webfont.ttf", "BgpBanner");
            });

        return builder.Build();
    }
}
