using Microsoft.Maui;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Avalonia.Controls.Maui;

public class FontManager : IFontManager
{
    private readonly ConcurrentDictionary<string, global::Avalonia.Media.FontFamily> _fontFamilies = new();
    private readonly IFontRegistrar _fontRegistrar;
    private readonly IServiceProvider? _serviceProvider;
    private global::Avalonia.Media.FontFamily? _defaultFontFamily;

    /// <summary>
    /// Creates a new <see cref="FontManager"/> instance.
    /// </summary>
    /// <param name="fontRegistrar">A <see cref="IFontRegistrar"/> instance to retrieve details from about registered fonts.</param>
    /// <param name="serviceProvider">The applications <see cref="IServiceProvider"/>.
    /// Typically this is provided through dependency injection.</param>
    public FontManager(IFontRegistrar fontRegistrar, IServiceProvider? serviceProvider = null)
    {
        _fontRegistrar = fontRegistrar;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public double DefaultFontSize => 14;

    /// <summary>
    /// Gets the default font family for the application.
    /// </summary>
    public global::Avalonia.Media.FontFamily DefaultFontFamily =>
        _defaultFontFamily ??= global::Avalonia.Media.FontFamily.Default;

    /// <summary>
    /// Retrieves the Avalonia <see cref="global::Avalonia.Media.FontFamily"/> for the given MAUI font.
    /// </summary>
    /// <param name="font">The MAUI font to convert.</param>
    /// <returns>The Avalonia font family.</returns>
    public global::Avalonia.Media.FontFamily GetFontFamily(Font font)
    {
        if (font.IsDefault || string.IsNullOrWhiteSpace(font.Family))
            return DefaultFontFamily;

        return _fontFamilies.GetOrAdd(font.Family, CreateFontFamily);
    }

    /// <summary>
    /// Gets the font size for the provided font.
    /// </summary>
    /// <param name="font">The font to get the size for.</param>
    /// <param name="defaultFontSize">Default font size when the provided font does not have a (valid) value.</param>
    /// <returns>
    /// If <see cref="Font.Size"/> is more than 0 and not equal to <see cref="double.NaN"/>, returns <see cref="Font.Size"/>.
    /// Else, if <paramref name="defaultFontSize"/> is more than 0, returns <paramref name="defaultFontSize"/>.
    /// Else, returns <see cref="DefaultFontSize"/>.
    /// </returns>
    public virtual double GetFontSize(Font font, double defaultFontSize = 0)
    {
        if (font.Size > 0 && !double.IsNaN(font.Size))
            return font.Size;

        if (defaultFontSize > 0)
            return defaultFontSize;

        return DefaultFontSize;
    }

    /// <summary>
    /// Converts a MAUI <see cref="FontWeight"/> to an Avalonia <see cref="global::Avalonia.Media.FontWeight"/>.
    /// </summary>
    /// <param name="weight">The MAUI font weight.</param>
    /// <returns>The Avalonia font weight.</returns>
    public static global::Avalonia.Media.FontWeight ToAvaloniaFontWeight(FontWeight weight)
    {
        // Both MAUI and Avalonia use the same numeric values for font weights
        return (global::Avalonia.Media.FontWeight)(int)weight;
    }

    /// <summary>
    /// Converts a MAUI <see cref="FontSlant"/> to an Avalonia <see cref="global::Avalonia.Media.FontStyle"/>.
    /// </summary>
    /// <param name="slant">The MAUI font slant.</param>
    /// <returns>The Avalonia font style.</returns>
    public static global::Avalonia.Media.FontStyle ToAvaloniaFontStyle(FontSlant slant)
    {
        return slant switch
        {
            FontSlant.Italic => global::Avalonia.Media.FontStyle.Italic,
            FontSlant.Oblique => global::Avalonia.Media.FontStyle.Oblique,
            _ => global::Avalonia.Media.FontStyle.Normal
        };
    }

    private global::Avalonia.Media.FontFamily CreateFontFamily(string fontFamily)
    {
        try
        {
            // Check if the font is registered
            var registeredFontName = _fontRegistrar.GetFont(fontFamily);
            if (registeredFontName != null)
            {
                var logger = (_serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger<FontManager>();
                logger?.LogDebug("Font '{FontFamily}' resolved to '{RegisteredFontName}'", fontFamily, registeredFontName);

                // Return a FontFamily with just the font family name
                // Avalonia will look for this font in the system or embedded resources
                return new global::Avalonia.Media.FontFamily(registeredFontName);
            }

            // Otherwise, use the font family name directly
            return new global::Avalonia.Media.FontFamily(fontFamily);
        }
        catch (Exception ex)
        {
            // Log the error if possible
            var loggerFactory = _serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            loggerFactory?.CreateLogger<FontManager>()?.LogWarning(ex, "Unable to load font '{FontFamily}'. Using default font.", fontFamily);

            return DefaultFontFamily;
        }
    }
}
