using Microsoft.Maui;
using Avalonia.Controls.Maui.Maps.Controls;

namespace Avalonia.Controls.Maui.Maps.Handlers;

/// <summary>
/// Interface for a Map view with MAUI bindings.
/// </summary>
public interface IMapView : IView
{
    /// <summary>Gets or sets the display style of the map.</summary>
    MapType MapType { get; }

    /// <summary>Gets or sets a value indicating whether the user's current location is visible on the map.</summary>
    bool IsShowingUser { get; }

    /// <summary>Gets or sets whether scroll (pan) gestures are enabled.</summary>
    bool IsScrollEnabled { get; }
    
    /// <summary>Gets or sets whether zoom gestures are enabled.</summary>
    bool IsZoomEnabled { get; }
    
    /// <summary>Gets or sets whether rotation gestures are enabled.</summary>
    bool IsRotationEnabled { get; }
    
    /// <summary>Gets or sets whether the OpenStreetMap attribution is shown.</summary>
    bool ShowAttribution { get; }
    
    /// <summary>Gets or sets the center latitude of the map.</summary>
    double CenterLatitude { get; }
    
    /// <summary>Gets or sets the center longitude of the map.</summary>
    double CenterLongitude { get; }
    
    /// <summary>Gets or sets the zoom level (1 = world, 18 = street level).</summary>
    double ZoomLevel { get; }
    
    /// <summary>Gets the collection of pins on the map.</summary>
    IList<MapPin> Pins { get; }
    
    /// <summary>Gets the collection of map elements (polygons, polylines, circles).</summary>
    IList<MapElement> MapElements { get; }
    
    /// <summary>Occurs when the map is clicked.</summary>
    event EventHandler<MapClickedEventArgs>? MapClicked;
    
    /// <summary>Raises the MapClicked event.</summary>
    void OnMapClicked(MapClickedEventArgs args);
}
