using Avalonia;
using Avalonia.Controls;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Custom RadioButton control that extends Avalonia's RadioButton with MAUI-specific features.
/// This control adds support for hiding the default indicator when a custom ControlTemplate is applied,
/// and provides proper BorderBrush/BorderThickness/CornerRadius support on an outer border.
/// </summary>
public class MauiRadioButton : RadioButton
{
    /// <summary>
    /// Defines the <see cref="ShowIndicator"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowIndicatorProperty =
        AvaloniaProperty.Register<MauiRadioButton, bool>(nameof(ShowIndicator), defaultValue: true);

    /// <summary>
    /// Gets or sets whether the default radio button indicator (circle) should be visible.
    /// Set to false when a custom MAUI ControlTemplate is applied.
    /// </summary>
    public bool ShowIndicator
    {
        get => GetValue(ShowIndicatorProperty);
        set => SetValue(ShowIndicatorProperty, value);
    }

    static MauiRadioButton()
    {
        AffectsRender<MauiRadioButton>(IsCheckedProperty);
    }
}
