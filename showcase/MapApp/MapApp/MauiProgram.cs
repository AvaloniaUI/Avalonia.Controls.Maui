using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MapApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp(bool useSingleAppLifetime = false)
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
			.UseAvaloniaApp(useSingleAppLifetime)
			.UseAvaloniaSkiaSharp()
#else
			.UseSkiaSharp()
#endif
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
