using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class SliderRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Slider()
    {
        var control = new Microsoft.Maui.Controls.Slider 
        { 
            Value = 0.5, 
            MinimumTrackColor = Colors.Blue, 
            MaximumTrackColor = Colors.Gray, 
            ThumbColor = Colors.Red,
            WidthRequest = 200
        };
        await RenderToFile(control);
        CompareImages();
    }
}
