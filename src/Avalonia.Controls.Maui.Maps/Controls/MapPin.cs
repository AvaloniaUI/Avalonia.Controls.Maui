using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Maps.Controls;

/// <summary>
/// Represents a marker pin on a map.
/// </summary>
public class MapPin : BindableObject
{
    /// <summary>
    /// Bindable property for <see cref="Address"/>.
    /// </summary>
    public static readonly BindableProperty AddressProperty =
        BindableProperty.Create(nameof(Address), typeof(string), typeof(MapPin), string.Empty);

    /// <summary>
    /// Bindable property for <see cref="Label"/>.
    /// </summary>
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(MapPin), string.Empty);

    /// <summary>
    /// Bindable property for <see cref="Location"/>.
    /// </summary>
    public static readonly BindableProperty LocationProperty =
        BindableProperty.Create(nameof(Location), typeof(Microsoft.Maui.Devices.Sensors.Location), typeof(MapPin), null);

    /// <summary>
    /// Bindable property for <see cref="Type"/>.
    /// </summary>
    public static readonly BindableProperty TypeProperty =
        BindableProperty.Create(nameof(Type), typeof(PinType), typeof(MapPin), PinType.Generic);

    /// <summary>
    /// Gets or sets the address for the pin location.
    /// </summary>
    public string Address
    {
        get => (string)GetValue(AddressProperty);
        set => SetValue(AddressProperty, value);
    }

    /// <summary>
    /// Gets or sets the label (title) for the pin.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Gets or sets the location (latitude/longitude) of the pin.
    /// </summary>
    public Microsoft.Maui.Devices.Sensors.Location? Location
    {
        get => (Microsoft.Maui.Devices.Sensors.Location?)GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }

    /// <summary>
    /// Bindable property for <see cref="Icon"/>.
    /// </summary>
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(MapPin), null);

    /// <summary>
    /// Bindable property for <see cref="IconScale"/>.
    /// </summary>
    public static readonly BindableProperty IconScaleProperty =
        BindableProperty.Create(nameof(IconScale), typeof(double), typeof(MapPin), 1.0);

    /// <summary>
    /// Gets or sets the custom icon for the pin.
    /// </summary>
    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the scale of the icon. Default is 1.0.
    /// </summary>
    public double IconScale
    {
        get => (double)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of pin.
    /// </summary>
    public PinType Type
    {
        get => (PinType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>
    /// Occurs when the pin marker is clicked.
    /// </summary>
    public event EventHandler<PinClickedEventArgs>? MarkerClicked;

    /// <summary>
    /// Occurs when the info window is clicked.
    /// </summary>
    public event EventHandler<PinClickedEventArgs>? InfoWindowClicked;

    /// <summary>
    /// Raises the <see cref="MarkerClicked"/> event.
    /// </summary>
    internal void OnMarkerClicked(PinClickedEventArgs args)
    {
        MarkerClicked?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the <see cref="InfoWindowClicked"/> event.
    /// </summary>
    internal void OnInfoWindowClicked(PinClickedEventArgs args)
    {
        InfoWindowClicked?.Invoke(this, args);
    }
}

/// <summary>
/// Specifies the type of pin.
/// </summary>
public enum PinType
{
    /// <summary>A generic pin.</summary>
    Generic,
    /// <summary>A pin representing a place.</summary>
    Place,
    /// <summary>A pin representing a saved location.</summary>
    SavedPin,
    /// <summary>A pin representing a search result.</summary>
    SearchResult
}

/// <summary>
/// Event arguments for pin click events.
/// </summary>
public class PinClickedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets whether to hide the info window.
    /// </summary>
    public bool HideInfoWindow { get; set; }
}
