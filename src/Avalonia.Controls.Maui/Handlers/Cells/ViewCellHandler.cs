using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;

namespace Avalonia.Controls.Maui.Handlers.Cells;

/// <summary>
/// Handler for MAUI ViewCell (Cell with custom View content)
/// </summary>
public class ViewCellHandler : ElementHandler<ViewCell, global::Avalonia.Controls.Border>
{
    public static IPropertyMapper<ViewCell, ViewCellHandler> Mapper =
        new PropertyMapper<ViewCell, ViewCellHandler>(ElementMapper)
        {
            [nameof(ViewCell.View)] = MapView,
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

    protected override global::Avalonia.Controls.Border CreatePlatformElement()
    {
        return new global::Avalonia.Controls.Border
        {
            Padding = new Thickness(0),
            Background = global::Avalonia.Media.Brushes.Transparent,
            MinHeight = 44
        };
    }

    public static void MapView(ViewCellHandler handler, ViewCell viewCell)
    {
        if (handler.PlatformView is null || handler.MauiContext is null)
            return;

        handler.PlatformView.Child = null;

        if (viewCell.View != null)
        {
            var platformView = viewCell.View.ToPlatform(handler.MauiContext);
            if (platformView is Control control)
            {
                handler.PlatformView.Child = control;
            }
        }
    }
}
