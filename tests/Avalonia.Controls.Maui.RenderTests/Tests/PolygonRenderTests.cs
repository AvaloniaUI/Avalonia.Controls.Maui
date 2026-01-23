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
}
