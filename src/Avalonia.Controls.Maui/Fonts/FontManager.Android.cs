using Android.Graphics;
using Android.Util;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui;

public partial class FontManager
{
    /// <summary>
    /// Gets the default Typeface for the operating system.
    /// This is a stub implementation for Avalonia - native font handling is not used.
    /// </summary>
    public Typeface DefaultTypeface => Typeface.Default!;

    /// <summary>
    /// Retrieves the platform equivalent Typeface for an abstract Font object.
    /// This is a stub implementation for Avalonia - native font handling is not used.
    /// </summary>
    /// <param name="font">The abstract font representation.</param>
    /// <returns>A Typeface representing the requested font, or null if not found.</returns>
    public Typeface? GetTypeface(Font font)
    {
        if (font.IsDefault)
            return DefaultTypeface;

        // Return a basic typeface - Avalonia handles the actual font rendering
        // Use TypefaceStyle for broader API compatibility
        var style = font.Slant != FontSlant.Default
            ? (font.Weight >= FontWeight.Bold ? TypefaceStyle.BoldItalic : TypefaceStyle.Italic)
            : (font.Weight >= FontWeight.Bold ? TypefaceStyle.Bold : TypefaceStyle.Normal);

        return Typeface.Create(Typeface.Default, style);
    }

    /// <summary>
    /// Gets the font size for the provided font.
    /// This is a stub implementation for Avalonia - native font handling is not used.
    /// </summary>
    /// <param name="font">The font to get the size for.</param>
    /// <param name="defaultFontSize">Default font size when the font doesn't have a valid value.</param>
    /// <returns>A FontSize struct with the size and unit type.</returns>
    public FontSize GetFontSize(Font font, float defaultFontSize = 0)
    {
        double size;
        if (font.Size > 0 && !double.IsNaN(font.Size))
            size = font.Size;
        else if (defaultFontSize > 0)
            size = defaultFontSize;
        else
            size = DefaultFontSize;

        // If auto-scaling is enabled, use SP (scaled pixels), otherwise DP (device-independent pixels)
        var unit = font.AutoScalingEnabled ? ComplexUnitType.Sp : ComplexUnitType.Dip;
        return new FontSize((float)size, unit);
    }
}
