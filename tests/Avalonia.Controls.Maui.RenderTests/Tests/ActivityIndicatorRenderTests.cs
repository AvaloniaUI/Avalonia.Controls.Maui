using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class ActivityIndicatorRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_ActivityIndicator()
    {
        var control = new Microsoft.Maui.Controls.ActivityIndicator 
        { 
            IsRunning = true, 
            Color = Colors.Blue,
            WidthRequest = 50,
            HeightRequest = 50
        };
        await RenderToFile(control, handler => 
        {
            if (handler.PlatformView is ProgressRing ring)
            {
                ring.IsIndeterminate = false;
                ring.Value = 60;
            }
        });
        CompareImages();
    }
}
