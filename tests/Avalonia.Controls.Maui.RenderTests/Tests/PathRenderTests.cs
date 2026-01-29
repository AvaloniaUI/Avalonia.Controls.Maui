using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class PathRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Path()
    {
        // Simple triangle path
        var control = new Microsoft.Maui.Controls.Shapes.Path
        {
            Data = (Microsoft.Maui.Controls.Shapes.Geometry)new Microsoft.Maui.Controls.Shapes.PathGeometryConverter().ConvertFromInvariantString("M 10,10 L 90,10 L 50,90 Z")!,
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Lime),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Black),
            StrokeThickness = 2,
            WidthRequest = 100,
            HeightRequest = 100
        };
        await RenderToFile(control);
        CompareImages();
    }
}
