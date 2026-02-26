using Avalonia.Controls.Maui.Maps.Mapsui;
using Avalonia.Controls.Maui.Compatibility;
using Microsoft.Extensions.Logging;

namespace MauiSandboxApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
			 .UseAvaloniaApp()
			 .UseAvaloniaGraphics()
			 .UseAvaloniaMapsui()
			 .UseAvaloniaCompatibility()
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
