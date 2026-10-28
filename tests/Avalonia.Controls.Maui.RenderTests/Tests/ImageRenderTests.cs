using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using MauiPoint = Microsoft.Maui.Graphics.Point;

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

    [AvaloniaFact]
    public async Task Render_Image_WithEllipseClip()
    {
        var imagePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Resources", "Images", "redbug.png");
        var layout = new Microsoft.Maui.Controls.Grid
        {
            WidthRequest = 100,
            HeightRequest = 100,
            BackgroundColor = Colors.DeepSkyBlue
        };

        var control = new Microsoft.Maui.Controls.Image
        {
            Source = imagePath,
            WidthRequest = 100,
            HeightRequest = 100,
            Aspect = Microsoft.Maui.Aspect.AspectFill,
            Clip = new EllipseGeometry
            {
                Center = new MauiPoint(50, 50),
                RadiusX = 50,
                RadiusY = 50
            }
        };

        layout.Add(control);

        await RenderToFile(layout);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Image_WithRoundRectangleClip()
    {
        var imagePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Resources", "Images", "redbug.png");
        var layout = new Microsoft.Maui.Controls.Grid
        {
            WidthRequest = 100,
            HeightRequest = 100,
            BackgroundColor = Colors.DeepSkyBlue
        };

        var control = new Microsoft.Maui.Controls.Image
        {
            Source = imagePath,
            WidthRequest = 100,
            HeightRequest = 100,
            Aspect = Microsoft.Maui.Aspect.AspectFill,
            Clip = new RoundRectangleGeometry
            {
                CornerRadius = new Microsoft.Maui.CornerRadius(24)
            }
        };

        layout.Add(control);

        await RenderToFile(layout);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Image_FontImageSource_Sized()
    {
        // The image's background shows its bounds; the glyph is fitted into them
        var control = new Microsoft.Maui.Controls.Image
        {
            Source = new Microsoft.Maui.Controls.FontImageSource { Glyph = "M", Size = 30, Color = Colors.Black },
            WidthRequest = 60,
            HeightRequest = 60,
            Aspect = Microsoft.Maui.Aspect.AspectFit,
            BackgroundColor = Colors.LightGray
        };

        await RenderToFile(control);
        // The glyph is drawn into a bitmap of its own, which is hinted on Linux and not on macOS
        CompareImages(tolerance: 0.03);
    }
}
