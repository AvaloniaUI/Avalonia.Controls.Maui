using ControlGallery.Handlers;
using ControlGallery.Views;
using ControlGallery.Effects;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
#if IOS || MACCATALYST || ANDROID || WINDOWS
using SkiaSharp.Views.Maui.Controls.Hosting;
#endif

namespace ControlGallery;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp(bool useSingleAppLifetime = false)
	{
		var builder = MauiApp.CreateBuilder();

		// The toolkit marks this as supported on the native platforms only, but the views it
		// registers are cross-platform and work on Avalonia too
#pragma warning disable CA1416
		builder.UseMauiCommunityToolkit();
#pragma warning restore CA1416

		builder
			.UseMauiApp<MauiAppStub>()
			#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
			.UseAvaloniaApp(useSingleAppLifetime)
			.UseAvaloniaCompatibility()
			.UseAvaloniaEssentials()
			.UseAvaloniaSkiaSharp()
			#else
			.UseAvaloniaEmbedding<AvaloniaApp>()
			.UseSkiaSharp()
			#endif
			.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<CounterView, CounterViewHandler>();
			})
			.ConfigureEffects(effects =>
			{
				effects.Add<FocusRoutingEffect, FocusPlatformEffect>();
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("bpg-banner-webfont.ttf", "BgpBanner");
				fonts.AddFont("GalleryScript-Regular.ttf", "GalleryScript");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
