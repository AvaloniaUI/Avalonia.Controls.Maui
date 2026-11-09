using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class FontImageRenderTests : RenderTestBase
{
    // Glyph hinting differs between Linux and macOS
    static FontImageSource Glyph(string glyph = "M", double size = 30, Color? color = null) => new()
    {
        Glyph = glyph,
        Size = size,
        Color = color ?? Colors.Black
    };

    Task RenderButton(double scale, [CallerMemberName] string testName = "") =>
        RenderToFile(new Microsoft.Maui.Controls.Button
        {
            Text = "Settings",
            ImageSource = Glyph(size: 20),
            WidthRequest = 160,
            HeightRequest = 50
        }, scale, testName: testName);

    Task RenderImage(double scale, [CallerMemberName] string testName = "") =>
        RenderToFile(new Microsoft.Maui.Controls.Image
        {
            Source = Glyph(),
            WidthRequest = 60,
            HeightRequest = 60,
            Aspect = Microsoft.Maui.Aspect.Center
        }, scale, testName: testName);

    Task RenderImageButton(double scale, [CallerMemberName] string testName = "") =>
        RenderToFile(new ImageButton
        {
            Source = Glyph(color: Colors.White),
            BackgroundColor = Colors.SteelBlue,
            Padding = new Microsoft.Maui.Thickness(8),
            WidthRequest = 60,
            HeightRequest = 60
        }, scale, testName: testName);

    [AvaloniaFact]
    public async Task Render_Button_FontImageSource()
    {
        await RenderButton(1);
        CompareImages(tolerance: 0.03);
    }

    [AvaloniaFact]
    public async Task Render_Button_FontImageSource_2x()
    {
        await RenderButton(2);
        CompareImages(tolerance: 0.03);
    }

    [AvaloniaFact]
    public async Task Render_Image_FontImageSource()
    {
        await RenderImage(1);
        CompareImages(tolerance: 0.03);
    }

    [AvaloniaFact]
    public async Task Render_Image_FontImageSource_2x()
    {
        await RenderImage(2);
        CompareImages(tolerance: 0.03);
    }

    [AvaloniaFact]
    public async Task Render_ImageButton_FontImageSource()
    {
        await RenderImageButton(1);
        CompareImages(tolerance: 0.03);
    }

    [AvaloniaFact]
    public async Task Render_ImageButton_FontImageSource_2x()
    {
        await RenderImageButton(2);
        CompareImages(tolerance: 0.03);
    }

    [AvaloniaFact]
    public async Task Render_Image_FontImageSource_2x_EdgeIsSharp()
    {
        await RenderToFile(new Microsoft.Maui.Controls.Image
        {
            Source = Glyph("T", 40),
            WidthRequest = 60,
            HeightRequest = 60,
            Aspect = Microsoft.Maui.Aspect.Center
        }, 2);

        var (pixels, width, height) = ReadLuminance(GetOutputPath(nameof(Render_Image_FontImageSource_2x_EdgeIsSharp)));
        var ink = FindInkBounds(pixels, width, height);

        // A column through the crossbar of the T, clear of the stem, crosses its top edge
        var x = ink.X + ink.Width / 5;
        var transition = 0;
        for (var y = ink.Y - 4; y <= ink.Y + 4; y++)
        {
            // Neither background nor ink
            if (pixels[y * width + x] is > 3 and < 252)
                transition++;
        }

        // A vector glyph changes from background to ink within 2 device pixels; a stretched 1x bitmap takes about 4
        Assert.InRange(transition, 0, 2);
    }

    static (byte[] Pixels, int Width, int Height) ReadLuminance(string path)
    {
        using var bitmap = new Bitmap(path);
        var size = bitmap.PixelSize;
        var stride = size.Width * 4;
        var bgra = new byte[stride * size.Height];
        var handle = GCHandle.Alloc(bgra, GCHandleType.Pinned);
        try
        {
            bitmap.CopyPixels(new PixelRect(size), handle.AddrOfPinnedObject(), bgra.Length, stride);
        }
        finally
        {
            handle.Free();
        }

        // Black ink on white: any colour channel is the luminance
        var luminance = new byte[size.Width * size.Height];
        for (var i = 0; i < luminance.Length; i++)
            luminance[i] = bgra[i * 4 + 1];

        return (luminance, size.Width, size.Height);
    }

    static PixelRect FindInkBounds(byte[] pixels, int width, int height)
    {
        int left = width, top = height, right = -1, bottom = -1;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (pixels[y * width + x] >= 128)
                    continue;

                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
        }

        Assert.True(right >= 0, "The glyph was not drawn");
        return new PixelRect(left, top, right - left + 1, bottom - top + 1);
    }
}
