using Mapsui;
using Microsoft.Maui;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Maps.Controls;
using Avalonia.Controls.Maui.Maps.Extensions;
using Mapsui.Tiling;
using Mapsui.Projections;
using Mapsui.Nts;
using Microsoft.Maui.Devices.Sensors;
using System.Runtime.CompilerServices;
using BruTile.Predefined;
using MapsuiMapControl = Mapsui.UI.Avalonia.MapControl;

namespace Avalonia.Controls.Maui.Maps.Handlers;

public partial class MapHandler : ViewHandler<IMapView, MapsuiMapControl>
{
    private Mapsui.Layers.MemoryLayer? _pinsLayer;
    private Mapsui.Layers.MemoryLayer? _shapesLayer;
    
    private bool _isInitialSyncPerformed;

    public static IPropertyMapper<IMapView, MapHandler> Mapper = 
        new PropertyMapper<IMapView, MapHandler>(ViewMapper)
        {
            [nameof(IMapView.IsScrollEnabled)] = MapIsScrollEnabled,
            [nameof(IMapView.IsZoomEnabled)] = MapIsZoomEnabled,
            [nameof(IMapView.IsRotationEnabled)] = MapIsRotationEnabled,
            [nameof(IMapView.CenterLatitude)] = MapCenter,
            [nameof(IMapView.CenterLongitude)] = MapCenter,
            [nameof(IMapView.ZoomLevel)] = MapZoomLevel,
            [nameof(IMapView.MapType)] = MapMapType,
            [nameof(IMapView.IsShowingUser)] = MapIsShowingUser,
        };
    
    public static CommandMapper<IMapView, MapHandler> CommandMapper = 
        new(ViewCommandMapper)
        {
            ["CenterTo"] = MapCenterToCommand,
            ["ZoomTo"] = MapZoomToCommand,
            ["UpdatePins"] = MapUpdatePinsCommand,
            ["UpdateMapElements"] = MapUpdateMapElementsCommand,
        };
    
    public MapHandler() : base(Mapper, CommandMapper)
    {
    }

    protected override MapsuiMapControl CreatePlatformView()
    {
        var mapControl = new MapsuiMapControl();
        
        // Disable Mapsui's debug logging and performance widgets
        Mapsui.Logging.Logger.LogDelegate = null;
        mapControl.Map.Widgets.Clear();
        
        // Initialize with OpenStreetMap
        var initialLayer = OpenStreetMap.CreateTileLayer("AvaloniaControlGallery");
        initialLayer.Name = "Basemap";
        mapControl.Map.Layers.Add(initialLayer);
        
        // Pins layer
        _pinsLayer = new Mapsui.Layers.MemoryLayer
        {
            Name = "Pins",
            Style = null
        };
        mapControl.Map.Layers.Add(_pinsLayer);
        
        // Shapes layer (for polygons, polylines, circles)
        _shapesLayer = new Mapsui.Layers.MemoryLayer
        {
            Name = "Shapes",
            Style = null
        };
        mapControl.Map.Layers.Insert(1, _shapesLayer);
        
        return mapControl;
    }
    
    protected override void ConnectHandler(MapsuiMapControl platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Refresh();
        
        platformView.Map.Info += OnMapInfo;
        platformView.LayoutUpdated += OnLayoutUpdated;
        
        // Sync pins
        if (VirtualView?.Pins != null)
        {
            UpdatePins();
        }
    }
    
    protected override void DisconnectHandler(MapsuiMapControl platformView)
    {
        platformView.Map.Info -= OnMapInfo;
        platformView.LayoutUpdated -= OnLayoutUpdated;

        base.DisconnectHandler(platformView);
    }
    
    public static void MapIsScrollEnabled(MapHandler handler, IMapView map)
    {
        handler.PlatformView?.UpdateIsScrollEnabled(map.IsScrollEnabled);
    }

    public static void MapIsZoomEnabled(MapHandler handler, IMapView map)
    {
        handler.PlatformView?.UpdateIsZoomEnabled(map.IsZoomEnabled);
    }
    
    public static void MapIsRotationEnabled(MapHandler handler, IMapView map)
    {
        handler.PlatformView?.UpdateIsRotationEnabled(map.IsRotationEnabled);
    }
    
    public static void MapCenter(MapHandler handler, IMapView map)
    {
        handler.PlatformView?.UpdateCenter(map.CenterLatitude, map.CenterLongitude);
    }

    public static void MapZoomLevel(MapHandler handler, IMapView map)
    {
        if (map.ZoomLevel > 0)
        {
            handler.PlatformView?.UpdateZoomLevel(map.ZoomLevel);
        }
    }

    public static void MapMapType(MapHandler handler, IMapView map)
    {
        if (handler.PlatformView?.Map == null) return;
        
        var mapControl = handler.PlatformView;
        var mapType = map.MapType;
        
        var layers = mapControl.Map.Layers.Where(l => l.Name == "Basemap").ToList();
        foreach (var l in layers)
        {
            mapControl.Map.Layers.Remove(l);
        }
        
        Mapsui.Layers.ILayer layer;
        
        switch (mapType)
        {
            case MapType.Street:
            default:
                layer = OpenStreetMap.CreateTileLayer("AvaloniaControlGallery");
                layer.Name = "Basemap";
                mapControl.Map.Layers.Insert(0, layer);
                break;
                
            case MapType.Satellite:
                layer = new Mapsui.Tiling.Layers.TileLayer(KnownTileSources.Create(KnownTileSource.EsriWorldPhysical)) { Name = "Basemap" };
                mapControl.Map.Layers.Insert(0, layer);
                break;
                
            case MapType.Hybrid:
                layer = new Mapsui.Tiling.Layers.TileLayer(KnownTileSources.Create(KnownTileSource.EsriWorldShadedRelief)) { Name = "Basemap" };
                mapControl.Map.Layers.Insert(0, layer);
                break;
        }
        
        mapControl.RefreshGraphics();
        mapControl.Refresh();
    }

    public static async void MapIsShowingUser(MapHandler handler, IMapView map)
    {
        if (handler.PlatformView?.Map == null) return;
        var mapControl = handler.PlatformView;
        
        // Find existing location layer
        var locationLayer = mapControl.Map.Layers.OfType<Mapsui.Layers.MemoryLayer>().FirstOrDefault(l => l.Name == "MyLocation");
        
        if (map.IsShowingUser)
        {
            // Add layer if missing
            if (locationLayer == null)
            {
                locationLayer = new Mapsui.Layers.MemoryLayer { Name = "MyLocation" };
                mapControl.Map.Layers.Add(locationLayer);
            }
 
            try 
            {
                Microsoft.Maui.Devices.Sensors.Location? location = null;

                // Use IP-based geolocation on Desktop
                if (OperatingSystem.IsMacOS() || OperatingSystem.IsWindows())
                {
                    location = await GetLocationFromIpAsync();
                }
                else
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    location = await Geolocation.Default.GetLocationAsync(request);
                }
                
                if (location != null)
                {
                     UpdateUserLocation(handler, mapControl, locationLayer, location);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting location: {ex.Message}");
            }
        }
        else
        {
            if (locationLayer != null)
            {
                mapControl.Map.Layers.Remove(locationLayer);
            }
        }
        
        mapControl.Refresh();
    }
    
    public static void MapCenterToCommand(MapHandler handler, IMapView map, object? args)
    {
        if (args is ValueTuple<double, double> tuple)
        {
            handler.PlatformView?.UpdateCenter(tuple.Item1, tuple.Item2);
        }
        else if (args is ITuple t && t.Length >= 2 && t[0] is double lat && t[1] is double lon)
        {
            handler.PlatformView?.UpdateCenter(lat, lon);
        }
    }

    public static void MapZoomToCommand(MapHandler handler, IMapView map, object? args)
    {
        if (args is not double zoomLevel || zoomLevel <= 0) return;
        handler.PlatformView?.UpdateZoomLevel(zoomLevel);
    }

    public static void MapUpdatePinsCommand(MapHandler handler, IMapView map, object? args)
    {
        handler.UpdatePins();
    }
    
    public static void MapUpdateMapElementsCommand(MapHandler handler, IMapView map, object? args)
    {
        handler.UpdateShapes();
    }
    
    private void UpdateShapes()
    {
        if (_shapesLayer == null || VirtualView?.MapElements == null) return;
        
        PlatformView?.UpdateShapes(VirtualView.MapElements, _shapesLayer);
    }
    
    private static async Task<Microsoft.Maui.Devices.Sensors.Location?> GetLocationFromIpAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            
            var response = await httpClient.GetStringAsync("http://ip-api.com/json/?fields=lat,lon,status");
            
            if (response.Contains("\"status\":\"success\""))
            {
                var latMatch = System.Text.RegularExpressions.Regex.Match(response, "\"lat\":([\\d.-]+)");
                var lonMatch = System.Text.RegularExpressions.Regex.Match(response, "\"lon\":([\\d.-]+)");
                
                if (latMatch.Success && lonMatch.Success)
                {
                    var lat = double.Parse(latMatch.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                    var lon = double.Parse(lonMatch.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                    
                    System.Diagnostics.Debug.WriteLine($"IP Geolocation: {lat}, {lon}");
                    return new Microsoft.Maui.Devices.Sensors.Location(lat, lon);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"IP Geolocation failed: {ex.Message}");
        }
        
        // Fallback to Tallinn, Estonia (Home of Avalonia UI) if IP geolocation fails
        return new Microsoft.Maui.Devices.Sensors.Location(59.4370, 24.7536);
    }
    
    private static void UpdateUserLocation(MapHandler handler, MapsuiMapControl mapControl, Mapsui.Layers.MemoryLayer layer, Microsoft.Maui.Devices.Sensors.Location location)
    {
        var (x, y) = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
        var point = new MPoint(x, y);
        
        // Create User Location Style (Blue Puck with Halo)
        var feature = new GeometryFeature
        {
            Geometry = new NetTopologySuite.Geometries.Point(x, y),
            Styles = new List<Mapsui.Styles.IStyle>
            {
                new Mapsui.Styles.SymbolStyle
                {
                    Fill = new Mapsui.Styles.Brush(new Mapsui.Styles.Color(33, 150, 243, 100)), // Light Blue transparent
                    Outline = null,
                    SymbolScale = 1.5
                },
                new Mapsui.Styles.SymbolStyle
                {
                    Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Blue),
                    Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.White, 3),
                    SymbolScale = 0.8
                }
            }
        };
        
        layer.Features = new List<IFeature> { feature };
        
        // Move camera to user location
        handler.PlatformView.UpdateCenter(location.Latitude, location.Longitude);

        if (handler.PlatformView.Map.Navigator.Viewport.Resolution > 20)
        {
            handler.PlatformView.UpdateZoomLevel(14); 
        }
        
        mapControl.Refresh();
    }

    
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (PlatformView == null) return;
        
        var mapControl = PlatformView;
        if (mapControl.Bounds.Width > 0 && mapControl.Bounds.Height > 0)
        {
            if (!_isInitialSyncPerformed && VirtualView != null)
            {
                _isInitialSyncPerformed = true;
                mapControl.UpdateCenter(VirtualView.CenterLatitude, VirtualView.CenterLongitude);
                mapControl.UpdateZoomLevel(VirtualView.ZoomLevel);
            }
            mapControl.Refresh();
        }
    }

    private void OnMapInfo(object? sender, MapInfoEventArgs e)
    {
        if (e.WorldPosition != null && VirtualView != null)
        {
            var worldPosition = e.WorldPosition;
            var (lon, lat) = SphericalMercator.ToLonLat(
                worldPosition.X, 
                worldPosition.Y);

            var location = new Microsoft.Maui.Devices.Sensors.Location(lat, lon);
            var args = new MapClickedEventArgs(location);
            var layers = new List<Mapsui.Layers.ILayer>();
            
            if (_pinsLayer != null) layers.Add(_pinsLayer);
            if (_shapesLayer != null) layers.Add(_shapesLayer);

            var mapInfo = e.GetMapInfo(layers);

            if (mapInfo?.Feature != null)
            {
                var feature = mapInfo.Feature;
                if (feature["MauiObject"] is MapPin pin)
                {
                    args.Pin = pin;
                }
                else if (feature["MauiObject"] is MapElement element)
                {
                    args.Element = element;
                }
            }

            VirtualView.OnMapClicked(args);
        }
    }

    private void UpdatePins()
    {
        if (_pinsLayer == null || VirtualView?.Pins == null) return;
        
        PlatformView?.UpdatePins(VirtualView.Pins, _pinsLayer);
    }
}