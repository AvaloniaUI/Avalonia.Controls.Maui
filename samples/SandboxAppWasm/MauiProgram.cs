using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;

namespace SandboxApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder();

        builder
             .UseMauiApp<MauiAppStub>()
             .UseAvaloniaApp(useSingleViewLifetime: true)
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

