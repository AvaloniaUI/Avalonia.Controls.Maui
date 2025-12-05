using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Media.Imaging;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiImageButtonHandler = Avalonia.Controls.Maui.Handlers.ImageButtonHandler;
using MauiThickness = Microsoft.Maui.Thickness;
using Stretch = Avalonia.Media.Stretch;
using IImage = Microsoft.Maui.IImage;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class ImageButtonHandlerTests : HandlerTestBase<MauiImageButtonHandler, ImageButtonStub>
{
    [AvaloniaFact(DisplayName = "Background Initializes Correctly")]
    public async Task BackgroundInitializesCorrectly()
    {
        var imageButton = new ImageButtonStub
        {
            Background = new SolidPaint(Colors.Blue)
        };

        var platformColor = await GetValueAsync(imageButton, GetPlatformBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Blue, platformColor);
    }

    [AvaloniaTheory(DisplayName = "Background Updates Correctly")]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    [InlineData(0, 0, 255)]
    public async Task BackgroundUpdatesCorrectly(byte r, byte g, byte b)
    {
        var imageButton = new ImageButtonStub
        {
            Background = new SolidPaint(Colors.White)
        };

        var newColor = Color.FromRgb(r, g, b);

        var platformColor = await GetValueAsync(imageButton, handler =>
        {
            imageButton.Background = new SolidPaint(newColor);
            handler.UpdateValue(nameof(IImageButton.Background));
            return GetPlatformBackgroundColor(handler);
        });

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null Background Doesn't Crash")]
    public async Task NullBackgroundDoesntCrash()
    {
        var imageButton = new ImageButtonStub { Background = null! };
        await CreateHandlerAsync(imageButton);
    }

    [AvaloniaFact(DisplayName = "Background Is Properly Set Via Extension Method")]
    public async Task BackgroundIsProperlySetViaExtensionMethod()
    {
        var imageButton = new ImageButtonStub
        {
            Background = new SolidPaint(Colors.Green)
        };

        var handler = await CreateHandlerAsync(imageButton);

        Assert.NotNull(handler.PlatformView);
        Assert.NotNull(handler.PlatformView.Background);

        var brush = Assert.IsType<Avalonia.Media.SolidColorBrush>(handler.PlatformView.Background);
        Assert.Equal(0, brush.Color.R);
        Assert.Equal(128, brush.Color.G);
        Assert.Equal(0, brush.Color.B);
    }

    [AvaloniaFact(DisplayName = "Padding Initializes Correctly")]
    public async Task PaddingInitializesCorrectly()
    {
        var padding = new MauiThickness(10, 20, 30, 40);
        var imageButton = new ImageButtonStub
        {
            Padding = padding
        };

        await ValidatePropertyInitValue(
            imageButton,
            () => imageButton.Padding,
            GetPlatformPadding,
            padding);
    }

    [AvaloniaTheory(DisplayName = "Padding Updates Correctly")]
    [InlineData(0, 0, 0, 0)]
    [InlineData(5, 5, 5, 5)]
    [InlineData(10, 20, 30, 40)]
    [InlineData(15, 0, 15, 0)]
    public async Task PaddingUpdatesCorrectly(double left, double top, double right, double bottom)
    {
        var imageButton = new ImageButtonStub { Padding = new MauiThickness(0) };
        var newPadding = new MauiThickness(left, top, right, bottom);

        var platformPadding = await GetValueAsync(imageButton, handler =>
        {
            imageButton.Padding = newPadding;
            handler.UpdateValue(nameof(IImageButton.Padding));
            return GetPlatformPadding(handler);
        });

        Assert.Equal(left, platformPadding.Left);
        Assert.Equal(top, platformPadding.Top);
        Assert.Equal(right, platformPadding.Right);
        Assert.Equal(bottom, platformPadding.Bottom);
    }

    [AvaloniaFact(DisplayName = "Uniform Padding Works")]
    public async Task UniformPaddingWorks()
    {
        var imageButton = new ImageButtonStub { Padding = new MauiThickness(15) };
        var padding = await GetValueAsync(imageButton, GetPlatformPadding);

        Assert.Equal(15, padding.Left);
        Assert.Equal(15, padding.Top);
        Assert.Equal(15, padding.Right);
        Assert.Equal(15, padding.Bottom);
    }

    [AvaloniaFact(DisplayName = "Zero Padding Works")]
    public async Task ZeroPaddingWorks()
    {
        var imageButton = new ImageButtonStub { Padding = new MauiThickness(0) };
        var padding = await GetValueAsync(imageButton, GetPlatformPadding);

        Assert.Equal(0, padding.Left);
        Assert.Equal(0, padding.Top);
        Assert.Equal(0, padding.Right);
        Assert.Equal(0, padding.Bottom);
    }

    [AvaloniaFact(DisplayName = "StrokeColor Initializes Correctly")]
    public async Task StrokeColorInitializesCorrectly()
    {
        var imageButton = new ImageButtonStub
        {
            StrokeColor = Colors.Red
        };

        var platformColor = await GetValueAsync(imageButton, GetPlatformStrokeColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, platformColor);
    }

    [AvaloniaTheory(DisplayName = "StrokeColor Updates Correctly")]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    [InlineData(0, 0, 255)]
    public async Task StrokeColorUpdatesCorrectly(byte r, byte g, byte b)
    {
        var imageButton = new ImageButtonStub { StrokeColor = Colors.Black };
        var newColor = Color.FromRgb(r, g, b);

        var platformColor = await GetValueAsync(imageButton, handler =>
        {
            imageButton.StrokeColor = newColor;
            handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
            return GetPlatformStrokeColor(handler);
        });

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null StrokeColor Doesn't Crash")]
    public async Task NullStrokeColorDoesntCrash()
    {
        var imageButton = new ImageButtonStub { StrokeColor = null! };
        await CreateHandlerAsync(imageButton);
    }

    [AvaloniaFact(DisplayName = "StrokeThickness Initializes Correctly")]
    public async Task StrokeThicknessInitializesCorrectly()
    {
        var imageButton = new ImageButtonStub { StrokeThickness = 5 };
        await ValidatePropertyInitValue(imageButton, () => imageButton.StrokeThickness, GetPlatformStrokeThickness, 5.0);
    }

    [AvaloniaTheory(DisplayName = "StrokeThickness Updates Correctly")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task StrokeThicknessUpdatesCorrectly(double thickness)
    {
        var imageButton = new ImageButtonStub { StrokeThickness = 1 };

        await ValidatePropertyUpdatesValue(
            imageButton,
            nameof(IButtonStroke.StrokeThickness),
            GetPlatformStrokeThickness,
            thickness,
            1.0);
    }

    [AvaloniaFact(DisplayName = "CornerRadius Initializes Correctly")]
    public async Task CornerRadiusInitializesCorrectly()
    {
        var imageButton = new ImageButtonStub { CornerRadius = 10 };
        await ValidatePropertyInitValue(imageButton, () => imageButton.CornerRadius, GetPlatformCornerRadius, 10);
    }

    [AvaloniaTheory(DisplayName = "CornerRadius Updates Correctly")]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task CornerRadiusUpdatesCorrectly(int radius)
    {
        var imageButton = new ImageButtonStub { CornerRadius = 0 };

        await ValidatePropertyUpdatesValue(
            imageButton,
            nameof(IButtonStroke.CornerRadius),
            GetPlatformCornerRadius,
            radius,
            0);
    }

    [AvaloniaFact(DisplayName = "Updating StrokeColor Does Not Affect CornerRadius")]
    public async Task StrokeColorDoesNotAffectCornerRadius()
    {
        var imageButton = new ImageButtonStub
        {
            StrokeColor = Colors.Black,
            CornerRadius = 10
        };

        await ValidateUnrelatedPropertyUnaffected(
            imageButton,
            GetPlatformCornerRadius,
            nameof(IButtonStroke.StrokeColor),
            () => imageButton.StrokeColor = Colors.Red);
    }

    [AvaloniaFact(DisplayName = "Updating Padding Does Not Affect CornerRadius")]
    public async Task PaddingDoesNotAffectCornerRadius()
    {
        var imageButton = new ImageButtonStub
        {
            Padding = new MauiThickness(5),
            CornerRadius = 10
        };

        await ValidateUnrelatedPropertyUnaffected(
            imageButton,
            GetPlatformCornerRadius,
            nameof(IImageButton.Padding),
            () => imageButton.Padding = new MauiThickness(10));
    }

    [AvaloniaFact(DisplayName = "Handler Creates MauiImageButton")]
    public async Task HandlerCreatesMauiImageButton()
    {
        var imageButton = new ImageButtonStub();
        var handler = await CreateHandlerAsync(imageButton);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<MauiImageButton>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Default CornerRadius Is Zero")]
    public async Task DefaultCornerRadiusIsZero()
    {
        var imageButton = new ImageButtonStub();
        var cornerRadius = await GetValueAsync(imageButton, GetPlatformCornerRadius);
        Assert.Equal(0, cornerRadius);
    }

    [AvaloniaFact(DisplayName = "Default StrokeThickness Is Zero")]
    public async Task DefaultStrokeThicknessIsZero()
    {
        var imageButton = new ImageButtonStub();
        var thickness = await GetValueAsync(imageButton, GetPlatformStrokeThickness);
        Assert.Equal(0, thickness);
    }

    [AvaloniaFact(DisplayName = "Click Event Triggers Clicked")]
    public async Task ClickEventTriggersClicked()
    {
        var imageButton = new ImageButtonStub();
        var handler = await CreateHandlerAsync(imageButton);

        handler.PlatformView!.RaiseEvent(new Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        Assert.Equal(1, imageButton.ClickedCount);
    }

    [AvaloniaFact(DisplayName = "PointerPressed Event Triggers Pressed")]
    public async Task PointerPressedEventTriggersPressed()
    {
        var imageButton = new ImageButtonStub();
        var handler = await CreateHandlerAsync(imageButton);

        var args = CreatePointerPressedEventArgs(handler.PlatformView!);
        handler.PlatformView.RaiseEvent(args);

        Assert.Equal(1, imageButton.PressedCount);
    }

    [AvaloniaFact(DisplayName = "PointerReleased Event Triggers Released")]
    public async Task PointerReleasedEventTriggersReleased()
    {
        var imageButton = new ImageButtonStub();
        var handler = await CreateHandlerAsync(imageButton);

        var args = CreatePointerReleasedEventArgs(handler.PlatformView!);
        handler.PlatformView.RaiseEvent(args);

        Assert.Equal(1, imageButton.ReleasedCount);
    }

    [AvaloniaFact(DisplayName = "Multiple Clicks Increment Count")]
    public async Task MultipleClicksIncrementCount()
    {
        var imageButton = new ImageButtonStub();
        var handler = await CreateHandlerAsync(imageButton);

        for (int i = 0; i < 5; i++)
        {
            handler.PlatformView!.RaiseEvent(new Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        }

        Assert.Equal(5, imageButton.ClickedCount);
    }

    [AvaloniaFact(DisplayName = "Full Click Sequence Triggers All Events")]
    public async Task FullClickSequenceTriggersAllEvents()
    {
        var imageButton = new ImageButtonStub();
        var handler = await CreateHandlerAsync(imageButton);
        var platformView = handler.PlatformView!;

        var pressedArgs = CreatePointerPressedEventArgs(platformView);
        platformView.RaiseEvent(pressedArgs);

        var releasedArgs = CreatePointerReleasedEventArgs(platformView);
        platformView.RaiseEvent(releasedArgs);

        platformView.RaiseEvent(new Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        Assert.Equal(1, imageButton.PressedCount);
        Assert.Equal(1, imageButton.ReleasedCount);
        Assert.Equal(1, imageButton.ClickedCount);
    }

    [AvaloniaTheory(DisplayName = "Aspect Initializes Correctly")]
    [InlineData(Aspect.AspectFit, Stretch.Uniform)]
    [InlineData(Aspect.AspectFill, Stretch.UniformToFill)]
    [InlineData(Aspect.Fill, Stretch.Fill)]
    [InlineData(Aspect.Center, Stretch.None)]
    public async Task AspectInitializesCorrectly(Aspect aspect, Stretch expectedStretch)
    {
        var imageButton = new ImageButtonStub { Aspect = aspect };
        var handler = await CreateHandlerAsync(imageButton);

        var image = handler.PlatformView?.GetImage();
        Assert.NotNull(image);
        Assert.Equal(expectedStretch, image.Stretch);
    }

    [AvaloniaTheory(DisplayName = "Aspect Updates Correctly")]
    [InlineData(Aspect.AspectFit, Stretch.Uniform)]
    [InlineData(Aspect.AspectFill, Stretch.UniformToFill)]
    [InlineData(Aspect.Fill, Stretch.Fill)]
    [InlineData(Aspect.Center, Stretch.None)]
    public async Task AspectUpdatesCorrectly(Aspect aspect, Stretch expectedStretch)
    {
        var imageButton = new ImageButtonStub { Aspect = Aspect.AspectFit };
        var handler = await CreateHandlerAsync(imageButton);

        await InvokeOnMainThreadAsync(() =>
        {
            imageButton.Aspect = aspect;
            handler.UpdateValue(nameof(IImage.Aspect));
        });

        var image = handler.PlatformView?.GetImage();
        Assert.NotNull(image);
        Assert.Equal(expectedStretch, image.Stretch);
    }

    [AvaloniaFact(DisplayName = "ImageSource Can Be Set")]
    public async Task ImageSourceCanBeSet()
    {
        var imageButton = new ImageButtonStub();
        var handler = await CreateHandlerAsync(imageButton);

        await InvokeOnMainThreadAsync(() =>
        {
            var bitmap = new RenderTargetBitmap(new PixelSize(10, 10), new Avalonia.Vector(96, 96));
            handler.PlatformView!.ImageSource = bitmap;
        });

        Assert.NotNull(handler.PlatformView?.ImageSource);
    }

    [AvaloniaFact(DisplayName = "ImageSource Can Be Cleared")]
    public async Task ImageSourceCanBeCleared()
    {
        var imageButton = new ImageButtonStub();
        var handler = await CreateHandlerAsync(imageButton);

        await InvokeOnMainThreadAsync(() =>
        {
            var bitmap = new RenderTargetBitmap(new PixelSize(10, 10), new Avalonia.Vector(96, 96));
            handler.PlatformView!.ImageSource = bitmap;
            handler.PlatformView.ImageSource = null;
        });

        Assert.Null(handler.PlatformView?.ImageSource);
    }

    Color? GetPlatformBackgroundColor(MauiImageButtonHandler handler) =>
        handler.PlatformView?.Background is Avalonia.Media.ISolidColorBrush brush
            ? Color.FromRgba(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            : null;

    MauiThickness GetPlatformPadding(MauiImageButtonHandler handler)
    {
        var padding = handler.PlatformView?.Padding ?? default;
        return new MauiThickness(padding.Left, padding.Top, padding.Right, padding.Bottom);
    }

    Color? GetPlatformStrokeColor(MauiImageButtonHandler handler) =>
        handler.PlatformView?.BorderBrush is Avalonia.Media.ISolidColorBrush brush
            ? Color.FromRgba(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            : null;

    double GetPlatformStrokeThickness(MauiImageButtonHandler handler) =>
        handler.PlatformView?.BorderThickness.Left ?? 0;

    int GetPlatformCornerRadius(MauiImageButtonHandler handler) =>
        (int)(handler.PlatformView?.CornerRadius.TopLeft ?? 0);

    Input.PointerPressedEventArgs CreatePointerPressedEventArgs(Visual target)
    {
        var pointer = new Input.Pointer(1, Input.PointerType.Mouse, true);
        var point = new Avalonia.Point(10, 10);
        var properties = new Input.PointerPointProperties(
            Input.RawInputModifiers.None,
            Input.PointerUpdateKind.LeftButtonPressed);

        return new Input.PointerPressedEventArgs(
            target,
            pointer,
            target,
            point,
            0,
            properties,
            Input.KeyModifiers.None);
    }

    Input.PointerReleasedEventArgs CreatePointerReleasedEventArgs(Visual target)
    {
        var pointer = new Input.Pointer(1, Input.PointerType.Mouse, true);
        var point = new Avalonia.Point(10, 10);
        var properties = new Input.PointerPointProperties(
            Input.RawInputModifiers.None,
            Input.PointerUpdateKind.LeftButtonReleased);

        return new Input.PointerReleasedEventArgs(
            target,
            pointer,
            target,
            point,
            0,
            properties,
            Input.KeyModifiers.None,
            Input.MouseButton.Left);
    }
}
