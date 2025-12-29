using Avalonia.Controls.Maui.Maps.Controls;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using MapsuiMapControl = Mapsui.UI.Avalonia.MapControl;
using System.IO;

namespace Avalonia.Controls.Maui.Maps.Extensions;

/// <summary>
/// Extension methods for <see cref="Mapsui.UI.Avalonia.MapControl"/>.
/// </summary>
public static class MapExtensions
{
    /// <summary>
    /// The base resolution at zoom level 0 (meters per pixel).
    /// </summary>
    private const double BaseResolution = 156543.03;

    /// <summary>
    /// Updates the map center to the specified latitude and longitude.
    /// </summary>
    /// <param name="mapControl">The Mapsui map control.</param>
    /// <param name="latitude">Latitude in WGS84 (degrees).</param>
    /// <param name="longitude">Longitude in WGS84 (degrees).</param>
    public static void UpdateCenter(this MapsuiMapControl mapControl, double latitude, double longitude)
    {
        if (mapControl?.Map?.Navigator == null) return;
        
        var (x, y) = SphericalMercator.FromLonLat(longitude, latitude);
        var point = new Mapsui.MPoint(x, y);
        
        mapControl.Map.Navigator.CenterOn(point);
        mapControl.Refresh();
    }

    /// <summary>
    /// Updates the map zoom level.
    /// </summary>
    /// <param name="mapControl">The Mapsui map control.</param>
    /// <param name="zoomLevel">The zoom level (1 = world, 18 = street level).</param>
    public static void UpdateZoomLevel(this MapsuiMapControl mapControl, double zoomLevel)
    {
        if (mapControl?.Map?.Navigator == null) return;
        if (zoomLevel <= 0) return;
        
        double resolution = BaseResolution / Math.Pow(2, zoomLevel);
        mapControl.Map.Navigator.ZoomTo(resolution);
        mapControl.Refresh();
    }

    /// <summary>
    /// Centers and zooms the map to the specified location and zoom level.
    /// </summary>
    /// <param name="mapControl">The Mapsui map control.</param>
    /// <param name="latitude">Latitude in WGS84 (degrees).</param>
    /// <param name="longitude">Longitude in WGS84 (degrees).</param>
    /// <param name="zoomLevel">The zoom level (1 = world, 18 = street level).</param>
    public static void NavigateTo(this MapsuiMapControl mapControl, double latitude, double longitude, double zoomLevel)
    {
        if (mapControl?.Map?.Navigator == null) return;
        
        var (x, y) = SphericalMercator.FromLonLat(longitude, latitude);
        var point = new Mapsui.MPoint(x, y);
        
        double resolution = zoomLevel > 0 ? BaseResolution / Math.Pow(2, zoomLevel) : mapControl.Map.Navigator.Viewport.Resolution;
        
        mapControl.Map.Navigator.CenterOnAndZoomTo(point, resolution);
        mapControl.Refresh();
    }

    /// <summary>
    /// Updates whether scrolling (pan) is enabled on the map.
    /// </summary>
    /// <param name="mapControl">The Mapsui map control.</param>
    /// <param name="enabled">True to enable panning, false to disable.</param>
    public static void UpdateIsScrollEnabled(this MapsuiMapControl mapControl, bool enabled)
    {
        if (mapControl?.Map?.Navigator == null) return;
        mapControl.Map.Navigator.PanLock = !enabled;
    }

    /// <summary>
    /// Updates whether zooming is enabled on the map.
    /// </summary>
    /// <param name="mapControl">The Mapsui map control.</param>
    /// <param name="enabled">True to enable zooming, false to disable.</param>
    public static void UpdateIsZoomEnabled(this MapsuiMapControl mapControl, bool enabled)
    {
        if (mapControl?.Map?.Navigator == null) return;
        mapControl.Map.Navigator.ZoomLock = !enabled;
    }

    /// <summary>
    /// Updates whether rotation is enabled on the map.
    /// </summary>
    /// <param name="mapControl">The Mapsui map control.</param>
    /// <param name="enabled">True to enable rotation, false to disable.</param>
    public static void UpdateIsRotationEnabled(this MapsuiMapControl mapControl, bool enabled)
    {
        if (mapControl?.Map?.Navigator == null) return;
        mapControl.Map.Navigator.RotationLock = !enabled;
    }
    
    /// <summary>
    /// Updates the pins on the map from the specified collection.
    /// </summary>
    /// <param name="mapControl">The Mapsui map control.</param>
    /// <param name="pins">The collection of pins to display.</param>
    /// <param name="pinsLayer">The memory layer to store pin features.</param>
    public static void UpdatePins(this MapsuiMapControl mapControl, IList<MapPin> pins, Mapsui.Layers.MemoryLayer pinsLayer)
    {
        if (mapControl == null || pinsLayer == null) return;
        
        var features = new List<Mapsui.IFeature>();
        
        foreach (var pin in pins)
        {
            if (pin.Location == null) continue;
            
            var (x, y) = SphericalMercator.FromLonLat(pin.Location.Longitude, pin.Location.Latitude);
            var point = new Mapsui.MPoint(x, y);
            
            var feature = new Mapsui.Layers.PointFeature(point);
            
            // Handle Custom Icon
            if (pin.Icon != null)
            {
                // Synchronous loading attempt for FileImageSource
                if (pin.Icon is Microsoft.Maui.Controls.FileImageSource fileSource && !string.IsNullOrEmpty(fileSource.File))
                {
                    var filePath = fileSource.File;
                    
                    // Try to load the image file for Mapsui
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            // Use file:// URI scheme for local files
                            var fileUri = new Uri(filePath).AbsoluteUri;
                            
                            feature.Styles.Add(new ImageStyle
                            {
                                Image = new Mapsui.Styles.Image { Source = fileUri },
                                SymbolScale = (float)pin.IconScale
                            });
                        }
                        catch
                        {
                            // Fallback to default circle on error
                            feature.Styles.Add(CreateDefaultStyle(pin));
                        }
                    }
                    else
                    {
                        // File doesn't exist, use default
                        feature.Styles.Add(CreateDefaultStyle(pin));
                    }
                }
                else
                {
                    // Fallback to default circle
                    feature.Styles.Add(CreateDefaultStyle(pin));
                }
            }
            else
            {
                // Default Style
                feature.Styles.Add(CreateDefaultStyle(pin));
            }

            // Store original pin for hit-testing
            feature["MauiObject"] = pin;
            
            // Add label if present
            if (!string.IsNullOrEmpty(pin.Label))
            {
                feature.Styles.Add(new LabelStyle
                {
                    Text = pin.Label,
                    Offset = new Offset(0, -30),
                    Halo = new Pen(Color.White, 2),
                    ForeColor = Color.Black
                });
            }
            
            // Store pin reference for event handling
            feature["Pin"] = pin;
            
            features.Add(feature);
        }
        
        pinsLayer.Features = features;
        mapControl.Refresh();
    }
    
    /// <summary>
    /// Clears all pins from the map.
    /// </summary>
    /// <param name="mapControl">The Mapsui map control.</param>
    /// <param name="pinsLayer">The memory layer containing pin features.</param>
    public static void ClearPins(this MapsuiMapControl mapControl, Mapsui.Layers.MemoryLayer pinsLayer)
    {
        if (pinsLayer == null) return;
        pinsLayer.Features = new List<Mapsui.IFeature>();
        mapControl?.Refresh();
    }
    
    /// <summary>
    /// Updates the shapes on the map from the specified collection.
    /// </summary>
    /// <param name="mapControl">The Mapsui map control.</param>
    /// <param name="elements">The collection of map elements to display.</param>
    /// <param name="shapesLayer">The memory layer to store shape features.</param>
    public static void UpdateShapes(this MapsuiMapControl mapControl, IList<MapElement> elements, Mapsui.Layers.MemoryLayer shapesLayer)
    {
        if (mapControl == null || shapesLayer == null) return;
        
        var features = new List<Mapsui.IFeature>();
        
        foreach (var element in elements)
        {
            Mapsui.IFeature? feature = element switch
            {
                MapPolygon polygon => CreatePolygonFeature(polygon),
                MapPolyline polyline => CreatePolylineFeature(polyline),
                MapCircle circle => CreateCircleFeature(circle),
                _ => null
            };
            
            if (feature != null)
            {
                features.Add(feature);
            }
        }
        
        shapesLayer.Features = features;
        mapControl.Refresh();
    }
    
    private static Mapsui.IFeature? CreatePolygonFeature(MapPolygon polygon)
    {
        if (polygon.GeoPath.Count < 3) return null;
        
        var points = polygon.GeoPath.Select(loc =>
        {
            var (x, y) = SphericalMercator.FromLonLat(loc.Longitude, loc.Latitude);
            return new Mapsui.MPoint(x, y);
        }).ToList();
        
        // Close the polygon
        points.Add(points[0]);
        
        var linearRing = new NetTopologySuite.Geometries.LinearRing(
            points.Select(p => new NetTopologySuite.Geometries.Coordinate(p.X, p.Y)).ToArray());
        var geometry = new NetTopologySuite.Geometries.Polygon(linearRing);
        
        var feature = new GeometryFeature(geometry);
        feature.Styles.Add(new VectorStyle
        {
            Fill = new Brush(ToMapsuiColor(polygon.FillColor)),
            Outline = new Pen(ToMapsuiColor(polygon.StrokeColor), polygon.StrokeWidth)
        });

        // Store original element for hit-testing
        feature["MauiObject"] = polygon;
        
        return feature;
    }
    
    private static Mapsui.IFeature? CreatePolylineFeature(MapPolyline polyline)
    {
        if (polyline.GeoPath.Count < 2) return null;
        
        var points = polyline.GeoPath.Select(loc =>
        {
            var (x, y) = SphericalMercator.FromLonLat(loc.Longitude, loc.Latitude);
            return new NetTopologySuite.Geometries.Coordinate(x, y);
        }).ToArray();
        
        var geometry = new NetTopologySuite.Geometries.LineString(points);
        
        var feature = new GeometryFeature(geometry);
        feature.Styles.Add(new VectorStyle
        {
            Line = new Pen(ToMapsuiColor(polyline.StrokeColor), polyline.StrokeWidth)
        });

        // Store original element for hit-testing
        feature["MauiObject"] = polyline;
        
        return feature;
    }
    
    private static Mapsui.IFeature? CreateCircleFeature(MapCircle circle)
    {
        if (circle.Center == null || circle.Radius <= 0) return null;
        
        var (centerX, centerY) = SphericalMercator.FromLonLat(circle.Center.Longitude, circle.Center.Latitude);
        
        // Create a circular polygon approximation (32 points)
        var points = new List<NetTopologySuite.Geometries.Coordinate>();
        const int segments = 32;
        for (int i = 0; i <= segments; i++)
        {
            double angle = (2 * Math.PI * i) / segments;
            double x = centerX + (circle.Radius * Math.Cos(angle));
            double y = centerY + (circle.Radius * Math.Sin(angle));
            points.Add(new NetTopologySuite.Geometries.Coordinate(x, y));
        }
        
        var linearRing = new NetTopologySuite.Geometries.LinearRing(points.ToArray());
        var geometry = new NetTopologySuite.Geometries.Polygon(linearRing);
        
        var feature = new GeometryFeature(geometry);
        feature.Styles.Add(new VectorStyle
        {
            Fill = new Brush(ToMapsuiColor(circle.FillColor)),
            Outline = new Pen(ToMapsuiColor(circle.StrokeColor), circle.StrokeWidth)
        });

        // Store original element for hit-testing
        feature["MauiObject"] = circle;
        
        return feature;
    }
    
    private static Color ToMapsuiColor(Microsoft.Maui.Graphics.Color? mauiColor)
    {
        if (mauiColor == null) return Color.Transparent;
        return new Color(
            (int)(mauiColor.Red * 255),
            (int)(mauiColor.Green * 255),
            (int)(mauiColor.Blue * 255),
            (int)(mauiColor.Alpha * 255));
    }
    private static SymbolStyle CreateDefaultStyle(MapPin pin)
    {
        // Style based on pin type
        var color = pin.Type switch
        {
            PinType.Place => Color.Blue,
            PinType.SavedPin => Color.Green,
            PinType.SearchResult => Color.Orange,
            _ => Color.Red // Generic
        };

        return new SymbolStyle
        {
            Fill = new Brush(color),
            SymbolScale = 0.5,
            Outline = new Pen(Color.Black, 1)
        };
    }
}

