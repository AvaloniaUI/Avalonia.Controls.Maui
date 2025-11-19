using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using System;

namespace Avalonia.Controls.Maui.Handlers.Cells;

/// <summary>
/// Handler for MAUI EntryCell
/// </summary>
public class EntryCellHandler : ElementHandler<EntryCell, global::Avalonia.Controls.Border>
{
    public static IPropertyMapper<EntryCell, EntryCellHandler> Mapper =
        new PropertyMapper<EntryCell, EntryCellHandler>(ElementMapper)
        {
            [nameof(EntryCell.Label)] = MapLabel,
            [nameof(EntryCell.Text)] = MapText,
            [nameof(EntryCell.Placeholder)] = MapPlaceholder,
            [nameof(EntryCell.LabelColor)] = MapLabelColor,
            [nameof(EntryCell.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        };

    public static CommandMapper<EntryCell, EntryCellHandler> CommandMapper =
        new(ElementCommandMapper);

    private TextBlock? _labelBlock;
    private global::Avalonia.Controls.TextBox? _textBox;
    private bool _isUpdating;

    public EntryCellHandler() : base(Mapper, CommandMapper)
    {
    }

    public EntryCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public EntryCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override global::Avalonia.Controls.Border CreatePlatformElement()
    {
        _labelBlock = new TextBlock
        {
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 100,
            Margin = new Thickness(0, 0, 12, 0)
        };

        _textBox = new global::Avalonia.Controls.TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent
        };

        _textBox.TextChanged += (s, e) =>
        {
            if (_isUpdating || VirtualView is null)
                return;

            _isUpdating = true;
            VirtualView.Text = _textBox.Text ?? string.Empty;
            _isUpdating = false;
        };

        var grid = new global::Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            Margin = new Thickness(16, 8)
        };

        global::Avalonia.Controls.Grid.SetColumn(_labelBlock, 0);
        global::Avalonia.Controls.Grid.SetColumn(_textBox, 1);

        grid.Children.Add(_labelBlock);
        grid.Children.Add(_textBox);

        return new global::Avalonia.Controls.Border
        {
            Child = grid,
            Background = global::Avalonia.Media.Brushes.Transparent,
            MinHeight = 44
        };
    }

    public static void MapLabel(EntryCellHandler handler, EntryCell entryCell)
    {
        if (handler._labelBlock is null)
            return;

        handler._labelBlock.Text = entryCell.Label ?? string.Empty;
        handler._labelBlock.IsVisible = !string.IsNullOrEmpty(entryCell.Label);
    }

    public static void MapText(EntryCellHandler handler, EntryCell entryCell)
    {
        if (handler._textBox is null || handler._isUpdating)
            return;

        handler._isUpdating = true;
        handler._textBox.Text = entryCell.Text ?? string.Empty;
        handler._isUpdating = false;
    }

    public static void MapPlaceholder(EntryCellHandler handler, EntryCell entryCell)
    {
        if (handler._textBox is null)
            return;

        handler._textBox.Watermark = entryCell.Placeholder;
    }

    public static void MapLabelColor(EntryCellHandler handler, EntryCell entryCell)
    {
        if (handler._labelBlock is null)
            return;

        if (entryCell.LabelColor != null)
        {
            handler._labelBlock.Foreground = new global::Avalonia.Media.SolidColorBrush(
                Color.FromArgb(
                    (byte)(entryCell.LabelColor.Alpha * 255),
                    (byte)(entryCell.LabelColor.Red * 255),
                    (byte)(entryCell.LabelColor.Green * 255),
                    (byte)(entryCell.LabelColor.Blue * 255)
                )
            );
        }
        else
        {
            handler._labelBlock.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    public static void MapHorizontalTextAlignment(EntryCellHandler handler, EntryCell entryCell)
    {
        if (handler._textBox is null)
            return;

        handler._textBox.TextAlignment = entryCell.HorizontalTextAlignment switch
        {
            Microsoft.Maui.TextAlignment.Start => global::Avalonia.Media.TextAlignment.Left,
            Microsoft.Maui.TextAlignment.Center => global::Avalonia.Media.TextAlignment.Center,
            Microsoft.Maui.TextAlignment.End => global::Avalonia.Media.TextAlignment.Right,
            _ => global::Avalonia.Media.TextAlignment.Left
        };
    }
}
