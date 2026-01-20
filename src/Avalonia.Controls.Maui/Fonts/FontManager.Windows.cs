using Microsoft.Maui;
using Microsoft.UI.Xaml.Media;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Windows-specific stub implementation for FontManager.
/// Avalonia handles all font rendering directly via GetFontFamily and related methods.
/// </summary>
public partial class FontManager
{
    private Microsoft.Maui.FontManager? _mauiFontManager => _serviceProvider?.GetService(typeof(Microsoft.Maui.FontManager)) as Microsoft.Maui.FontManager;

    FontFamily IFontManager.DefaultFontFamily => _mauiFontManager?.DefaultFontFamily ?? new FontFamily("Segoe UI");

    FontFamily IFontManager.GetFontFamily(Font font)
    {
        if (_mauiFontManager != null)
        {
            return _mauiFontManager.GetFontFamily(font);
        }
        
        // Return a basic system font family - Avalonia handles the actual font rendering for its controls.
        return new FontFamily("Segoe UI");
    }
}
