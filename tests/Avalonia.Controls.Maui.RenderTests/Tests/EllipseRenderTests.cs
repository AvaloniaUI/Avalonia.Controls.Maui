using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class EllipseRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Ellipse()
    {
        var control = new Microsoft.Maui.Controls.Shapes.Ellipse 
        { 
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Red),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Blue),
            StrokeThickness = 4,
            WidthRequest = 100,
            HeightRequest = 50
        };
        await RenderToFile(control);
        CompareImages();
    }
}
