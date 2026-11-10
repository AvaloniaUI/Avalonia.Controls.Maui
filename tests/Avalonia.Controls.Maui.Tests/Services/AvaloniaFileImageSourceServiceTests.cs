using Avalonia.Controls.Maui.Services;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Services;

public sealed class AvaloniaFileImageSourceServiceTests : IDisposable
{
    static readonly PixelSize BaseSize = new(4, 3);

    readonly string _directory = Path.Combine(Path.GetTempPath(), "Avalonia.Controls.Maui.Tests", $"variants-{Guid.NewGuid():N}");

    public AvaloniaFileImageSourceServiceTests() => Directory.CreateDirectory(_directory);

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [AvaloniaTheory(DisplayName = "The file variant for the scale is loaded")]
    [InlineData(1f, 1)]
    [InlineData(1.25f, 2)]
    [InlineData(2f, 2)]
    [InlineData(2.5f, 3)]
    [InlineData(3f, 3)]
    [InlineData(4f, 3)]
    public async Task VariantForScaleIsLoaded(float scale, int expectedVariant)
    {
        var file = CreateImages(1, 2, 3);

        using var result = await new AvaloniaFileImageSourceService().GetImageAsync(new FileImageSource { File = file }, scale);

        AssertVariant(result, expectedVariant);

        // The file has variants, so another scale could load a different file
        Assert.True(result!.IsResolutionDependent);
    }

    [AvaloniaFact(DisplayName = "A missing variant loads the base image")]
    public async Task MissingVariantLoadsBaseImage()
    {
        var file = CreateImages(1, 3);

        using var result = await new AvaloniaFileImageSourceService().GetImageAsync(new FileImageSource { File = file }, 2f);

        AssertVariant(result, 1);

        // The base image stands in for the variant, so a later scale change loads again
        Assert.True(result!.IsResolutionDependent);
    }

    [AvaloniaFact(DisplayName = "A file with no variants loads at every scale")]
    public async Task FileWithoutVariantsLoads()
    {
        var file = CreateImages(1);
        var service = new AvaloniaFileImageSourceService();

        using var at1 = await service.GetImageAsync(new FileImageSource { File = file }, 1f);
        using var at3 = await service.GetImageAsync(new FileImageSource { File = file }, 3f);

        AssertVariant(at1, 1);
        AssertVariant(at3, 1);

        // Above 1x the base image stands in for a missing variant
        Assert.False(at1!.IsResolutionDependent);
        Assert.True(at3!.IsResolutionDependent);
    }

    [AvaloniaTheory(DisplayName = "A desktop Resizetizer image loads at a quarter of its pixel size")]
    [InlineData(1f)]
    [InlineData(2f)]
    public async Task DesktopResizetizerImageLoadsAtBaseSize(float scale)
    {
        // Resizetizer's desktop output is a single file at 4x, embedded as icon@4x.png
        var file = CreateImages(4);

        using var result = await new AvaloniaFileImageSourceService().GetImageAsync(new FileImageSource { File = file }, scale);

        AssertVariant(result, 4);
    }

    [AvaloniaFact(DisplayName = "An image shows the whole of a variant at the variant's scale")]
    public async Task ImageShowsWholeVariant()
    {
        var file = CreateImages(1, 2);
        using var result = await new AvaloniaFileImageSourceService().GetImageAsync(new FileImageSource { File = file }, 2f);

        var window = new Avalonia.Controls.Window { Width = 40, Height = 40, Background = Avalonia.Media.Brushes.White };
        Avalonia.Headless.HeadlessWindowExtensions.SetRenderScaling(window, 2);
        window.Content = new Avalonia.Controls.Image
        {
            Source = result!.Value,
            Stretch = Avalonia.Media.Stretch.None,
            HorizontalAlignment = Layout.HorizontalAlignment.Left,
            VerticalAlignment = Layout.VerticalAlignment.Top
        };
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        using var bitmap = new RenderTargetBitmap(new PixelSize(80, 80), new Vector(192, 192));
        bitmap.Render(window);
        window.Close();

        // The variant is red with a blue bottom-right pixel; a bitmap drawn by pixel rect would lose that corner
        var size = new PixelSize(BaseSize.Width * 2, BaseSize.Height * 2);
        Assert.Equal(Avalonia.Media.Colors.Red, ReadPixel(bitmap, new PixelPoint(0, 0)));
        Assert.Equal(Avalonia.Media.Colors.Blue, ReadPixel(bitmap, new PixelPoint(size.Width - 1, size.Height - 1)));
        Assert.Equal(Avalonia.Media.Colors.White, ReadPixel(bitmap, new PixelPoint(size.Width, size.Height)));
    }

    static Avalonia.Media.Color ReadPixel(Bitmap bitmap, PixelPoint point)
    {
        var pixel = new byte[4];
        var handle = System.Runtime.InteropServices.GCHandle.Alloc(pixel, System.Runtime.InteropServices.GCHandleType.Pinned);
        try
        {
            bitmap.CopyPixels(new PixelRect(point, new PixelSize(1, 1)), handle.AddrOfPinnedObject(), 4, 4);
        }
        finally
        {
            handle.Free();
        }

        // Rendered bitmaps are RGBA or BGRA, which depends on the platform
        return bitmap.Format == Avalonia.Platform.PixelFormat.Rgba8888
            ? Avalonia.Media.Color.FromArgb(pixel[3], pixel[0], pixel[1], pixel[2])
            : Avalonia.Media.Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
    }

    /// <summary>
    /// Writes icon.png and the requested @kx variants, each k times the base size.
    /// </summary>
    string CreateImages(params int[] variants)
    {
        var file = Path.Combine(_directory, "icon.png");
        foreach (var variant in variants)
        {
            var path = variant == 1 ? file : Path.Combine(_directory, $"icon@{variant}x.png");
            using var bitmap = new RenderTargetBitmap(new PixelSize(BaseSize.Width * variant, BaseSize.Height * variant));
            using (var context = bitmap.CreateDrawingContext())
            {
                context.FillRectangle(Avalonia.Media.Brushes.Red, new Rect(bitmap.Size));
                context.FillRectangle(Avalonia.Media.Brushes.Blue, new Rect(bitmap.Size.Width - 1, bitmap.Size.Height - 1, 1, 1));
            }
            bitmap.Save(path);
        }

        return file;
    }

    static void AssertVariant(IImageSourceServiceResult<Avalonia.Media.IImage>? result, int variant)
    {
        var image = result!.Value;
        var bitmap = variant == 1
            ? Assert.IsAssignableFrom<Bitmap>(image)
            : Assert.IsType<ScaledBitmapImage>(image).Bitmap;

        Assert.Equal(new PixelSize(BaseSize.Width * variant, BaseSize.Height * variant), bitmap.PixelSize);
        Assert.Equal(new Size(BaseSize.Width, BaseSize.Height), image.Size);
        if (variant > 1)
            Assert.Equal(variant, ((ScaledBitmapImage)image).Scale);
    }
}
