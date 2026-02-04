using global::Mapsui;
using Microsoft.Maui;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Maps.Controls;
using Avalonia.Controls.Maui.Maps.Handlers;
using Avalonia.Controls.Maui.Maps.Mapsui.Extensions;
using global::Mapsui.Tiling;
using global::Mapsui.Projections;
using global::Mapsui.Nts;
using Microsoft.Maui.Devices.Sensors;
using System.Runtime.CompilerServices;
using BruTile.Predefined;
using MapsuiMapControl = global::Mapsui.UI.Avalonia.MapControl;
using MapsuiLayers = global::Mapsui.Layers;
using MapsuiStyles = global::Mapsui.Styles;

namespace Avalonia.Controls.Maui.Maps.Mapsui.Handlers;

/// <summary>
/// Mapsui-based implementation of the Map handler for Avalonia.
/// More information: https://mapsui.com
/// </summary>
public partial class MapsuiMapHandler : ViewHandler<IMapView, MapsuiMapControl>, IMapHandler
{
    private MapsuiLayers.MemoryLayer? _pinsLayer;
    private MapsuiLayers.MemoryLayer? _shapesLayer;
    private CancellationTokenSource? _locationCts;
    
    private bool _isInitialSyncPerformed;

    /// <summary>
    /// Property mapper for the Map handler.
    /// </summary>
    public static IPropertyMapper<IMapView, MapsuiMapHandler> Mapper = 
        new PropertyMapper<IMapView, MapsuiMapHandler>(ViewMapper)
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
    
    /// <summary>
    /// Command mapper for the Map handler.
    /// </summary>
    public static CommandMapper<IMapView, MapsuiMapHandler> CommandMapper = 
        new(ViewCommandMapper)
        {
            ["CenterTo"] = MapCenterToCommand,
            ["ZoomTo"] = MapZoomToCommand,
            ["UpdatePins"] = MapUpdatePinsCommand,
            ["UpdateMapElements"] = MapUpdateMapElementsCommand,
        };
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MapsuiMapHandler"/> class.
    /// </summary>
    public MapsuiMapHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <inheritdoc/>
    protected override MapsuiMapControl CreatePlatformView()
    {
        var mapControl = new MapsuiMapControl();
        
        // Disable Mapsui's debug logging and performance widgets
        global::Mapsui.Logging.Logger.LogDelegate = null;
        mapControl.Map.Widgets.Clear();
        
        // Initialize with OpenStreetMap
        var initialLayer = OpenStreetMap.CreateTileLayer("AvaloniaControlGallery");
        initialLayer.Name = "Basemap";
        mapControl.Map.Layers.Add(initialLayer);
        
        // Pins layer
        _pinsLayer = new MapsuiLayers.MemoryLayer
        {
            Name = "Pins",
            Style = null
        };
        mapControl.Map.Layers.Add(_pinsLayer);
        
        // Shapes layer (for polygons, polylines, circles)
        _shapesLayer = new MapsuiLayers.MemoryLayer
        {
            Name = "Shapes",
            Style = null
        };
        mapControl.Map.Layers.Insert(1, _shapesLayer);
        
        return mapControl;
    }
    
    /// <inheritdoc/>
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
    
    /// <inheritdoc/>
    protected override void DisconnectHandler(MapsuiMapControl platformView)
    {
        platformView.Map.Info -= OnMapInfo;
        platformView.LayoutUpdated -= OnLayoutUpdated;

        CancelLocationTask();

        base.DisconnectHandler(platformView);
    }
    
    private static void MapIsScrollEnabled(MapsuiMapHandler handler, IMapView map)
    {
        handler.PlatformView?.UpdateIsScrollEnabled(map.IsScrollEnabled);
    }

    private static void MapIsZoomEnabled(MapsuiMapHandler handler, IMapView map)
    {
        handler.PlatformView?.UpdateIsZoomEnabled(map.IsZoomEnabled);
    }
    
    private static void MapIsRotationEnabled(MapsuiMapHandler handler, IMapView map)
    {
        handler.PlatformView?.UpdateIsRotationEnabled(map.IsRotationEnabled);
    }
    
    private static void MapCenter(MapsuiMapHandler handler, IMapView map)
    {
        handler.PlatformView?.UpdateCenter(map.CenterLatitude, map.CenterLongitude);
    }

    private static void MapZoomLevel(MapsuiMapHandler handler, IMapView map)
    {
        if (map.ZoomLevel > 0)
        {
            handler.PlatformView?.UpdateZoomLevel(map.ZoomLevel);
        }
    }

    private static void MapMapType(MapsuiMapHandler handler, IMapView map)
    {
        if (handler.PlatformView?.Map == null) return;
        
        var mapControl = handler.PlatformView;
        var mapType = map.MapType;
        
        var layers = mapControl.Map.Layers.Where(l => l.Name == "Basemap").ToList();
        foreach (var l in layers)
        {
            mapControl.Map.Layers.Remove(l);
        }
        
        MapsuiLayers.ILayer layer;
        
        switch (mapType)
        {
            case MapType.Street:
            default:
                layer = OpenStreetMap.CreateTileLayer("AvaloniaControlGallery");
                layer.Name = "Basemap";
                mapControl.Map.Layers.Insert(0, layer);
                break;
                
            case MapType.Satellite:
                layer = new global::Mapsui.Tiling.Layers.TileLayer(KnownTileSources.Create(KnownTileSource.EsriWorldPhysical)) { Name = "Basemap" };
                mapControl.Map.Layers.Insert(0, layer);
                break;
                
            case MapType.Hybrid:
                layer = new global::Mapsui.Tiling.Layers.TileLayer(KnownTileSources.Create(KnownTileSource.EsriWorldShadedRelief)) { Name = "Basemap" };
                mapControl.Map.Layers.Insert(0, layer);
                break;
        }
        
        mapControl.RefreshGraphics();
        mapControl.Refresh();
    }

    private static void MapIsShowingUser(MapsuiMapHandler handler, IMapView map)
    {
        if (handler.PlatformView?.Map == null) return;
        var mapControl = handler.PlatformView;
        
        // Cancel any pending location request
        handler.CancelLocationTask();

        // Find existing location layer
        var locationLayer = mapControl.Map.Layers.OfType<MapsuiLayers.MemoryLayer>().FirstOrDefault(l => l.Name == "MyLocation");
        
        if (map.IsShowingUser)
        {
            // Add layer if missing
            if (locationLayer == null)
            {
                locationLayer = new MapsuiLayers.MemoryLayer { Name = "MyLocation" };
                mapControl.Map.Layers.Add(locationLayer);
            }
 
            handler.StartLocationTask(locationLayer);
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

    private void CancelLocationTask()
    {
        try
        {
            _locationCts?.Cancel();
            _locationCts?.Dispose();
        }
        catch
        {
            // Ignore errors during cancellation
        }
        finally
        {
            _locationCts = null;
        }
    }

    private async void StartLocationTask(MapsuiLayers.MemoryLayer locationLayer)
    {
        if (PlatformView == null) return;
        
        var mapControl = PlatformView;
        
        try 
        {
            _locationCts = new CancellationTokenSource();
            var token = _locationCts.Token;

            // Run geolocation on a background thread to avoid blocking the UI thread
            await Task.Run(async () =>
            {
                Microsoft.Maui.Devices.Sensors.Location? location = null;

                try
                {
                    // Use IP-based geolocation on Desktop
                    if (OperatingSystem.IsMacOS() || OperatingSystem.IsWindows())
                    {
                        location = await GetLocationFromIpAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                        location = await Geolocation.Default.GetLocationAsync(request, token).ConfigureAwait(false);
                    }
                }
                catch (PlatformNotSupportedException)
                {
                    // Fallback to IP matching if Geolocation is not supported
                    location = await GetLocationFromIpAsync(token).ConfigureAwait(false);
                }
                
                if (location != null && !token.IsCancellationRequested)
                {
                    // Dispatch the UI update back to the UI thread
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                    {
                        if (!token.IsCancellationRequested)
                        {
                            UpdateUserLocation(this, mapControl, locationLayer, location);
                        }
                    });
                }
            }, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Request was canceled, ignore
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting location: {ex.Message}");
        }
    }
    
    private static void MapCenterToCommand(MapsuiMapHandler handler, IMapView map, object? args)
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

    private static void MapZoomToCommand(MapsuiMapHandler handler, IMapView map, object? args)
    {
        if (args is not double zoomLevel || zoomLevel <= 0) return;
        handler.PlatformView?.UpdateZoomLevel(zoomLevel);
    }

    private static void MapUpdatePinsCommand(MapsuiMapHandler handler, IMapView map, object? args)
    {
        handler.UpdatePins();
    }
    
    private static void MapUpdateMapElementsCommand(MapsuiMapHandler handler, IMapView map, object? args)
    {
        handler.UpdateMapElements();
    }
    
    /// <inheritdoc/>
    public void UpdateMapElements()
    {
        if (_shapesLayer == null || VirtualView?.MapElements == null) return;
        
        PlatformView?.UpdateShapes(VirtualView.MapElements, _shapesLayer);
    }
    
    /// <inheritdoc/>
    public void CenterTo(double latitude, double longitude)
    {
        PlatformView?.UpdateCenter(latitude, longitude);
    }
    
    /// <inheritdoc/>
    public void ZoomTo(double zoomLevel)
    {
        if (zoomLevel > 0)
        {
            PlatformView?.UpdateZoomLevel(zoomLevel);
        }
    }
    
    private static async Task<Microsoft.Maui.Devices.Sensors.Location?> GetLocationFromIpAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            
            var response = await httpClient.GetStringAsync("http://ip-api.com/json/?fields=lat,lon,status", cancellationToken).ConfigureAwait(false);
            
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
    
    private static void UpdateUserLocation(MapsuiMapHandler handler, MapsuiMapControl mapControl, MapsuiLayers.MemoryLayer layer, Microsoft.Maui.Devices.Sensors.Location location)
    {
        var (x, y) = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
        var point = new MPoint(x, y);
        
        // Create User Location Style (Blue Puck with Halo)
        var feature = new GeometryFeature
        {
            Geometry = new NetTopologySuite.Geometries.Point(x, y),
            Styles = new List<MapsuiStyles.IStyle>
            {
                new MapsuiStyles.SymbolStyle
                {
                    Fill = new MapsuiStyles.Brush(new MapsuiStyles.Color(33, 150, 243, 100)), // Light Blue transparent
                    Outline = null,
                    SymbolScale = 1.5
                },
                new MapsuiStyles.SymbolStyle
                {
                    Fill = new MapsuiStyles.Brush(MapsuiStyles.Color.Blue),
                    Outline = new MapsuiStyles.Pen(MapsuiStyles.Color.White, 3),
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
            var layers = new List<MapsuiLayers.ILayer>();
            
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

    /// <inheritdoc/>
    public void UpdatePins()
    {
        if (_pinsLayer == null || VirtualView?.Pins == null) return;
        
        PlatformView?.UpdatePins(VirtualView.Pins, _pinsLayer);
    }
}
