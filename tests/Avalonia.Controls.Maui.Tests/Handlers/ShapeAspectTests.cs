using Avalonia.Controls.Maui.Handlers.Shapes;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using AvaloniaShape = global::Avalonia.Controls.Shapes.Shape;
using MauiPath = Microsoft.Maui.Controls.Shapes.Path;
using MauiPoint = Microsoft.Maui.Graphics.Point;
using MauiPolygon = Microsoft.Maui.Controls.Shapes.Polygon;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ShapeAspectTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "Uniform Path Is Centered In Its Bounds")]
    public async Task UniformPathIsCenteredInItsBounds()
    {
        var path = CreatePath("M0,0 H20 V10 H0 Z", Stretch.Uniform, size: 16);

        var bounds = await GetRenderedBoundsAsync<PathHandler>(path);

        AssertRect(new Rect(0, 4, 16, 8), bounds);
    }

    [AvaloniaFact(DisplayName = "Stroked Uniform Path Keeps Its Stroke Inside Its Bounds")]
    public async Task StrokedUniformPathKeepsItsStrokeInsideItsBounds()
    {
        var path = CreatePath("M0,0 H20 V10 H0 Z", Stretch.Uniform, size: 16);
        path.Stroke = Brush.Black;
        path.StrokeThickness = 2;

        var bounds = await GetRenderedBoundsAsync<PathHandler>(path);

        AssertRect(new Rect(1, 4.5, 14, 7), bounds);
    }

    [AvaloniaFact(DisplayName = "UniformToFill Path Is Placed At The Top Left")]
    public async Task UniformToFillPathIsPlacedAtTheTopLeft()
    {
        var path = CreatePath("M0,0 H20 V10 H0 Z", Stretch.UniformToFill, size: 16);

        var bounds = await GetRenderedBoundsAsync<PathHandler>(path);

        AssertRect(new Rect(0, 0, 32, 16), bounds);
    }

    [AvaloniaFact(DisplayName = "Unstretched Path Is Moved Inside Its Bounds")]
    public async Task UnstretchedPathIsMovedInsideItsBounds()
    {
        var path = CreatePath("M-5,-5 L10,10", Stretch.None, size: 20);

        var bounds = await GetRenderedBoundsAsync<PathHandler>(path);

        AssertRect(new Rect(0, 0, 15, 15), bounds);
    }

    [AvaloniaFact(DisplayName = "Uniform Polygon Is Centered In Its Bounds")]
    public async Task UniformPolygonIsCenteredInItsBounds()
    {
        var polygon = new MauiPolygon
        {
            Points = [new MauiPoint(0, 0), new MauiPoint(20, 0), new MauiPoint(20, 10)],
            Aspect = Stretch.Uniform,
            Fill = Brush.Black,
            WidthRequest = 16,
            HeightRequest = 16,
        };

        var bounds = await GetRenderedBoundsAsync<PolygonHandler>(polygon);

        AssertRect(new Rect(0, 4, 16, 8), bounds);
    }

    private static MauiPath CreatePath(string data, Stretch aspect, double size) => new()
    {
        Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(data)!,
        Aspect = aspect,
        Fill = Brush.Black,
        WidthRequest = size,
        HeightRequest = size,
    };

    private async Task<Avalonia.Rect> GetRenderedBoundsAsync<THandler>(Shape shape)
        where THandler : Microsoft.Maui.IElementHandler, new()
    {
        var handler = await CreateHandlerAsync<THandler>(shape);
        var platformView = Assert.IsAssignableFrom<AvaloniaShape>(handler.PlatformView);
        ShowInWindow(platformView);
        return platformView.RenderedGeometry!.Bounds;
    }

    private static void AssertRect(Rect expected, Avalonia.Rect actual)
    {
        Assert.Equal(expected.X, actual.X, 3);
        Assert.Equal(expected.Y, actual.Y, 3);
        Assert.Equal(expected.Width, actual.Width, 3);
        Assert.Equal(expected.Height, actual.Height, 3);
    }
}
