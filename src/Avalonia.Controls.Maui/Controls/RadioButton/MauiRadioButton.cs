using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Custom Avalonia RadioButton for MAUI that only shows content without the default indicator.
/// This is necessary because MAUI's RadioButton ControlTemplate typically includes its own
/// visual indicator, and we don't want to show the Avalonia default indicator as well.
/// </summary>
public class MauiRadioButton : RadioButton
{
    static MauiRadioButton()
    {
        // Ensure styling is applied
        AffectsRender<MauiRadioButton>(IsCheckedProperty);
    }

    public MauiRadioButton()
    {
    }
}
