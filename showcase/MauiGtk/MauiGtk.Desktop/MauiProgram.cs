using MauiIcons.FontAwesome;
using MauiIcons.FontAwesome.Solid;
using Microsoft.Maui.Hosting;
using Avalonia.Controls.Maui.Essentials;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui;
using Avalonia.Platform;
using Microsoft.Maui.Controls.Hosting;

namespace Platform.Maui.Linux.Gtk4.Sample;

public static class MauiProgram
{

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp
			.CreateBuilder()
			.UseMauiApp<App>()
			.UseAvaloniaApp()
			.UseAvaloniaGraphics()
			.UseFontAwesomeMauiIcons()
			.UseFontAwesomeSolidMauiIcons();

		builder.ConfigureFonts(fonts =>
		{
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
		});

		return builder.Build();
	}
}
