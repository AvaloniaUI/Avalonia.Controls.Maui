using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Color = Microsoft.Maui.Graphics.Color;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class ButtonRenderTests : RenderTestBase
{
    [AvaloniaFact(Skip = "Run locally to verify")]
    public async Task Render_Button_Default()
    {
        var button = new Microsoft.Maui.Controls.Button
        {
            Text = "Click Me",
            BackgroundColor = Color.FromRgb(0, 120, 215),
            TextColor = Color.FromRgb(255, 255, 255),
            WidthRequest = 120,
            HeightRequest = 40
        };

        await RenderToFile(button);
        CompareImages();
    }
}


