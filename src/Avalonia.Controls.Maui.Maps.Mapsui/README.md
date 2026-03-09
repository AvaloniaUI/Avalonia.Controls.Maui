# Avalonia.Controls.Maui.Maps.Mapsui

Avalonia.Controls.Maui.Maps.Mapsui provides an Avalonia-based implementation of [Microsoft.Maui.Controls.Maps](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/map) using [Mapsui](https://mapsui.com/) as the underlying map rendering engine.

## Setup

Call `UseAvaloniaMapsui()` on your `MauiAppBuilder`:

```csharp
var builder = MauiApp.CreateBuilder();
builder
    .UseMauiApp<App>()
    .UseAvaloniaApp()
    .UseAvaloniaMapsui();
```

## Tile Sources

Map types are backed by the following tile providers:

| MapType | Tile Source |
|---------|-------------|
| Street | OpenStreetMap |
| Satellite | ESRI World Physical |
| Hybrid | ESRI World Shaded Relief |

## Implemented APIs

### Map Properties

| Property | Status | Notes |
|----------|--------|-------|
| `MapType` | Implemented | Street, Satellite, and Hybrid supported via OpenStreetMap and ESRI tile sources. |
| `IsShowingUser` | Implemented | On desktop, uses IP-based geolocation (`ip-api.com`). On mobile, uses native device geolocation. Falls back to a default location if geolocation fails. |
| `IsScrollEnabled` | Implemented | Maps to Mapsui's `Navigator.PanLock`. |
| `IsZoomEnabled` | Implemented | Maps to Mapsui's `Navigator.ZoomLock`. |
| `IsTrafficEnabled` | Not supported | No-op. OpenStreetMap does not provide traffic overlays. |
| `Pins` | Implemented | Supports all `PinType` values (Generic, Place, SavedPin, SearchResult) with color coding and labels. Pin click events are supported. |
| `MapElements` | Implemented | Polygons, Polylines, and Circles with fill/stroke styling. |

### Map Commands & Events

| Feature | Status | Notes |
|---------|--------|-------|
| `MoveToRegion` | Implemented | Navigates to a `MapSpan` with calculated zoom level. |
| `MapClicked` | Implemented | Returns latitude/longitude of the clicked location. |
| Pin tap events | Implemented | Fires `MarkerClicked` on the tapped pin. |

### Map Elements

| Element | Status | Notes |
|---------|--------|-------|
| `Polygon` | Implemented | Rendered via NetTopologySuite geometry. Supports fill color, stroke color, and stroke width. |
| `Polyline` | Implemented | Rendered via NetTopologySuite geometry. Supports stroke color and stroke width. |
| `Circle` | Implemented | Approximated as a 32-segment polygon from center and radius. Supports fill and stroke styling. |

## Not Supported

| Feature | Reason |
|---------|--------|
| Traffic overlays | OpenStreetMap/ESRI tile sources do not provide traffic data. |
| Custom tile sources | Tile providers are currently hardcoded. |
| 3D/tilt view | Not available in Mapsui. |
| Pin clustering | Not yet implemented. |
