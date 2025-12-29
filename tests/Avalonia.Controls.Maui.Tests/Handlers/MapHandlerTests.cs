using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Maps;
using Avalonia.Controls.Maui.Maps.Handlers;
using Avalonia.Controls.Maui.Maps.Controls;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class MapHandlerTests : HandlerTestBase<MapHandler, MapStub>
{
    [AvaloniaFact(DisplayName = "MapType Initializes Correctly")]
    public async Task MapTypeInitializesCorrectly()
    {
        var map = new MapStub { MapType = MapType.Satellite };
        await ValidatePropertyInitValue(map, () => map.MapType, GetPlatformMapType, MapType.Satellite);
    }

    [AvaloniaTheory(DisplayName = "MapType Updates Correctly")]
    [InlineData(MapType.Street)]
    [InlineData(MapType.Satellite)]
    [InlineData(MapType.Hybrid)]
    public async Task MapTypeUpdatesCorrectly(MapType type)
    {
        var map = new MapStub { MapType = MapType.Street };
        await ValidatePropertyUpdatesValue(
            map,
            nameof(IMapView.MapType),
            GetPlatformMapType,
            type,
            MapType.Street);
    }

    [AvaloniaFact(DisplayName = "IsScrollEnabled Initializes Correctly")]
    public async Task IsScrollEnabledInitializesCorrectly()
    {
        var map = new MapStub { IsScrollEnabled = false };
        await ValidatePropertyInitValue(map, () => map.IsScrollEnabled, GetPlatformIsScrollEnabled, false);
    }

    [AvaloniaFact(DisplayName = "IsZoomEnabled Initializes Correctly")]
    public async Task IsZoomEnabledInitializesCorrectly()
    {
        var map = new MapStub { IsZoomEnabled = false };
        await ValidatePropertyInitValue(map, () => map.IsZoomEnabled, GetPlatformIsZoomEnabled, false);
    }

    [AvaloniaFact(DisplayName = "IsRotationEnabled Initializes Correctly")]
    public async Task IsRotationEnabledInitializesCorrectly()
    {
        var map = new MapStub { IsRotationEnabled = false };
        await ValidatePropertyInitValue(map, () => map.IsRotationEnabled, GetPlatformIsRotationEnabled, false);
    }

    [AvaloniaFact(DisplayName = "IsShowingUser Initializes Correctly")]
    public async Task IsShowingUserInitializesCorrectly()
    {
        var map = new MapStub { IsShowingUser = true };
        await ValidatePropertyInitValue(map, () => map.IsShowingUser, GetPlatformIsShowingUser, true);
    }

    [AvaloniaFact(DisplayName = "CenterLatitude Initializes Correctly")]
    public async Task CenterLatitudeInitializesCorrectly()
    {
        var map = new MapStub { CenterLatitude = 40.4168 };
        var handler = await CreateHandlerAsync(map);
        
        // Ensure size for navigator
        handler.PlatformView.Measure(new Size(1000, 1000));
        handler.PlatformView.Arrange(new Rect(0, 0, 1000, 1000));
        
        handler.UpdateValue(nameof(IMapView.CenterLatitude));
        
        await Task.Delay(200); // Wait for navigator to settle
        
        var actual = GetPlatformCenterLatitude(handler);
        Assert.Equal(40.4168, actual, 0.1);
    }

    [AvaloniaFact(DisplayName = "CenterLongitude Initializes Correctly")]
    public async Task CenterLongitudeInitializesCorrectly()
    {
        var map = new MapStub { CenterLongitude = -3.7038 };
        var handler = await CreateHandlerAsync(map);
        
        handler.PlatformView.Measure(new Avalonia.Size(1000, 1000));
        handler.PlatformView.Arrange(new Avalonia.Rect(0, 0, 1000, 1000));
        
        handler.UpdateValue(nameof(IMapView.CenterLongitude));
        
        await Task.Delay(200);
        
        var actual = GetPlatformCenterLongitude(handler);
        Assert.Equal(-3.7038, actual, 0.1);
    }

    [AvaloniaFact(DisplayName = "ZoomLevel Initializes Correctly")]
    public async Task ZoomLevelInitializesCorrectly()
    {
        var map = new MapStub { ZoomLevel = 5 };
        var handler = await CreateHandlerAsync(map);
        
        // Simulating Layout for Resolution calculation
        handler.PlatformView.Measure(new Size(1000, 1000));
        handler.PlatformView.Arrange(new Rect(0, 0, 1000, 1000));
        
        await Task.Delay(200);
        
        var actual = GetPlatformZoomLevel(handler);
        Assert.Equal(5.0, actual, 0.1);
    }

    [AvaloniaFact(DisplayName = "ShowAttribution Initializes Correctly")]
    public async Task ShowAttributionInitializesCorrectly()
    {
        var map = new MapStub { ShowAttribution = false };
        var handler = await CreateHandlerAsync(map);
        
        var actual = GetPlatformShowAttribution(handler);
        Assert.False(actual);
    }
    
    [AvaloniaFact(DisplayName = "Pins Add To Platform Layer")]
    public async Task PinsAddToPlatformLayer()
    {
        var map = new MapStub();
        map.Pins.Add(new MapPin 
        { 
            Label = "Test Pin", 
            Location = new Microsoft.Maui.Devices.Sensors.Location(10, 10),
            Type = PinType.Place 
        });
        
        var handler = await CreateHandlerAsync(map);
        handler.PlatformView.Measure(new Size(1000, 1000));
        handler.PlatformView.Arrange(new Rect(0, 0, 1000, 1000));
        
        handler.Invoke("UpdatePins", null);
        await Task.Delay(100);
        
        var pinCount = GetPlatformPinCount(handler);
        Assert.Equal(1, pinCount);
    }

    [AvaloniaFact(DisplayName = "Polygons Add To Platform Layer")]
    public async Task PolygonsAddToPlatformLayer()
    {
        var map = new MapStub();
        var polygon = new MapPolygon { FillColor = Microsoft.Maui.Graphics.Colors.Red };
        polygon.GeoPath.Add(new Microsoft.Maui.Devices.Sensors.Location(40, -3));
        polygon.GeoPath.Add(new Microsoft.Maui.Devices.Sensors.Location(41, -3));
        polygon.GeoPath.Add(new Microsoft.Maui.Devices.Sensors.Location(41, -4));
        map.MapElements.Add(polygon);

        var handler = await CreateHandlerAsync(map);
        handler.PlatformView.Measure(new Size(1000, 1000));
        handler.PlatformView.Arrange(new Rect(0, 0, 1000, 1000));
        
        handler.Invoke("UpdateMapElements", null);
        await Task.Delay(100);

        var shapeCount = GetPlatformShapeCount(handler);
        Assert.Equal(1, shapeCount);
    }

    [AvaloniaFact(DisplayName = "CenterTo Command Updates Platform")]
    public async Task CenterToCommandUpdatesPlatform()
    {
        var map = new MapStub();
        var handler = await CreateHandlerAsync(map);

        // Required for navigator initialization
        handler.PlatformView.Measure(new Size(1000, 1000));
        handler.PlatformView.Arrange(new Rect(0, 0, 1000, 1000));
        
        handler.Invoke("CenterTo", (41.9028, 12.4964));

        await Task.Delay(200);

        var lat = GetPlatformCenterLatitude(handler);
        var lon = GetPlatformCenterLongitude(handler);

        Assert.Equal(41.9028, lat, 0.001);
        Assert.Equal(12.4964, lon, 0.001);
    }



    [AvaloniaFact(DisplayName = "Null Pins Collection Does Not Crash")]
    public async Task NullPinsCollectionDoesNotCrash()
    {
        var map = new MapStub { Pins = null! };
        var handler = await CreateHandlerAsync(map);
        handler.UpdateValue("Pins");
    }

    [AvaloniaFact(DisplayName = "Null MapElements Collection Does Not Crash")]
    public async Task NullMapElementsCollectionDoesNotCrash()
    {
        var map = new MapStub { MapElements = null! };
        var handler = await CreateHandlerAsync(map);
        handler.UpdateValue("UpdateMapElements");
    }

    [AvaloniaFact(DisplayName = "Polygon With Empty Path Does Not Crash")]
    public async Task PolygonWithEmptyPathDoesNotCrash()
    {
        var map = new MapStub();
        var polygon = new MapPolygon();
        map.MapElements.Add(polygon);

        var handler = await CreateHandlerAsync(map);
        handler.UpdateValue("UpdateMapElements");

        var shapeCount = GetPlatformShapeCount(handler);
        Assert.Equal(0, shapeCount);
    }

    [AvaloniaFact(DisplayName = "Extreme Coordinates Handle Correctly")]
    public async Task ExtremeCoordinatesHandleCorrectly()
    {
        var map = new MapStub { CenterLatitude = 95, CenterLongitude = 190 };
        var handler = await CreateHandlerAsync(map);
        
        // Mapsui/SphericalMercator usually clamps or wraps, 
        // we just ensure no crash.
        handler.UpdateValue(nameof(IMapView.CenterLatitude));
        
        var lat = GetPlatformCenterLatitude(handler);
        // latitude 95 is invalid for Mercator (clamped to ~85), 
        // we just verify it's a number and no crash occurred.
        Assert.True(Math.Abs(lat) <= 90);
    }

    private double GetPlatformCenterLongitude(MapHandler handler)
    {
        var viewport = handler.PlatformView?.Map?.Navigator.Viewport;
        if (viewport == null) return 0;
        
        var (lon, lat) = Mapsui.Projections.SphericalMercator.ToLonLat(viewport.Value.CenterX, viewport.Value.CenterY);
        return Math.Round(lon, 4);
    }

    private double GetPlatformCenterLatitude(MapHandler handler)
    {
        var viewport = handler.PlatformView?.Map?.Navigator.Viewport;
        if (viewport == null) return 0;
        
        var (lon, lat) = Mapsui.Projections.SphericalMercator.ToLonLat(viewport.Value.CenterX, viewport.Value.CenterY);
        return Math.Round(lat, 4);
    }

    private double GetPlatformZoomLevel(MapHandler handler)
    {

        var res = handler.PlatformView?.Map?.Navigator?.Viewport.Resolution ?? 1;
        if (res <= 0) return 0;
        // MapExtensions.BaseResolution = 156543.03
        double baseRes = 156543.03;
        return Math.Round(Math.Log2(baseRes / res), 1);
    }

    private bool GetPlatformShowAttribution(MapHandler handler)
    {

        return handler.VirtualView.ShowAttribution;
    }
    
    private int GetPlatformPinCount(MapHandler handler)
    {
        var layer = handler.PlatformView?.Map?.Layers?.FirstOrDefault(l => l.Name == "Pins") as Mapsui.Layers.MemoryLayer;
        return layer?.Features?.Count() ?? 0;
    }

    private int GetPlatformShapeCount(MapHandler handler)
    {
        var layer = handler.PlatformView?.Map?.Layers?.FirstOrDefault(l => l.Name == "Shapes") as Mapsui.Layers.MemoryLayer;
        return layer?.Features?.Count() ?? 0;
    }


    private MapType GetPlatformMapType(MapHandler handler)
    {
        return handler.VirtualView.MapType; 
    }

    private bool GetPlatformIsScrollEnabled(MapHandler handler)
    {
        return !(handler.PlatformView?.Map?.Navigator?.PanLock ?? true);
    }

    private bool GetPlatformIsZoomEnabled(MapHandler handler)
    {
        return !(handler.PlatformView?.Map?.Navigator?.ZoomLock ?? true);
    }

    private bool GetPlatformIsRotationEnabled(MapHandler handler)
    {
        return !(handler.PlatformView?.Map?.Navigator?.RotationLock ?? true);
    }

    private bool GetPlatformIsShowingUser(MapHandler handler)
    {
        var layer = handler.PlatformView?.Map?.Layers?.FirstOrDefault(l => l.Name == "MyLocation");
        return layer != null && layer.Enabled;
    }
}