namespace Avalonia.Controls.Maui.Maps.Controls;

/// <summary>
/// Event arguments for the <see cref="MapView.MapClicked"/> event.
/// </summary>
public class MapClickedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapClickedEventArgs"/> class.
    /// </summary>
    /// <param name="location">The location that was clicked.</param>
    public MapClickedEventArgs(Microsoft.Maui.Devices.Sensors.Location location)
    {
        Location = location;
    }

    /// <summary>
    /// Gets the location (latitude/longitude) that was clicked.
    /// </summary>
    public Microsoft.Maui.Devices.Sensors.Location Location { get; }

    /// <summary>
    /// Gets the pin that was clicked, if any.
    /// </summary>
    public MapPin? Pin { get; internal set; }

    /// <summary>
    /// Gets the map element (polygon, polyline, circle) that was clicked, if any.
    /// </summary>
    public MapElement? Element { get; internal set; }
}
