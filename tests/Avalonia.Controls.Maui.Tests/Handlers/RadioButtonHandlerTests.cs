using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiRadioButtonHandler = Avalonia.Controls.Maui.Handlers.RadioButtonHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class RadioButtonHandlerTests : HandlerTestBase<MauiRadioButtonHandler, RadioButtonStub>
{
    [AvaloniaFact(DisplayName = "IsChecked Initializes Correctly")]
    public async Task IsCheckedInitializesCorrectly()
    {
        var radioButton = new RadioButtonStub { IsChecked = true };
        await ValidatePropertyInitValue(radioButton, () => radioButton.IsChecked, GetNativeIsChecked, true);
    }

    [AvaloniaTheory(DisplayName = "IsChecked Updates Correctly")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsCheckedUpdatesCorrectly(bool isChecked)
    {
        var radioButton = new RadioButtonStub { IsChecked = false };

        await ValidatePropertyUpdatesValue(
            radioButton,
            nameof(IRadioButton.IsChecked),
            GetNativeIsChecked,
            isChecked,
            !isChecked);
    }

    [AvaloniaFact(DisplayName = "Content Initializes Correctly")]
    public async Task ContentInitializesCorrectly()
    {
        var radioButton = new RadioButtonStub { Content = "Option A" };
        await ValidatePropertyInitValue(radioButton, () => radioButton.Content, GetNativeContent, "Option A");
    }

    [AvaloniaFact(DisplayName = "StrokeColor Initializes Correctly")]
    public async Task StrokeColorInitializesCorrectly()
    {
        var radioButton = new RadioButtonStub { StrokeColor = Colors.Red };

        var nativeColor = await GetValueAsync(radioButton, GetNativeBorderBrushColor);
        Assert.NotNull(nativeColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, nativeColor);
    }

    [AvaloniaTheory(DisplayName = "StrokeColor Updates Correctly")]
    [InlineData(255, 0, 0)]   // Red
    [InlineData(0, 255, 0)]   // Green
    [InlineData(0, 0, 255)]   // Blue
    public async Task StrokeColorUpdatesCorrectly(byte r, byte g, byte b)
    {
        var radioButton = new RadioButtonStub { StrokeColor = Colors.Black };
        var expectedColor = Color.FromRgb(r, g, b);

        var handler = await CreateHandlerAsync(radioButton);
        await InvokeOnMainThreadAsync(() =>
        {
            radioButton.StrokeColor = expectedColor;
            handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
        });

        var nativeColor = GetNativeBorderBrushColor(handler);
        Assert.NotNull(nativeColor);
        ColorComparisonHelpers.AssertColorsAreEqual(expectedColor, nativeColor);
    }

    [AvaloniaFact(DisplayName = "BorderColor (string) Updates Correctly")]
    public async Task BorderColorStringUpdatesCorrectly()
    {
        var radioButton = new RadioButtonStub { StrokeColor = Colors.Black };
        var expectedColor = Colors.Red;

        var handler = await CreateHandlerAsync(radioButton);
        await InvokeOnMainThreadAsync(() =>
        {
            radioButton.StrokeColor = expectedColor;
            handler.UpdateValue("BorderColor");
        });

        var nativeColor = GetNativeBorderBrushColor(handler);
        Assert.NotNull(nativeColor);
        ColorComparisonHelpers.AssertColorsAreEqual(expectedColor, nativeColor);
    }

    [AvaloniaTheory(DisplayName = "StrokeThickness Updates Correctly")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public async Task StrokeThicknessUpdatesCorrectly(double thickness)
    {
        var radioButton = new RadioButtonStub { StrokeThickness = 0 };

        var handler = await CreateHandlerAsync(radioButton);
        await InvokeOnMainThreadAsync(() =>
        {
            radioButton.StrokeThickness = thickness;
            handler.UpdateValue("BorderWidth");
        });

        Assert.Equal(thickness, GetNativeBorderThickness(handler).Left);
    }

    [AvaloniaTheory(DisplayName = "CornerRadius Updates Correctly")]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(10)]
    public async Task CornerRadiusUpdatesCorrectly(int radius)
    {
        var radioButton = new RadioButtonStub { CornerRadius = 0 };

        var handler = await CreateHandlerAsync(radioButton);
        await InvokeOnMainThreadAsync(() =>
        {
            radioButton.CornerRadius = radius;
            handler.UpdateValue("CornerRadius");
        });

        Assert.Equal(radius, GetNativeCornerRadius(handler).TopLeft);
    }

    [AvaloniaFact(DisplayName = "GroupName Can Be Set On Platform View")]
    public async Task GroupNameCanBeSetOnPlatformView()
    {
        var radioButton = new RadioButtonStub();

        var handler = await CreateHandlerAsync(radioButton);
        var platform = handler.PlatformView;
        Assert.NotNull(platform);

        await InvokeOnMainThreadAsync(() =>
        {
            platform.GroupName = "testGroup";
        });

        Assert.Equal("testGroup", platform.GroupName);
    }

    [AvaloniaFact(DisplayName = "TextColor Initializes Correctly")]
    public async Task TextColorInitializesCorrectly()
    {
        var radioButton = new RadioButtonStub { TextColor = Colors.Purple };

        var nativeColor = await GetValueAsync(radioButton, GetNativeForegroundColor);
        Assert.NotNull(nativeColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Purple, nativeColor);
    }

    [AvaloniaTheory(DisplayName = "TextColor Updates Correctly")]
    [InlineData(255, 0, 0)]   // Red
    [InlineData(0, 255, 0)]   // Green
    [InlineData(0, 0, 255)]   // Blue
    public async Task TextColorUpdatesCorrectly(byte r, byte g, byte b)
    {
        var radioButton = new RadioButtonStub { TextColor = Colors.Black };
        var expectedColor = Color.FromRgb(r, g, b);

        var handler = await CreateHandlerAsync(radioButton);
        await InvokeOnMainThreadAsync(() =>
        {
            radioButton.TextColor = expectedColor;
            handler.UpdateValue(nameof(ITextStyle.TextColor));
        });

        var nativeColor = GetNativeForegroundColor(handler);
        Assert.NotNull(nativeColor);
        ColorComparisonHelpers.AssertColorsAreEqual(expectedColor, nativeColor);
    }

    [AvaloniaTheory(DisplayName = "CharacterSpacing Updates Correctly")]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(5)]
    public async Task CharacterSpacingUpdatesCorrectly(double spacing)
    {
        var radioButton = new RadioButtonStub { Content = "Test", CharacterSpacing = spacing };

        var handler = await CreateHandlerAsync(radioButton);
        await InvokeOnMainThreadAsync(() =>
        {
            // Trigger content update which applies CharacterSpacing
            handler.UpdateValue(nameof(IRadioButton.Content));
        });

        var platform = handler.PlatformView;
        Assert.NotNull(platform);

        // When CharacterSpacing > 0 and content is string, it creates a TextBlock
        if (spacing > 0)
        {
            var textBlock = platform.Content as TextBlock;
            Assert.NotNull(textBlock);
            Assert.Equal(spacing, textBlock.LetterSpacing);
        }
        else
        {
            // When CharacterSpacing is 0, content remains as string
            Assert.Equal("Test", platform.Content);
        }
    }

    [AvaloniaTheory(DisplayName = "FontSize Updates Correctly")]
    [InlineData(10)]
    [InlineData(14)]
    [InlineData(24)]
    public async Task FontSizeUpdatesCorrectly(double fontSize)
    {
        var radioButton = new RadioButtonStub { Font = Microsoft.Maui.Font.Default };

        var handler = await CreateHandlerAsync(radioButton);
        await InvokeOnMainThreadAsync(() =>
        {
            radioButton.Font = Microsoft.Maui.Font.OfSize(string.Empty, fontSize);
            handler.UpdateValue(nameof(ITextStyle.Font));
        });

        var platform = handler.PlatformView;
        Assert.NotNull(platform);
        Assert.Equal(fontSize, platform.FontSize);
    }

    [AvaloniaFact(DisplayName = "ShowIndicator Defaults To True")]
    public async Task ShowIndicatorDefaultsToTrue()
    {
        var radioButton = new RadioButtonStub { Content = "Test" };

        var handler = await CreateHandlerAsync(radioButton);
        var platform = handler.PlatformView;

        Assert.NotNull(platform);
        Assert.True(platform.ShowIndicator);
    }

    [AvaloniaFact(DisplayName = "ShowIndicator Can Be Set To False")]
    public async Task ShowIndicatorCanBeSetToFalse()
    {
        var radioButton = new RadioButtonStub { Content = "Test" };

        var handler = await CreateHandlerAsync(radioButton);
        var platform = handler.PlatformView;
        Assert.NotNull(platform);

        await InvokeOnMainThreadAsync(() =>
        {
            platform.ShowIndicator = false;
        });

        Assert.False(platform.ShowIndicator);
    }

    bool GetNativeIsChecked(MauiRadioButtonHandler handler)
    {
        var platform = handler.PlatformView;
        Assert.NotNull(platform);
        return platform.IsChecked == true;
    }

    object? GetNativeContent(MauiRadioButtonHandler handler)
    {
        var platform = handler.PlatformView;
        Assert.NotNull(platform);
        return platform.Content;
    }

    Color? GetNativeBorderBrushColor(MauiRadioButtonHandler handler)
    {
        var platform = handler.PlatformView;
        Assert.NotNull(platform);

        if (platform.BorderBrush is Media.SolidColorBrush brush)
        {
            var color = brush.Color;
            return Color.FromRgba(color.R, color.G, color.B, color.A);
        }

        return null;
    }

    Color? GetNativeForegroundColor(MauiRadioButtonHandler handler)
    {
        var platform = handler.PlatformView;
        Assert.NotNull(platform);

        if (platform.Foreground is Media.SolidColorBrush brush)
        {
            var color = brush.Color;
            return Color.FromRgba(color.R, color.G, color.B, color.A);
        }

        return null;
    }

    Thickness GetNativeBorderThickness(MauiRadioButtonHandler handler)
    {
        var platform = handler.PlatformView;
        Assert.NotNull(platform);
        return platform.BorderThickness;
    }

    CornerRadius GetNativeCornerRadius(MauiRadioButtonHandler handler)
    {
        var platform = handler.PlatformView;
        Assert.NotNull(platform);
        return platform.CornerRadius;
    }
}
