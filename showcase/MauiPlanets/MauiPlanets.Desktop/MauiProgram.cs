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

namespace MauiPlanets;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder();

        builder
             .UseMauiApp<MauiAppStub>()
             .UseAvaloniaApp()
             .UseAvaloniaGraphics()
             			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Montserrat-Medium.ttf", "RegularFont");
                fonts.AddFont("Montserrat-SemiBold.ttf", "MediumFont");
                fonts.AddFont("Montserrat-Bold.ttf", "BoldFont");
			});

        return builder.Build();
    }
}

