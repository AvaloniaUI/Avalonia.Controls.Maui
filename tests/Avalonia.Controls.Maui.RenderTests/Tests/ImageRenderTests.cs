using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class ImageRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Image_WithSource()
    {
        var imagePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Resources", "Images", "redbug.png");
        var control = new Microsoft.Maui.Controls.Image 
        { 
            Source = imagePath,
            WidthRequest = 100,
            HeightRequest = 100
        };
        await RenderToFile(control);
        CompareImages();
    }
}
