using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class LineRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Line()
    {
        var control = new Microsoft.Maui.Controls.Shapes.Line 
        { 
            X1 = 0, Y1 = 0,
            X2 = 100, Y2 = 50,
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Purple),
            StrokeThickness = 5,
            WidthRequest = 100,
            HeightRequest = 50
        };
        await RenderToFile(control);
        CompareImages();
    }
}
