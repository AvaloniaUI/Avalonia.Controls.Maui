using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class PolygonRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Polygon()
    {
        var control = new Microsoft.Maui.Controls.Shapes.Polygon
        {
            Points = new Microsoft.Maui.Controls.PointCollection { new Microsoft.Maui.Graphics.Point(0,50), new Microsoft.Maui.Graphics.Point(50,0), new Microsoft.Maui.Graphics.Point(100,50) },
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Cyan),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Colors.DarkBlue),
            StrokeThickness = 2,
            WidthRequest = 100,
            HeightRequest = 50
        };
        await RenderToFile(control);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Polygon_FillRule_EvenOdd()
    {
        var control = new Microsoft.Maui.Controls.Shapes.Polygon
        {
            Points = StarPoints(),
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Gold),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Black),
            StrokeThickness = 2,
            FillRule = Microsoft.Maui.Controls.Shapes.FillRule.EvenOdd,
            WidthRequest = 120,
            HeightRequest = 120
        };
        await RenderToFile(control);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Polygon_FillRule_NonZero()
    {
        var control = new Microsoft.Maui.Controls.Shapes.Polygon
        {
            Points = StarPoints(),
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Gold),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Black),
            StrokeThickness = 2,
            FillRule = Microsoft.Maui.Controls.Shapes.FillRule.Nonzero,
            WidthRequest = 120,
            HeightRequest = 120
        };
        await RenderToFile(control);
        CompareImages();
    }

    private static Microsoft.Maui.Controls.PointCollection StarPoints()
    {
        // Five-pointed star (self-intersecting) — EvenOdd leaves center hollow, NonZero fills it
        return new Microsoft.Maui.Controls.PointCollection
        {
            new Microsoft.Maui.Graphics.Point(60, 5),
            new Microsoft.Maui.Graphics.Point(25, 110),
            new Microsoft.Maui.Graphics.Point(110, 40),
            new Microsoft.Maui.Graphics.Point(10, 40),
            new Microsoft.Maui.Graphics.Point(95, 110)
        };
    }
}
