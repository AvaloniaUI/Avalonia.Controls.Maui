using EmbedApp.Handlers;
using EmbedApp.Views;
using Microsoft.Extensions.Logging;

namespace EmbedApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
			.UseAvaloniaApp()
#else
			.UseAvaloniaEmbedding<AvaloniaApp>()
#endif
			.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<CounterView, CounterViewHandler>();
			})
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