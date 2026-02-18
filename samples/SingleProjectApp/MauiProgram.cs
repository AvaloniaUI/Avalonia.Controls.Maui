using Avalonia.Controls.Maui;
using Microsoft.Extensions.Logging;

namespace SingleProjectApp;

public static class MauiProgram
{
	[AvaloniaMauiApp]
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if !ANDROID && !IOS && !MACCATALYST && !WINDOWS
			.UseAvaloniaApp(useSingleViewLifetime: OperatingSystem.IsBrowser())
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
