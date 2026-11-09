using Avalonia.Controls.Maui.Services;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Avalonia.Controls.Maui.Tests.Services;

public class ImageBrushesTests
{
    [AvaloniaFact(DisplayName = "A bitmap gives an image brush")]
    public void BitmapGivesImageBrush()
    {
        using var bitmap = new RenderTargetBitmap(new PixelSize(4, 4), new Vector(96, 96));

        var brush = Assert.IsType<ImageBrush>(ImageBrushes.Create(bitmap));

        Assert.Same(bitmap, brush.Source);
    }

    [AvaloniaFact(DisplayName = "A drawing image gives a drawing brush over its viewbox")]
    public void DrawingImageGivesDrawingBrushOverViewbox()
    {
        var drawing = new GeometryDrawing
        {
            Brush = Brushes.Black,
            Geometry = new RectangleGeometry(new Rect(2, 3, 10, 12))
        };
        var viewbox = new Rect(1, 2, 12, 14);
        var image = new DrawingImage(drawing) { Viewbox = viewbox };

        var brush = Assert.IsType<DrawingBrush>(ImageBrushes.Create(image));

        Assert.Same(drawing, brush.Drawing);
        Assert.Equal(new RelativeRect(viewbox, RelativeUnit.Absolute), brush.SourceRect);
    }

    [AvaloniaFact(DisplayName = "A drawing image with no viewbox uses the drawing's bounds")]
    public void DrawingImageWithoutViewboxUsesDrawingBounds()
    {
        var drawing = new GeometryDrawing
        {
            Brush = Brushes.Black,
            Geometry = new RectangleGeometry(new Rect(2, 3, 10, 12))
        };

        var brush = Assert.IsType<DrawingBrush>(ImageBrushes.Create(new DrawingImage(drawing)));

        Assert.Equal(new RelativeRect(drawing.GetBounds(), RelativeUnit.Absolute), brush.SourceRect);
    }
}
