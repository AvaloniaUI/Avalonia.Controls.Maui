using MauiIcons.FontAwesome;
using MauiIcons.FontAwesome.Solid;
using Platform.Maui.Linux.Gtk4.Hosting;
using Platform.Maui.Linux.Gtk4.Essentials.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Platform.Maui.Linux.Gtk4.Sample;

public static class MauiProgram
{
#if MAUIDEVFLOW
	static bool _devFlowAgentStarted;
#endif

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp
			.CreateBuilder()
			.UseMauiAppLinuxGtk4<App>()
			.AddLinuxGtk4Essentials()
			.UseFontAwesomeMauiIcons()
			.UseFontAwesomeSolidMauiIcons();

		builder.ConfigureFonts(fonts =>
		{
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
		});

		return builder.Build();
	}
}
