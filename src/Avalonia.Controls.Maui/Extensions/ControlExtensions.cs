using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Extension methods for IFontManager to provide consistent double return for font size.
/// </summary>
public static class FontManagerExtensions
{
    /// <summary>
    /// Gets the font size as a double value, consistent across all platforms.
    /// On Android, IFontManager.GetFontSize returns FontSize struct, but Avalonia needs double.
    /// </summary>
    public static double GetFontSizeAsDouble(this IFontManager fontManager, Font font, double defaultFontSize = 0)
    {
#if ANDROID
        var fontSize = fontManager.GetFontSize(font, (float)defaultFontSize);
        return fontSize.Value;
#else
        return fontManager.GetFontSize(font, defaultFontSize);
#endif
    }
}

/// <summary>
/// Extension methods for Avalonia TemplatedControl to support MAUI font handling.
/// </summary>
public static class ControlExtensions
{
    /// <summary>
    /// Updates the font properties of a TemplatedControl using the MAUI Font and FontManager.
    /// </summary>
    /// <param name="control">The Avalonia TemplatedControl to update.</param>
    /// <param name="font">The MAUI Font to apply.</param>
    /// <param name="fontManager">The FontManager to use for font resolution.</param>
    public static void UpdateFont(this TemplatedControl control, Font font, IFontManager fontManager)
    {
        if (control == null)
            throw new ArgumentNullException(nameof(control));

        if (fontManager == null)
            throw new ArgumentNullException(nameof(fontManager));

        if (font.IsDefault)
            return;

        // Set font size using FontManager's size resolution logic
        control.FontSize = fontManager.GetFontSizeAsDouble(font);

        // Set font family using FontManager (handles custom fonts via IFontRegistrar)
        control.FontFamily = Avalonia.Controls.Maui.FontManagerExtensions.GetFontFamily(fontManager, font);

        // Set font style (italic/oblique)
        control.FontStyle = FontManager.ToAvaloniaFontStyle(font.Slant);

        // Set font weight (thin/light/normal/bold/etc)
        control.FontWeight = FontManager.ToAvaloniaFontWeight(font.Weight);
    }

    /// <summary>
    /// Updates the font properties of a TemplatedControl using an ITextStyle element.
    /// </summary>
    /// <param name="control">The Avalonia TemplatedControl to update.</param>
    /// <param name="textStyle">The ITextStyle element containing the font.</param>
    /// <param name="fontManager">The FontManager to use for font resolution.</param>
    public static void UpdateFont(this TemplatedControl control, ITextStyle textStyle, IFontManager fontManager)
    {
        control.UpdateFont(textStyle.Font, fontManager);
    }
}
