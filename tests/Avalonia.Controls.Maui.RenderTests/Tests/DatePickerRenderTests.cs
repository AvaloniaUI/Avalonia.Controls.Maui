using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class DatePickerRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_DatePicker()
    {
        var control = new Microsoft.Maui.Controls.DatePicker 
        { 
            Date = new DateTime(2023, 10, 15),
            Format = "yyyy-MM-dd",
            TextColor = Colors.Green,
            WidthRequest = 300
        };
        await RenderToFile(control);
        // Text rendering differs slightly between platforms
        CompareImages(tolerance: 0.085);
    }
}
