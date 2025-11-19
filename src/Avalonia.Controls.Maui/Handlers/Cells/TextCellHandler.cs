using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using System;

namespace Avalonia.Controls.Maui.Handlers.Cells;

/// <summary>
/// Handler for MAUI TextCell
/// </summary>
public class TextCellHandler : ElementHandler<TextCell, global::Avalonia.Controls.Border>
{
    public static IPropertyMapper<TextCell, TextCellHandler> Mapper =
        new PropertyMapper<TextCell, TextCellHandler>(ElementMapper)
        {
            [nameof(TextCell.Text)] = MapText,
            [nameof(TextCell.Detail)] = MapDetail,
            [nameof(TextCell.TextColor)] = MapTextColor,
            [nameof(TextCell.DetailColor)] = MapDetailColor,
        };

    public static CommandMapper<TextCell, TextCellHandler> CommandMapper =
        new(ElementCommandMapper);

    private StackPanel? _stackPanel;
    private TextBlock? _textBlock;
    private TextBlock? _detailBlock;

    public TextCellHandler() : base(Mapper, CommandMapper)
    {
    }

    public TextCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public TextCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override global::Avalonia.Controls.Border CreatePlatformElement()
    {
        _textBlock = new TextBlock
        {
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center
        };

        _detailBlock = new TextBlock
        {
            FontSize = 13,
            Opacity = 0.7,
            VerticalAlignment = VerticalAlignment.Center
        };

        _stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Margin = new Thickness(16, 12)
        };

        _stackPanel.Children.Add(_textBlock);
        _stackPanel.Children.Add(_detailBlock);

        var border = new global::Avalonia.Controls.Border
        {
            Child = _stackPanel,
            MinHeight = 44,
            Background = global::Avalonia.Media.Brushes.Transparent,
            Cursor = new global::Avalonia.Input.Cursor(global::Avalonia.Input.StandardCursorType.Hand)
        };

        // Handle cell taps
        border.Tapped += (s, e) =>
        {
            if (VirtualView?.Command?.CanExecute(VirtualView.CommandParameter) == true)
            {
                VirtualView.Command.Execute(VirtualView.CommandParameter);
            }
        };

        return border;
    }

    public static void MapText(TextCellHandler handler, TextCell textCell)
    {
        if (handler._textBlock is null)
            return;

        handler._textBlock.Text = textCell.Text ?? string.Empty;
        handler._textBlock.IsVisible = !string.IsNullOrEmpty(textCell.Text);
    }

    public static void MapDetail(TextCellHandler handler, TextCell textCell)
    {
        if (handler._detailBlock is null)
            return;

        handler._detailBlock.Text = textCell.Detail ?? string.Empty;
        handler._detailBlock.IsVisible = !string.IsNullOrEmpty(textCell.Detail);
    }

    public static void MapTextColor(TextCellHandler handler, TextCell textCell)
    {
        if (handler._textBlock is null)
            return;

        if (textCell.TextColor != null)
        {
            handler._textBlock.Foreground = new global::Avalonia.Media.SolidColorBrush(
                Color.FromArgb(
                    (byte)(textCell.TextColor.Alpha * 255),
                    (byte)(textCell.TextColor.Red * 255),
                    (byte)(textCell.TextColor.Green * 255),
                    (byte)(textCell.TextColor.Blue * 255)
                )
            );
        }
        else
        {
            handler._textBlock.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    public static void MapDetailColor(TextCellHandler handler, TextCell textCell)
    {
        if (handler._detailBlock is null)
            return;

        if (textCell.DetailColor != null)
        {
            handler._detailBlock.Foreground = new global::Avalonia.Media.SolidColorBrush(
                Color.FromArgb(
                    (byte)(textCell.DetailColor.Alpha * 255),
                    (byte)(textCell.DetailColor.Red * 255),
                    (byte)(textCell.DetailColor.Green * 255),
                    (byte)(textCell.DetailColor.Blue * 255)
                )
            );
        }
        else
        {
            handler._detailBlock.ClearValue(TextBlock.ForegroundProperty);
        }
    }
}
