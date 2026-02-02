using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class SwitchRenderTests : RenderTestBase
{
    [AvaloniaFact(Skip = "Fails to find template parts (PART_MovingKnobs) in headless mode with Avalonia 12 alpha. Probably a theme initialization issue in this version.")]
    public async Task Render_Switch()
    {
        var control = new Microsoft.Maui.Controls.Switch 
        { 
            IsToggled = true, 
            OnColor = Colors.Purple, 
            ThumbColor = Colors.White 
        };
        await RenderToFile(control);
        CompareImages();
    }
}
