using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class TimePickerRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_TimePicker()
    {
        var control = new Microsoft.Maui.Controls.TimePicker 
        { 
            Time = new TimeSpan(14, 30, 0),
            TextColor = Colors.Purple,
            WidthRequest = 200
        };
        await RenderToFile(control);
        CompareImages();
    }
}
