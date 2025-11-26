using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.VisualTree;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiLabelHandler = Avalonia.Controls.Maui.Handlers.LabelHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class LabelHandlerTests : HandlerTestBase<MauiLabelHandler, LabelStub>
{
    [AvaloniaFact(DisplayName = "Text Initializes Correctly")]
    public async Task TextInitializesCorrectly()
    {
        var label = new LabelStub
        {
            Text = "Test"
        };

        await ValidatePropertyInitValue(label, () => label.Text, GetNativeText, label.Text);
    }

    [AvaloniaFact(DisplayName = "Text Color Initializes Correctly")]
    public async Task TextColorInitializesCorrectly()
    {
        var label = new LabelStub
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
        var label = new LabelStub
        {
            Text = "Test",
            TextColor = null!
        };

        await CreateHandlerAsync(label);
    }

    [AvaloniaFact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
    public async Task HorizontalTextAlignmentInitializesCorrectly()
    {
        var label = new LabelStub
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
        var label = new LabelStub
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
        var label = new LabelStub
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

        var label = new LabelStub
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
        var label = new LabelStub
        {
            Text = "This is TEXT!",
            HorizontalTextAlignment = TextAlignment.Center,
            Font = Microsoft.Maui.Font.SystemFontOfSize(initialSize),
        };

        await ValidateUnrelatedPropertyUnaffected(
            label,
            GetNativeHorizontalTextAlignment,
            nameof(ILabel.Font),
            () => label.Font = Microsoft.Maui.Font.SystemFontOfSize(newSize));
    }

    [AvaloniaTheory(DisplayName = "Updating Text Does Not Affect HorizontalTextAlignment")]
    [InlineData("Short", "Longer Text")]
    [InlineData("Long text here", "Short")]
    public async Task TextDoesNotAffectHorizontalTextAlignment(string initialText, string newText)
    {
        var label = new LabelStub
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
        var label = new LabelStub
        {
            Text = "Test",
            Background = new SolidPaint(color)
        };

        var platformColor = await GetValueAsync(label, GetNativeBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Text Update Changes Label Desired Width")]
    public async Task TextUpdateChangesLabelDesiredWidth()
    {
        var label = new LabelStub
        {
            Text = "Hi"
        };

        var handler = await CreateHandlerAsync<MauiLabelHandler>(label);

        var result = await InvokeOnMainThreadAsync(() =>
        {
            var textBlock = (TextBlock)handler.PlatformView;

            // Attach to a window for proper text measurement
            var window = new Window { Content = textBlock };
            window.Show();

            // Measure initial text
            textBlock.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
            var initial = textBlock.DesiredSize.Width;

            // Update to longer text
            label.Text = "This is a much longer text that should definitely be wider than Hi";
            handler.UpdateValue(nameof(ILabel.Text));

            // Re-measure after text change (MapText calls InvalidateMeasure)
            textBlock.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
            var updated = textBlock.DesiredSize.Width;

            window.Close();
            return (Initial: initial, Updated: updated);
        });

        Assert.True(result.Updated > result.Initial, $"Expected new width ({result.Updated}) to be greater than initial width ({result.Initial})");
    }

    [AvaloniaFact(DisplayName = "Text Update To Shorter Text Changes Label Desired Width")]
    public async Task TextUpdateToShorterTextChangesLabelDesiredWidth()
    {
        var label = new LabelStub
        {
            Text = "This is a very long text that takes up a lot of horizontal space"
        };

        var handler = await CreateHandlerAsync<MauiLabelHandler>(label);

        var result = await InvokeOnMainThreadAsync(() =>
        {
            var textBlock = (TextBlock)handler.PlatformView;

            // Attach to a window for proper text measurement
            var window = new Window { Content = textBlock };
            window.Show();

            // Measure initial text
            textBlock.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
            var initial = textBlock.DesiredSize.Width;

            // Update to shorter text
            label.Text = "Hi";
            handler.UpdateValue(nameof(ILabel.Text));

            // Re-measure after text change (MapText calls InvalidateMeasure)
            textBlock.Measure(new global::Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
            var updated = textBlock.DesiredSize.Width;

            window.Close();
            return (Initial: initial, Updated: updated);
        });

        Assert.True(result.Updated < result.Initial, $"Expected new width ({result.Updated}) to be less than initial width ({result.Initial})");
    }

    [AvaloniaFact(DisplayName = "Text Update Reflects In Native Text")]
    public async Task TextUpdateReflectsInNativeText()
    {
        var label = new LabelStub
        {
            Text = "Initial"
        };

        var handler = await CreateHandlerAsync<MauiLabelHandler>(label);

        var initialText = await InvokeOnMainThreadAsync(() => GetNativeText(handler));
        Assert.Equal("Initial", initialText);

        var updatedText = await InvokeOnMainThreadAsync(() =>
        {
            label.Text = "Updated";
            handler.UpdateValue(nameof(ILabel.Text));
            return GetNativeText(handler);
        });

        Assert.Equal("Updated", updatedText);
    }

    [AvaloniaFact(DisplayName = "Multiple Text Updates Reflect Correctly")]
    public async Task MultipleTextUpdatesReflectCorrectly()
    {
        var label = new LabelStub
        {
            Text = "First"
        };

        var handler = await CreateHandlerAsync<MauiLabelHandler>(label);

        // Update multiple times
        var texts = new[] { "Second", "Third", "Fourth and final text" };

        foreach (var text in texts)
        {
            var updatedText = await InvokeOnMainThreadAsync(() =>
            {
                label.Text = text;
                handler.UpdateValue(nameof(ILabel.Text));
                return GetNativeText(handler);
            });

            Assert.Equal(text, updatedText);
        }
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
