using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiLabelHandler = Avalonia.Controls.Maui.Handlers.LabelHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class LabelHandlerTests : WindowHandlerTestBase<MauiLabelHandler, MauiLabel>
{
    [AvaloniaFact(DisplayName = "Text Initializes Correctly")]
    public async Task TextInitializesCorrectly()
    {
        var label = new MauiLabel
        {
            Text = "Test"
        };

        await ValidatePropertyInitValue(label, () => label.Text, GetNativeText, label.Text);
    }

    [AvaloniaFact(DisplayName = "Text Color Initializes Correctly")]
    public async Task TextColorInitializesCorrectly()
    {
        var label = new MauiLabel
        {
            Text = "Test",
            TextColor = Colors.Red
        };

        var values = await GetValueAsync(label, (handler) =>
        {
            return new
            {
                ViewValue = label.TextColor,
                PlatformViewValue = GetNativeTextColor(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, values.PlatformViewValue);
    }

    [AvaloniaFact(DisplayName = "Null Text Color Doesn't Crash")]
    public async Task NullTextColorDoesntCrash()
    {
        var label = new MauiLabel
        {
            Text = "Test",
            TextColor = null!
        };

        await CreateHandlerAsync(label);
    }

    [AvaloniaFact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
    public async Task HorizontalTextAlignmentInitializesCorrectly()
    {
        var label = new MauiLabel
        {
            Text = "Test",
            HorizontalTextAlignment = TextAlignment.Center
        };

        await ValidatePropertyInitValue(
            label,
            () => label.HorizontalTextAlignment,
            GetNativeHorizontalTextAlignment,
            label.HorizontalTextAlignment);
    }

    [AvaloniaTheory(DisplayName = "Horizontal TextAlignment Updates Correctly")]
    [InlineData(TextAlignment.Start)]
    [InlineData(TextAlignment.Center)]
    [InlineData(TextAlignment.End)]
    public async Task HorizontalTextAlignmentUpdatesCorrectly(TextAlignment alignment)
    {
        var label = new MauiLabel
        {
            Text = "Test"
        };

        await ValidatePropertyUpdatesValue(
            label,
            nameof(ILabel.HorizontalTextAlignment),
            GetNativeHorizontalTextAlignment,
            alignment,
            TextAlignment.Start);
    }

    [AvaloniaFact(DisplayName = "Padding Initializes Correctly")]
    public async Task PaddingInitializesCorrectly()
    {
        var label = new MauiLabel
        {
            Text = "Test",
            Padding = new Microsoft.Maui.Thickness(5, 10, 15, 20)
        };

        await ValidatePropertyInitValue<Microsoft.Maui.Thickness>(
            label,
            () => label.Padding,
            GetNativePadding,
            label.Padding);
    }

    [AvaloniaFact(DisplayName = "LineHeight Initializes Correctly")]
    public async Task LineHeightInitializesCorrectly()
    {
        var xplatLineHeight = 1.5d;

        var label = new MauiLabel
        {
            Text = "test",
            LineHeight = xplatLineHeight
        };

        var values = await GetValueAsync(label, (handler) =>
        {
            return new
            {
                ViewValue = label.LineHeight,
                PlatformViewValue = GetNativeLineHeight(handler)
            };
        });

        Assert.Equal(xplatLineHeight, values.ViewValue);
        // LineHeight in Avalonia is absolute, should be fontSize * lineHeight
        var expectedPlatformValue = GetNativeFontSize(await CreateHandlerAsync(label)) * xplatLineHeight;
        Assert.Equal(expectedPlatformValue, values.PlatformViewValue, 0.1);
    }

    [AvaloniaTheory(DisplayName = "Updating Font Does Not Affect HorizontalTextAlignment")]
    [InlineData(10, 20)]
    [InlineData(20, 10)]
    public async Task FontDoesNotAffectHorizontalTextAlignment(double initialSize, double newSize)
    {
        var label = new MauiLabel
        {
            Text = "This is TEXT!",
            HorizontalTextAlignment = TextAlignment.Center,
            FontSize = initialSize
        };

        await ValidateUnrelatedPropertyUnaffected(
            label,
            GetNativeHorizontalTextAlignment,
            nameof(ILabel.Font),
            () => label.FontSize = newSize);
    }

    [AvaloniaTheory(DisplayName = "Updating Text Does Not Affect HorizontalTextAlignment")]
    [InlineData("Short", "Longer Text")]
    [InlineData("Long text here", "Short")]
    public async Task TextDoesNotAffectHorizontalTextAlignment(string initialText, string newText)
    {
        var label = new MauiLabel
        {
            Text = initialText,
            HorizontalTextAlignment = TextAlignment.Center,
        };

        await ValidateUnrelatedPropertyUnaffected(
            label,
            GetNativeHorizontalTextAlignment,
            nameof(ILabel.Text),
            () => label.Text = newText);
    }

    [AvaloniaFact(DisplayName = "Background Initializes Correctly")]
    public async Task BackgroundInitializesCorrectly()
    {
        var color = Colors.Blue;
        var label = new MauiLabel
        {
            Text = "Test",
            Background = new SolidPaint(color)
        };

        var platformColor = await GetValueAsync(label, GetNativeBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    // Platform-specific property getters
    string? GetNativeText(MauiLabelHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeText(handler);

    Color? GetNativeTextColor(MauiLabelHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeTextColor(handler);

    double GetNativeFontSize(MauiLabelHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeFontSize(handler);

    TextAlignment GetNativeHorizontalTextAlignment(MauiLabelHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeHorizontalTextAlignment(handler);

    Microsoft.Maui.Thickness GetNativePadding(MauiLabelHandler handler) =>
        AvaloniaPropertyHelpers.GetNativePadding(handler);

    double GetNativeLineHeight(MauiLabelHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeLineHeight(handler);

    Color? GetNativeBackgroundColor(MauiLabelHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeBackgroundColor(handler);
}
