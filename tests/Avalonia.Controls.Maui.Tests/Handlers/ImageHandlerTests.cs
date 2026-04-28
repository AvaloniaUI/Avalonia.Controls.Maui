using System.Diagnostics;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiImageHandler = Avalonia.Controls.Maui.Handlers.ImageHandler;
using MauiIImage = Microsoft.Maui.IImage;
using MauiEllipseGeometry = Microsoft.Maui.Controls.Shapes.EllipseGeometry;
using MauiPoint = Microsoft.Maui.Graphics.Point;

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
