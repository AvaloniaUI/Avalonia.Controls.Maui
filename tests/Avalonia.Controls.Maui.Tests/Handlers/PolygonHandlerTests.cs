using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using PolygonStub = Avalonia.Controls.Maui.Tests.Stubs.PolygonStub;
using MauiPolygonHandler = Avalonia.Controls.Maui.Tests.Stubs.PolygonStubHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class PolygonHandlerTests : HandlerTestBase<MauiPolygonHandler, PolygonStub>
{
    [AvaloniaFact(DisplayName = "Polygon Points Map Correctly")]
    public async Task PolygonPointsMapCorrectly()
    {
        var polygon = new PolygonStub
        {
            Points = new PointCollection
            {
                new Microsoft.Maui.Graphics.Point(0, 0),
                new Microsoft.Maui.Graphics.Point(20, 0),
                new Microsoft.Maui.Graphics.Point(10, 30)
            }
        };

        var handler = await CreateHandlerAsync(polygon);

        Assert.Equal(polygon.Points?.Count, handler.PlatformView.Points.Count);
    }
}
