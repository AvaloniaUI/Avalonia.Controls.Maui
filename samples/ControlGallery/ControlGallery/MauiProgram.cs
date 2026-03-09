using ControlGallery.Handlers;
using ControlGallery.Views;
using Microsoft.Extensions.Logging;

namespace ControlGallery;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp(bool useSingleAppLifetime = false)
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<MauiAppStub>()
			#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
			.UseAvaloniaApp(useSingleAppLifetime)
			.UseAvaloniaCompatibility()
			.UseAvaloniaEssentials()
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
				fonts.AddFont("bpg-banner-webfont.ttf", "BgpBanner");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
