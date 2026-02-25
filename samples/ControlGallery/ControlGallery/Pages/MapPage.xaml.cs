using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Maps.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace ControlGallery.Pages;

public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();
        BindingContext = this;
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

    private async void OnSelectionMapClicked(object? sender, MapClickedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"OnSelectionMapClicked: Pin={e.Pin != null}, Element={e.Element?.GetType().Name}");
        
        if (e.Pin != null)
        {
            SelectionInfoLabel.Text = $"📍 Pin: {e.Pin.Label} at {e.Pin.Location?.Latitude:F4}, {e.Pin.Location?.Longitude:F4}";
            SelectionInfoLabel.TextColor = Microsoft.Maui.Graphics.Colors.Blue;
        }
        else if (e.Element is MapPolygon polygon)
        {
            SelectionInfoLabel.Text = $"🔷 Polygon: {polygon.GeoPath.Count} points, Stroke: {polygon.StrokeColor}";
            SelectionInfoLabel.TextColor = Microsoft.Maui.Graphics.Colors.Blue;
        }
        else if (e.Element is MapCircle circle)
        {
            SelectionInfoLabel.Text = $"⭕ Circle: Radius={circle.Radius}m, Center: {circle.Center?.Latitude:F4}, {circle.Center?.Longitude:F4}";
            SelectionInfoLabel.TextColor = Microsoft.Maui.Graphics.Colors.Red;
        }
        else
        {
            SelectionInfoLabel.Text = $"📍 Clicked: {e.Location.Latitude:F4}, {e.Location.Longitude:F4}";
            SelectionInfoLabel.TextColor = Microsoft.Maui.Graphics.Colors.Gray;
        }

        await Task.CompletedTask; // Keep async signature
    }

    private void InitializeSelectionSample()
    {
        if (SelectionMap == null) return;

        // Add a Pin
        SelectionMap.Pins.Add(new MapPin
        {
            Label = "Seville Cathedral",
            Address = "Seville, Spain",
            Location = new Location(37.3858, -5.9931),
            Type = PinType.Place
        });

        // Add a Circle around the Cathedral
        SelectionMap.MapElements.Add(new MapCircle
        {
            Center = new Location(37.3858, -5.9931),
            Radius = 200,
            StrokeColor = Microsoft.Maui.Graphics.Colors.Red,
            StrokeWidth = 2,
            FillColor = Microsoft.Maui.Graphics.Color.FromRgba(255, 0, 0, 40)
        });

        // Add a Polygon for a nearby plaza
        var polygon = new MapPolygon
        {
            StrokeColor = Microsoft.Maui.Graphics.Colors.Blue,
            StrokeWidth = 2,
            FillColor = Microsoft.Maui.Graphics.Color.FromRgba(0, 0, 255, 40)
        };
        polygon.GeoPath.Add(new Location(37.3833, -5.9912));
        polygon.GeoPath.Add(new Location(37.3833, -5.9932));
        polygon.GeoPath.Add(new Location(37.3823, -5.9922));
        SelectionMap.MapElements.Add(polygon);
    }

    // Pins handlers
    private void OnAddRomePinClicked(object? sender, EventArgs e)
    {
        PinsMap.Pins.Add(new MapPin
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
        PinsMap.Pins.Add(new MapPin
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
        PinsMap.Pins.Add(new MapPin
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
        InteractionMap.ZoomTo(zoomLevel);
    }

    // Navigation handlers
    private void OnMoveToNYCClicked(object? sender, EventArgs e)
    {
        RegionMap.CenterTo(40.7128, -74.0060);
        RegionMap.ZoomTo(12);
        CurrentLocationLabel.Text = "Current: New York, USA";
    }

    private void OnMoveToLondonClicked(object? sender, EventArgs e)
    {
        RegionMap.CenterTo(51.5074, -0.1278);
        RegionMap.ZoomTo(12);
        CurrentLocationLabel.Text = "Current: London, UK";
    }

    private void OnMoveToTokyoClicked(object? sender, EventArgs e)
    {
        RegionMap.CenterTo(35.6762, 139.6503);
        RegionMap.ZoomTo(12);
        CurrentLocationLabel.Text = "Current: Tokyo, Japan";
    }

    private void OnMoveToSydneyClicked(object? sender, EventArgs e)
    {
        RegionMap.CenterTo(-33.8688, 151.2093);
        RegionMap.ZoomTo(12);
        CurrentLocationLabel.Text = "Current: Sydney, Australia";
    }

    // Shapes handlers
    private void OnAddPolygonClicked(object? sender, EventArgs e)
    {
        var polygon = new MapPolygon
        {
            StrokeColor = Microsoft.Maui.Graphics.Colors.Blue,
            StrokeWidth = 3,
            FillColor = Microsoft.Maui.Graphics.Color.FromRgba(0, 0, 255, 50)
        };
        // Triangle in France
        polygon.GeoPath.Add(new Location(48.8566, 2.3522)); // Paris
        polygon.GeoPath.Add(new Location(43.2965, 5.3698)); // Marseille
        polygon.GeoPath.Add(new Location(45.7640, 4.8357)); // Lyon
        
        ShapesMap.MapElements.Add(polygon);
        UpdateShapesCount();
    }

    private void OnAddPolylineClicked(object? sender, EventArgs e)
    {
        var polyline = new MapPolyline
        {
            StrokeColor = Microsoft.Maui.Graphics.Colors.Red,
            StrokeWidth = 4
        };
        polyline.GeoPath.Add(new Location(52.5200, 13.4050)); 
        polyline.GeoPath.Add(new Location(50.1109, 8.6821));  
        polyline.GeoPath.Add(new Location(48.1351, 11.5820)); 
        
        ShapesMap.MapElements.Add(polyline);
        UpdateShapesCount();
    }

    private void OnAddCircleClicked(object? sender, EventArgs e)
    {
        var circle = new MapCircle
        {
            Center = new Location(41.9028, 12.4964), // Rome
            Radius = 100000, // 100km in meters
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
                "Street" => Avalonia.Controls.Maui.Maps.MapType.Street,
                "Satellite" => Avalonia.Controls.Maui.Maps.MapType.Satellite,
                "Hybrid" => Avalonia.Controls.Maui.Maps.MapType.Hybrid,
                _ => Avalonia.Controls.Maui.Maps.MapType.Street
            };
        }
    }


    // ItemsSource handlers
    public System.Collections.ObjectModel.ObservableCollection<PlaceItem> PlaceData { get; } = new();

    private void OnAddMadridDataClicked(object? sender, EventArgs e)
    {
        var item = new PlaceItem("Madrid", "Capital of Spain", new Location(40.4168, -3.7038));
        
        // Use dotnet_bot.png as custom icon - try multiple paths
        var possiblePaths = new[]
        {
            System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "Images", "dotnet_bot.png"),
            System.IO.Path.Combine(AppContext.BaseDirectory, "dotnet_bot.png"),
            "/Users/jsuarezruiz/Documents/GitHub/Avalonia.Controls.Maui/samples/ControlGallery/ControlGallery/Resources/Images/dotnet_bot.png"
        };
        
        foreach (var path in possiblePaths)
        {
            if (System.IO.File.Exists(path))
            {
                item.Icon = ImageSource.FromFile(path);
                item.IconScale = 0.15;
                break;
            }
        }
        
        PlaceData.Add(item);
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
    public ImageSource? Icon { get; set; }
    public double IconScale { get; set; } = 1.0;

    public PlaceItem(string name, string description, Location position)
    {
        Name = name;
        Description = description;
        Position = position;
    }
}
