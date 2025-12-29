using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Maps.Controls;

/// <summary>
/// Base class for map elements (shapes).
/// </summary>
public abstract class MapElement : BindableObject
{
    /// <summary>Bindable property for <see cref="StrokeColor"/>.</summary>
    public static readonly BindableProperty StrokeColorProperty =
        BindableProperty.Create(nameof(StrokeColor), typeof(Color), typeof(MapElement), Colors.Black);

    /// <summary>Bindable property for <see cref="StrokeWidth"/>.</summary>
    public static readonly BindableProperty StrokeWidthProperty =
        BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(MapElement), 3f);

    /// <summary>
    /// Gets or sets the stroke color for the element.
    /// </summary>
    public Color StrokeColor
    {
        get => (Color)GetValue(StrokeColorProperty);
        set => SetValue(StrokeColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke width for the element.
    /// </summary>
    public float StrokeWidth
    {
        get => (float)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }
}
