using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class ScrollViewRenderTests : RenderTestBase
{
    [AvaloniaFact(Skip = "Run locally to verify")]
    public async Task Render_ScrollView()
    {
        var stack = new Microsoft.Maui.Controls.VerticalStackLayout();
        for (int i = 0; i < 5; i++)
        {
            stack.Children.Add(new Microsoft.Maui.Controls.Label { Text = $"Item {i}", HeightRequest = 30, BackgroundColor = i % 2 == 0 ? Colors.LightGray : Colors.White });
        }

        var control = new Microsoft.Maui.Controls.ScrollView 
        { 
            Content = stack,
            WidthRequest = 200,
            HeightRequest = 100,
            BackgroundColor = Colors.AliceBlue
        };
        await RenderToFile(control);
        CompareImages();
    }
}
