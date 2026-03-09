using Avalonia.Controls.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Compatibility.Handlers;

/// <summary>
/// Avalonia handler for <see cref="TableView"/>.
/// </summary>
public class TableViewHandler : ViewHandler<TableView, MauiTableView>
{
    /// <summary>
    /// Property mapper for <see cref="TableViewHandler"/>.
    /// </summary>
    public static IPropertyMapper<TableView, TableViewHandler> Mapper =
        new PropertyMapper<TableView, TableViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(TableView.Root)] = MapRoot,
            [nameof(TableView.RowHeight)] = MapRowHeight,
            [nameof(TableView.HasUnevenRows)] = MapHasUnevenRows,
            [nameof(TableView.Intent)] = MapIntent,
        };

    /// <summary>
    /// Command mapper for <see cref="TableViewHandler"/>.
    /// </summary>
    public static CommandMapper<TableView, TableViewHandler> CommandMapper =
        new(ViewCommandMapper);

    /// <summary>
    /// Initializes a new instance of the <see cref="TableViewHandler"/> class.
    /// </summary>
    public TableViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableViewHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public TableViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableViewHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public TableViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform view for this handler.
    /// </summary>
    protected override MauiTableView CreatePlatformView()
    {
        return new MauiTableView();
    }

    /// <inheritdoc/>
    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        PlatformView.TableView = VirtualView;
        PlatformView.MauiContext = MauiContext;
    }

    /// <inheritdoc/>
    public override bool NeedsContainer => false;

    /// <summary>
    /// Maps the Root property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TableView.</param>
    /// <param name="tableView">The MAUI TableView virtual view.</param>
    public static void MapRoot(TableViewHandler handler, TableView tableView)
    {
        if (handler.PlatformView is not null)
        {
            handler.PlatformView.UpdateRoot(tableView);
        }
    }

    /// <summary>
    /// Maps the RowHeight property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TableView.</param>
    /// <param name="tableView">The MAUI TableView virtual view.</param>
    public static void MapRowHeight(TableViewHandler handler, TableView tableView)
    {
        if (handler.PlatformView is not null)
        {
            handler.PlatformView.UpdateRowHeight(tableView);
        }
    }

    /// <summary>
    /// Maps the HasUnevenRows property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TableView.</param>
    /// <param name="tableView">The MAUI TableView virtual view.</param>
    public static void MapHasUnevenRows(TableViewHandler handler, TableView tableView)
    {
        if (handler.PlatformView is not null)
        {
            handler.PlatformView.UpdateHasUnevenRows(tableView);
        }
    }

    /// <summary>
    /// Maps the Intent property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TableView.</param>
    /// <param name="tableView">The MAUI TableView virtual view.</param>
    public static void MapIntent(TableViewHandler handler, TableView tableView)
    {
        if (handler.PlatformView is not null)
        {
            handler.PlatformView.UpdateIntent(tableView);
        }
    }
}