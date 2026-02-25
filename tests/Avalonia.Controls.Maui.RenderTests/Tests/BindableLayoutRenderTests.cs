using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class BindableLayoutRenderTests : RenderTestBase
{
    /// <summary>
    /// Verifies that when a layout's children are cleared and repopulated,
    /// the old Avalonia controls are fully removed and only the new children render.
    /// This simulates the BindableLayout reset scenario where ObservableCollection.Clear()
    /// is called followed by adding new items.
    /// </summary>
    [AvaloniaFact]
    public async Task Render_VerticalStackLayout_ClearAndRepopulate()
    {
        var layout = new VerticalStackLayout
        {
            BackgroundColor = Colors.White,
            WidthRequest = 200,
            HeightRequest = 200,
            Children =
            {
                new BoxView { Color = Colors.Red, HeightRequest = 50, WidthRequest = 200 },
                new BoxView { Color = Colors.Green, HeightRequest = 50, WidthRequest = 200 },
            }
        };

        await RenderToFile(layout, handler =>
        {
            // Simulate BindableLayout reset: clear all children and add new ones
            layout.Clear();
            layout.Children.Add(new BoxView { Color = Colors.Blue, HeightRequest = 50, WidthRequest = 200 });
            layout.Children.Add(new BoxView { Color = Colors.Yellow, HeightRequest = 50, WidthRequest = 200 });
        });
        CompareImages();
    }
}
