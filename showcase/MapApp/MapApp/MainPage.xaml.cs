using Mapsui;
using Mapsui.Limiting;
using Mapsui.Projections;
using Mapsui.Tiling;

namespace MapApp;

public partial class MainPage : ContentPage
{
	private static readonly Dictionary<string, (double Lat, double Lon)> PresetLocations = new()
	{
		["New York"] = (40.7128, -74.0060),
		["London"] = (51.5074, -0.1278),
		["Tokyo"] = (35.6762, 139.6503),
		["Sydney"] = (-33.8688, 151.2093),
		["Paris"] = (48.8566, 2.3522),
		["S\u00e3o Paulo"] = (-23.5505, -46.6333),
	};

	public MainPage()
	{
		InitializeComponent();
		InitializeMap();
	}

	// OSM zoom level 2 ≈ shows the full world; zoom level 19 ≈ building detail
	private static readonly double MaxResolution = GetResolutionForZoomLevel(2);
	private static readonly double MinResolution = GetResolutionForZoomLevel(19);

	private void InitializeMap()
	{
		MapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());

		if (MapControl.Map?.Navigator is { } navigator)
		{
			// Prevent zooming out beyond the world extent
			navigator.OverrideZoomBounds = new MMinMax(MinResolution, MaxResolution);

			// Keep the viewport within the map extent (no grey areas outside the map)
			navigator.Limiter = new ViewportLimiterKeepWithinExtent();
			navigator.OverridePanBounds = GetOsmPanBounds();

			navigator.ViewportChanged += OnViewportChanged;
		}

		UpdateCoordinateDisplay();
	}

	private static MRect GetOsmPanBounds()
	{
		// Full extent of the SphericalMercator projection (Web Mercator EPSG:3857)
		const double extent = 20037508.34;
		return new MRect(-extent, -extent, extent, extent);
	}

	private void OnViewportChanged(object? sender, EventArgs e)
	{
		Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(UpdateCoordinateDisplay);
	}

	private void UpdateCoordinateDisplay()
	{
		if (MapControl.Map?.Navigator is not { } navigator) return;

		var viewport = navigator.Viewport;
		var (lon, lat) = SphericalMercator.ToLonLat(viewport.CenterX, viewport.CenterY);
		CoordinateLabel.Text = $"Center: {lat:F4}, {lon:F4}";
	}

	private void OnGoClicked(object? sender, EventArgs e)
	{
		if (double.TryParse(LatEntry.Text, System.Globalization.NumberStyles.Float,
			    System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
		    double.TryParse(LonEntry.Text, System.Globalization.NumberStyles.Float,
			    System.Globalization.CultureInfo.InvariantCulture, out var lon))
		{
			NavigateTo(lat, lon);
		}
	}

	private void OnPresetClicked(object? sender, EventArgs e)
	{
		if (sender is Button button && PresetLocations.TryGetValue(button.Text, out var location))
		{
			NavigateTo(location.Lat, location.Lon);
			LatEntry.Text = location.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
			LonEntry.Text = location.Lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}
	}

	private void NavigateTo(double lat, double lon, int zoomLevel = 10)
	{
		var (x, y) = SphericalMercator.FromLonLat(lon, lat);
		MapControl.Map?.Navigator.CenterOnAndZoomTo(new MPoint(x, y), resolution: GetResolutionForZoomLevel(zoomLevel));
	}

	private void OnZoomInClicked(object? sender, EventArgs e)
	{
		MapControl.Map?.Navigator.ZoomIn(duration: 250);
	}

	private void OnZoomOutClicked(object? sender, EventArgs e)
	{
		MapControl.Map?.Navigator.ZoomOut(duration: 250);
	}

	private void OnResetClicked(object? sender, EventArgs e)
	{
		// Reset to world view (zoom level 2)
		MapControl.Map?.Navigator.CenterOnAndZoomTo(new MPoint(0, 0), resolution: MaxResolution);
	}

	/// <summary>
	/// Approximate resolution for a given OSM zoom level.
	/// Resolution at zoom level z ≈ 156543.03 / 2^z (meters/pixel at equator).
	/// </summary>
	private static double GetResolutionForZoomLevel(int zoomLevel)
	{
		return 156543.03 / Math.Pow(2, zoomLevel);
	}
}
