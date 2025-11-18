using Avalonia.Controls;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Extension methods for Avalonia TextBox to support MAUI font handling.
/// </summary>
public static class TextBoxExtensions
{
    /// <summary>
    /// Updates the font properties of a TextBox using the MAUI Font and FontManager.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox to update.</param>
    /// <param name="font">The MAUI Font to apply.</param>
    /// <param name="fontManager">The FontManager to use for font resolution.</param>
    public static void UpdateFont(this TextBox textBox, Font font, IFontManager fontManager)
    {
        if (textBox == null)
            throw new ArgumentNullException(nameof(textBox));

        if (fontManager == null)
            throw new ArgumentNullException(nameof(fontManager));

        if (font.IsDefault)
            return;

        // Set font size using FontManager's size resolution logic
        textBox.FontSize = fontManager.GetFontSize(font);

        // Set font family using FontManager (handles custom fonts via IFontRegistrar)
        textBox.FontFamily = fontManager.GetFontFamily(font);

        // Set font style (italic/oblique)
        textBox.FontStyle = FontManager.ToAvaloniaFontStyle(font.Slant);

        // Set font weight (thin/light/normal/bold/etc)
        textBox.FontWeight = FontManager.ToAvaloniaFontWeight(font.Weight);
    }

    /// <summary>
    /// Updates the font properties of a TextBox using an IEntry element.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox to update.</param>
    /// <param name="entry">The IEntry element containing the font.</param>
    /// <param name="fontManager">The FontManager to use for font resolution.</param>
    public static void UpdateFont(this TextBox textBox, IEntry entry, IFontManager fontManager)
    {
        textBox.UpdateFont(entry.Font, fontManager);
    }

    /// <summary>
    /// Updates the font properties of a TextBox using an IEditor element.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox to update.</param>
    /// <param name="editor">The IEditor element containing the font.</param>
    /// <param name="fontManager">The FontManager to use for font resolution.</param>
    public static void UpdateFont(this TextBox textBox, IEditor editor, IFontManager fontManager)
    {
        textBox.UpdateFont(editor.Font, fontManager);
    }
}
