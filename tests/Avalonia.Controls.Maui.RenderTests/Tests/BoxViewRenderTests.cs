using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class BoxViewRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_BoxView()
    {
        var control = new Microsoft.Maui.Controls.BoxView 
        { 
            Color = Colors.Red, 
            CornerRadius = new Microsoft.Maui.CornerRadius(10),
            WidthRequest = 100,
            HeightRequest = 100
        };
        await RenderToFile(control);
        CompareImages();
    }
}
