using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class MapStub : StubBase, IMap
{
    public MapType MapType { get; set; } = MapType.Street;

    public bool IsShowingUser { get; set; }

    public bool IsScrollEnabled { get; set; } = true;

    public bool IsZoomEnabled { get; set; } = true;

    public bool IsTrafficEnabled { get; set; }

    public MapSpan? VisibleRegion { get; set; }

    public IList<IMapPin> Pins { get; set; } = new ObservableCollection<IMapPin>();

    public IList<IMapElement> Elements { get; set; } = new ObservableCollection<IMapElement>();

    public Microsoft.Maui.Devices.Sensors.Location? LastClickedLocation { get; private set; }

    public MapSpan? LastMoveToRegion { get; private set; }

    void IMap.Clicked(Microsoft.Maui.Devices.Sensors.Location position)
    {
        LastClickedLocation = position;
    }

    public void MoveToRegion(MapSpan region)
    {
        LastMoveToRegion = region;
    }
}
