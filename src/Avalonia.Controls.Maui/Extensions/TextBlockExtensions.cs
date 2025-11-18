using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Extension methods for Avalonia TextBlock to support MAUI font handling.
/// </summary>
public static class TextBlockExtensions
{
    /// <summary>
    /// Updates the font properties of a TextBlock using the MAUI Font and FontManager.
    /// </summary>
    /// <param name="textBlock">The Avalonia TextBlock to update.</param>
    /// <param name="font">The MAUI Font to apply.</param>
    /// <param name="fontManager">The FontManager to use for font resolution.</param>
    public static void UpdateFont(this TextBlock textBlock, Font font, IFontManager fontManager)
    {
        if (textBlock == null)
            throw new ArgumentNullException(nameof(textBlock));

        if (fontManager == null)
            throw new ArgumentNullException(nameof(fontManager));

        if (font.IsDefault)
            return;

        // Cast to our FontManager implementation to access Avalonia-specific methods
        // Set font size using FontManager's size resolution logic
        textBlock.FontSize = fontManager.GetFontSize(font);

        // Set font family using FontManager (handles custom fonts via IFontRegistrar)
        textBlock.FontFamily = fontManager.GetFontFamily(font);

        // Set font style (italic/oblique)
        textBlock.FontStyle = FontManager.ToAvaloniaFontStyle(font.Slant);

        // Set font weight (thin/light/normal/bold/etc)
        textBlock.FontWeight = FontManager.ToAvaloniaFontWeight(font.Weight);
    }

    /// <summary>
    /// Updates the font properties of a TextBlock using an IText element.
    /// </summary>
    /// <param name="textBlock">The Avalonia TextBlock to update.</param>
    /// <param name="text">The IText element containing the font.</param>
    /// <param name="fontManager">The FontManager to use for font resolution.</param>
    public static void UpdateFont(this TextBlock textBlock, IText text, IFontManager fontManager)
    {
        textBlock.UpdateFont(text.Font, fontManager);
    }

    /// <summary>
    /// Updates the font properties of a TextBlock using an ITextStyle element.
    /// </summary>
    /// <param name="textBlock">The Avalonia TextBlock to update.</param>
    /// <param name="textStyle">The ITextStyle element containing the font.</param>
    /// <param name="fontManager">The FontManager to use for font resolution.</param>
    public static void UpdateFont(this TextBlock textBlock, ITextStyle textStyle, IFontManager fontManager)
    {
        textBlock.UpdateFont(textStyle.Font, fontManager);
    }

    /// <summary>
    /// Updates the font properties of a TextBlock using an ITextStyle element with a default font size.
    /// </summary>
    /// <param name="textBlock">The Avalonia TextBlock to update.</param>
    /// <param name="textStyle">The ITextStyle element containing the font.</param>
    /// <param name="fontManager">The FontManager to use for font resolution.</param>
    /// <param name="defaultFontSize">The default font size to use if the font doesn't specify one.</param>
    public static void UpdateFont(this TextBlock textBlock, ITextStyle textStyle, IFontManager fontManager, double defaultFontSize)
    {
        if (textBlock == null)
            throw new ArgumentNullException(nameof(textBlock));

        if (fontManager == null)
            throw new ArgumentNullException(nameof(fontManager));

        var font = textStyle.Font;

        if (font.IsDefault)
            return;

        // Set font size with custom default
        textBlock.FontSize = fontManager.GetFontSize(font, defaultFontSize);

        // Set font family
        textBlock.FontFamily = fontManager.GetFontFamily(font);

        // Set font style and weight
        textBlock.FontStyle = FontManager.ToAvaloniaFontStyle(font.Slant);
        textBlock.FontWeight = FontManager.ToAvaloniaFontWeight(font.Weight);
    }
}
