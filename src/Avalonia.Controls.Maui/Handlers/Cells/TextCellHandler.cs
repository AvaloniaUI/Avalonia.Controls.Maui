using Avalonia.Controls.Maui.Extensions;
using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Avalonia.Controls.Maui.Handlers.Cells;

public class TextCellHandler : ElementHandler<TextCell, MauiTextCell>
{
    public static IPropertyMapper<TextCell, TextCellHandler> Mapper =
        new PropertyMapper<TextCell, TextCellHandler>(ElementMapper)
        {
            [nameof(TextCell.Text)] = MapText,
            [nameof(TextCell.Detail)] = MapDetail,
            [nameof(TextCell.TextColor)] = MapTextColor,
            [nameof(TextCell.DetailColor)] = MapDetailColor,
            [nameof(Cell.IsEnabled)] = MapIsEnabled,
            [nameof(Cell.Height)] = MapHeight,
            ["ContextActions"] = MapContextActions,
        };

    public static CommandMapper<TextCell, TextCellHandler> CommandMapper =
        new(ElementCommandMapper);

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

    protected override MauiTextCell CreatePlatformElement()
    {
        var cell = new MauiTextCell();
        cell.PointerReleased += OnCellPointerReleased;
        cell.AttachedToVisualTree += OnCellAttachedToVisualTree;
        cell.DetachedFromVisualTree += OnCellDetachedFromVisualTree;
        return cell;
    }

    protected override void DisconnectHandler(MauiTextCell platformView)
    {
        platformView.PointerReleased -= OnCellPointerReleased;
        platformView.AttachedToVisualTree -= OnCellAttachedToVisualTree;
        platformView.DetachedFromVisualTree -= OnCellDetachedFromVisualTree;
        base.DisconnectHandler(platformView);
    }

    private void OnCellAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        VirtualView?.SendAppearing();
    }

    private void OnCellDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        VirtualView?.SendDisappearing();
    }

    private void OnCellPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (VirtualView?.Command?.CanExecute(VirtualView.CommandParameter) == true)
        {
            VirtualView.Command.Execute(VirtualView.CommandParameter);
        }
    }

    public static void MapText(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateText(textCell);
    }

    public static void MapDetail(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateDetail(textCell);
    }

    public static void MapTextColor(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateTextColor(textCell);
    }

    public static void MapDetailColor(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateDetailColor(textCell);
    }

    public static void MapIsEnabled(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateIsEnabled(textCell);
    }

    public static void MapHeight(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateHeight(textCell);
    }

    public static void MapContextActions(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateContextActions(textCell);
    }
}