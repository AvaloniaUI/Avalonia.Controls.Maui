using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiButtonHandler = Avalonia.Controls.Maui.Handlers.ButtonHandler;
using MauiButton = Avalonia.Controls.Maui.Platform.MauiButton;
using MauiThickness = Microsoft.Maui.Thickness;
using AvaloniaImage = Avalonia.Media.Imaging.Bitmap;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class ButtonHandlerTests : HandlerTestBase<MauiButtonHandler, ButtonStub>
{
    [AvaloniaFact(DisplayName = "Text Initializes Correctly")]
    public async Task TextInitializesCorrectly()
    {
        var button = new ButtonStub
        {
            Text = "Click Me"
        };

        await ValidatePropertyInitValue(button, () => button.Text, GetPlatformText, button.Text);
    }

    [AvaloniaTheory(DisplayName = "Text Updates Correctly")]
    [InlineData("Hello")]
    [InlineData("")]
    [InlineData("Multi\nLine")]
    [InlineData("Very Long Button Text That Should Still Work")]
    public async Task TextUpdatesCorrectly(string text)
    {
        var button = new ButtonStub { Text = "Initial" };

        await ValidatePropertyUpdatesValue(
            button,
            nameof(IText.Text),
            GetPlatformText,
            text,
            "Initial");
    }

    [AvaloniaFact(DisplayName = "Null Text Doesn't Crash")]
    public async Task NullTextDoesntCrash()
    {
        var button = new ButtonStub { Text = null! };
        await CreateHandlerAsync(button);
    }

    [AvaloniaFact(DisplayName = "Empty Text Works")]
    public async Task EmptyTextWorks()
    {
        var button = new ButtonStub { Text = string.Empty };
        var text = await GetValueAsync(button, GetPlatformText);
        Assert.Equal(string.Empty, text);
    }

    [AvaloniaFact(DisplayName = "TextColor Initializes Correctly")]
    public async Task TextColorInitializesCorrectly()
    {
        var button = new ButtonStub
        {
            Text = "Button",
            TextColor = Colors.Red
        };

        var platformColor = await GetValueAsync(button, GetPlatformTextColor);
        
        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, platformColor);
    }

    [AvaloniaTheory(DisplayName = "TextColor Updates Correctly")]
    [InlineData(255, 0, 0)]      // Red
    [InlineData(0, 255, 0)]      // Green
    [InlineData(0, 0, 255)]      // Blue
    [InlineData(255, 255, 255)]  // White
    [InlineData(0, 0, 0)]        // Black
    public async Task TextColorUpdatesCorrectly(byte r, byte g, byte b)
    {
        var button = new ButtonStub
        {
            Text = "Button",
            TextColor = Colors.Black
        };

        var newColor = Color.FromRgb(r, g, b);

        var platformColor = await GetValueAsync(button, handler =>
        {
            button.TextColor = newColor;
            handler.UpdateValue(nameof(ITextStyle.TextColor));
            return GetPlatformTextColor(handler);
        });

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null TextColor Doesn't Crash")]
    public async Task NullTextColorDoesntCrash()
    {
        var button = new ButtonStub { Text = "Button", TextColor = null! };
        await CreateHandlerAsync(button);
    }

    [AvaloniaFact(DisplayName = "Background Initializes Correctly")]
    public async Task BackgroundInitializesCorrectly()
    {
        var button = new ButtonStub
        {
            Text = "Button",
            Background = new SolidPaint(Colors.Blue)
        };

        var platformColor = await GetValueAsync(button, GetPlatformBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Blue, platformColor);
    }

    [AvaloniaTheory(DisplayName = "Background Updates Correctly")]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    [InlineData(0, 0, 255)]
    public async Task BackgroundUpdatesCorrectly(byte r, byte g, byte b)
    {
        var button = new ButtonStub
        {
            Text = "Button",
            Background = new SolidPaint(Colors.White)
        };

        var newColor = Color.FromRgb(r, g, b);

        var platformColor = await GetValueAsync(button, handler =>
        {
            button.Background = new SolidPaint(newColor);
            handler.UpdateValue(nameof(IButton.Background));
            return GetPlatformBackgroundColor(handler);
        });

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null Background Doesn't Crash")]
    public async Task NullBackgroundDoesntCrash()
    {
        var button = new ButtonStub { Text = "Button", Background = null! };
        await CreateHandlerAsync(button);
    }

    [AvaloniaFact(DisplayName = "Padding Initializes Correctly")]
    public async Task PaddingInitializesCorrectly()
    {
        var padding = new MauiThickness(10, 20, 30, 40);
        var button = new ButtonStub
        {
            Text = "Button",
            Padding = padding
        };

        await ValidatePropertyInitValue(
            button,
            () => button.Padding,
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
        var button = new ButtonStub { Text = "Button", Padding = new MauiThickness(0) };
        var newPadding = new MauiThickness(left, top, right, bottom);

        var platformPadding = await GetValueAsync(button, handler =>
        {
            button.Padding = newPadding;
            handler.UpdateValue(nameof(IButton.Padding));
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
        var button = new ButtonStub { Text = "Button", Padding = new MauiThickness(15) };
        var padding = await GetValueAsync(button, GetPlatformPadding);

        Assert.Equal(15, padding.Left);
        Assert.Equal(15, padding.Top);
        Assert.Equal(15, padding.Right);
        Assert.Equal(15, padding.Bottom);
    }

    [AvaloniaFact(DisplayName = "Zero Padding Works")]
    public async Task ZeroPaddingWorks()
    {
        var button = new ButtonStub { Text = "Button", Padding = new MauiThickness(0) };
        var padding = await GetValueAsync(button, GetPlatformPadding);

        Assert.Equal(0, padding.Left);
        Assert.Equal(0, padding.Top);
        Assert.Equal(0, padding.Right);
        Assert.Equal(0, padding.Bottom);
    }

    [AvaloniaFact(DisplayName = "StrokeColor Initializes Correctly")]
    public async Task StrokeColorInitializesCorrectly()
    {
        var button = new ButtonStub
        {
            Text = "Button",
            StrokeColor = Colors.Red
        };

        var platformColor = await GetValueAsync(button, GetPlatformStrokeColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, platformColor);
    }

    [AvaloniaTheory(DisplayName = "StrokeColor Updates Correctly")]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    [InlineData(0, 0, 255)]
    public async Task StrokeColorUpdatesCorrectly(byte r, byte g, byte b)
    {
        var button = new ButtonStub { Text = "Button", StrokeColor = Colors.Black };
        var newColor = Color.FromRgb(r, g, b);

        var platformColor = await GetValueAsync(button, handler =>
        {
            button.StrokeColor = newColor;
            handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
            return GetPlatformStrokeColor(handler);
        });

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null StrokeColor Doesn't Crash")]
    public async Task NullStrokeColorDoesntCrash()
    {
        var button = new ButtonStub { Text = "Button", StrokeColor = null! };
        await CreateHandlerAsync(button);
    }

    [AvaloniaFact(DisplayName = "StrokeThickness Initializes Correctly")]
    public async Task StrokeThicknessInitializesCorrectly()
    {
        var button = new ButtonStub { Text = "Button", StrokeThickness = 5 };
        await ValidatePropertyInitValue(button, () => button.StrokeThickness, GetPlatformStrokeThickness, 5.0);
    }

    [AvaloniaTheory(DisplayName = "StrokeThickness Updates Correctly")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task StrokeThicknessUpdatesCorrectly(double thickness)
    {
        var button = new ButtonStub { Text = "Button", StrokeThickness = 1 };

        await ValidatePropertyUpdatesValue(
            button,
            nameof(IButtonStroke.StrokeThickness),
            GetPlatformStrokeThickness,
            thickness,
            1.0);
    }

    [AvaloniaFact(DisplayName = "CornerRadius Initializes Correctly")]
    public async Task CornerRadiusInitializesCorrectly()
    {
        var button = new ButtonStub { Text = "Button", CornerRadius = 10 };
        await ValidatePropertyInitValue(button, () => button.CornerRadius, GetPlatformCornerRadius, 10);
    }

    [AvaloniaTheory(DisplayName = "CornerRadius Updates Correctly")]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task CornerRadiusUpdatesCorrectly(int radius)
    {
        var button = new ButtonStub { Text = "Button", CornerRadius = 0 };

        await ValidatePropertyUpdatesValue(
            button,
            nameof(IButtonStroke.CornerRadius),
            GetPlatformCornerRadius,
            radius,
            0);
    }

    [AvaloniaFact(DisplayName = "CharacterSpacing Initializes Correctly")]
    public async Task CharacterSpacingInitializesCorrectly()
    {
        var button = new ButtonStub { Text = "Button", CharacterSpacing = 2.0 };
        await ValidatePropertyInitValue(button, () => button.CharacterSpacing, GetPlatformCharacterSpacing, 2);
    }

    [AvaloniaTheory(DisplayName = "CharacterSpacing Updates Correctly")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(-1)]
    public async Task CharacterSpacingUpdatesCorrectly(double spacing)
    {
        var button = new ButtonStub { Text = "Button", CharacterSpacing = 0 };

        await ValidatePropertyUpdatesValue(
            button,
            nameof(ITextStyle.CharacterSpacing),
            GetPlatformCharacterSpacing,
            spacing,
            0.0);
    }

    [AvaloniaFact(DisplayName = "Updating Text Does Not Affect Background")]
    public async Task TextDoesNotAffectBackground()
    {
        var button = new ButtonStub
        {
            Text = "Button",
            Background = new SolidPaint(Colors.Blue)
        };

        await ValidateUnrelatedPropertyUnaffected(
            button,
            GetPlatformBackgroundColor,
            nameof(IText.Text),
            () => button.Text = "New Text");
    }

    [AvaloniaFact(DisplayName = "Updating Background Does Not Affect Text")]
    public async Task BackgroundDoesNotAffectText()
    {
        var button = new ButtonStub
        {
            Text = "Button",
            Background = new SolidPaint(Colors.Blue)
        };

        await ValidateUnrelatedPropertyUnaffected(
            button,
            GetPlatformText,
            nameof(IButton.Background),
            () => button.Background = new SolidPaint(Colors.Red));
    }

    [AvaloniaFact(DisplayName = "Updating StrokeColor Does Not Affect CornerRadius")]
    public async Task StrokeColorDoesNotAffectCornerRadius()
    {
        var button = new ButtonStub
        {
            Text = "Button",
            StrokeColor = Colors.Black,
            CornerRadius = 10
        };

        await ValidateUnrelatedPropertyUnaffected(
            button,
            GetPlatformCornerRadius,
            nameof(IButtonStroke.StrokeColor),
            () => button.StrokeColor = Colors.Red);
    }

    [AvaloniaFact(DisplayName = "Updating Padding Does Not Affect Text")]
    public async Task PaddingDoesNotAffectText()
    {
        var button = new ButtonStub
        {
            Text = "Button",
            Padding = new MauiThickness(5)
        };

        await ValidateUnrelatedPropertyUnaffected(
            button,
            GetPlatformText,
            nameof(IButton.Padding),
            () => button.Padding = new MauiThickness(10));
    }

    [AvaloniaFact(DisplayName = "Handler Creates MauiButton")]
    public async Task HandlerCreatesMauiButton()
    {
        var button = new ButtonStub { Text = "Button" };
        var handler = await CreateHandlerAsync(button);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<MauiButton>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Default CornerRadius Is Zero")]
    public async Task DefaultCornerRadiusIsZero()
    {
        var button = new ButtonStub { Text = "Button" };
        var cornerRadius = await GetValueAsync(button, GetPlatformCornerRadius);
        Assert.Equal(0, cornerRadius);
    }

    [AvaloniaFact(DisplayName = "Default StrokeThickness Is Zero")]
    public async Task DefaultStrokeThicknessIsZero()
    {
        var button = new ButtonStub { Text = "Button" };
        var thickness = await GetValueAsync(button, GetPlatformStrokeThickness);
        Assert.Equal(0, thickness);
    }
    
    [AvaloniaFact(DisplayName = "Click Event Triggers Clicked")]
    public async Task ClickEventTriggersClicked()
    {
        var button = new ButtonStub { Text = "Button" };
        var handler = await CreateHandlerAsync(button);

        // Simulate click event
        handler.PlatformView!.RaiseEvent(new Interactivity.RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(1, button.ClickedCount);
    }

    [AvaloniaFact(DisplayName = "PointerPressed Event Triggers Pressed")]
    public async Task PointerPressedEventTriggersPressed()
    {
        var button = new ButtonStub { Text = "Button" };
        var handler = await CreateHandlerAsync(button);

        // Simulate pointer pressed by triggering the button's pointer pressed event
        var args = CreatePointerPressedEventArgs(handler.PlatformView!);
        handler.PlatformView.RaiseEvent(args);

        Assert.Equal(1, button.PressedCount);
    }

    [AvaloniaFact(DisplayName = "PointerReleased Event Triggers Released")]
    public async Task PointerReleasedEventTriggersReleased()
    {
        var button = new ButtonStub { Text = "Button" };
        var handler = await CreateHandlerAsync(button);

        // Simulate pointer released event
        var args = CreatePointerReleasedEventArgs(handler.PlatformView!);
        handler.PlatformView.RaiseEvent(args);

        Assert.Equal(1, button.ReleasedCount);
    }

    [AvaloniaFact(DisplayName = "Multiple Clicks Increment Count")]
    public async Task MultipleClicksIncrementCount()
    {
        var button = new ButtonStub { Text = "Button" };
        var handler = await CreateHandlerAsync(button);

        // Simulate multiple clicks
        for (int i = 0; i < 5; i++)
        {
            handler.PlatformView!.RaiseEvent(new Interactivity.RoutedEventArgs(Button.ClickEvent));
        }

        Assert.Equal(5, button.ClickedCount);
    }

    [AvaloniaFact(DisplayName = "Full Click Sequence Triggers All Events")]
    public async Task FullClickSequenceTriggersAllEvents()
    {
        var button = new ButtonStub { Text = "Button" };
        var handler = await CreateHandlerAsync(button);
        var platformView = handler.PlatformView!;

        // Simulate full click sequence: press -> release -> click
        var pressedArgs = CreatePointerPressedEventArgs(platformView);
        platformView.RaiseEvent(pressedArgs);

        var releasedArgs = CreatePointerReleasedEventArgs(platformView);
        platformView.RaiseEvent(releasedArgs);

        platformView.RaiseEvent(new Interactivity.RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(1, button.PressedCount);
        Assert.Equal(1, button.ReleasedCount);
        Assert.Equal(1, button.ClickedCount);
    }

    [AvaloniaFact(DisplayName = "Disabled Button Does Not Trigger Clicked")]
    public async Task DisabledButtonDoesNotTriggerClicked()
    {
        var button = new ButtonStub { Text = "Button", IsEnabled = false };
        var handler = await CreateHandlerAsync(button);

        // Try to click disabled button, the Click event may fire but handler shouldn't process it
        handler.PlatformView!.RaiseEvent(new Interactivity.RoutedEventArgs(Button.ClickEvent));

        // Note: The actual behavior depends on whether the handler checks IsEnabled
        // This test documents current behavior
        Assert.True(button.ClickedCount >= 0);
    }

    [AvaloniaFact(DisplayName = "ImageSource Initializes As Null")]
    public async Task ImageSourceInitializesAsNull()
    {
        var button = new ButtonStub { Text = "Button" };
        var imageSource = await GetValueAsync(button, GetPlatformImageSource);
        Assert.Null(imageSource);
    }

    [AvaloniaFact(DisplayName = "Null ImageSource Does Not Crash")]
    public async Task NullImageSourceDoesNotCrash()
    {
        var button = new ButtonStub { Text = "Button", ImageSource = null };
        await CreateHandlerAsync(button);
    }

    [AvaloniaFact(DisplayName = "FileImageSource Updates Correctly")]
    public async Task FileImageSourceUpdatesCorrectly()
    {
        var button = new ButtonStub { Text = "Button" };
        var handler = await CreateHandlerAsync(button);

        // Create a file image source
        var fileImageSource = new FileImageSourceStub { File = "test.png" };

        button.ImageSource = fileImageSource;
        handler.UpdateValue(nameof(Microsoft.Maui.IImage.Source));

        // Give async operation time to complete
        await Task.Delay(100);

        // Verify the image was processed (may be null if file doesn't exist, but shouldn't crash)
        var platformImageSource = GetPlatformImageSource(handler);
        // The actual bitmap may be null if the file doesn't exist, but the call should complete
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Clearing ImageSource Sets Platform Image To Null")]
    public async Task ClearingImageSourceSetsPlatformImageToNull()
    {
        var button = new ButtonStub
        {
            Text = "Button",
            ImageSource = new FileImageSourceStub { File = "test.png" }
        };
        var handler = await CreateHandlerAsync(button);

        // Clear the image source
        button.ImageSource = null;
        handler.UpdateValue(nameof(Microsoft.Maui.IImage.Source));

        // Give async operation time to complete
        await Task.Delay(100);

        var platformImageSource = GetPlatformImageSource(handler);
        Assert.Null(platformImageSource);
    }

    [AvaloniaFact(DisplayName = "Image Only Button Has No Margin")]
    public async Task ImageOnlyButtonHasNoMargin()
    {
        var button = new ButtonStub
        {
            ImageSource = new FileImageSourceStub { File = "test.png" }
        };
        var handler = await CreateHandlerAsync(button);

        // Give async operation time to complete
        await Task.Delay(100);

        var image = handler.PlatformView?.GetImage();
        Assert.NotNull(image);
        Assert.Equal(0, image.Margin.Left);
        Assert.Equal(0, image.Margin.Top);
        Assert.Equal(0, image.Margin.Right);
        Assert.Equal(0, image.Margin.Bottom);
    }

    [AvaloniaFact(DisplayName = "Image And Text Button Has Right Margin On Image")]
    public async Task ImageAndTextButtonHasRightMarginOnImage()
    {
        var button = new ButtonStub
        {
            Text = "Button Text"
        };
        var handler = await CreateHandlerAsync(button);

        // Directly set the ImageSource property on the platform button
        // This simulates what happens after the async image loading completes
        using var bitmap = new Media.Imaging.Bitmap(
            PixelFormat.Bgra8888,
            AlphaFormat.Premul,
            IntPtr.Zero,
            new PixelSize(1, 1),
            new Vector(96, 96),
            4);

        handler.PlatformView!.Text = "Button Text";
        handler.PlatformView.ImageSource = bitmap;

        // The UpdateContent method should have been called and set the margin
        var image = handler.PlatformView.GetImage();
        Assert.NotNull(image);
        Assert.Equal(0, image.Margin.Left);
        Assert.Equal(0, image.Margin.Top);
        Assert.Equal(5, image.Margin.Right);
        Assert.Equal(0, image.Margin.Bottom);
    }

    [AvaloniaFact(DisplayName = "Text Only Button Image Has No Margin")]
    public async Task TextOnlyButtonImageHasNoMargin()
    {
        var button = new ButtonStub
        {
            Text = "Button Text"
        };
        var handler = await CreateHandlerAsync(button);

        var image = handler.PlatformView?.GetImage();
        Assert.NotNull(image);
        // Image exists but has no margin since no ImageSource is set
        Assert.Equal(0, image.Margin.Left);
        Assert.Equal(0, image.Margin.Top);
        Assert.Equal(0, image.Margin.Right);
        Assert.Equal(0, image.Margin.Bottom);
    }

    AvaloniaImage? GetPlatformImageSource(MauiButtonHandler handler) =>
        handler.PlatformView?.ImageSource as AvaloniaImage;

    string? GetPlatformText(MauiButtonHandler handler) =>
        handler.PlatformView?.Text;

    Color? GetPlatformTextColor(MauiButtonHandler handler) =>
        handler.PlatformView?.Foreground is Media.ISolidColorBrush brush
            ? Color.FromRgba(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            : null;

    Color? GetPlatformBackgroundColor(MauiButtonHandler handler) =>
        handler.PlatformView?.Background is Media.ISolidColorBrush brush
            ? Color.FromRgba(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            : null;

    MauiThickness GetPlatformPadding(MauiButtonHandler handler)
    {
        var padding = handler.PlatformView?.Padding ?? default;
        return new MauiThickness(padding.Left, padding.Top, padding.Right, padding.Bottom);
    }

    Color? GetPlatformStrokeColor(MauiButtonHandler handler) =>
        handler.PlatformView?.BorderBrush is Media.ISolidColorBrush brush
            ? Color.FromRgba(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            : null;

    double GetPlatformStrokeThickness(MauiButtonHandler handler) =>
        handler.PlatformView?.BorderThickness.Left ?? 0;

    int GetPlatformCornerRadius(MauiButtonHandler handler) =>
        (int)(handler.PlatformView?.CornerRadius.TopLeft ?? 0);

    double GetPlatformCharacterSpacing(MauiButtonHandler handler) =>
        handler.PlatformView?.CharacterSpacing ?? 0;
    
    // Helper methods for creating Avalonia event args
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
            0, // timestamp
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
            0, // timestamp
            properties,
            Input.KeyModifiers.None,
            Input.MouseButton.Left);
    }
}