using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiLabelHandler = Avalonia.Controls.Maui.Handlers.LabelHandler;
using Avalonia.Controls.Maui.Platform;

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
                PlatformViewValue = GetNativeLineHeight(handler),
                FontSize = GetNativeFontSize(handler)
            };
        });

        Assert.Equal(xplatLineHeight, values.ViewValue);
        // LineHeight in Avalonia is absolute, should be fontSize * lineHeight
        var expectedPlatformValue = values.FontSize * xplatLineHeight;
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

    [AvaloniaFact(DisplayName = "Text Update Changes Label Desired Width")]
    public async Task TextUpdateChangesLabelDesiredWidth()
    {
        var label = new MauiLabel
        {
            Text = "Hi"
        };

        // CreateHandlerAsync places the label in a MAUI Window, so it's already in a visual tree
        var handler = await CreateHandlerAsync(label);

        var result = await InvokeOnMainThreadAsync(() =>
        {
            var textBlock = (TextBlock)handler.PlatformView;

            // Measure initial text - control is already in a window via WindowHandlerTestBase
            textBlock.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
            var initial = textBlock.DesiredSize.Width;

            // Update to longer text
            label.Text = "This is a much longer text that should definitely be wider than Hi";
            handler.UpdateValue(nameof(ILabel.Text));

            // Re-measure after text change
            textBlock.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
            var updated = textBlock.DesiredSize.Width;

            return (Initial: initial, Updated: updated);
        });

        Assert.True(result.Updated > result.Initial, $"Expected new width ({result.Updated}) to be greater than initial width ({result.Initial})");
    }

        [AvaloniaFact(DisplayName = "UpdatingLabel In VerticalStackLayout Measures Correctly")]
    public async Task UpdatingLabelInVerticalStackLayoutMeasuresCorrectly()
    {
        var label = new MauiLabel
        {
            Text = "Hello World"
        };

        var stackLayout = new VerticalStackLayout
        {
            Children = { label }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(stackLayout);

        var initialResult = await InvokeOnMainThreadAsync(() =>
        {
            var stackPanel = (LayoutPanel)layoutHandler.PlatformView;
            var textBlock = (TextBlock)label.Handler!.PlatformView!;

            // Measure the layout
            stackPanel.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            return new
            {
                LabelWidth = textBlock.DesiredSize.Width,
                LabelHeight = textBlock.DesiredSize.Height,
                LayoutWidth = stackPanel.DesiredSize.Width,
                LayoutHeight = stackPanel.DesiredSize.Height
            };
        });

        // Update the label text to something longer
        var newText = "Hello World! This is an updated text that is significantly longer.";
        await InvokeOnMainThreadAsync(() =>
        {
            label.Text = newText;
            label.FontSize = 24;
            label.Handler!.UpdateValue(nameof(ILabel.Text));
            label.Handler!.UpdateValue(nameof(ILabel.Font));
        }); 

        var updatedResult = await InvokeOnMainThreadAsync(() =>
        {
            var stackPanel = (LayoutPanel)layoutHandler.PlatformView;
            var textBlock = (TextBlock)label.Handler!.PlatformView!;

            // Re-measure the layout
            stackPanel.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            return new
            {
                LabelWidth = textBlock.DesiredSize.Width,
                LabelHeight = textBlock.DesiredSize.Height,
                LayoutWidth = stackPanel.DesiredSize.Width,
                LayoutHeight = stackPanel.DesiredSize.Height
            };
        });

        // Label should have non-zero dimensions and be larger than before
        Assert.True(updatedResult.LabelWidth > 0, $"Label width should be > 0, was {updatedResult.LabelWidth}");
        Assert.True(updatedResult.LabelHeight > 0, $"Label height should be > 0, was {updatedResult.LabelHeight}");
        Assert.True(updatedResult.LabelWidth > initialResult.LabelWidth,
            $"Updated label width ({updatedResult.LabelWidth}) should be > initial label width ({initialResult.LabelWidth})");
        Assert.True(updatedResult.LabelHeight > initialResult.LabelHeight,
            $"Updated label height ({updatedResult.LabelHeight}) should be > initial label height ({initialResult.LabelHeight})");
    }

    [AvaloniaFact(DisplayName = "Label In VerticalStackLayout Measures Correctly")]
    public async Task LabelInVerticalStackLayoutMeasuresCorrectly()
    {
        var label = new MauiLabel
        {
            Text = "Hello World"
        };

        var stackLayout = new VerticalStackLayout
        {
            Children = { label }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(stackLayout);

        var result = await InvokeOnMainThreadAsync(() =>
        {
            var stackPanel = (LayoutPanel)layoutHandler.PlatformView;
            var textBlock = (TextBlock)label.Handler!.PlatformView!;

            // Measure the layout
            stackPanel.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            return new
            {
                LabelWidth = textBlock.DesiredSize.Width,
                LabelHeight = textBlock.DesiredSize.Height,
                LayoutWidth = stackPanel.DesiredSize.Width,
                LayoutHeight = stackPanel.DesiredSize.Height
            };
        });

        // Label should have non-zero dimensions
        Assert.True(result.LabelWidth > 0, $"Label width should be > 0, was {result.LabelWidth}");
        Assert.True(result.LabelHeight > 0, $"Label height should be > 0, was {result.LabelHeight}");

        // Layout should be at least as big as the label
        Assert.True(result.LayoutWidth >= result.LabelWidth,
            $"Layout width ({result.LayoutWidth}) should be >= label width ({result.LabelWidth})");
        Assert.True(result.LayoutHeight >= result.LabelHeight,
            $"Layout height ({result.LayoutHeight}) should be >= label height ({result.LabelHeight})");
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
