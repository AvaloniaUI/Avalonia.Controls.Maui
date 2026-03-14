using Microsoft.Extensions.Logging;
using MyConference.Services;
using MyConference.ViewModels;
using MyConference.Views;

namespace MyConference;

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
            #endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Remove native borders from Entry (used for search fields)
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("BorderlessEntry", (handler, view) =>
        {
#if IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#elif ANDROID
            handler.PlatformView.Background = null;
            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#endif
        });

        // Services
        builder.Services.AddSingleton<ICacheService, CacheService>();
        builder.Services.AddSingleton<IFavoritesService, FavoritesService>();
        builder.Services.AddSingleton<ISessionizeService, SessionizeService>();
#if BROWSER
        var httpClient = new HttpClient(new CorsProxyHandler());
#else
        var httpClient = new HttpClient();
#endif
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        builder.Services.AddSingleton(httpClient);
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
        builder.Services.AddSingleton(Connectivity.Current);
#endif

        // ViewModels
        builder.Services.AddTransient<SessionsViewModel>();
        builder.Services.AddTransient<SessionDetailViewModel>();
        builder.Services.AddTransient<SpeakersViewModel>();
        builder.Services.AddTransient<SpeakerDetailViewModel>();
        builder.Services.AddTransient<FavoritesViewModel>();
        builder.Services.AddTransient<AboutViewModel>();

        // Pages
        builder.Services.AddTransient<SessionsPage>();
        builder.Services.AddTransient<SessionDetailPage>();
        builder.Services.AddTransient<SpeakersPage>();
        builder.Services.AddTransient<SpeakerDetailPage>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<AboutPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
