using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class ZIndexRenderTests : RenderTestBase
{
    /// <summary>
    /// Verifies that ZIndex correctly controls the visual stacking order of overlapping views.
    /// Three overlapping BoxViews are placed with ZIndex 0, 1, 2. The green box (ZIndex 2)
    /// should be fully visible on top, with blue (ZIndex 1) partially visible, and red (ZIndex 0) mostly hidden.
    /// </summary>
    [AvaloniaFact]
    public async Task Render_ZIndex_InitialOrder()
    {
        var layout = new AbsoluteLayout
        {
            BackgroundColor = Colors.White,
            WidthRequest = 200,
            HeightRequest = 200,
        };

        var red = new BoxView
        {
            Color = Colors.Red,
            WidthRequest = 120,
            HeightRequest = 120,
            ZIndex = 0,
        };

        var blue = new BoxView
        {
            Color = Colors.Blue,
            WidthRequest = 120,
            HeightRequest = 120,
            ZIndex = 1,
        };

        var green = new BoxView
        {
            Color = Colors.Green,
            WidthRequest = 120,
            HeightRequest = 120,
            ZIndex = 2,
        };

        layout.Children.Add(red);
        AbsoluteLayout.SetLayoutBounds(red, new Microsoft.Maui.Graphics.Rect(0, 0, 120, 120));
        layout.Children.Add(blue);
        AbsoluteLayout.SetLayoutBounds(blue, new Microsoft.Maui.Graphics.Rect(40, 40, 120, 120));
        layout.Children.Add(green);
        AbsoluteLayout.SetLayoutBounds(green, new Microsoft.Maui.Graphics.Rect(80, 80, 120, 120));

        await RenderToFile(layout);
        CompareImages();
    }

    /// <summary>
    /// Verifies that updating ZIndex at runtime correctly reorders the visual stacking.
    /// Red starts at ZIndex 0 but is changed to ZIndex 10, so it should render on top of blue and green.
    /// </summary>
    [AvaloniaFact]
    public async Task Render_ZIndex_UpdatedOrder()
    {
        var layout = new AbsoluteLayout
        {
            BackgroundColor = Colors.White,
            WidthRequest = 200,
            HeightRequest = 200,
        };

        var red = new BoxView
        {
            Color = Colors.Red,
            WidthRequest = 120,
            HeightRequest = 120,
            ZIndex = 0,
        };

        var blue = new BoxView
        {
            Color = Colors.Blue,
            WidthRequest = 120,
            HeightRequest = 120,
            ZIndex = 1,
        };

        var green = new BoxView
        {
            Color = Colors.Green,
            WidthRequest = 120,
            HeightRequest = 120,
            ZIndex = 2,
        };

        layout.Children.Add(red);
        AbsoluteLayout.SetLayoutBounds(red, new Microsoft.Maui.Graphics.Rect(0, 0, 120, 120));
        layout.Children.Add(blue);
        AbsoluteLayout.SetLayoutBounds(blue, new Microsoft.Maui.Graphics.Rect(40, 40, 120, 120));
        layout.Children.Add(green);
        AbsoluteLayout.SetLayoutBounds(green, new Microsoft.Maui.Graphics.Rect(80, 80, 120, 120));

        await RenderToFile(layout, handler =>
        {
            // Move red to the top of the stack
            red.ZIndex = 10;
        });
        CompareImages();
    }
}
