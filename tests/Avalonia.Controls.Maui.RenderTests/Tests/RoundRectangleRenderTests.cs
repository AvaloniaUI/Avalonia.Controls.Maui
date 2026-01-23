using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class RoundRectangleRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_RoundRectangle()
    {
        var control = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        {
            CornerRadius = new Microsoft.Maui.CornerRadius(10),
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Colors.Pink),
            WidthRequest = 100,
            HeightRequest = 60
        };
        await RenderToFile(control);
        CompareImages();
    }
}
