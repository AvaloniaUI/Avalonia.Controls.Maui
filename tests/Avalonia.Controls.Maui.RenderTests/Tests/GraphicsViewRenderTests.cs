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
    
    [AvaloniaFact]
    public async Task Render_GraphicsView_StartAlignment()
    {
        // A view whose size comes from a MAUI-level MeasureOverride rather than
        // WidthRequest/HeightRequest, aligned Start so the platform view is not stretched.
        // The drawable fills the dirty rect, so a collapsed platform view renders nothing.
        var graphicsView = new FixedSizeGraphicsView
        {
            Drawable = new FillBoundsDrawable(),
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
            VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
            BackgroundColor = Colors.White
        };

        var layout = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            WidthRequest = 200,
            HeightRequest = 120,
            Children = { graphicsView }
        };

        await RenderToFile(layout);
        CompareImages();
    }

    class FixedSizeGraphicsView : Microsoft.Maui.Controls.GraphicsView
    {
        protected override Microsoft.Maui.Graphics.Size MeasureOverride(double widthConstraint, double heightConstraint) =>
            new(100, 100);
    }

    class FillBoundsDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Blue;
            canvas.FillRectangle(dirtyRect);
        }
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
