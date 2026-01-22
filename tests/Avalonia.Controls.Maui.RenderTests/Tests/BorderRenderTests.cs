using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class BorderRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Border()
    {
        var control = new Microsoft.Maui.Controls.Border 
        { 
            Stroke = Colors.Red, 
            StrokeThickness = 4,
            BackgroundColor = Colors.Yellow,
            Content = new Microsoft.Maui.Controls.Label { Text = "Inside Border", VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center, HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center },
            Padding = new Microsoft.Maui.Thickness(10),
            WidthRequest = 150,
            HeightRequest = 100
        };
        await RenderToFile(control);
        // Text rendering differs slightly between platforms
        CompareImages(tolerance: 0.05);
    }
}
