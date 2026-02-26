using Microsoft.Maui.Hosting;

/// <summary>
/// Provides extension methods for configuring Avalonia graphics services in a .NET MAUI application.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Configures all Avalonia-specific graphic services for MAUI
    /// </summary>
    public static MauiAppBuilder UseAvaloniaGraphics(this MauiAppBuilder builder)
    {
        return builder
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<Microsoft.Maui.Controls.GraphicsView, Avalonia.Controls.Maui.Handlers.GraphicsViewHandler>();

            })
            .ConfigureImageSources();
    }
}