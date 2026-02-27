using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Maui;
using Avalonia.Controls.Maui.Maps.Mapsui;
using Avalonia.Controls.Maui.Compatibility;
using Avalonia.Controls.Maui.Essentials;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using ControlGallery.Views;
using ControlGallery.Handlers;

namespace ControlGallery;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder();

        builder
             .UseMauiApp<MauiAppStub>()
             .UseAvaloniaApp()
             .UseAvaloniaCompatibility()
             .UseAvaloniaEssentials()
             .UseAvaloniaGraphics()
             .UseAvaloniaMapsui()
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
