using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class ProgressBarRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_ProgressBar()
    {
        var control = new Microsoft.Maui.Controls.ProgressBar 
        { 
            Progress = 0.5, 
            ProgressColor = Colors.Orange,
            WidthRequest = 200
        };
        await RenderToFile(control);
        CompareImages();
    }
}
