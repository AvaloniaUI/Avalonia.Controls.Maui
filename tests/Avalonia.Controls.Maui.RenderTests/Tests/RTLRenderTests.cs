using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class RTLRenderTests : RenderTestBase
{
    /// <summary>
    /// Verifies that a HorizontalStackLayout with FlowDirection.RightToLeft
    /// lays out children from right to left: Red on the right, Green in the middle, Blue on the left.
    /// </summary>
    [AvaloniaFact]
    public async Task Render_HorizontalStackLayout_RTL()
    {
        var layout = new HorizontalStackLayout
        {
            BackgroundColor = Colors.White,
            FlowDirection = Microsoft.Maui.FlowDirection.RightToLeft,
            WidthRequest = 200,
            HeightRequest = 60,
            Children =
            {
                new BoxView { Color = Colors.Red, HeightRequest = 40, WidthRequest = 40 },
                new BoxView { Color = Colors.Green, HeightRequest = 40, WidthRequest = 40 },
                new BoxView { Color = Colors.Blue, HeightRequest = 40, WidthRequest = 40 },
            }
        };

        await RenderToFile(layout);
        CompareImages();
    }

    /// <summary>
    /// Verifies that a HorizontalStackLayout with default LTR FlowDirection
    /// lays out children from left to right: Red on the left, Green in the middle, Blue on the right.
    /// This serves as a baseline comparison for the RTL test.
    /// </summary>
    [AvaloniaFact]
    public async Task Render_HorizontalStackLayout_LTR()
    {
        var layout = new HorizontalStackLayout
        {
            BackgroundColor = Colors.White,
            FlowDirection = Microsoft.Maui.FlowDirection.LeftToRight,
            WidthRequest = 200,
            HeightRequest = 60,
            Children =
            {
                new BoxView { Color = Colors.Red, HeightRequest = 40, WidthRequest = 40 },
                new BoxView { Color = Colors.Green, HeightRequest = 40, WidthRequest = 40 },
                new BoxView { Color = Colors.Blue, HeightRequest = 40, WidthRequest = 40 },
            }
        };

        await RenderToFile(layout);
        CompareImages();
    }

    /// <summary>
    /// Verifies that a Label with HorizontalTextAlignment.Start in an RTL context
    /// aligns text to the right side (since Start means right in RTL).
    /// </summary>
    [AvaloniaFact]
    public async Task Render_Label_TextAlignStart_RTL()
    {
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = "Hello",
            TextColor = Colors.Black,
            BackgroundColor = Colors.LightGray,
            FontSize = 20,
            FlowDirection = Microsoft.Maui.FlowDirection.RightToLeft,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Start,
            WidthRequest = 200,
            HeightRequest = 50,
        };

        await RenderToFile(label);
        CompareImages(tolerance: 0.07);
    }

    /// <summary>
    /// Verifies that a Label with HorizontalTextAlignment.End in an RTL context
    /// aligns text to the left side (since End means left in RTL).
    /// </summary>
    [AvaloniaFact]
    public async Task Render_Label_TextAlignEnd_RTL()
    {
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = "Hello",
            TextColor = Colors.Black,
            BackgroundColor = Colors.LightGray,
            FontSize = 20,
            FlowDirection = Microsoft.Maui.FlowDirection.RightToLeft,
            HorizontalTextAlignment = Microsoft.Maui.TextAlignment.End,
            WidthRequest = 200,
            HeightRequest = 50,
        };

        await RenderToFile(label);
        CompareImages(tolerance: 0.07);
    }

    /// <summary>
    /// Verifies that VerticalStackLayout with RTL and child items using HorizontalOptions.Start
    /// positions children on the right side (since Start means right in RTL).
    /// </summary>
    [AvaloniaFact]
    public async Task Render_VerticalStackLayout_RTL_ChildAlignment()
    {
        var layout = new VerticalStackLayout
        {
            BackgroundColor = Colors.White,
            FlowDirection = Microsoft.Maui.FlowDirection.RightToLeft,
            WidthRequest = 200,
            HeightRequest = 120,
            Children =
            {
                new BoxView
                {
                    Color = Colors.Red,
                    HeightRequest = 40,
                    WidthRequest = 80,
                    HorizontalOptions = LayoutOptions.Start,
                },
                new BoxView
                {
                    Color = Colors.Blue,
                    HeightRequest = 40,
                    WidthRequest = 80,
                    HorizontalOptions = LayoutOptions.End,
                },
            }
        };

        await RenderToFile(layout);
        CompareImages();
    }
}
