using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls.Maui.Maps.Handlers;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Maps.Controls;

/// <summary>
/// A Map control that displays OpenStreetMap tiles using Mapsui.Avalonia.
/// </summary>
public class MapView : View, IMapView
{
    /// <summary>Bindable property for <see cref="IsScrollEnabled"/>.</summary>
    public static readonly BindableProperty IsScrollEnabledProperty =
        BindableProperty.Create(nameof(IsScrollEnabled), typeof(bool), typeof(MapView), true);

    /// <summary>Bindable property for <see cref="IsZoomEnabled"/>.</summary>
    public static readonly BindableProperty IsZoomEnabledProperty =
        BindableProperty.Create(nameof(IsZoomEnabled), typeof(bool), typeof(MapView), true);

    /// <summary>Bindable property for <see cref="MapType"/>.</summary>
    public static readonly BindableProperty MapTypeProperty =
        BindableProperty.Create(nameof(MapType), typeof(MapType), typeof(MapView), MapType.Street);

    /// <summary>Bindable property for <see cref="IsShowingUser"/>.</summary>
    public static readonly BindableProperty IsShowingUserProperty =
        BindableProperty.Create(nameof(IsShowingUser), typeof(bool), typeof(MapView), false);

    /// <summary>
    /// Gets or sets the display style of the map.
    /// </summary>
    public MapType MapType
    {
        get => (MapType)GetValue(MapTypeProperty);
        set => SetValue(MapTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user's current location is visible on the map.
    /// </summary>
    public bool IsShowingUser
    {
        get => (bool)GetValue(IsShowingUserProperty);
        set => SetValue(IsShowingUserProperty, value);
    }

    /// <summary>Bindable property for <see cref="IsRotationEnabled"/>.</summary>
    public static readonly BindableProperty IsRotationEnabledProperty =
        BindableProperty.Create(nameof(IsRotationEnabled), typeof(bool), typeof(MapView), true);

    /// <summary>Bindable property for <see cref="ShowAttribution"/>.</summary>
    public static readonly BindableProperty ShowAttributionProperty =
        BindableProperty.Create(nameof(ShowAttribution), typeof(bool), typeof(MapView), true);

    /// <summary>Bindable property for <see cref="CenterLatitude"/>.</summary>
    public static readonly BindableProperty CenterLatitudeProperty =
        BindableProperty.Create(nameof(CenterLatitude), typeof(double), typeof(MapView), 0.0);

    /// <summary>Bindable property for <see cref="CenterLongitude"/>.</summary>
    public static readonly BindableProperty CenterLongitudeProperty =
        BindableProperty.Create(nameof(CenterLongitude), typeof(double), typeof(MapView), 0.0);

    /// <summary>Bindable property for <see cref="ZoomLevel"/>.</summary>
    public static readonly BindableProperty ZoomLevelProperty =
        BindableProperty.Create(nameof(ZoomLevel), typeof(double), typeof(MapView), 0.0);

    private readonly ObservableCollection<MapPin> _pins;
    private readonly ObservableCollection<MapElement> _mapElements;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapView"/> class.
    /// </summary>
    public MapView()
    {
        _pins = new ObservableCollection<MapPin>();
        _pins.CollectionChanged += OnPinsCollectionChanged;
        
        _mapElements = new ObservableCollection<MapElement>();
        _mapElements.CollectionChanged += OnMapElementsCollectionChanged;
    }

    /// <summary>
    /// Gets or sets whether scroll (pan) gestures are enabled.
    /// </summary>
    public bool IsScrollEnabled
    {
        get => (bool)GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether zoom gestures are enabled.
    /// </summary>
    public bool IsZoomEnabled
    {
        get => (bool)GetValue(IsZoomEnabledProperty);
        set => SetValue(IsZoomEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether rotation gestures are enabled.
    /// </summary>
    public bool IsRotationEnabled
    {
        get => (bool)GetValue(IsRotationEnabledProperty);
        set => SetValue(IsRotationEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the OpenStreetMap attribution is shown.
    /// </summary>
    public bool ShowAttribution
    {
        get => (bool)GetValue(ShowAttributionProperty);
        set => SetValue(ShowAttributionProperty, value);
    }

    /// <summary>
    /// Gets or sets the center latitude of the map (WGS84).
    /// </summary>
    public double CenterLatitude
    {
        get => (double)GetValue(CenterLatitudeProperty);
        set => SetValue(CenterLatitudeProperty, value);
    }

    /// <summary>
    /// Gets or sets the center longitude of the map (WGS84).
    /// </summary>
    public double CenterLongitude
    {
        get => (double)GetValue(CenterLongitudeProperty);
        set => SetValue(CenterLongitudeProperty, value);
    }

    /// <summary>
    /// Gets or sets the zoom level (1 = world, 18 = street level).
    /// </summary>
    public double ZoomLevel
    {
        get => (double)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    /// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(System.Collections.IEnumerable), typeof(MapView), null, 
            propertyChanged: OnItemsSourceChanged);

    /// <summary>Bindable property for <see cref="ItemTemplate"/>.</summary>
    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(MapView), null,
            propertyChanged: OnItemTemplateChanged);

    /// <summary>
    /// Gets or sets the collection of items to display as pins.
    /// </summary>
    public System.Collections.IEnumerable? ItemsSource
    {
        get => (System.Collections.IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the template to apply to each item in the <see cref="ItemsSource"/>.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MapView map)
        {
            map.UpdateItemsSource(oldValue as System.Collections.IEnumerable, newValue as System.Collections.IEnumerable);
        }
    }

    private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MapView map)
        {
            map.UpdatePinsFromSource();
        }
    }

    private void UpdateItemsSource(System.Collections.IEnumerable? oldSource, System.Collections.IEnumerable? newSource)
    {
        if (oldSource is INotifyCollectionChanged notifyOld)
        {
            notifyOld.CollectionChanged -= OnItemsSourceCollectionChanged;
        }

        if (newSource is INotifyCollectionChanged notifyNew)
        {
            notifyNew.CollectionChanged += OnItemsSourceCollectionChanged;
        }

        UpdatePinsFromSource();
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdatePinsFromSource();
    }

    private void UpdatePinsFromSource()
    {
        _pins.Clear();
        
        if (ItemsSource == null || ItemTemplate == null) return;

        foreach (var item in ItemsSource)
        {
            if (item == null) continue;
            var content = ItemTemplate.CreateContent();
            if (content is MapPin pin)
            {
                pin.BindingContext = item;
                _pins.Add(pin);
            }
        }
    }


    /// <summary>
    /// Gets the collection of pins on the map.
    /// </summary>
    public IList<MapPin> Pins => _pins;

    /// <summary>
    /// Gets the collection of map elements (polygons, polylines, circles).
    /// </summary>
    public IList<MapElement> MapElements => _mapElements;

    /// <summary>
    /// Occurs when the map is clicked.
    /// </summary>
    public event EventHandler<MapClickedEventArgs>? MapClicked;

    /// <summary>
    /// Centers the map on the specified coordinates.
    /// </summary>
    /// <param name="latitude">Latitude in WGS84.</param>
    /// <param name="longitude">Longitude in WGS84.</param>
    public void CenterTo(double latitude, double longitude)
    {
        Handler?.Invoke("CenterTo", (latitude, longitude));
    }

    /// <summary>
    /// Zooms the map to the specified level.
    /// </summary>
    /// <param name="zoomLevel">Zoom level (1-18).</param>
    public void ZoomTo(double zoomLevel)
    {
        Handler?.Invoke("ZoomTo", zoomLevel);
    }

    /// <summary>
    /// Raises the <see cref="MapClicked"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public void OnMapClicked(MapClickedEventArgs args)
    {
        MapClicked?.Invoke(this, args);
    }

    private void OnPinsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Handler?.Invoke("UpdatePins", null);
    }

    private void OnMapElementsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Handler?.Invoke("UpdateMapElements", null);
    }
}
