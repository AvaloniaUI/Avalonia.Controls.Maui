using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaThickness = Avalonia.Thickness;
using MauiLabel = Microsoft.Maui.Controls.Label;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;
using PlatformView = Avalonia.Controls.Control;


namespace Avalonia.Controls.Maui.Handlers;

public class LabelHandler : ViewHandler<ILabel, AvaloniaTextBlock>, ILabelHandler
{
    public static IPropertyMapper<ILabel, ILabelHandler> Mapper = new PropertyMapper<ILabel, ILabelHandler>(ViewHandler.ViewMapper)
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
    };

    public static CommandMapper<ILabel, ILabelHandler> CommandMapper = new(ViewCommandMapper)
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

    ILabel ILabelHandler.VirtualView => VirtualView;

    System.Object ILabelHandler.PlatformView => PlatformView;

    protected override AvaloniaTextBlock CreatePlatformView()
    {
        return new AvaloniaTextBlock();
    }

    public override bool NeedsContainer => VirtualView?.Background != null;

    public static void MapBackground(ILabelHandler handler, ILabel label)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateBackground(label);
    }

    public static void MapText(ILabelHandler handler, ILabel label)
    {
        var textBlock = (AvaloniaTextBlock)handler.PlatformView;
        textBlock?.UpdateText(label);

        // Invalidate measure up the visual tree so layout updates
        if (textBlock != null)
        {
            textBlock.InvalidateMeasure(label);
            textBlock.InvalidateArrange();

            // Also invalidate parent containers up the tree
            var parent = textBlock.Parent;
            while (parent is global::Avalonia.Controls.Control parentControl)
            {
                parentControl.InvalidateMeasure(label);
                parentControl.InvalidateArrange();
                parent = parentControl.Parent;
            }
        }
    }

    public static void MapTextColor(ILabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateTextColor(label);

    public static void MapCharacterSpacing(ILabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateCharacterSpacing(label);

    public static void MapFont(ILabelHandler handler, ILabel label)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();

        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateFont(label, fontManager);
    }

    public static void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateHorizontalTextAlignment(label);

    public static void MapVerticalTextAlignment(ILabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateVerticalTextAlignment(label);

    public static void MapLineBreakMode(ILabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateLineBreakMode(label);

    public static void MapTextDecorations(ILabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateTextDecorations(label);

    public static void MapMaxLines(ILabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateMaxLines(label);

    public static void MapPadding(ILabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdatePadding(label);

    public static void MapLineHeight(ILabelHandler handler, ILabel label) =>
        ((AvaloniaTextBlock)handler.PlatformView)?.UpdateLineHeight(label);
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
        switch (label.VerticalTextAlignment)
        {
            case Microsoft.Maui.TextAlignment.Start:
                textBlock.VerticalAlignment = AvaloniaVerticalAlignment.Top;
                break;
            case Microsoft.Maui.TextAlignment.Center:
                textBlock.VerticalAlignment = AvaloniaVerticalAlignment.Center;
                break;
            case Microsoft.Maui.TextAlignment.End:
                textBlock.VerticalAlignment = AvaloniaVerticalAlignment.Bottom;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
        textBlock.Text = label.Text;
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
}