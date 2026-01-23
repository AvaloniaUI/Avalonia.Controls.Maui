using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui;
using Avalonia.Controls.Maui.Maps;
using Avalonia.Controls.Maui.Maps.Handlers;
using Avalonia.Controls.Maui.Maps.Controls;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class MapStub : StubBase, IMapView
{
    public MapType MapType { get; set; } = MapType.Street;

    public bool IsShowingUser { get; set; }

    public bool IsScrollEnabled { get; set; } = true;

    public bool IsZoomEnabled { get; set; } = true;

    public bool IsRotationEnabled { get; set; } = true;

    public bool ShowAttribution { get; set; } = true;

    public double CenterLatitude { get; set; }

    public double CenterLongitude { get; set; }

    public double ZoomLevel { get; set; } = 1;

    public IList<MapPin> Pins { get; set; } = new ObservableCollection<MapPin>();

    public IList<MapElement> MapElements { get; set; } = new ObservableCollection<MapElement>();

    public event EventHandler<MapClickedEventArgs>? MapClicked;

    public void OnMapClicked(MapClickedEventArgs args)
    {
        MapClicked?.Invoke(this, args);
    }
}
