using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class LayoutMarginRenderTests : RenderTestBase
{
    /// <summary>
    /// Verifies that margin on BoxViews inside a VerticalStackLayout is not double-counted.
    /// Three colored BoxViews with specific top margins should be spaced correctly.
    /// If margin is double-counted, the gaps between boxes will be twice as large.
    /// </summary>
    [AvaloniaFact]
    public async Task Render_VerticalStackLayout_BoxView_Margins()
    {
        var layout = new VerticalStackLayout
        {
            BackgroundColor = Colors.White,
            WidthRequest = 200,
            HeightRequest = 200,
            Children =
            {
                new BoxView { Color = Colors.Red, HeightRequest = 40, WidthRequest = 200 },
                new BoxView { Color = Colors.Green, HeightRequest = 40, WidthRequest = 200, Margin = new Microsoft.Maui.Thickness(0, 20, 0, 0) },
                new BoxView { Color = Colors.Blue, HeightRequest = 40, WidthRequest = 200, Margin = new Microsoft.Maui.Thickness(0, 20, 0, 0) },
            }
        };

        await RenderToFile(layout);
        CompareImages();
    }

    /// <summary>
    /// Verifies that symmetric margin on a BoxView inside a VerticalStackLayout
    /// creates equal spacing on all sides.
    /// </summary>
    [AvaloniaFact]
    public async Task Render_VerticalStackLayout_BoxView_UniformMargin()
    {
        var layout = new VerticalStackLayout
        {
            BackgroundColor = Colors.LightGray,
            WidthRequest = 200,
            HeightRequest = 100,
            Children =
            {
                new BoxView { Color = Colors.Red, HeightRequest = 40, WidthRequest = 160, Margin = new Microsoft.Maui.Thickness(20) },
            }
        };

        await RenderToFile(layout);
        CompareImages();
    }

    /// <summary>
    /// Verifies that content with margin inside a Border is properly inset.
    /// The red BoxView should be inset 20px from the yellow border.
    /// </summary>
    [AvaloniaFact]
    public async Task Render_Border_ContentWithMargin()
    {
        var control = new Microsoft.Maui.Controls.Border
        {
            BackgroundColor = Colors.Yellow,
            StrokeThickness = 0,
            WidthRequest = 200,
            HeightRequest = 200,
            Content = new BoxView
            {
                Color = Colors.Red,
                Margin = new Microsoft.Maui.Thickness(20),
            }
        };

        await RenderToFile(control);
        CompareImages();
    }

    /// <summary>
    /// Verifies that multiple BoxViews with asymmetric margins in a HorizontalStackLayout
    /// are spaced correctly (margin not double-counted).
    /// </summary>
    [AvaloniaFact]
    public async Task Render_HorizontalStackLayout_BoxView_Margins()
    {
        var layout = new HorizontalStackLayout
        {
            BackgroundColor = Colors.White,
            WidthRequest = 250,
            HeightRequest = 60,
            Children =
            {
                new BoxView { Color = Colors.Red, HeightRequest = 40, WidthRequest = 40 },
                new BoxView { Color = Colors.Green, HeightRequest = 40, WidthRequest = 40, Margin = new Microsoft.Maui.Thickness(20, 0, 0, 0) },
                new BoxView { Color = Colors.Blue, HeightRequest = 40, WidthRequest = 40, Margin = new Microsoft.Maui.Thickness(20, 0, 0, 0) },
            }
        };

        await RenderToFile(layout);
        CompareImages();
    }
}
