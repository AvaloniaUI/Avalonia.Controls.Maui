using Avalonia.Controls.Maui.Services;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaFontImageSourceServiceTests
{
    static readonly string OpenSans =
        $"avares://{typeof(AvaloniaFontImageSourceServiceTests).Assembly.GetName().Name}/Assets/Fonts/OpenSans-Regular.ttf#Open Sans";

    [AvaloniaTheory(DisplayName = "Font image is sized to the glyph it draws")]
    [InlineData(1f)]
    [InlineData(2f)]
    public async Task FontImageIsSizedToTheGlyph(float scale)
    {
        var source = new FontImageSource
        {
            Glyph = "M",
            FontFamily = OpenSans,
            Size = 30,
            Color = Microsoft.Maui.Graphics.Colors.Black,
        };

        var result = await new AvaloniaFontImageSourceService().GetImageAsync((IFontImageSource)source, scale);
        var bitmap = Assert.IsAssignableFrom<Bitmap>(result!.Value);
        var ink = GetInkBounds(bitmap);

        // As on Windows: the image holds the glyph's drawn bounds plus a narrow border
        Assert.InRange(bitmap.PixelSize.Width - ink.Width, 0, 4 * scale);
        Assert.InRange(bitmap.PixelSize.Height - ink.Height, 0, 4 * scale);

        // No part of the glyph is cut off at an edge
        Assert.True(ink.X >= 1 && ink.Y >= 1, $"Glyph {ink} touches the top or left edge");
        Assert.True(ink.Right <= bitmap.PixelSize.Width - 1 && ink.Bottom <= bitmap.PixelSize.Height - 1,
            $"Glyph {ink} touches the bottom or right edge of {bitmap.PixelSize}");
    }

    static PixelRect GetInkBounds(Bitmap bitmap)
    {
        var size = bitmap.PixelSize;
        var stride = size.Width * 4;
        var pixels = new byte[stride * size.Height];
        var handle = System.Runtime.InteropServices.GCHandle.Alloc(pixels, System.Runtime.InteropServices.GCHandleType.Pinned);
        try
        {
            bitmap.CopyPixels(new PixelRect(size), handle.AddrOfPinnedObject(), pixels.Length, stride);
        }
        finally
        {
            handle.Free();
        }

        int left = size.Width, top = size.Height, right = -1, bottom = -1;
        for (var y = 0; y < size.Height; y++)
        {
            for (var x = 0; x < size.Width; x++)
            {
                // Alpha is the fourth byte in both BGRA and RGBA
                if (pixels[y * stride + x * 4 + 3] == 0)
                    continue;

                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
        }

        Assert.True(right >= 0, "The font image has no visible pixels");
        return new PixelRect(left, top, right - left + 1, bottom - top + 1);
    }
}
