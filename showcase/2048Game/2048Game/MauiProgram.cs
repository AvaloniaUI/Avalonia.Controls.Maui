using Microsoft.Extensions.Logging;

namespace _2048Game;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp(bool useSingleAppLifetime = false)
	{
		var builder = MauiApp.CreateBuilder();

#if MACCATALYST
		Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("ButtonChange", (handler, view) =>
		{
			handler.PlatformView.PreferredBehavioralStyle = UIKit.UIBehavioralStyle.Pad;
			handler.PlatformView.ClipsToBounds = true;
		});
#endif

		builder
			.UseMauiApp<_2048Game.App>()
			#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
			.UseAvaloniaApp(useSingleAppLifetime)
			#else
			.UseAvaloniaEmbedding<AvaloniaApp>()
			#endif
			.UseAvaloniaGraphics()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
				fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
				fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemibold");
				fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
