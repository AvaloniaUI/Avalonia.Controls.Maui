using Avalonia.Controls.Maui.Maps.Controls;
using Avalonia.Controls.Maui.Maps.Handlers;
using Microsoft.Maui.Hosting;

namespace Avalonia.Controls.Maui.Maps;

/// <summary>
/// Extension methods for configuring Avalonia Maps handlers in MAUI.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Configures Avalonia-specific Map handlers for MAUI.
    /// </summary>
    /// <param name="builder">The MAUI app builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static MauiAppBuilder UseAvaloniaMaps(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<MapView, MapHandler>();
        });
        
        return builder;
    }
}

