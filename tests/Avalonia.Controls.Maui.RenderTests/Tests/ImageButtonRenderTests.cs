using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class ImageButtonRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_ImageButton_WithSource()
    {
        var imagePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Resources", "Images", "redbug.png");
        var control = new Microsoft.Maui.Controls.ImageButton 
        { 
            Source = imagePath,
            BackgroundColor = Colors.LightBlue,
            BorderColor = Colors.Blue,
            BorderWidth = 2,
            CornerRadius = 10,
            WidthRequest = 100,
            HeightRequest = 50,
            Padding = new Microsoft.Maui.Thickness(5)
        };
        await RenderToFile(control);
        CompareImages();
    }
}
