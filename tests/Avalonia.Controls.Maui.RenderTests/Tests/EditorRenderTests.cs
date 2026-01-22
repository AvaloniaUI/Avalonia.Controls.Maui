using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class EditorRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Editor()
    {
        var control = new Microsoft.Maui.Controls.Editor 
        { 
            Text = "Multi-line\nEditor Content", 
            TextColor = Colors.DarkBlue,
            Placeholder = "Enter text here",
            HeightRequest = 100,
            WidthRequest = 200
        };
        await RenderToFile(control);
        // Text rendering differs slightly between platforms
        CompareImages(tolerance: 0.06);
    }
}
