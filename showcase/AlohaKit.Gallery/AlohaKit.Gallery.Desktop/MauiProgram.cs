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

namespace AlohaKit.Gallery.Desktop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder();

        builder
             .UseMauiApp<AlohaKit.Gallery.App>()
             .UseAvaloniaApp()
             .UseAvaloniaGraphics()
             .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Aloha.ttf", "Aloha");
            });

        return builder.Build();
    }
}

