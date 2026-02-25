using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Maps.Controls;

/// <summary>
/// Represents a polyline shape on a map.
/// </summary>
public class MapPolyline : MapElement
{
    private readonly ObservableCollection<Microsoft.Maui.Devices.Sensors.Location> _geoPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapPolyline"/> class.
    /// </summary>
    public MapPolyline()
    {
        _geoPath = new ObservableCollection<Microsoft.Maui.Devices.Sensors.Location>();
    }

    /// <summary>
    /// Gets the collection of locations that define the polyline.
    /// </summary>
    public IList<Microsoft.Maui.Devices.Sensors.Location> GeoPath => _geoPath;
}
