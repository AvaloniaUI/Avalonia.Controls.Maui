using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class PolylineRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Polyline()
    {
        var control = new Microsoft.Maui.Controls.Shapes.Polyline
        {
            Points = new Microsoft.Maui.Controls.PointCollection { new Microsoft.Maui.Graphics.Point(0,0), new Microsoft.Maui.Graphics.Point(50,50), new Microsoft.Maui.Graphics.Point(100,0) },
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Orange),
            StrokeThickness = 4,
            WidthRequest = 100,
            HeightRequest = 50
        };
        await RenderToFile(control);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Polyline_FillRule_EvenOdd()
    {
        var control = new Microsoft.Maui.Controls.Shapes.Polyline
        {
            Points = StarPoints(),
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Colors.LightBlue),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Colors.DarkBlue),
            StrokeThickness = 2,
            FillRule = Microsoft.Maui.Controls.Shapes.FillRule.EvenOdd,
            WidthRequest = 120,
            HeightRequest = 120
        };
        await RenderToFile(control);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Polyline_FillRule_NonZero()
    {
        var control = new Microsoft.Maui.Controls.Shapes.Polyline
        {
            Points = StarPoints(),
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Colors.LightBlue),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Colors.DarkBlue),
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
