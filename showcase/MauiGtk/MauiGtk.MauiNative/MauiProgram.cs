using MauiIcons.FontAwesome;
using MauiIcons.FontAwesome.Solid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Platform.Maui.Linux.Gtk4.Sample;

namespace MauiGtk;

public static class MauiProgram
{
#if MAUIDEVFLOW
	static bool _devFlowAgentStarted;
#endif

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp
			.CreateBuilder()
			.UseMauiApp<App>()
			.UseFontAwesomeMauiIcons()
			.UseFontAwesomeSolidMauiIcons();

		builder.ConfigureFonts(fonts =>
		{
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
		});

		return builder.Build();
	}
}
