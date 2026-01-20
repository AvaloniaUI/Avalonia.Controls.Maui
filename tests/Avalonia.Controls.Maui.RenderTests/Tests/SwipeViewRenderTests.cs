using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class SwipeViewRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_SwipeView()
    {
        var control = new Microsoft.Maui.Controls.SwipeView
        {
            Content = new Microsoft.Maui.Controls.Grid { BackgroundColor = Colors.LightGreen, WidthRequest = 200, HeightRequest = 50, Children = { new Microsoft.Maui.Controls.Label { Text = "Swipe Me", HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center, VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center } } },
            RightItems = new Microsoft.Maui.Controls.SwipeItems { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete", BackgroundColor = Colors.Red } }
        };
        
        // SwipeItems are usually hidden effectively validating content rendering
        await RenderToFile(control);
        CompareImages();
    }
}
