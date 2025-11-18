using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using System;
using System.Reflection;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Extension methods for configuring Avalonia AppBuilder with MAUI fonts.
/// </summary>
public static class AvaloniaAppBuilderExtensions
{
    /// <summary>
    /// Configures embedded MAUI fonts for use in Avalonia.
    /// </summary>
    public static AppBuilder WithMauiFonts(this AppBuilder builder)
    {
        return builder.ConfigureFonts(fontManager =>
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (!string.IsNullOrEmpty(assemblyName))
            {
                // Create an embedded font collection for fonts in Assets/Fonts
                var fontUri = new Uri($"fonts:{assemblyName}", UriKind.Absolute);
                var sourceUri = new Uri($"avares://{assemblyName}/Assets/Fonts", UriKind.Absolute);

                var fontCollection = new EmbeddedFontCollection(fontUri, sourceUri);
                fontManager.AddFontCollection(fontCollection);
            }
        });
    }
}
