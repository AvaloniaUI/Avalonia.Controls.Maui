using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaThickness = Avalonia.Thickness;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiSpan = Microsoft.Maui.Controls.Span;
using MauiFormattedString = Microsoft.Maui.Controls.FormattedString;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;
using PlatformView = Avalonia.Controls.Control;


namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="ILabel"/>.</summary>
public class LabelHandler : ViewHandler<ILabel, AvaloniaTextBlock>
{
    /// <summary>Property mapper for <see cref="LabelHandler"/>.</summary>
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
        [nameof(MauiLabel.TextType)] = MapTextType,
    };

    /// <summary>Command mapper for <see cref="LabelHandler"/>.</summary>
    public static CommandMapper<ILabel, LabelHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="LabelHandler"/>.</summary>
    public LabelHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="LabelHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public LabelHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="LabelHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    /// <param name="commandMapper">The command mapper to use.</param>
    public LabelHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override AvaloniaTextBlock CreatePlatformView()
    {
        return new AvaloniaTextBlock();
    }

    /// <summary>Gets a value indicating whether this handler requires a container view.</summary>
    public override bool NeedsContainer => VirtualView?.Background != null;

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapBackground(LabelHandler handler, ILabel label)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateBackground(label);
    }

    /// <summary>Maps the Text property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapText(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateText(label);

    /// <summary>Maps the TextColor property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapTextColor(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateTextColor(label);

    /// <summary>Maps the CharacterSpacing property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapCharacterSpacing(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateCharacterSpacing(label);

    /// <summary>Maps the Font property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapFont(LabelHandler handler, ILabel label)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();

        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateFont(label, fontManager);
    }

    /// <summary>Maps the HorizontalTextAlignment property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateHorizontalTextAlignment(label);

    /// <summary>Maps the VerticalTextAlignment property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapVerticalTextAlignment(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateVerticalTextAlignment(label);

    /// <summary>Maps the LineBreakMode property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapLineBreakMode(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateLineBreakMode(label);

    /// <summary>Maps the TextDecorations property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapTextDecorations(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateTextDecorations(label);

    /// <summary>Maps the MaxLines property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapMaxLines(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateMaxLines(label);

    /// <summary>Maps the Padding property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapPadding(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdatePadding(label);

    /// <summary>Maps the LineHeight property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapLineHeight(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateLineHeight(label);

    /// <summary>Maps the TextTransform property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapTextTransform(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateTextTransform(label);

    /// <summary>Maps the FormattedText property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapFormattedText(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateFormattedText(label, handler);

    /// <summary>
    /// Maps the TextType property to the platform view.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <param name="label">The virtual view.</param>
    public static void MapTextType(LabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateTextType(label);
}

/// <summary>Extension methods for mapping MAUI Label properties to Avalonia controls.</summary>
public static class LabelTextBlockExtensions
{
    /// <summary>Updates the Text property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the text.</param>
    public static void UpdateText(this AvaloniaTextBlock textBlock, ILabel label)
    {
        if (label is MauiLabel mauiLabel)
        {
            if (mauiLabel.TextType == TextType.Html)
            {
                textBlock.UpdateHtmlText(mauiLabel);
                return;
            }

            if (mauiLabel.FormattedText != null && mauiLabel.FormattedText.Spans.Count > 0)
                return;
        }

        textBlock.UpdateTextPlainText(label);
    }

    /// <summary>Updates the TextType property on the platform view, re-rendering text as plain or HTML.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the text type.</param>
    public static void UpdateTextType(this AvaloniaTextBlock textBlock, ILabel label)
    {
        textBlock.UpdateText(label);
    }

    /// <summary>Updates the TextColor property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="text">The MAUI text style providing the color.</param>
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

    /// <summary>Updates the Padding property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the padding.</param>
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

    /// <summary>Updates the CharacterSpacing property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the character spacing.</param>
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

    /// <summary>Updates the MaxLines property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the max lines value.</param>
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

    /// <summary>Updates the TextDecorations property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the text decorations.</param>
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

    /// <summary>Updates the LineHeight property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the line height.</param>
    public static void UpdateLineHeight(this AvaloniaTextBlock textBlock, ILabel label)
    {
        if (label.LineHeight >= 0)
        {
            textBlock.LineHeight = label.LineHeight * textBlock.FontSize;
        }
    }

    /// <summary>Updates the HorizontalTextAlignment property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the horizontal text alignment.</param>
    public static void UpdateHorizontalTextAlignment(this AvaloniaTextBlock textBlock, ILabel label)
    {
        switch (label.HorizontalTextAlignment)
        {
            case Microsoft.Maui.TextAlignment.Start:
                textBlock.TextAlignment = AvaloniaTextAlignment.Start;
                break;
            case Microsoft.Maui.TextAlignment.Center:
                textBlock.TextAlignment = AvaloniaTextAlignment.Center;
                break;
            case Microsoft.Maui.TextAlignment.End:
                textBlock.TextAlignment = AvaloniaTextAlignment.End;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>Updates the VerticalTextAlignment property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the vertical text alignment.</param>
    public static void UpdateVerticalTextAlignment(this TextBlock textBlock, ILabel label)
    {
        // TODO: Vertical Text Alignment is not directly supported in Avalonia TextBox yet.
    }

    /// <summary>Updates the LineBreakMode property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the line break mode.</param>
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
                textBlock.TextTrimming = TextTrimming.PrefixCharacterEllipsis;
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

    internal static void UpdateHtmlText(this AvaloniaTextBlock textBlock, MauiLabel label)
    {
        var html = label.Text ?? string.Empty;

        textBlock.Text = null;

        if (string.IsNullOrEmpty(html))
            return;

        var convertedInlines = Avalonia.Controls.Maui.Platform.HtmlToInlinesConverter.Convert(html);
        var inlines = new Avalonia.Controls.Documents.InlineCollection();
        foreach (var inline in convertedInlines)
        {
            inlines.Add(inline);
        }
        textBlock.Inlines = inlines;
    }

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

    /// <summary>Updates the TextTransform property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the text transform.</param>
    public static void UpdateTextTransform(this AvaloniaTextBlock textBlock, ILabel label)
    {
        if (label is not MauiLabel mauiLabel)
        {
            return;
        }

        // Re-apply text (respects TextType)
        textBlock.UpdateText(label);
    }

    

    /// <summary>Updates the FormattedText property on the platform view.</summary>
    /// <param name="textBlock">The Avalonia text block.</param>
    /// <param name="label">The MAUI label providing the formatted text.</param>
    /// <param name="handler">The label handler for accessing services.</param>
    public static void UpdateFormattedText(this AvaloniaTextBlock textBlock, ILabel label, LabelHandler handler)
    {
        if (label is not MauiLabel mauiLabel)
        {
            return;
        }

        var formattedText = mauiLabel.FormattedText;
        if (formattedText == null || formattedText.Spans.Count == 0)
        {
            textBlock.UpdateTextPlainText(label);
            return;
        }

        textBlock.Text = null;
        var inlines = new Avalonia.Controls.Documents.InlineCollection();

        var fontManager = handler.GetRequiredService<IFontManager>();

        foreach (var span in formattedText.Spans)
        {
            var run = CreateRun(span, fontManager);
            inlines.Add(run);
        }

        textBlock.Inlines = inlines;
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
            run.FontFamily = Avalonia.Controls.Maui.FontManagerExtensions.GetFontFamily(fontManager, font);
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