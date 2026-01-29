using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class GraphicsViewRenderTests : RenderTestBase
{
    [AvaloniaFact]
    public async Task Render_GraphicsView()
    {
        var control = new Microsoft.Maui.Controls.GraphicsView
        {
            Drawable = new TestDrawable(),
            WidthRequest = 100,
            HeightRequest = 100,
            BackgroundColor = Colors.White
        };
        await RenderToFile(control);
        CompareImages();
    }
    
    class TestDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.Blue;
            canvas.StrokeSize = 4;
            canvas.DrawEllipse(10, 10, 80, 80);
        }
    }
}
