using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Avalonia handler for <see cref="ViewCell"/>.
/// </summary>
public class ViewCellHandler : ElementHandler<ViewCell, MauiViewCell>
{
    /// <summary>
    /// Property mapper for <see cref="ViewCellHandler"/>.
    /// </summary>
    public static IPropertyMapper<ViewCell, ViewCellHandler> Mapper =
        new PropertyMapper<ViewCell, ViewCellHandler>(ElementMapper)
        {
            [nameof(ViewCell.View)] = MapView,
            [nameof(Cell.IsEnabled)] = MapIsEnabled,
            ["ContextActions"] = MapContextActions,
        };

    /// <summary>
    /// Command mapper for <see cref="ViewCellHandler"/>.
    /// </summary>
    public static CommandMapper<ViewCell, ViewCellHandler> CommandMapper =
        new(ElementCommandMapper);

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewCellHandler"/> class.
    /// </summary>
    public ViewCellHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewCellHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public ViewCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewCellHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public ViewCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform element for this handler.
    /// </summary>
    protected override MauiViewCell CreatePlatformElement()
    {
        var cell = new MauiViewCell();
        cell.AttachedToVisualTree += OnCellAttachedToVisualTree;
        cell.DetachedFromVisualTree += OnCellDetachedFromVisualTree;
        return cell;
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// Maps the IsEnabled property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ViewCell.</param>
    /// <param name="viewCell">The MAUI ViewCell virtual view.</param>
    public static void MapIsEnabled(ViewCellHandler handler, ViewCell viewCell)
    {
        handler.PlatformView.UpdateIsEnabled(viewCell);
    }

    /// <summary>
    /// Maps the View property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ViewCell.</param>
    /// <param name="viewCell">The MAUI ViewCell virtual view.</param>
    public static void MapView(ViewCellHandler handler, ViewCell viewCell)
    {
        handler.PlatformView.UpdateView(viewCell, handler.MauiContext);
    }

    /// <summary>
    /// Maps the ContextActions property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ViewCell.</param>
    /// <param name="viewCell">The MAUI ViewCell virtual view.</param>
    public static void MapContextActions(ViewCellHandler handler, ViewCell viewCell)
    {
        handler.PlatformView.UpdateContextActions(viewCell);
    }
}