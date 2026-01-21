using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

public class TableViewHandler : ViewHandler<TableView, MauiTableView>
{
    public static IPropertyMapper<TableView, TableViewHandler> Mapper =
        new PropertyMapper<TableView, TableViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(TableView.Root)] = MapRoot,
            [nameof(TableView.RowHeight)] = MapRowHeight,
            [nameof(TableView.HasUnevenRows)] = MapHasUnevenRows,
            [nameof(TableView.Intent)] = MapIntent,
        };

    public static CommandMapper<TableView, TableViewHandler> CommandMapper =
        new(ViewCommandMapper);

    public TableViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public TableViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public TableViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override MauiTableView CreatePlatformView()
    {
        return new MauiTableView();
    }

    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        PlatformView.TableView = VirtualView;
        PlatformView.MauiContext = MauiContext;
    }

    public override bool NeedsContainer => false;

    public static void MapRoot(TableViewHandler handler, TableView tableView)
    {
        if (handler.PlatformView is not null)
        {
            handler.PlatformView.UpdateRoot(tableView);
        }
    }

    public static void MapRowHeight(TableViewHandler handler, TableView tableView)
    {
        if (handler.PlatformView is not null)
        {
            handler.PlatformView.UpdateRowHeight(tableView);
        }
    }

    public static void MapHasUnevenRows(TableViewHandler handler, TableView tableView)
    {
        if (handler.PlatformView is not null)
        {
            handler.PlatformView.UpdateHasUnevenRows(tableView);
        }
    }

    public static void MapIntent(TableViewHandler handler, TableView tableView)
    {
        if (handler.PlatformView is not null)
        {
            handler.PlatformView.UpdateIntent(tableView);
        }
    }
}