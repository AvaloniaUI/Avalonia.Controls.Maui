using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Controls;
using MauiTextAlignment = Microsoft.Maui.TextAlignment;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class TableViewRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_EntryCell_VerticalTextAlignment()
    {
        var section = new TableSection("Vertical Text Alignment")
        {
            new EntryCell
            {
                Label = "Start",
                Text = "Top aligned",
                VerticalTextAlignment = MauiTextAlignment.Start
            },
            new EntryCell
            {
                Label = "Center",
                Text = "Center aligned",
                VerticalTextAlignment = MauiTextAlignment.Center
            },
            new EntryCell
            {
                Label = "End",
                Text = "Bottom aligned",
                VerticalTextAlignment = MauiTextAlignment.End
            }
        };

        var control = new TableView
        {
            Intent = TableIntent.Form,
            RowHeight = 72,
            WidthRequest = 360,
            HeightRequest = 270,
            Root = new TableRoot { section }
        };

        await RenderToFile(control);
        CompareImages(tolerance: 0.06);
    }
}
