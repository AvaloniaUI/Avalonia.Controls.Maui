namespace AlohaKit.Gallery.Mac;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder();

        builder
             .UseMauiApp<AlohaKit.Gallery.App>()
             .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Aloha.ttf", "Aloha");
            });

        return builder.Build();
    }
}
