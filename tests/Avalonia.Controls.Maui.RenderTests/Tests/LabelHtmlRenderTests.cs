using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class LabelHtmlRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_Label_TextType_Html()
    {
        var control = new Microsoft.Maui.Controls.Label
        {
            Text = "This has <b>bold</b>, <i>italic</i>, and <u>underline</u> text.",
            TextType = Microsoft.Maui.TextType.Html,
            TextColor = Colors.Black,
            BackgroundColor = Colors.White,
            WidthRequest = 300,
            HeightRequest = 60
        };
        await RenderToFile(control);
        CompareImages(tolerance: 0.07);
    }
}
