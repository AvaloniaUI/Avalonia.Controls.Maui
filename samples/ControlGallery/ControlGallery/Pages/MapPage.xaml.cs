using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;

namespace ControlGallery.Pages;

public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Set initial map positions via MoveToRegion
        BasicMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(37.3891, -5.9845), Distance.FromKilometers(5)));
        MapTypeMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(37.3891, -5.9845), Distance.FromKilometers(5)));
        UserLocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(59.4370, 24.7536), Distance.FromKilometers(20)));
        ClickableMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(37.3891, -5.9845), Distance.FromKilometers(5)));
        SelectionMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(37.3891, -5.9845), Distance.FromKilometers(5)));
        PinsMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(47.0, 10.0), Distance.FromKilometers(1000)));
        ItemsSourceMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(40.4168, -3.7038), Distance.FromKilometers(200)));
        InteractionMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(37.3891, -5.9845), Distance.FromKilometers(5)));
        ShapesMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(46.0, 8.0), Distance.FromKilometers(1000)));
        RegionMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(40.7128, -74.0060), Distance.FromKilometers(5)));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Subscribe to MapClicked
        ClickableMap.MapClicked += OnMapClicked;

        try
        {
            var status = PermissionStatus.Unknown;

            // Mock permissions on Desktop (macOS/Windows) since we'll mock the location too
            if (OperatingSystem.IsMacOS() || OperatingSystem.IsWindows())
            {
                status = PermissionStatus.Granted;
            }
            else
            {
                status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
            }

            if (status == PermissionStatus.Granted)
            {
                if (UserLocationMap != null) UserLocationMap.IsShowingUser = true;

                // Initialize Selection Sample
                InitializeSelectionSample();
            }
            else
            {
                 await DisplayAlert("Permission Denied", "Location permission is required to show your location.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
        }
    }

    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        ClickedCoordsLabel.Text = $"Lat: {e.Location.Latitude:F4}, Lon: {e.Location.Longitude:F4}";
    }

    private void OnSelectionMapClicked(object? sender, MapClickedEventArgs e)
    {
        SelectionInfoLabel.Text = $"Clicked: {e.Location.Latitude:F4}, {e.Location.Longitude:F4}";
        SelectionInfoLabel.TextColor = Microsoft.Maui.Graphics.Colors.Gray;
    }

    private void InitializeSelectionSample()
    {
        if (SelectionMap == null) return;

        // Add a Pin
        var pin = new Pin
        {
            Label = "Seville Cathedral",
            Address = "Seville, Spain",
            Location = new Location(37.3858, -5.9931),
            Type = PinType.Place
        };
        pin.MarkerClicked += OnSelectionPinClicked;
        SelectionMap.Pins.Add(pin);

        // Add a Circle around the Cathedral
        SelectionMap.MapElements.Add(new Circle
        {
            Center = new Location(37.3858, -5.9931),
            Radius = Distance.FromMeters(200),
            StrokeColor = Microsoft.Maui.Graphics.Colors.Red,
            StrokeWidth = 2,
            FillColor = Microsoft.Maui.Graphics.Color.FromRgba(255, 0, 0, 40)
        });

        // Add a Polygon for a nearby plaza
        var polygon = new Polygon
        {
            StrokeColor = Microsoft.Maui.Graphics.Colors.Blue,
            StrokeWidth = 2,
            FillColor = Microsoft.Maui.Graphics.Color.FromRgba(0, 0, 255, 40)
        };
        polygon.Geopath.Add(new Location(37.3833, -5.9912));
        polygon.Geopath.Add(new Location(37.3833, -5.9932));
        polygon.Geopath.Add(new Location(37.3823, -5.9922));
        SelectionMap.MapElements.Add(polygon);
    }

    private void OnSelectionPinClicked(object? sender, PinClickedEventArgs e)
    {
        if (sender is Pin pin)
        {
            SelectionInfoLabel.Text = $"Pin: {pin.Label} at {pin.Location.Latitude:F4}, {pin.Location.Longitude:F4}";
            SelectionInfoLabel.TextColor = Microsoft.Maui.Graphics.Colors.Blue;
        }
    }

    // Pins handlers
    private void OnAddRomePinClicked(object? sender, EventArgs e)
    {
        PinsMap.Pins.Add(new Pin
        {
            Label = "Rome",
            Address = "Italy",
            Location = new Location(41.9028, 12.4964),
            Type = PinType.Place
        });
        UpdatePinsCount();
    }

    private void OnAddParisPinClicked(object? sender, EventArgs e)
    {
        PinsMap.Pins.Add(new Pin
        {
            Label = "Paris",
            Address = "France",
            Location = new Location(48.8566, 2.3522),
            Type = PinType.Place
        });
        UpdatePinsCount();
    }

    private void OnAddBerlinPinClicked(object? sender, EventArgs e)
    {
        PinsMap.Pins.Add(new Pin
        {
            Label = "Berlin",
            Address = "Germany",
            Location = new Location(52.5200, 13.4050),
            Type = PinType.SavedPin
        });
        UpdatePinsCount();
    }

    private void OnClearPinsClicked(object? sender, EventArgs e)
    {
        PinsMap.Pins.Clear();
        UpdatePinsCount();
    }

    private void UpdatePinsCount()
    {
        PinsCountLabel.Text = $"Pins: {PinsMap.Pins.Count}";
    }

    // Scroll/Zoom handlers
    private void OnScrollSwitchToggled(object? sender, ToggledEventArgs e)
    {
        InteractionMap.IsScrollEnabled = e.Value;
    }

    private void OnZoomSwitchToggled(object? sender, ToggledEventArgs e)
    {
        InteractionMap.IsZoomEnabled = e.Value;
        ZoomSlider.IsEnabled = e.Value;
    }

    private void OnZoomSliderChanged(object? sender, ValueChangedEventArgs e)
    {
        int zoomLevel = (int)Math.Round(e.NewValue);
        ZoomLevelLabel.Text = zoomLevel.ToString();
        var center = InteractionMap.VisibleRegion?.Center ?? new Location(37.3891, -5.9845);
        var latDegrees = 360.0 / Math.Pow(2, zoomLevel);
        InteractionMap.MoveToRegion(new MapSpan(center, latDegrees, latDegrees));
    }

    // Navigation handlers
    private void OnMoveToNYCClicked(object? sender, EventArgs e)
    {
        RegionMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(40.7128, -74.0060), Distance.FromKilometers(5)));
        CurrentLocationLabel.Text = "Current: New York, USA";
    }

    private void OnMoveToLondonClicked(object? sender, EventArgs e)
    {
        RegionMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(51.5074, -0.1278), Distance.FromKilometers(5)));
        CurrentLocationLabel.Text = "Current: London, UK";
    }

    private void OnMoveToTokyoClicked(object? sender, EventArgs e)
    {
        RegionMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(35.6762, 139.6503), Distance.FromKilometers(5)));
        CurrentLocationLabel.Text = "Current: Tokyo, Japan";
    }

    private void OnMoveToSydneyClicked(object? sender, EventArgs e)
    {
        RegionMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(-33.8688, 151.2093), Distance.FromKilometers(5)));
        CurrentLocationLabel.Text = "Current: Sydney, Australia";
    }

    // Shapes handlers
    private void OnAddPolygonClicked(object? sender, EventArgs e)
    {
        var polygon = new Polygon
        {
            StrokeColor = Microsoft.Maui.Graphics.Colors.Blue,
            StrokeWidth = 3,
            FillColor = Microsoft.Maui.Graphics.Color.FromRgba(0, 0, 255, 50)
        };
        // Triangle in France
        polygon.Geopath.Add(new Location(48.8566, 2.3522)); // Paris
        polygon.Geopath.Add(new Location(43.2965, 5.3698)); // Marseille
        polygon.Geopath.Add(new Location(45.7640, 4.8357)); // Lyon

        ShapesMap.MapElements.Add(polygon);
        UpdateShapesCount();
    }

    private void OnAddPolylineClicked(object? sender, EventArgs e)
    {
        var polyline = new Polyline
        {
            StrokeColor = Microsoft.Maui.Graphics.Colors.Red,
            StrokeWidth = 4
        };
        polyline.Geopath.Add(new Location(52.5200, 13.4050));
        polyline.Geopath.Add(new Location(50.1109, 8.6821));
        polyline.Geopath.Add(new Location(48.1351, 11.5820));

        ShapesMap.MapElements.Add(polyline);
        UpdateShapesCount();
    }

    private void OnAddCircleClicked(object? sender, EventArgs e)
    {
        var circle = new Circle
        {
            Center = new Location(41.9028, 12.4964), // Rome
            Radius = Distance.FromMeters(100000), // 100km
            StrokeColor = Microsoft.Maui.Graphics.Colors.Green,
            StrokeWidth = 2,
            FillColor = Microsoft.Maui.Graphics.Color.FromRgba(0, 255, 0, 50)
        };

        ShapesMap.MapElements.Add(circle);
        UpdateShapesCount();
    }

    private void OnClearShapesClicked(object? sender, EventArgs e)
    {
        ShapesMap.MapElements.Clear();
        UpdateShapesCount();
    }

    private void UpdateShapesCount()
    {
        ShapesCountLabel.Text = $"Shapes: {ShapesMap.MapElements.Count}";
    }

    // Map Type handlers
    private void OnMapTypeChanged(object? sender, EventArgs e)
    {
        if (sender is Picker picker && picker.SelectedItem is string selectedType)
        {
            if (MapTypeMap == null) return;

            MapTypeMap.MapType = selectedType switch
            {
                "Street" => Microsoft.Maui.Maps.MapType.Street,
                "Satellite" => Microsoft.Maui.Maps.MapType.Satellite,
                "Hybrid" => Microsoft.Maui.Maps.MapType.Hybrid,
                _ => Microsoft.Maui.Maps.MapType.Street
            };
        }
    }


    // ItemsSource handlers
    public System.Collections.ObjectModel.ObservableCollection<PlaceItem> PlaceData { get; } = new();

    private void OnAddMadridDataClicked(object? sender, EventArgs e)
    {
        PlaceData.Add(new PlaceItem("Madrid", "Capital of Spain", new Location(40.4168, -3.7038)));
    }

    private void OnAddBarcelonaDataClicked(object? sender, EventArgs e)
    {
        PlaceData.Add(new PlaceItem("Barcelona", "Famous for architecture", new Location(41.3851, 2.1734)));
    }

    private void OnClearDataClicked(object? sender, EventArgs e)
    {
        PlaceData.Clear();
    }
}

public class PlaceItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Location Position { get; set; }

    public PlaceItem(string name, string description, Location position)
    {
        Name = name;
        Description = description;
        Position = position;
    }
}
