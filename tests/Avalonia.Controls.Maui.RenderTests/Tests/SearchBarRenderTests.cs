using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class SearchBarRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_SearchBar()
    {
        var control = new Microsoft.Maui.Controls.SearchBar 
        { 
            Placeholder = "Search...", 
            Text = "Query",
            WidthRequest = 300
        };
        await RenderToFile(control);
        // Text rendering differs slightly between platforms
        CompareImages(tolerance: 0.045);
    }
}
