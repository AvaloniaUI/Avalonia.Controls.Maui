using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaThickness = Avalonia.Thickness;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiSpan = Microsoft.Maui.Controls.Span;
using MauiFormattedString = Microsoft.Maui.Controls.FormattedString;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;
using PlatformView = Avalonia.Controls.Control;


namespace Avalonia.Controls.Maui.Handlers;

public class LabelHandler : ViewHandler<ILabel, AvaloniaTextBlock>
{
    public static IPropertyMapper<ILabel, LabelHandler> Mapper = new PropertyMapper<ILabel, LabelHandler>(ViewHandler.ViewMapper)
    {
        [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(ITextStyle.Font)] = MapFont,
        [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
        [nameof(MauiLabel.LineBreakMode)] = MapLineBreakMode,
        [nameof(ILabel.LineHeight)] = MapLineHeight,
        [nameof(MauiLabel.MaxLines)] = MapMaxLines,
        [nameof(ILabel.Padding)] = MapPadding,
        [nameof(ILabel.Text)] = MapText,
        [nameof(ITextStyle.TextColor)] = MapTextColor,
        [nameof(ILabel.TextDecorations)] = MapTextDecorations,
        [nameof(ILabel.Background)] = MapBackground,
        [nameof(MauiLabel.TextTransform)] = MapTextTransform,
        [nameof(MauiLabel.FormattedText)] = MapFormattedText,
    };

    public static CommandMapper<ILabel, LabelHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public LabelHandler() : base(Mapper, CommandMapper)
    {
    }

    public LabelHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public LabelHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper)
    {
    }

    protected override AvaloniaTextBlock CreatePlatformView()
    {
        return new AvaloniaTextBlock();
    }

    public override bool NeedsContainer => VirtualView?.Background != null;

    public static void MapBackground(LabelHandler handler, ILabel label)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateBackground(label);
    }

    public static void MapText(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateText(label);

    public static void MapTextColor(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateTextColor(label);

    public static void MapCharacterSpacing(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateCharacterSpacing(label);

    public static void MapFont(LabelHandler handler, ILabel label)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();

        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateFont(label, fontManager);
    }

    public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateHorizontalTextAlignment(label);

    public static void MapVerticalTextAlignment(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateVerticalTextAlignment(label);

    public static void MapLineBreakMode(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateLineBreakMode(label);

    public static void MapTextDecorations(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateTextDecorations(label);

    public static void MapMaxLines(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateMaxLines(label);

    public static void MapPadding(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdatePadding(label);

    public static void MapLineHeight(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateLineHeight(label);

    public static void MapTextTransform(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateTextTransform(label);

    public static void MapFormattedText(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateFormattedText(label, handler);
}

public static class LabelTextBlockExtensions
{
    public static void UpdateText(this AvaloniaTextBlock textBlock, ILabel label)
    {
        textBlock.UpdateTextPlainText(label);
    }

    public static void UpdateTextColor(this AvaloniaTextBlock textBlock, IText text)
    {
        if (text.TextColor != null)
        {
            textBlock.Foreground = text.TextColor.ToPlatform();
        }
        else
        {
            textBlock.ClearValue(AvaloniaTextBlock.ForegroundProperty);
        }
    }

    public static void UpdatePadding(this AvaloniaTextBlock textBlock, ILabel label)
    {
        if (!label.Padding.IsEmpty)
        {
            textBlock.Padding = new AvaloniaThickness(label.Padding.Left, label.Padding.Top, label.Padding.Right, label.Padding.Bottom);
        }
        else
        {
            textBlock.Padding = new AvaloniaThickness(0);
        }
    }

    public static void UpdateCharacterSpacing(this AvaloniaTextBlock textBlock, ILabel label)
    {
        if (label.CharacterSpacing != 0)
        {
            textBlock.LetterSpacing = (int)(label.CharacterSpacing * textBlock.FontSize / 1000);
        }
        else
        {
            textBlock.LetterSpacing = 0;
        }
    }

    public static void UpdateMaxLines(this AvaloniaTextBlock textBlock, ILabel label)
    {
        if (label is not MauiLabel mauiLabel)
        {
            return;
        }

        if (mauiLabel.MaxLines >= 0)
        {
            textBlock.MaxLines = mauiLabel.MaxLines;
        }
        else
        {
            textBlock.MaxLines = 0;
        }
    }

    public static void UpdateTextDecorations(this AvaloniaTextBlock textBlock, ILabel label)
    {
        var decorations = label.TextDecorations;
        var collection = new TextDecorationCollection();

        if (decorations.HasFlag(Microsoft.Maui.TextDecorations.Underline))
        {
            collection.Add(new TextDecoration
            {
                Location = TextDecorationLocation.Underline
            });
        }

        if (decorations.HasFlag(Microsoft.Maui.TextDecorations.Strikethrough))
        {
            collection.Add(new TextDecoration
            {
                Location = TextDecorationLocation.Strikethrough
            });
        }

        if (collection.Count > 0)
        {
            textBlock.TextDecorations = collection;
        }
        else
        {
            textBlock.TextDecorations = null;
        }
    }

    public static void UpdateLineHeight(this AvaloniaTextBlock textBlock, ILabel label)
    {
        if (label.LineHeight >= 0)
        {
            textBlock.LineHeight = label.LineHeight * textBlock.FontSize;
        }
    }

    public static void UpdateHorizontalTextAlignment(this AvaloniaTextBlock textBlock, ILabel label)
    {
        switch (label.HorizontalTextAlignment)
        {
            case Microsoft.Maui.TextAlignment.Start:
                textBlock.TextAlignment = AvaloniaTextAlignment.Left;
                break;
            case Microsoft.Maui.TextAlignment.Center:
                textBlock.TextAlignment = AvaloniaTextAlignment.Center;
                break;
            case Microsoft.Maui.TextAlignment.End:
                textBlock.TextAlignment = AvaloniaTextAlignment.Right;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void UpdateVerticalTextAlignment(this TextBlock textBlock, ILabel label)
    {
        // TODO: Vertical Text Alignment is not directly supported in Avalonia TextBox yet.
    }

    public static void UpdateLineBreakMode(this AvaloniaTextBlock textBlock, ILabel label)
    {
        if (label is not MauiLabel mauiLabel)
        {
            return;
        }

        switch (mauiLabel.LineBreakMode)
        {
            case LineBreakMode.NoWrap:
                textBlock.TextTrimming = TextTrimming.None;
                textBlock.TextWrapping = TextWrapping.NoWrap;
                break;
            case LineBreakMode.WordWrap:
                textBlock.TextTrimming = TextTrimming.None;
                textBlock.TextWrapping = TextWrapping.Wrap;
                break;
            case LineBreakMode.CharacterWrap:
                textBlock.TextTrimming = TextTrimming.None;
                textBlock.TextWrapping = TextWrapping.Wrap;
                break;
            case LineBreakMode.HeadTruncation:
                textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                textBlock.TextWrapping = TextWrapping.NoWrap;
                break;
            case LineBreakMode.TailTruncation:
                textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                textBlock.TextWrapping = TextWrapping.NoWrap;
                break;
            case LineBreakMode.MiddleTruncation:
                textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                textBlock.TextWrapping = TextWrapping.NoWrap;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void DetermineTruncatedTextWrapping(this AvaloniaTextBlock textBlock) =>
        textBlock.TextWrapping = textBlock.MaxLines > 1 ? TextWrapping.Wrap : TextWrapping.NoWrap;

    internal static void UpdateTextPlainText(this AvaloniaTextBlock textBlock, ILabel label)
    {
        var text = label.Text ?? string.Empty;

        // Apply text transform if this is a MAUI Label
        if (label is MauiLabel mauiLabel)
        {
            text = mauiLabel.UpdateFormsText(text, mauiLabel.TextTransform);
        }

        textBlock.Inlines?.Clear();
        textBlock.Text = text;
    }

    internal static void UpdateBackground(this AvaloniaTextBlock textBlock, IView view)
    {
        // Background is handled by the container when NeedsContainer is true
        // Only set background directly when no container is used
        if (view.Background != null)
        {
            textBlock.Background = view.Background.ToPlatform();
        }
        else
        {
            textBlock.ClearValue(AvaloniaTextBlock.BackgroundProperty);
        }
    }

    public static void UpdateTextTransform(this AvaloniaTextBlock textBlock, ILabel label)
    {
        if (label is not MauiLabel mauiLabel)
        {
            return;
        }

        // Re-apply text with transform
        textBlock.UpdateTextPlainText(label);
    }

    

    public static void UpdateFormattedText(this AvaloniaTextBlock textBlock, ILabel label, LabelHandler handler)
    {
        if (label is not MauiLabel mauiLabel)
        {
            return;
        }

        var formattedText = mauiLabel.FormattedText;
        if (formattedText == null || formattedText.Spans.Count == 0)
        {
            // Fall back to plain text if no formatted text
            textBlock.UpdateTextPlainText(label);
            return;
        }

        // Clear existing content
        textBlock.Text = null;
        textBlock.Inlines?.Clear();

        if (textBlock.Inlines == null)
        {
            return;
        }

        var fontManager = handler.GetRequiredService<IFontManager>();

        foreach (var span in formattedText.Spans)
        {
            var run = CreateRun(span, fontManager);
            textBlock.Inlines.Add(run);
        }
    }

    private static Run CreateRun(MauiSpan span, IFontManager fontManager)
    {
        var text = span.Text ?? string.Empty;

        // Apply text transform
        text = span.UpdateFormsText(text, span.TextTransform);

        var run = new Run(text);

        // Apply text color
        if (span.TextColor != null)
        {
            run.Foreground = span.TextColor.ToPlatform();
        }

        // Apply background color
        if (span.BackgroundColor != null)
        {
            run.Background = span.BackgroundColor.ToPlatform();
        }

        // Apply font
        var font = span.ToFont();
        if (!font.IsDefault)
        {
            run.FontSize = fontManager.GetFontSizeAsDouble(font);
            run.FontFamily = fontManager.GetFontFamily(font);
            run.FontStyle = FontManager.ToAvaloniaFontStyle(font.Slant);
            run.FontWeight = FontManager.ToAvaloniaFontWeight(font.Weight);
        }

        // Apply text decorations
        var decorations = span.TextDecorations;
        if (decorations != Microsoft.Maui.TextDecorations.None)
        {
            var textDecorations = new TextDecorationCollection();

            if (decorations.HasFlag(Microsoft.Maui.TextDecorations.Underline))
            {
                textDecorations.Add(new TextDecoration { Location = TextDecorationLocation.Underline });
            }

            if (decorations.HasFlag(Microsoft.Maui.TextDecorations.Strikethrough))
            {
                textDecorations.Add(new TextDecoration { Location = TextDecorationLocation.Strikethrough });
            }

            if (textDecorations.Count > 0)
            {
                run.TextDecorations = textDecorations;
            }
        }

        // Apply character spacing
        if (span.CharacterSpacing != 0 && run.FontSize > 0)
        {
            run.LetterSpacing = span.CharacterSpacing * run.FontSize / 1000;
        }

        return run;
    }
}