using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Avalonia.Controls.Maui.Handlers.Cells;

public class ViewCellHandler : ElementHandler<ViewCell, MauiViewCell>
{
    public static IPropertyMapper<ViewCell, ViewCellHandler> Mapper =
        new PropertyMapper<ViewCell, ViewCellHandler>(ElementMapper)
        {
            [nameof(ViewCell.View)] = MapView,
            [nameof(Cell.IsEnabled)] = MapIsEnabled,
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
        return new MauiViewCell();
    }

    public static void MapIsEnabled(ViewCellHandler handler, ViewCell viewCell)
    {
        handler.PlatformView.UpdateIsEnabled(viewCell);
    }

    public static void MapView(ViewCellHandler handler, ViewCell viewCell)
    {
        handler.PlatformView.UpdateView(viewCell, handler.MauiContext);
    }
}