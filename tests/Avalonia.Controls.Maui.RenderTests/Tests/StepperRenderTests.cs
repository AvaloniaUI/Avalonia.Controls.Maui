using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class StepperRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Stepper()
    {
        var control = new Microsoft.Maui.Controls.Stepper 
        { 
            Value = 5, 
            Maximum = 10, 
            Minimum = 0, 
            Increment = 1 
        };
        await RenderToFile(control);
        CompareImages();
    }
}
