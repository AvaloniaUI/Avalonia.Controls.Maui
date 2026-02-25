using Avalonia.Controls.Maui.Maps.Controls;
using Avalonia.Controls.Maui.Maps.Mapsui.Handlers;
using Microsoft.Maui.Hosting;

namespace Avalonia.Controls.Maui.Maps.Mapsui;

/// <summary>
/// Extension methods for configuring Mapsui-based Map handlers in MAUI.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Configures Mapsui-based Avalonia Map handlers for MAUI.
    /// </summary>
    /// <param name="builder">The MAUI app builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static MauiAppBuilder UseAvaloniaMapsui(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<MapView, MapsuiMapHandler>();
        });
        
        return builder;
    }
}
