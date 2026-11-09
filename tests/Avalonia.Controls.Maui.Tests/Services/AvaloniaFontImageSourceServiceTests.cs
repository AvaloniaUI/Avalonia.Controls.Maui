using Avalonia.Controls.Maui.Services;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Stretch = Avalonia.Media.Stretch;
using Image = Avalonia.Controls.Image;
using Border = Avalonia.Controls.Border;
using Window = Avalonia.Controls.Window;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaFontImageSourceServiceTests
{
    static readonly string OpenSans =
        $"avares://{typeof(AvaloniaFontImageSourceServiceTests).Assembly.GetName().Name}/Assets/Fonts/OpenSans-Regular.ttf#Open Sans";

    [AvaloniaFact(DisplayName = "Font image is a drawing of glyph runs in the source's colour")]
    public async Task FontImageIsDrawingOfGlyphRuns()
    {
        var image = await GetDrawingImageAsync(CreateSource("M", Microsoft.Maui.Graphics.Colors.Red));

        var runs = GetGlyphRunDrawings(image.Drawing!).ToList();

        Assert.NotEmpty(runs);
        Assert.All(runs, run =>
        {
            var brush = Assert.IsAssignableFrom<ISolidColorBrush>(run.Foreground);
            Assert.Equal(Colors.Red, brush.Color);
        });
    }

    [AvaloniaTheory(DisplayName = "Font image is sized to the glyph's ink bounds plus a one-unit border")]
    [InlineData("M")]
    [InlineData("g")]
    [InlineData("Å")]
    public async Task FontImageIsSizedToInkBounds(string glyph)
    {
        var image = await GetDrawingImageAsync(CreateSource(glyph));

        var ink = GetGlyphRunDrawings(image.Drawing!)
            .Select(run => run.GlyphRun!.BuildGeometry().Bounds)
            .Aggregate((a, b) => a.Union(b));

        var expected = new Rect(
            Math.Floor(ink.X) - 1,
            Math.Floor(ink.Y) - 1,
            Math.Ceiling(ink.Right) - Math.Floor(ink.X) + 2,
            Math.Ceiling(ink.Bottom) - Math.Floor(ink.Y) + 2);

        Assert.Equal(expected, image.Viewbox);
        Assert.Equal(expected.Size, image.Size);
    }

    [AvaloniaFact(DisplayName = "Font image does not depend on the scale")]
    public async Task FontImageDoesNotDependOnScale()
    {
        var source = CreateSource("M");
        var service = new AvaloniaFontImageSourceService();

        using var at1 = await service.GetImageAsync(source, 1f);
        using var at2 = await service.GetImageAsync(source, 2f);
        using var at3 = await service.GetImageAsync(source, 2.5f);

        Assert.False(at1!.IsResolutionDependent);
        Assert.False(at2!.IsResolutionDependent);
        Assert.Equal(at1.Value.Size, at2.Value.Size);
        Assert.Equal(at1.Value.Size, at3!.Value.Size);
    }

    [AvaloniaTheory(DisplayName = "Empty glyph gives no image")]
    [InlineData(null)]
    [InlineData("")]
    public async Task EmptyGlyphGivesNoImage(string? glyph)
    {
        var result = await new AvaloniaFontImageSourceService().GetImageAsync(CreateSource(glyph!));

        Assert.Null(result);
    }

    [AvaloniaFact(DisplayName = "Glyph missing from the font still gives glyph runs")]
    public async Task MissingGlyphGivesGlyphRuns()
    {
        // Open Sans has no CJK glyphs, so this goes through font fallback
        var image = await GetDrawingImageAsync(CreateSource("漢"));

        Assert.NotEmpty(GetGlyphRunDrawings(image.Drawing!));
        Assert.True(image.Size.Width > 0 && image.Size.Height > 0);
    }

    [AvaloniaFact(DisplayName = "Font image built off the UI thread is usable")]
    public async Task FontImageBuiltOffUiThread()
    {
        var source = CreateSource("M");
        var service = new AvaloniaFontImageSourceService();

        var result = await Task.Run(() => service.GetImageAsync(source));

        var image = Assert.IsType<DrawingImage>(result!.Value);
        Assert.NotEmpty(GetGlyphRunDrawings(image.Drawing!));
    }

    [AvaloniaFact(DisplayName = "Font image keeps greyscale antialiasing in an image, a brush and an opacity mask under an aliased host")]
    public async Task FontImageKeepsAntialiasingUnderAliasedHost()
    {
        var image = await GetDrawingImageAsync(CreateSource("M"));

        // Glyph runs drawn straight into the host take the host's text mode
        var plain = new DrawingGroup();
        using (var context = plain.Open())
        {
            foreach (var run in GetGlyphRunDrawings(image.Drawing!))
                context.DrawGlyphRun(run.Foreground, run.GlyphRun!);
        }
        var plainImage = new DrawingImage(plain) { Viewbox = image.Viewbox };

        Assert.Equal(0, CountGreys(RenderInAliasedHost(new Image { Source = plainImage, Stretch = Stretch.None })));

        Assert.True(CountGreys(RenderInAliasedHost(new Image { Source = image, Stretch = Stretch.None })) > 0,
            "The glyph in an image is not antialiased");

        var brush = ImageBrushes.Create(image);
        brush.Stretch = Stretch.None;
        Assert.True(CountGreys(RenderInAliasedHost(new Border { Width = image.Size.Width, Height = image.Size.Height, Background = brush })) > 0,
            "The glyph in a brush is not antialiased");

        var mask = ImageBrushes.Create(image);
        mask.Stretch = Stretch.None;
        Assert.True(CountGreys(RenderInAliasedHost(new Border { Width = image.Size.Width, Height = image.Size.Height, Background = Brushes.Black, OpacityMask = mask })) > 0,
            "The glyph in an opacity mask is not antialiased");
    }

    static FontImageSource CreateSource(string glyph, Microsoft.Maui.Graphics.Color? color = null) => new()
    {
        Glyph = glyph,
        FontFamily = OpenSans,
        Size = 30,
        Color = color ?? Microsoft.Maui.Graphics.Colors.Black,
    };

    static async Task<DrawingImage> GetDrawingImageAsync(FontImageSource source)
    {
        var result = await new AvaloniaFontImageSourceService().GetImageAsync((IFontImageSource)source);
        return Assert.IsType<DrawingImage>(result!.Value);
    }

    static IEnumerable<GlyphRunDrawing> GetGlyphRunDrawings(Drawing drawing) => drawing switch
    {
        GlyphRunDrawing run => [run],
        DrawingGroup group => group.Children.SelectMany(GetGlyphRunDrawings),
        GeometryDrawing { Brush: DrawingBrush { Drawing: { } content } } => GetGlyphRunDrawings(content),
        _ => []
    };

    static byte[] RenderInAliasedHost(Control content)
    {
        content.HorizontalAlignment = Layout.HorizontalAlignment.Left;
        content.VerticalAlignment = Layout.VerticalAlignment.Top;

        var window = new Window { Width = 60, Height = 60, Background = Brushes.White, Content = content };
        TextOptions.SetTextRenderingMode(window, TextRenderingMode.Alias);
        window.Show();
        Threading.Dispatcher.UIThread.RunJobs();

        var size = new PixelSize(60, 60);
        using var bitmap = new RenderTargetBitmap(size);
        bitmap.Render(window);
        window.Close();

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

        return pixels;
    }

    // Black ink on white: an aliased glyph has no grey pixels
    static int CountGreys(byte[] pixels) =>
        Enumerable.Range(0, pixels.Length / 4).Count(i => pixels[i * 4 + 1] is > 0 and < 255);
}
