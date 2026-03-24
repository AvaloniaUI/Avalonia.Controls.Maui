using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
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


    int GetNativeInlineCount(MauiLabelHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeInlineCount(handler);

    string? GetNativeRunText(MauiLabelHandler handler, int index) =>
        AvaloniaPropertyHelpers.GetNativeRunText(handler, index);

    Color? GetNativeRunForeground(MauiLabelHandler handler, int index) =>
        AvaloniaPropertyHelpers.GetNativeRunForeground(handler, index);

    [AvaloniaTheory(DisplayName = "TextTransform Uppercase Initializes Correctly")]
    [InlineData("hello world", "HELLO WORLD")]
    [InlineData("Test", "TEST")]
    [InlineData("MixedCase", "MIXEDCASE")]
    public async Task TextTransformUppercaseInitializesCorrectly(string input, string expected)
    {
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = input,
            TextTransform = Microsoft.Maui.TextTransform.Uppercase,
            WidthRequest = 200,
            HeightRequest = 50
        };

        var nativeText = await GetValueAsync<string?, MauiLabelHandler>(label, GetNativeText);

        Assert.Equal(expected, nativeText);
    }

    [AvaloniaTheory(DisplayName = "TextTransform Lowercase Initializes Correctly")]
    [InlineData("HELLO WORLD", "hello world")]
    [InlineData("Test", "test")]
    [InlineData("MixedCase", "mixedcase")]
    public async Task TextTransformLowercaseInitializesCorrectly(string input, string expected)
    {
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = input,
            TextTransform = Microsoft.Maui.TextTransform.Lowercase,
            WidthRequest = 200,
            HeightRequest = 50
        };

        var nativeText = await GetValueAsync<string?, MauiLabelHandler>(label, GetNativeText);

        Assert.Equal(expected, nativeText);
    }

    [AvaloniaFact(DisplayName = "TextTransform None Preserves Original Text")]
    public async Task TextTransformNonePreservesOriginalText()
    {
        var originalText = "MixedCase Text";
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = originalText,
            TextTransform = Microsoft.Maui.TextTransform.None,
            WidthRequest = 200,
            HeightRequest = 50
        };

        var nativeText = await GetValueAsync<string?, MauiLabelHandler>(label, GetNativeText);

        Assert.Equal(originalText, nativeText);
    }

    [AvaloniaFact(DisplayName = "TextTransform Updates When Changed")]
    public async Task TextTransformUpdatesWhenChanged()
    {
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = "Hello",
            TextTransform = Microsoft.Maui.TextTransform.None,
            WidthRequest = 200,
            HeightRequest = 50
        };

        var handler = await CreateHandlerAsync<MauiLabelHandler>(label);

        // Initial value should be unchanged
        var initialText = await InvokeOnMainThreadAsync(() => GetNativeText(handler));
        Assert.Equal("Hello", initialText);

        // Update to uppercase
        var uppercaseText = await InvokeOnMainThreadAsync(() =>
        {
            label.TextTransform = Microsoft.Maui.TextTransform.Uppercase;
            handler.UpdateValue(nameof(Microsoft.Maui.Controls.Label.TextTransform));
            return GetNativeText(handler);
        });
        Assert.Equal("HELLO", uppercaseText);

        // Update to lowercase
        var lowercaseText = await InvokeOnMainThreadAsync(() =>
        {
            label.TextTransform = Microsoft.Maui.TextTransform.Lowercase;
            handler.UpdateValue(nameof(Microsoft.Maui.Controls.Label.TextTransform));
            return GetNativeText(handler);
        });
        Assert.Equal("hello", lowercaseText);
    }

    [AvaloniaFact(DisplayName = "FormattedText Creates Correct Number Of Inlines")]
    public async Task FormattedTextCreatesCorrectNumberOfInlines()
    {
        var formattedString = new Microsoft.Maui.Controls.FormattedString();
        formattedString.Spans.Add(new Microsoft.Maui.Controls.Span { Text = "Hello " });
        formattedString.Spans.Add(new Microsoft.Maui.Controls.Span { Text = "World" });
        formattedString.Spans.Add(new Microsoft.Maui.Controls.Span { Text = "!" });

        var label = new Microsoft.Maui.Controls.Label
        {
            FormattedText = formattedString,
            WidthRequest = 200,
            HeightRequest = 50
        };

        var inlineCount = await GetValueAsync<int, MauiLabelHandler>(label, GetNativeInlineCount);

        Assert.Equal(3, inlineCount);
    }

    [AvaloniaFact(DisplayName = "FormattedText Span Text Initializes Correctly")]
    public async Task FormattedTextSpanTextInitializesCorrectly()
    {
        var formattedString = new Microsoft.Maui.Controls.FormattedString();
        formattedString.Spans.Add(new Microsoft.Maui.Controls.Span { Text = "First" });
        formattedString.Spans.Add(new Microsoft.Maui.Controls.Span { Text = "Second" });

        var label = new Microsoft.Maui.Controls.Label
        {
            FormattedText = formattedString,
            WidthRequest = 200,
            HeightRequest = 50
        };

        var handler = await CreateHandlerAsync<MauiLabelHandler>(label);

        var firstText = await InvokeOnMainThreadAsync(() => GetNativeRunText(handler, 0));
        var secondText = await InvokeOnMainThreadAsync(() => GetNativeRunText(handler, 1));

        Assert.Equal("First", firstText);
        Assert.Equal("Second", secondText);
    }

    [AvaloniaFact(DisplayName = "FormattedText Span TextColor Applies Correctly")]
    public async Task FormattedTextSpanTextColorAppliesCorrectly()
    {
        var formattedString = new Microsoft.Maui.Controls.FormattedString();
        formattedString.Spans.Add(new Microsoft.Maui.Controls.Span { Text = "Red", TextColor = Colors.Red });
        formattedString.Spans.Add(new Microsoft.Maui.Controls.Span { Text = "Blue", TextColor = Colors.Blue });

        var label = new Microsoft.Maui.Controls.Label
        {
            FormattedText = formattedString,
            WidthRequest = 200,
            HeightRequest = 50
        };

        var handler = await CreateHandlerAsync<MauiLabelHandler>(label);

        var firstColor = await InvokeOnMainThreadAsync(() => GetNativeRunForeground(handler, 0));
        var secondColor = await InvokeOnMainThreadAsync(() => GetNativeRunForeground(handler, 1));

        Assert.NotNull(firstColor);
        Assert.NotNull(secondColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, firstColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Blue, secondColor);
    }

    [AvaloniaFact(DisplayName = "FormattedText Span TextTransform Applies Correctly")]
    public async Task FormattedTextSpanTextTransformAppliesCorrectly()
    {
        var formattedString = new Microsoft.Maui.Controls.FormattedString();
        formattedString.Spans.Add(new Microsoft.Maui.Controls.Span
        {
            Text = "hello",
            TextTransform = Microsoft.Maui.TextTransform.Uppercase
        });
        formattedString.Spans.Add(new Microsoft.Maui.Controls.Span
        {
            Text = "WORLD",
            TextTransform = Microsoft.Maui.TextTransform.Lowercase
        });

        var label = new Microsoft.Maui.Controls.Label
        {
            FormattedText = formattedString,
            WidthRequest = 200,
            HeightRequest = 50
        };

        var handler = await CreateHandlerAsync<MauiLabelHandler>(label);

        var firstText = await InvokeOnMainThreadAsync(() => GetNativeRunText(handler, 0));
        var secondText = await InvokeOnMainThreadAsync(() => GetNativeRunText(handler, 1));

        Assert.Equal("HELLO", firstText);
        Assert.Equal("world", secondText);
    }

    [AvaloniaFact(DisplayName = "FormattedText Empty Clears Text")]
    public async Task FormattedTextEmptyClearsText()
    {
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = "Plain text",
            FormattedText = new Microsoft.Maui.Controls.FormattedString(), // Empty
            WidthRequest = 200,
            HeightRequest = 50
        };

        var handler = await CreateHandlerAsync<MauiLabelHandler>(label);

        // When FormattedText is set (even if empty), it takes precedence and clears the text
        // This matches MAUI's behavior where FormattedText overrides Text
        var nativeText = await InvokeOnMainThreadAsync(() => GetNativeText(handler));
        var inlineCount = await InvokeOnMainThreadAsync(() => GetNativeInlineCount(handler));

        Assert.Equal("", nativeText);
        Assert.Equal(0, inlineCount);
    }

    [AvaloniaFact(DisplayName = "FormattedText Null Falls Back To PlainText")]
    public async Task FormattedTextNullFallsBackToPlainText()
    {
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = "Plain text",
            FormattedText = null!,
            WidthRequest = 200,
            HeightRequest = 50
        };

        var nativeText = await GetValueAsync<string?, MauiLabelHandler>(label, GetNativeText);

        Assert.Equal("Plain text", nativeText);
    }

    [AvaloniaFact(DisplayName = "TextType Html Creates Inlines")]
    public async Task TextTypeHtmlCreatesInlines()
    {
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = "<b>Bold</b> and <i>Italic</i>",
            TextType = TextType.Html,
            WidthRequest = 300,
            HeightRequest = 50
        };

        var inlineCount = await GetValueAsync<int, MauiLabelHandler>(label, GetNativeInlineCount);

        // Should have multiple inlines (bold run, " and " run, italic run)
        Assert.True(inlineCount >= 3);
    }

    [AvaloniaFact(DisplayName = "TextType Text Uses Plain Text")]
    public async Task TextTypeTextUsesPlainText()
    {
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = "<b>Not Bold</b>",
            TextType = TextType.Text,
            WidthRequest = 300,
            HeightRequest = 50
        };

        var nativeText = await GetValueAsync<string?, MauiLabelHandler>(label, GetNativeText);

        // Should display raw HTML as plain text
        Assert.Equal("<b>Not Bold</b>", nativeText);
    }

    [AvaloniaFact(DisplayName = "TextType Switch From Text To Html")]
    public async Task TextTypeSwitchFromTextToHtml()
    {
        var label = new Microsoft.Maui.Controls.Label
        {
            Text = "<b>Bold</b>",
            TextType = TextType.Text,
            WidthRequest = 300,
            HeightRequest = 50
        };

        var handler = await CreateHandlerAsync<MauiLabelHandler>(label);

        // Initially plain text
        var plainText = await InvokeOnMainThreadAsync(() => GetNativeText(handler));
        Assert.Equal("<b>Bold</b>", plainText);

        // Switch to HTML
        var inlineCount = await InvokeOnMainThreadAsync(() =>
        {
            label.TextType = TextType.Html;
            handler.UpdateValue(nameof(Microsoft.Maui.Controls.Label.TextType));
            return GetNativeInlineCount(handler);
        });

        Assert.True(inlineCount >= 1);
    }
}
