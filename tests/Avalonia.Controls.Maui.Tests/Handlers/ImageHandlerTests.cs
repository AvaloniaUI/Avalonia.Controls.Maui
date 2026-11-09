using System.Diagnostics;
using Avalonia.Controls.Maui.Services;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiCornerRadius = Microsoft.Maui.CornerRadius;
using MauiImageHandler = Avalonia.Controls.Maui.Handlers.ImageHandler;
using MauiIImage = Microsoft.Maui.IImage;
using MauiEllipseGeometry = Microsoft.Maui.Controls.Shapes.EllipseGeometry;
using MauiPoint = Microsoft.Maui.Graphics.Point;
using MauiRoundRectangleGeometry = Microsoft.Maui.Controls.Shapes.RoundRectangleGeometry;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class ImageHandlerTests : HandlerTestBase<MauiImageHandler, ImageStub>
{
    [AvaloniaTheory(DisplayName = "Aspect Initializes Correctly")]
    [InlineData(Aspect.AspectFill, Media.Stretch.UniformToFill)]
    [InlineData(Aspect.AspectFit, Media.Stretch.Uniform)]
    [InlineData(Aspect.Fill, Media.Stretch.Fill)]
    [InlineData(Aspect.Center, Media.Stretch.None)]
    public async Task AspectInitializesCorrectly(Aspect aspect, Avalonia.Media.Stretch expectedStretch)
    {
        var image = new ImageStub
        {
            Aspect = aspect
        };

        var handler = await CreateHandlerAsync(image);
        var imageControl = await InvokeOnMainThreadAsync(() =>
            handler.PlatformView.Children.OfType<Image>().FirstOrDefault());
        var stretch = await InvokeOnMainThreadAsync(() => imageControl?.Stretch ?? Avalonia.Media.Stretch.None);

        Assert.Equal(expectedStretch, stretch);
    }

    [AvaloniaTheory(DisplayName = "Aspect Updates Correctly")]
    [InlineData(Aspect.AspectFit, Aspect.AspectFill, Media.Stretch.UniformToFill)]
    [InlineData(Aspect.Fill, Aspect.AspectFit, Media.Stretch.Uniform)]
    [InlineData(Aspect.Center, Aspect.Fill, Media.Stretch.Fill)]
    public async Task AspectUpdatesCorrectly(Aspect initialAspect, Aspect newAspect, Avalonia.Media.Stretch expectedStretch)
    {
        var image = new ImageStub
        {
            Aspect = initialAspect
        };

        var handler = await CreateHandlerAsync(image);

        await InvokeOnMainThreadAsync(() =>
        {
            image.Aspect = newAspect;
            handler.UpdateValue(nameof(MauiIImage.Aspect));
        });

        var imageControl = await InvokeOnMainThreadAsync(() =>
            handler.PlatformView.Children.OfType<Image>().FirstOrDefault());
        var stretch = await InvokeOnMainThreadAsync(() => imageControl?.Stretch ?? Media.Stretch.None);

        Assert.Equal(expectedStretch, stretch);
    }

    [AvaloniaFact(DisplayName = "Default Aspect is AspectFit")]
    public async Task DefaultAspectIsAspectFit()
    {
        var image = new ImageStub();

        var handler = await CreateHandlerAsync(image);
        var imageControl = await InvokeOnMainThreadAsync(() =>
            handler.PlatformView.Children.OfType<Image>().FirstOrDefault());
        var stretch = await InvokeOnMainThreadAsync(() => imageControl?.Stretch ?? Avalonia.Media.Stretch.None);

        Assert.Equal(Media.Stretch.Uniform, stretch);
    }

    [AvaloniaFact(DisplayName = "Null Source Sets Null Platform Source")]
    public async Task NullSourceSetsNullPlatformSource()
    {
        var image = new ImageStub
        {
            Source = null
        };

        var handler = await CreateHandlerAsync(image);

        await InvokeOnMainThreadAsync(() =>
        {
            MauiImageHandler.MapSource(handler, image);
        });

        await WaitForLoadingStateAsync(image, expected: false);

        var imageControl = await InvokeOnMainThreadAsync(() =>
            handler.PlatformView.Children.OfType<Image>().FirstOrDefault());
        var platformSource = await InvokeOnMainThreadAsync(() => imageControl?.Source);

        Assert.Null(platformSource);
    }

    [AvaloniaFact(DisplayName = "Handler Creates Platform View")]
    public async Task HandlerCreatesPlatformView()
    {
        var image = new ImageStub();

        var handler = await CreateHandlerAsync(image);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<Grid>(handler.PlatformView);

        // Verify the Grid contains Image control (GifImage is added dynamically when needed)
        var imageControl = await InvokeOnMainThreadAsync(() =>
            handler.PlatformView.Children.OfType<Image>().FirstOrDefault());

        Assert.NotNull(imageControl);
    }

    [AvaloniaFact(DisplayName = "IsAnimationPlaying Does Not Throw")]
    public async Task IsAnimationPlayingDoesNotThrow()
    {
        var image = new ImageStub
        {
            IsAnimationPlaying = true
        };

        var handler = await CreateHandlerAsync(image);

        // Should not throw - animation support is placeholder for now
        await InvokeOnMainThreadAsync(() =>
        {
            MauiImageHandler.MapIsAnimationPlaying(handler, image);
        });

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaTheory(DisplayName = "All Aspect Values Map Correctly")]
    [InlineData(Aspect.AspectFill)]
    [InlineData(Aspect.AspectFit)]
    [InlineData(Aspect.Fill)]
    [InlineData(Aspect.Center)]
    public async Task AllAspectValuesMapCorrectly(Aspect aspect)
    {
        var image = new ImageStub
        {
            Aspect = aspect
        };

        var handler = await CreateHandlerAsync(image);
        var imageControl = await InvokeOnMainThreadAsync(() =>
            handler.PlatformView.Children.OfType<Image>().FirstOrDefault());
        var stretch = await InvokeOnMainThreadAsync(() => imageControl?.Stretch);

        var expectedStretch = aspect switch
        {
            Aspect.AspectFill => Media.Stretch.UniformToFill,
            Aspect.AspectFit => Media.Stretch.Uniform,
            Aspect.Fill => Media.Stretch.Fill,
            Aspect.Center => Media.Stretch.None,
            _ => Media.Stretch.Uniform
        };

        Assert.Equal(expectedStretch, stretch);
    }

    [AvaloniaFact(DisplayName = "Clearing Source Does Not Force Visibility")]
    public async Task ClearingSourceDoesNotForceVisibility()
    {
        var image = new ImageStub
        {
            Visibility = Visibility.Collapsed
        };

        var handler = await CreateHandlerAsync(image);

        await InvokeOnMainThreadAsync(() =>
        {
            image.Source = null;
            MauiImageHandler.MapSource(handler, image);
        });

        var isVisible = await InvokeOnMainThreadAsync(() => handler.PlatformView.IsVisible);

        Assert.False(isVisible);
    }

    [AvaloniaFact(DisplayName = "IsLoading toggles during source load")]
    public async Task IsLoadingTogglesDuringSourceLoad()
    {
        var image = new ImageStub
        {
            Source = new FileImageSource { File = "fake.png" }
        };

        var handler = await CreateHandlerAsync(image);

        await InvokeOnMainThreadAsync(() =>
        {
            ShowInWindow(handler.PlatformView);
            MauiImageHandler.MapSource(handler, image);
        });

        await WaitForLoadingStateAsync(image, expected: false);

        Assert.Contains(true, image.LoadingStates);
        Assert.Equal(false, image.LastIsLoading);
    }

    [AvaloniaFact(DisplayName = "Clip maps to inner image control")]
    public async Task ClipMapsToInnerImageControl()
    {
        var image = new ImageStub
        {
            Width = 75,
            Height = 75,
            Clip = new MauiEllipseGeometry
            {
                Center = new MauiPoint(36, 36),
                RadiusX = 36,
                RadiusY = 36
            }
        };

        var handler = await CreateHandlerAsync(image);

        var imageControl = await InvokeOnMainThreadAsync(() =>
            handler.PlatformView.Children.OfType<Image>().First());

        var clip = await InvokeOnMainThreadAsync(() => Assert.IsType<EllipseGeometry>(imageControl.Clip));

        Assert.Equal(new Rect(0, 0, 72, 72), clip.Rect);
    }

    [AvaloniaFact(DisplayName = "RoundRectangle clip without Rect maps to inner image bounds")]
    public async Task RoundRectangleClipWithoutRectMapsToInnerImageBounds()
    {
        var image = new ImageStub
        {
            Width = 50,
            Height = 50,
            Clip = new MauiRoundRectangleGeometry
            {
                CornerRadius = new MauiCornerRadius(12)
            }
        };

        var handler = await CreateHandlerAsync(image);

        var imageControl = await InvokeOnMainThreadAsync(() =>
            handler.PlatformView.Children.OfType<Image>().First());

        var clip = await InvokeOnMainThreadAsync(() => Assert.IsType<RectangleGeometry>(imageControl.Clip));

        Assert.Equal(new Rect(0, 0, 50, 50), clip.Rect);
        Assert.Equal(12, clip.RadiusX);
        Assert.Equal(12, clip.RadiusY);
    }
    
    [AvaloniaFact(DisplayName = "Image loads the file variant for a new display scale")]
    public async Task ImageLoadsFileVariantForNewScale()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Avalonia.Controls.Maui.Tests", $"image-scale-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var file = await InvokeOnMainThreadAsync(() =>
            {
                SavePng(Path.Combine(directory, "icon.png"), new PixelSize(4, 3));
                SavePng(Path.Combine(directory, "icon@2x.png"), new PixelSize(8, 6));
                SavePng(Path.Combine(directory, "icon@3x.png"), new PixelSize(12, 9));
                return Path.Combine(directory, "icon.png");
            });

            var image = new ImageStub { Source = new FileImageSource { File = file } };
            var handler = await CreateHandlerAsync(image);

            var sizes = await InvokeOnMainThreadAsync(() =>
            {
                var window = ShowInWindow(handler.PlatformView);
                var staticImage = handler.PlatformView.Children.OfType<Image>().First();
                var loaded = new List<PixelSize>();

                foreach (var scale in new[] { 1.0, 2.0, 3.0, 1.0, 2.0 })
                {
                    Avalonia.Headless.HeadlessWindowExtensions.SetRenderScaling(window, scale);
                    Threading.Dispatcher.UIThread.RunJobs();
                    loaded.Add(staticImage.Source switch
                    {
                        ScaledBitmapImage scaled => scaled.Bitmap.PixelSize,
                        var source => Assert.IsAssignableFrom<Media.Imaging.Bitmap>(source).PixelSize
                    });
                }

                return loaded;
            });

            // Each scale shows its own file, including 1x back to 2x
            Assert.Equal(
                [new PixelSize(4, 3), new PixelSize(8, 6), new PixelSize(12, 9), new PixelSize(4, 3), new PixelSize(8, 6)],
                sizes);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [AvaloniaTheory(DisplayName = "An image measures as MAUI's SizeThatFitsImage does")]
    // Unsized AspectFit: natural size, shrunk to fit, never enlarged
    [InlineData(Aspect.AspectFit, 1000, double.PositiveInfinity, false, false, 32, 24)]
    [InlineData(Aspect.AspectFit, 16, double.PositiveInfinity, false, false, 16, 12)]
    [InlineData(Aspect.AspectFit, 1000, 12, false, false, 16, 12)]
    [InlineData(Aspect.AspectFit, double.PositiveInfinity, double.PositiveInfinity, false, false, 32, 24)]
    // A request on either axis lets AspectFit grow; the other axis follows the aspect ratio
    [InlineData(Aspect.AspectFit, 64, double.PositiveInfinity, true, false, 64, 48)]
    [InlineData(Aspect.AspectFit, 1000, 48, false, true, 64, 48)]
    // Other aspects take the natural size, cut to the constraints, on each axis
    [InlineData(Aspect.AspectFill, 1000, 1000, false, false, 32, 24)]
    [InlineData(Aspect.Fill, 16, 1000, false, false, 16, 24)]
    [InlineData(Aspect.Center, 1000, 10, false, false, 32, 10)]
    public void FitImageMatchesMaui(Aspect aspect, double widthConstraint, double heightConstraint,
        bool widthIsExplicit, bool heightIsExplicit, double expectedWidth, double expectedHeight)
    {
        var size = MauiImageHandler.FitImage(new Microsoft.Maui.Graphics.Size(32, 24), widthConstraint, heightConstraint, aspect, widthIsExplicit, heightIsExplicit);

        Assert.Equal(new Microsoft.Maui.Graphics.Size(expectedWidth, expectedHeight), size);
    }

    [AvaloniaTheory(DisplayName = "An image with no size request is not enlarged")]
    [InlineData(double.NaN, double.NaN, 1000, double.PositiveInfinity, 32, 24)]
    [InlineData(double.NaN, double.NaN, 16, double.PositiveInfinity, 16, 12)]
    [InlineData(64, double.NaN, 1000, double.PositiveInfinity, 64, 48)]
    [InlineData(64, 64, 1000, double.PositiveInfinity, 64, 64)]
    public async Task UnsizedImageIsNotEnlarged(double width, double height, double widthConstraint, double heightConstraint,
        double expectedWidth, double expectedHeight)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Avalonia.Controls.Maui.Tests", $"image-size-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var file = Path.Combine(directory, "icon.png");
            await InvokeOnMainThreadAsync(() => SavePng(file, new PixelSize(32, 24)));

            var image = new ImageStub { Source = new FileImageSource { File = file }, Width = width, Height = height };
            var handler = await CreateHandlerAsync(image);

            var desired = await InvokeOnMainThreadAsync(() =>
            {
                ShowInWindow(handler.PlatformView);
                return handler.GetDesiredSize(widthConstraint, heightConstraint);
            });

            Assert.Equal(new Microsoft.Maui.Graphics.Size(expectedWidth, expectedHeight), desired);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    static void SavePng(string path, PixelSize size)
    {
        using var bitmap = new Media.Imaging.RenderTargetBitmap(size);
        using (var context = bitmap.CreateDrawingContext())
            context.FillRectangle(Brushes.Red, new Rect(bitmap.Size));
        bitmap.Save(path);
    }

    static async Task WaitForLoadingStateAsync(ImageStub image, bool expected, int timeoutMs = 1000)
    {
        var sw = Stopwatch.StartNew();
        while (image.LastIsLoading != expected && sw.ElapsedMilliseconds < timeoutMs)
        {
            await Task.Delay(10);
        }

        Assert.Equal(expected, image.LastIsLoading);
    }
}
