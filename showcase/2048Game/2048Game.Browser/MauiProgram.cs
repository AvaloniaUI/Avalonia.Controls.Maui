using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace _2048Game.Browser;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
		var builder = MauiApp
			.CreateBuilder();

		builder
			.UseMauiApp<_2048Game.App>()
			.UseAvaloniaApp(true)
			.UseAvaloniaGraphics()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
				fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
				fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemibold");
				fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
			});

		return builder.Build();
    }
}
