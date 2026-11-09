using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaImage = Avalonia.Controls.Image;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Creates brushes that paint an image loaded by an image source service.
/// </summary>
internal static class ImageBrushes
{
    /// <summary>
    /// Creates a brush for <paramref name="image"/>. The caller sets the stretch and alignment.
    /// </summary>
    public static TileBrush Create(IImage image)
    {
        switch (image)
        {
            case Bitmap bitmap:
                return new ImageBrush(bitmap);

            case DrawingImage { Drawing: { } drawing } drawingImage:
                // Use the image's viewbox so the brush keeps the image's border and lines up with it
                var viewbox = drawingImage.Viewbox ?? drawing.GetBounds();
                return new DrawingBrush(drawing)
                {
                    SourceRect = new RelativeRect(viewbox, RelativeUnit.Absolute)
                };

            default:
                var size = image.Size;
                return new VisualBrush(new AvaloniaImage
                {
                    Source = image,
                    Width = size.Width,
                    Height = size.Height
                });
        }
    }
}
