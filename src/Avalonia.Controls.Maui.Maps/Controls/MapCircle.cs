using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Maps.Controls;

/// <summary>
/// Represents a circle shape on a map.
/// </summary>
public class MapCircle : MapElement
{
    /// <summary>Bindable property for <see cref="Center"/>.</summary>
    public static readonly BindableProperty CenterProperty =
        BindableProperty.Create(nameof(Center), typeof(Microsoft.Maui.Devices.Sensors.Location), typeof(MapCircle), null);

    /// <summary>Bindable property for <see cref="Radius"/>.</summary>
    public static readonly BindableProperty RadiusProperty =
        BindableProperty.Create(nameof(Radius), typeof(double), typeof(MapCircle), 0.0);

    /// <summary>Bindable property for <see cref="FillColor"/>.</summary>
    public static readonly BindableProperty FillColorProperty =
        BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(MapCircle), Colors.Transparent);

    /// <summary>
    /// Gets or sets the center location of the circle.
    /// </summary>
    public Microsoft.Maui.Devices.Sensors.Location? Center
    {
        get => (Microsoft.Maui.Devices.Sensors.Location?)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }

    /// <summary>
    /// Gets or sets the radius of the circle in meters.
    /// </summary>
    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the fill color of the circle.
    /// </summary>
    public Color FillColor
    {
        get => (Color)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }
}
