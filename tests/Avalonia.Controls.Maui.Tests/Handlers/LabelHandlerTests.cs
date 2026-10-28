using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiLabelHandler = Avalonia.Controls.Maui.Handlers.LabelHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class LabelHandlerTests : HandlerTestBase<MauiLabelHandler, LabelStub>
{
    [AvaloniaTheory(DisplayName = "VerticalTextAlignment Positions The Text")]
    [InlineData(TextAlignment.Start, 0, 10)]
    [InlineData(TextAlignment.Center, 35, 50)]
    [InlineData(TextAlignment.End, 75, 100)]
    public async Task VerticalTextAlignmentPositionsText(TextAlignment alignment, int minTop, int maxTop)
    {
        var label = new LabelStub
        {
            Text = "Hi",
            TextColor = Colors.Black,
            Height = 100,
            Width = 100,
            VerticalTextAlignment = alignment,
        };
        var handler = await CreateHandlerAsync(label);

        var top = await InvokeOnMainThreadAsync(() =>
        {
            var window = new Window
            {
                Width = 100,
                Height = 100,
                Background = Avalonia.Media.Brushes.White,
                Content = handler.PlatformView,
            };
            window.Show();
            try
            {
                window.UpdateLayout();
                using var frame = Avalonia.Headless.HeadlessWindowExtensions.CaptureRenderedFrame(window)!;
                return FirstInkRow(frame);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.InRange(top, minTop, maxTop);
    }

    [AvaloniaFact(DisplayName = "TextBlock styles apply to labels")]
    public async Task TextBlockStylesApplyToLabels()
    {
        var handler = await CreateHandlerAsync(new LabelStub { Text = "Hi" });

        var tag = await InvokeOnMainThreadAsync(() =>
        {
            var window = new Window { Content = handler.PlatformView };
            window.Styles.Add(new Avalonia.Styling.Style(x => Avalonia.Styling.Selectors.OfType<TextBlock>(x))
            {
                Setters = { new Avalonia.Styling.Setter(Control.TagProperty, "styled") },
            });
            window.Show();
            try
            {
                return ((Control)handler.PlatformView).Tag;
            }
            finally
            {
                window.Close();
            }
        });

        Assert.Equal("styled", tag);
    }

    [AvaloniaTheory(DisplayName = "Glyphs that reach past the text's width are drawn whole")]
    // Open Sans and Gallery Script have no italic or bold faces, so those are made from the regular face
    [InlineData("OpenSans-Regular.ttf#Open Sans", false, false, "fff")]
    [InlineData("OpenSans-Regular.ttf#Open Sans", false, true, "Click count: 4")]
    [InlineData("OpenSans-Regular.ttf#Open Sans", true, false, "fff")]
    [InlineData("OpenSans-Regular.ttf#Open Sans", true, true, "Click count: 4")]
    [InlineData("GalleryScript-Regular.ttf#Gallery Script", false, false, "Wj")]
    [InlineData("GalleryScript-Regular.ttf#Gallery Script", false, true, "fff")]
    [InlineData("GalleryScript-Regular.ttf#Gallery Script", true, false, "fff")]
    [InlineData("GalleryScript-Regular.ttf#Gallery Script", true, true, "fff")]
    [InlineData(null, false, true, "Click count: 4")]
    [InlineData(null, true, true, "Wj")]
    public async Task GlyphsPastTextWidthAreDrawnWhole(string? font, bool bold, bool italic, string text)
    {
        var family = font is null ? null : $"avares://{typeof(LabelHandlerTests).Assembly.GetName().Name}/Assets/Fonts/{font}";
        var mauiFont = Microsoft.Maui.Font.OfSize(family, 40, bold ? FontWeight.Bold : FontWeight.Regular);
        var label = new LabelStub
        {
            Text = text,
            TextColor = Colors.Black,
            Font = italic ? mauiFont.WithSlant(FontSlant.Italic) : mauiFont,
            Width = double.NaN,
            Height = double.NaN,
        };
        var handler = await CreateHandlerAsync(label);

        var (labelInk, unclippedInk) = await InvokeOnMainThreadAsync(() =>
        {
            // The label above, and below it the same text drawn without any clipping
            var textBlock = (TextBlock)handler.PlatformView;
            textBlock.Margin = new Avalonia.Thickness(50, 10, 0, 0);
            var unclipped = new TextBlock
            {
                Text = textBlock.Text,
                FontFamily = textBlock.FontFamily,
                FontSize = textBlock.FontSize,
                FontStyle = textBlock.FontStyle,
                FontWeight = textBlock.FontWeight,
                Foreground = Avalonia.Media.Brushes.Black,
                ClipToBounds = false,
                Margin = new Avalonia.Thickness(50, 110, 0, 0),
            };
            foreach (var control in new[] { textBlock, unclipped })
            {
                control.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                control.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
            }

            var window = new Window
            {
                Width = 500,
                Height = 220,
                Background = Avalonia.Media.Brushes.White,
                Content = new Panel { Children = { textBlock, unclipped } },
            };
            window.Show();
            try
            {
                window.UpdateLayout();
                using var frame = Avalonia.Headless.HeadlessWindowExtensions.CaptureRenderedFrame(window)!;
                return (InkColumns(frame, 0, 105), InkColumns(frame, 105, 215));
            }
            finally
            {
                window.Close();
            }
        });

        Assert.Equal(unclippedInk, labelInk);
    }

    [AvaloniaFact(DisplayName = "Text that does not fit is clipped to the label's bounds")]
    public async Task TextThatDoesNotFitIsClipped()
    {
        var label = new LabelStub
        {
            Text = "Text that is much wider than the label",
            TextColor = Colors.Black,
            Width = 50,
            Height = double.NaN,
        };
        var handler = await CreateHandlerAsync(label);

        var (labelRight, inkRight) = await InvokeOnMainThreadAsync(() =>
        {
            var textBlock = (TextBlock)handler.PlatformView;
            textBlock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            textBlock.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

            var window = new Window
            {
                Width = 400,
                Height = 100,
                Background = Avalonia.Media.Brushes.White,
                Content = textBlock,
            };
            window.Show();
            try
            {
                window.UpdateLayout();
                using var frame = Avalonia.Headless.HeadlessWindowExtensions.CaptureRenderedFrame(window)!;
                return (textBlock.Bounds.Right, InkColumns(frame, 0, 100).Last);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(inkRight < labelRight, $"Ink ends at {inkRight}, the label at {labelRight}");
    }

    // The first and last pixel columns that hold dark text pixels, between two rows
    static (int First, int Last) InkColumns(Avalonia.Media.Imaging.WriteableBitmap frame, int top, int bottom)
    {
        using var locked = frame.Lock();
        int first = -1, last = -1;
        for (var x = 0; x < locked.Size.Width; x++)
        {
            for (var y = top; y < bottom; y++)
            {
                var offset = y * locked.RowBytes + x * 4;
                if (System.Runtime.InteropServices.Marshal.ReadByte(locked.Address, offset) < 128)
                {
                    if (first < 0)
                        first = x;
                    last = x;
                    break;
                }
            }
        }

        return (first, last);
    }

    // The first pixel row that holds dark text pixels
    static int FirstInkRow(Avalonia.Media.Imaging.WriteableBitmap frame)
    {
        using var locked = frame.Lock();
        for (var y = 0; y < locked.Size.Height; y++)
        {
            for (var x = 0; x < locked.Size.Width; x++)
            {
                var offset = y * locked.RowBytes + x * 4;
                var b0 = System.Runtime.InteropServices.Marshal.ReadByte(locked.Address, offset);
                var b1 = System.Runtime.InteropServices.Marshal.ReadByte(locked.Address, offset + 1);
                if (b0 < 128 && b1 < 128)
                    return y;
            }
        }

        return -1;
    }

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

    // ============================================================
    // TextTransform Tests
    // ============================================================

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

    // ============================================================
    // FormattedText Tests
    // ============================================================

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
}
