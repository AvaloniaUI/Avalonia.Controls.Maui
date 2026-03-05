using Microsoft.Extensions.Logging;

namespace MauiPlanets;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp(bool useSingleAppLifetime = false)
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<MauiAppStub>()
			#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
			.UseAvaloniaApp(useSingleAppLifetime)
			#else
			.UseAvaloniaEmbedding<AvaloniaApp>()
			#endif
			.UseAvaloniaGraphics()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Montserrat-Medium.ttf", "RegularFont");
				fonts.AddFont("Montserrat-SemiBold.ttf", "MediumFont");
				fonts.AddFont("Montserrat-Bold.ttf", "BoldFont");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
