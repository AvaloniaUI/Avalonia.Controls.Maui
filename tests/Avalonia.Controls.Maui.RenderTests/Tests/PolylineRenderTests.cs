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
}
