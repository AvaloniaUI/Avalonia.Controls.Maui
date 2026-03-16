using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using WeatherTwentyOne.Pages;
using WeatherTwentyOne.ViewModels;

namespace WeatherTwentyOne;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp(bool useSingleAppLifetime = false)
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            #if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
            .UseAvaloniaApp(useSingleAppLifetime)
            .UseAvaloniaEssentials()
            #else
            .UseAvaloniaEmbedding<AvaloniaApp>()
            #endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            });

        builder.ConfigureLifecycleEvents(lifecycle => {
#if WINDOWS
            lifecycle.AddWindows(windows => windows.OnWindowCreated((del) => {
                del.ExtendsContentIntoTitleBar = true;
            }));
#endif
        });

        var services = builder.Services;
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<HomePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}