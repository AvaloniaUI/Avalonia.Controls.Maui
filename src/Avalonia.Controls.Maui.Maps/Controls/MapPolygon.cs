using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Maps.Controls;

/// <summary>
/// Represents a polygon shape on a map.
/// </summary>
public class MapPolygon : MapElement
{
    /// <summary>Bindable property for <see cref="FillColor"/>.</summary>
    public static readonly BindableProperty FillColorProperty =
        BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(MapPolygon), Colors.Transparent);

    private readonly ObservableCollection<Microsoft.Maui.Devices.Sensors.Location> _geoPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapPolygon"/> class.
    /// </summary>
    public MapPolygon()
    {
        _geoPath = new ObservableCollection<Microsoft.Maui.Devices.Sensors.Location>();
    }

    /// <summary>
    /// Gets or sets the fill color of the polygon.
    /// </summary>
    public Color FillColor
    {
        get => (Color)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    /// <summary>
    /// Gets the collection of locations that define the polygon.
    /// </summary>
    public IList<Microsoft.Maui.Devices.Sensors.Location> GeoPath => _geoPath;
}
