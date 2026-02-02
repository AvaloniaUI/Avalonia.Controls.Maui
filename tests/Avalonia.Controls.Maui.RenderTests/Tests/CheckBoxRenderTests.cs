using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class CheckBoxRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_CheckBox()
    {
        var control = new Microsoft.Maui.Controls.CheckBox 
        { 
            IsChecked = true, 
            Color = Colors.Green 
        };
        await RenderToFile(control);
        CompareImages();
    }
}
