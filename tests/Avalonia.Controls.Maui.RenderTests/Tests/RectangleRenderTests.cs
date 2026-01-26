using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class RectangleRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Rectangle()
    {
        var control = new Microsoft.Maui.Controls.Shapes.Rectangle 
        { 
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Green),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Black),
            StrokeThickness = 2,
            WidthRequest = 80,
            HeightRequest = 80
        };
        await RenderToFile(control);
        CompareImages();
    }
}
