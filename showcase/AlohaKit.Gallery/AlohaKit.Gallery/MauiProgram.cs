using Microsoft.Extensions.Logging;

namespace AlohaKit.Gallery;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp(bool useSingleAppLifetime = false)
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<AlohaKit.Gallery.App>()
			#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
			.UseAvaloniaApp(useSingleAppLifetime)
			#else
			.UseAvaloniaEmbedding<AvaloniaApp>()
			#endif
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Aloha.ttf", "Aloha");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
