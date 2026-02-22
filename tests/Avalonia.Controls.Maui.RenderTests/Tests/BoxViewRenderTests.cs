using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class BoxViewRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_BoxView()
    {
        var control = new BoxView
        {
            Color = Colors.Red,
            CornerRadius = new Microsoft.Maui.CornerRadius(10),
            WidthRequest = 100,
            HeightRequest = 100
        };
        await RenderToFile(control);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_BoxView_BackgroundColor()
    {
        var control = new BoxView
        {
            BackgroundColor = Colors.Blue,
            WidthRequest = 100,
            HeightRequest = 100
        };
        await RenderToFile(control);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_BoxView_GradientBackground()
    {
        var control = new BoxView
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new Microsoft.Maui.Graphics.Point(0, 0),
                EndPoint = new Microsoft.Maui.Graphics.Point(1, 1),
                GradientStops =
                {
                    new GradientStop(Colors.Red, 0.0f),
                    new GradientStop(Colors.Blue, 1.0f),
                }
            },
            WidthRequest = 100,
            HeightRequest = 100
        };
        await RenderToFile(control);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_BoxView_ColorOverridesBackground()
    {
        var control = new BoxView
        {
            Color = Colors.Green,
            BackgroundColor = Colors.Blue,
            WidthRequest = 100,
            HeightRequest = 100
        };
        await RenderToFile(control);
        CompareImages();
    }
}
