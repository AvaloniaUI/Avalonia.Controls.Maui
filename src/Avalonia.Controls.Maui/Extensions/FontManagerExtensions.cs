using Microsoft.Maui;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Extension methods for IFontManager to access Avalonia-specific font functionality.
/// </summary>
public static class FontManagerExtensions
{
    /// <summary>
    /// Gets the font size for the provided font, with fallback logic.
    /// </summary>
    public static double GetFontSize(this IFontManager fontManager, Font font, double defaultFontSize = 0)
    {
        if (fontManager is FontManager avaloniaFontManager)
        {
            return avaloniaFontManager.GetFontSize(font, defaultFontSize);
        }

        // Fallback for other IFontManager implementations
        if (font.Size > 0 && !double.IsNaN(font.Size))
            return font.Size;

        if (defaultFontSize > 0)
            return defaultFontSize;

        return fontManager.DefaultFontSize;
    }

    /// <summary>
    /// Gets the Avalonia FontFamily for the provided font.
    /// </summary>
    public static global::Avalonia.Media.FontFamily GetFontFamily(this IFontManager fontManager, Font font)
    {
        if (fontManager is FontManager avaloniaFontManager)
        {
            return avaloniaFontManager.GetFontFamily(font);
        }

        // Fallback for other IFontManager implementations
        if (font.IsDefault || string.IsNullOrWhiteSpace(font.Family))
            return global::Avalonia.Media.FontFamily.Default;

        return new global::Avalonia.Media.FontFamily(font.Family);
    }
}
