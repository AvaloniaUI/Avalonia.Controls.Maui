using Microsoft.Maui;
using UIKit;

namespace Avalonia.Controls.Maui;

public partial class FontManager
{
    private Microsoft.Maui.FontManager? _mauiFontManager => _serviceProvider?.GetService(typeof(Microsoft.Maui.FontManager)) as Microsoft.Maui.FontManager;

    /// <summary>
    /// Gets the default UIFont for the operating system.
    /// This is a stub implementation, in case native font handling is needed or used by other components.
    /// </summary>
    public UIFont DefaultFont => _mauiFontManager?.DefaultFont ?? UIFont.SystemFontOfSize(UIFont.SystemFontSize)!;

    /// <summary>
    /// Retrieves the platform equivalent UIFont for an abstract Font object.
    /// This is a stub implementation, in case native font handling is needed or used by other components.
    /// </summary>
    /// <param name="font">The abstract font representation.</param>
    /// <param name="defaultFontSize">The default font size to use if not specified.</param>
    /// <returns>A UIFont representing the requested font.</returns>
    public UIFont GetFont(Font font, double defaultFontSize = 0)
    {
        if (_mauiFontManager != null)
        {
            return _mauiFontManager.GetFont(font, defaultFontSize)!;
        }
        
        var size = GetFontSize(font, defaultFontSize);

        // Return a basic system font - Avalonia handles the actual font rendering for its controls.
        if (font.Weight != FontWeight.Regular)
        {
            return UIFont.SystemFontOfSize((nfloat)size, font.Weight switch
            {
                FontWeight.Bold => UIFontWeight.Bold,
                FontWeight.Medium => UIFontWeight.Medium,
                FontWeight.Semibold => UIFontWeight.Semibold,
                FontWeight.Light => UIFontWeight.Light,
                FontWeight.Thin => UIFontWeight.Thin,
                FontWeight.Ultralight => UIFontWeight.UltraLight,
                FontWeight.Heavy => UIFontWeight.Heavy,
                FontWeight.Black => UIFontWeight.Black,
                _ => UIFontWeight.Regular
            })!;
        }

        return font.Slant == FontSlant.Italic
            ? UIFont.ItalicSystemFontOfSize((nfloat)size)!
            : UIFont.SystemFontOfSize((nfloat)size)!;
    }
}
