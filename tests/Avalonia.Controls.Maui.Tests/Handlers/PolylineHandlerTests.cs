using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using PolylineStub = Avalonia.Controls.Maui.Tests.Stubs.PolylineStub;
using MauiPolylineHandler = Avalonia.Controls.Maui.Tests.Stubs.PolylineStubHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class PolylineHandlerTests : HandlerTestBase<MauiPolylineHandler, PolylineStub>
{
    [AvaloniaFact(DisplayName = "Polyline Points Map Correctly")]
    public async Task PolylinePointsMapCorrectly()
    {
        var polyline = new PolylineStub
        {
            Points = new PointCollection
            {
                new Microsoft.Maui.Graphics.Point(0, 0),
                new Microsoft.Maui.Graphics.Point(10, 20),
                new Microsoft.Maui.Graphics.Point(20, 10)
            }
        };

        var handler = await CreateHandlerAsync(polyline);

        Assert.Equal(polyline.Points?.Count, handler.PlatformView.Points.Count);
    }
}
