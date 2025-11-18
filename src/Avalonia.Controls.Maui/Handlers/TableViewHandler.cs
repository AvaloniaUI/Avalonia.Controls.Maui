using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using System;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI TableView to Avalonia MauiTableView mapping
/// </summary>
public class TableViewHandler : ViewHandler<Microsoft.Maui.Controls.TableView, MauiTableView>
{
    public static IPropertyMapper<Microsoft.Maui.Controls.TableView, TableViewHandler> Mapper =
        new PropertyMapper<Microsoft.Maui.Controls.TableView, TableViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.TableView.Root)] = MapRoot,
            [nameof(Microsoft.Maui.Controls.TableView.RowHeight)] = MapRowHeight,
            [nameof(Microsoft.Maui.Controls.TableView.HasUnevenRows)] = MapHasUnevenRows,
            [nameof(Microsoft.Maui.Controls.TableView.Intent)] = MapIntent,
        };

    public static CommandMapper<Microsoft.Maui.Controls.TableView, TableViewHandler> CommandMapper =
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

    public static void MapRoot(TableViewHandler handler, Microsoft.Maui.Controls.TableView tableView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // The platform view listens to ModelChanged events, so we just need to trigger an update
        handler.PlatformView.TableView = tableView;
    }

    public static void MapRowHeight(TableViewHandler handler, Microsoft.Maui.Controls.TableView tableView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // RowHeight handling - could be implemented if needed
        // For now, cells will size themselves
    }

    public static void MapHasUnevenRows(TableViewHandler handler, Microsoft.Maui.Controls.TableView tableView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // HasUnevenRows handling - cells already size themselves in Avalonia
        // This is the default behavior
    }

    public static void MapIntent(TableViewHandler handler, Microsoft.Maui.Controls.TableView tableView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // TableIntent (Data, Form, Settings, Menu) - could affect styling
        // For now, we use the same rendering for all intents
    }
}
