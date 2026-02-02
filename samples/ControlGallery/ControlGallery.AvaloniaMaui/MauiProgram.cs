using Microsoft.Extensions.Logging;

namespace ControlGallery.AvaloniaMaui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<MauiAppStub>()
			.UseAvaloniaApp(true)
			.UseAvaloniaGraphics()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("bpg-banner-webfont.ttf", "BgpBanner");
			});

#if ANDROID
// Add Android Activity to builder services
builder.Services.AddSingleton<global::Android.App.Activity, MainActivity>();
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
