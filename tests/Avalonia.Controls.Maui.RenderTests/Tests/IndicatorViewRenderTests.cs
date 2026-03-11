using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class IndicatorViewRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_IndicatorView_Default()
    {
        var control = new IndicatorView
        {
            Count = 5,
            Position = 0,
            WidthRequest = 200,
            HeightRequest = 40
        };
        await RenderToFile(control);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_IndicatorView_MiddleSelected()
    {
        var control = new IndicatorView
        {
            Count = 5,
            Position = 2,
            WidthRequest = 200,
            HeightRequest = 40
        };
        await RenderToFile(control);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_IndicatorView_CustomColors()
    {
        var control = new IndicatorView
        {
            Count = 5,
            Position = 1,
            IndicatorColor = Colors.LightGray,
            SelectedIndicatorColor = Colors.Blue,
            WidthRequest = 200,
            HeightRequest = 40
        };
        await RenderToFile(control);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_IndicatorView_MaximumVisible()
    {
        var control = new IndicatorView
        {
            Count = 10,
            Position = 0,
            MaximumVisible = 3,
            WidthRequest = 200,
            HeightRequest = 40
        };
        await RenderToFile(control);
        CompareImages();
    }
}
