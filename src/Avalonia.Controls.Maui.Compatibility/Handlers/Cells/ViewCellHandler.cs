using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Compatibility;

public class ViewCellHandler : ElementHandler<ViewCell, MauiViewCell>
{
    public static IPropertyMapper<ViewCell, ViewCellHandler> Mapper =
        new PropertyMapper<ViewCell, ViewCellHandler>(ElementMapper)
        {
            [nameof(ViewCell.View)] = MapView,
            [nameof(Cell.IsEnabled)] = MapIsEnabled,
            ["ContextActions"] = MapContextActions,
        };

    public static CommandMapper<ViewCell, ViewCellHandler> CommandMapper =
        new(ElementCommandMapper);

    public ViewCellHandler() : base(Mapper, CommandMapper)
    {
    }

    public ViewCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ViewCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override MauiViewCell CreatePlatformElement()
    {
        var cell = new MauiViewCell();
        cell.AttachedToVisualTree += OnCellAttachedToVisualTree;
        cell.DetachedFromVisualTree += OnCellDetachedFromVisualTree;
        return cell;
    }

    protected override void DisconnectHandler(MauiViewCell platformView)
    {
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

    public static void MapIsEnabled(ViewCellHandler handler, ViewCell viewCell)
    {
        handler.PlatformView.UpdateIsEnabled(viewCell);
    }

    public static void MapView(ViewCellHandler handler, ViewCell viewCell)
    {
        handler.PlatformView.UpdateView(viewCell, handler.MauiContext);
    }

    public static void MapContextActions(ViewCellHandler handler, ViewCell viewCell)
    {
        handler.PlatformView.UpdateContextActions(viewCell);
    }
}