using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers.Cells;

public class EntryCellHandler : ElementHandler<EntryCell, MauiEntryCell>
{
    public static IPropertyMapper<EntryCell, EntryCellHandler> Mapper =
        new PropertyMapper<EntryCell, EntryCellHandler>(ElementMapper)
        {
            [nameof(EntryCell.Label)] = MapLabel,
            [nameof(EntryCell.Text)] = MapText,
            [nameof(EntryCell.Placeholder)] = MapPlaceholder,
            [nameof(EntryCell.LabelColor)] = MapLabelColor,
            [nameof(EntryCell.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(EntryCell.VerticalTextAlignment)] = MapVerticalTextAlignment,
            [nameof(EntryCell.Keyboard)] = MapKeyboard,
            [nameof(Cell.IsEnabled)] = MapIsEnabled,
            ["ContextActions"] = MapContextActions,
        };

    public static CommandMapper<EntryCell, EntryCellHandler> CommandMapper =
        new(ElementCommandMapper);

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

    protected override MauiEntryCell CreatePlatformElement()
    {
        var cell = new MauiEntryCell();
        cell.Input.TextChanged += OnTextChanged;
        cell.Input.KeyDown += OnInputKeyDown;
        cell.AttachedToVisualTree += OnCellAttachedToVisualTree;
        cell.DetachedFromVisualTree += OnCellDetachedFromVisualTree;
        return cell;
    }

    protected override void DisconnectHandler(MauiEntryCell platformView)
    {
        base.DisconnectHandler(platformView);
        platformView.Input.TextChanged -= OnTextChanged;
        platformView.Input.KeyDown -= OnInputKeyDown;
        platformView.AttachedToVisualTree -= OnCellAttachedToVisualTree;
        platformView.DetachedFromVisualTree -= OnCellDetachedFromVisualTree;
    }

    private void OnCellAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        VirtualView?.SendAppearing();
    }

    private void OnCellDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        VirtualView?.SendDisappearing();
    }

    private void OnInputKeyDown(object? sender, Input.KeyEventArgs e)
    {
        if (e.Key == Input.Key.Enter)
        {
            VirtualView?.SendCompleted();
        }
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (VirtualView != null)
        {
             if (VirtualView.Text != PlatformView.Input.Text)
             {
                 VirtualView.Text = PlatformView.Input.Text;
             }
        }
    }

    public static void MapLabel(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateLabel(entryCell);
    }

    public static void MapText(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateText(entryCell, false);
    }

    public static void MapPlaceholder(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdatePlaceholder(entryCell);
    }

    public static void MapLabelColor(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateLabelColor(entryCell);
    }

    public static void MapHorizontalTextAlignment(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateHorizontalTextAlignment(entryCell);
    }

    public static void MapVerticalTextAlignment(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateVerticalTextAlignment(entryCell);
    }

    public static void MapKeyboard(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateKeyboard(entryCell);
    }

    public static void MapIsEnabled(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateIsEnabled(entryCell);
    }

    public static void MapContextActions(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateContextActions(entryCell);
    }
}