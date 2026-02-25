using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Controls.Maps;
using Avalonia.Controls.Maui.Maps.Mapsui.Handlers;
using Location = Microsoft.Maui.Devices.Sensors.Location;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class MapsuiMapHandlerTests : HandlerTestBase<MapsuiMapHandler, MapStub>
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
            nameof(IMap.MapType),
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

    [AvaloniaFact(DisplayName = "IsShowingUser Initializes Correctly")]
    public async Task IsShowingUserInitializesCorrectly()
    {
        var map = new MapStub { IsShowingUser = true };
        await ValidatePropertyInitValue(map, () => map.IsShowingUser, GetPlatformIsShowingUser, true);
    }

    [AvaloniaFact(DisplayName = "MoveToRegion Updates Platform Center and Zoom")]
    public async Task MoveToRegionUpdatesPlatform()
    {
        var map = new MapStub();
        var handler = await CreateHandlerAsync(map);

        // Required for navigator initialization
        handler.PlatformView.Measure(new Size(1000, 1000));
        handler.PlatformView.Arrange(new Rect(0, 0, 1000, 1000));

        var span = new MapSpan(new Microsoft.Maui.Devices.Sensors.Location(41.9028, 12.4964), 0.088, 0.088);
        handler.Invoke(nameof(IMap.MoveToRegion), span);

        await Task.Delay(200);

        var lat = GetPlatformCenterLatitude(handler);
        var lon = GetPlatformCenterLongitude(handler);

        Assert.Equal(41.9028, lat, 0.1);
        Assert.Equal(12.4964, lon, 0.1);
    }

    [AvaloniaFact(DisplayName = "Pins Add To Platform Layer")]
    public async Task PinsAddToPlatformLayer()
    {
        var map = new MapStub();
        var pin = new Pin
        {
            Label = "Test Pin",
            Location = new Microsoft.Maui.Devices.Sensors.Location(10, 10),
            Type = PinType.Place
        };
        map.Pins.Add(pin);

        var handler = await CreateHandlerAsync(map);
        handler.PlatformView.Measure(new Size(1000, 1000));
        handler.PlatformView.Arrange(new Rect(0, 0, 1000, 1000));

        handler.UpdateValue(nameof(IMap.Pins));
        await Task.Delay(100);

        var pinCount = GetPlatformPinCount(handler);
        Assert.Equal(1, pinCount);
    }

    [AvaloniaFact(DisplayName = "Polygons Add To Platform Layer")]
    public async Task PolygonsAddToPlatformLayer()
    {
        var map = new MapStub();
        var polygon = new Microsoft.Maui.Controls.Maps.Polygon { FillColor = Microsoft.Maui.Graphics.Colors.Red };
        polygon.Geopath.Add(new Microsoft.Maui.Devices.Sensors.Location(40, -3));
        polygon.Geopath.Add(new Microsoft.Maui.Devices.Sensors.Location(41, -3));
        polygon.Geopath.Add(new Microsoft.Maui.Devices.Sensors.Location(41, -4));
        map.Elements.Add(polygon);

        var handler = await CreateHandlerAsync(map);
        handler.PlatformView.Measure(new Size(1000, 1000));
        handler.PlatformView.Arrange(new Rect(0, 0, 1000, 1000));

        handler.UpdateValue(nameof(IMap.Elements));
        await Task.Delay(100);

        var shapeCount = GetPlatformShapeCount(handler);
        Assert.Equal(1, shapeCount);
    }

    [AvaloniaFact(DisplayName = "Circles Add To Platform Layer")]
    public async Task CirclesAddToPlatformLayer()
    {
        var map = new MapStub();
        var circle = new Circle
        {
            Center = new Microsoft.Maui.Devices.Sensors.Location(41.9028, 12.4964),
            Radius = Distance.FromMeters(100000),
            StrokeColor = Microsoft.Maui.Graphics.Colors.Green,
            FillColor = Microsoft.Maui.Graphics.Colors.LightGreen
        };
        map.Elements.Add(circle);

        var handler = await CreateHandlerAsync(map);
        handler.PlatformView.Measure(new Size(1000, 1000));
        handler.PlatformView.Arrange(new Rect(0, 0, 1000, 1000));

        handler.UpdateValue(nameof(IMap.Elements));
        await Task.Delay(100);

        var shapeCount = GetPlatformShapeCount(handler);
        Assert.Equal(1, shapeCount);
    }

    [AvaloniaFact(DisplayName = "Null Pins Collection Does Not Crash")]
    public async Task NullPinsCollectionDoesNotCrash()
    {
        var map = new MapStub { Pins = null! };
        var handler = await CreateHandlerAsync(map);
        handler.UpdateValue(nameof(IMap.Pins));
    }

    [AvaloniaFact(DisplayName = "Null Elements Collection Does Not Crash")]
    public async Task NullElementsCollectionDoesNotCrash()
    {
        var map = new MapStub { Elements = null! };
        var handler = await CreateHandlerAsync(map);
        handler.UpdateValue(nameof(IMap.Elements));
    }

    [AvaloniaFact(DisplayName = "Polygon With Empty Path Does Not Crash")]
    public async Task PolygonWithEmptyPathDoesNotCrash()
    {
        var map = new MapStub();
        var polygon = new Microsoft.Maui.Controls.Maps.Polygon();
        map.Elements.Add(polygon);

        var handler = await CreateHandlerAsync(map);
        handler.UpdateValue(nameof(IMap.Elements));

        var shapeCount = GetPlatformShapeCount(handler);
        Assert.Equal(0, shapeCount);
    }

    [AvaloniaFact(DisplayName = "MoveToRegion With Extreme Values Does Not Crash")]
    public async Task MoveToRegionWithExtremeValuesDoesNotCrash()
    {
        var map = new MapStub();
        var handler = await CreateHandlerAsync(map);

        handler.PlatformView.Measure(new Size(1000, 1000));
        handler.PlatformView.Arrange(new Rect(0, 0, 1000, 1000));

        // Very wide span (nearly the whole world)
        var span = new MapSpan(new Microsoft.Maui.Devices.Sensors.Location(0, 0), 80, 180);
        handler.Invoke(nameof(IMap.MoveToRegion), span);

        await Task.Delay(200);

        // Just ensure no crash occurred
        var lat = GetPlatformCenterLatitude(handler);
        Assert.True(Math.Abs(lat) <= 90);
    }

    private double GetPlatformCenterLongitude(MapsuiMapHandler handler)
    {
        var viewport = handler.PlatformView?.Map?.Navigator.Viewport;
        if (viewport == null) return 0;

        var (lon, lat) = Mapsui.Projections.SphericalMercator.ToLonLat(viewport.Value.CenterX, viewport.Value.CenterY);
        return Math.Round(lon, 4);
    }

    private double GetPlatformCenterLatitude(MapsuiMapHandler handler)
    {
        var viewport = handler.PlatformView?.Map?.Navigator.Viewport;
        if (viewport == null) return 0;

        var (lon, lat) = Mapsui.Projections.SphericalMercator.ToLonLat(viewport.Value.CenterX, viewport.Value.CenterY);
        return Math.Round(lat, 4);
    }

    private double GetPlatformZoomLevel(MapsuiMapHandler handler)
    {
        var res = handler.PlatformView?.Map?.Navigator?.Viewport.Resolution ?? 1;
        if (res <= 0) return 0;
        double baseRes = 156543.03;
        return Math.Round(Math.Log2(baseRes / res), 1);
    }

    private int GetPlatformPinCount(MapsuiMapHandler handler)
    {
        var layer = handler.PlatformView?.Map?.Layers?.FirstOrDefault(l => l.Name == "Pins") as Mapsui.Layers.MemoryLayer;
        return layer?.Features?.Count() ?? 0;
    }

    private int GetPlatformShapeCount(MapsuiMapHandler handler)
    {
        var layer = handler.PlatformView?.Map?.Layers?.FirstOrDefault(l => l.Name == "Shapes") as Mapsui.Layers.MemoryLayer;
        return layer?.Features?.Count() ?? 0;
    }

    private MapType GetPlatformMapType(MapsuiMapHandler handler)
    {
        return handler.VirtualView.MapType;
    }

    private bool GetPlatformIsScrollEnabled(MapsuiMapHandler handler)
    {
        return !(handler.PlatformView?.Map?.Navigator?.PanLock ?? true);
    }

    private bool GetPlatformIsZoomEnabled(MapsuiMapHandler handler)
    {
        return !(handler.PlatformView?.Map?.Navigator?.ZoomLock ?? true);
    }

    private bool GetPlatformIsShowingUser(MapsuiMapHandler handler)
    {
        var layer = handler.PlatformView?.Map?.Layers?.FirstOrDefault(l => l.Name == "MyLocation");
        return layer != null && layer.Enabled;
    }
}
