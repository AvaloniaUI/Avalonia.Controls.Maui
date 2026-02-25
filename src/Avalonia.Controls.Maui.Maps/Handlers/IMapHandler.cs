namespace Avalonia.Controls.Maui.Maps.Handlers;

/// <summary>
/// Interface for Map handler implementations.
/// Implement this interface in provider-specific packages to provide map functionality.
/// </summary>
/// <remarks>
/// Provider implementations should:
/// <list type="bullet">
///   <item>Create and manage the native map control</item>
///   <item>Handle property mappings for <see cref="IMapView"/> properties</item>
///   <item>Handle command mappings for navigation (CenterTo, ZoomTo) and updates (UpdatePins, UpdateMapElements)</item>
/// </list>
/// </remarks>
public interface IMapHandler
{
    /// <summary>
    /// Gets the virtual view (MAUI control) for this handler.
    /// </summary>
    IMapView? VirtualView { get; }
    
    /// <summary>
    /// Updates the pins displayed on the map.
    /// </summary>
    void UpdatePins();
    
    /// <summary>
    /// Updates the map elements (polygons, polylines, circles) displayed on the map.
    /// </summary>
    void UpdateMapElements();
    
    /// <summary>
    /// Centers the map on the specified coordinates.
    /// </summary>
    /// <param name="latitude">The latitude in WGS84 degrees.</param>
    /// <param name="longitude">The longitude in WGS84 degrees.</param>
    void CenterTo(double latitude, double longitude);
    
    /// <summary>
    /// Sets the zoom level of the map.
    /// </summary>
    /// <param name="zoomLevel">The zoom level (1 = world, 18 = street level).</param>
    void ZoomTo(double zoomLevel);
}
