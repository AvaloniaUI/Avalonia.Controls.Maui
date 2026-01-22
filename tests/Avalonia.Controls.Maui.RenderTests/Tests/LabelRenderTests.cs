using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class LabelRenderTests : RenderTestBase
{
    [AvaloniaFact(Skip = "Run locally to verify")]
    public async Task Render_Label()
    {
        var control = new Microsoft.Maui.Controls.Label 
        { 
            Text = "Hello World", 
            TextColor = Colors.Black,
            BackgroundColor = Colors.LightGray,
            FontSize = 24,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
            VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center,
            WidthRequest = 200,
            HeightRequest = 100
        };
        await RenderToFile(control);
        CompareImages();
    }
}
