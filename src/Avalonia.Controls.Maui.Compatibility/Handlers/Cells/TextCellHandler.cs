using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Avalonia handler for <see cref="TextCell"/>.
/// </summary>
public class TextCellHandler : ElementHandler<TextCell, MauiTextCell>
{
    /// <summary>
    /// Property mapper for <see cref="TextCellHandler"/>.
    /// </summary>
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

    /// <summary>
    /// Command mapper for <see cref="TextCellHandler"/>.
    /// </summary>
    public static CommandMapper<TextCell, TextCellHandler> CommandMapper =
        new(ElementCommandMapper);

    /// <summary>
    /// Initializes a new instance of the <see cref="TextCellHandler"/> class.
    /// </summary>
    public TextCellHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextCellHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public TextCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextCellHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public TextCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform element for this handler.
    /// </summary>
    protected override MauiTextCell CreatePlatformElement()
    {
        var cell = new MauiTextCell();
        cell.AddHandler(global::Avalonia.Input.InputElement.TappedEvent, OnCellTapped, global::Avalonia.Interactivity.RoutingStrategies.Bubble, true);
        cell.AttachedToVisualTree += OnCellAttachedToVisualTree;
        cell.DetachedFromVisualTree += OnCellDetachedFromVisualTree;
        return cell;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(MauiTextCell platformView)
    {
        platformView.RemoveHandler(global::Avalonia.Input.InputElement.TappedEvent, OnCellTapped);
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

    private void OnCellTapped(object? sender, global::Avalonia.Input.TappedEventArgs e)
    {
        var cell = VirtualView;
        if (cell == null) return;

        if (cell.Command?.CanExecute(cell.CommandParameter) == true)
        {
            cell.Command.Execute(cell.CommandParameter);
        }
    }

    /// <summary>
    /// Maps the Text property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TextCell.</param>
    /// <param name="textCell">The MAUI TextCell virtual view.</param>
    public static void MapText(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateText(textCell);
    }

    /// <summary>
    /// Maps the Detail property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TextCell.</param>
    /// <param name="textCell">The MAUI TextCell virtual view.</param>
    public static void MapDetail(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateDetail(textCell);
    }

    /// <summary>
    /// Maps the TextColor property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TextCell.</param>
    /// <param name="textCell">The MAUI TextCell virtual view.</param>
    public static void MapTextColor(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateTextColor(textCell);
    }

    /// <summary>
    /// Maps the DetailColor property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TextCell.</param>
    /// <param name="textCell">The MAUI TextCell virtual view.</param>
    public static void MapDetailColor(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateDetailColor(textCell);
    }

    /// <summary>
    /// Maps the IsEnabled property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TextCell.</param>
    /// <param name="textCell">The MAUI TextCell virtual view.</param>
    public static void MapIsEnabled(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateIsEnabled(textCell);
    }

    /// <summary>
    /// Maps the Height property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TextCell.</param>
    /// <param name="textCell">The MAUI TextCell virtual view.</param>
    public static void MapHeight(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateHeight(textCell);
    }

    /// <summary>
    /// Maps the ContextActions property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the TextCell.</param>
    /// <param name="textCell">The MAUI TextCell virtual view.</param>
    public static void MapContextActions(TextCellHandler handler, TextCell textCell)
    {
        handler.PlatformView.UpdateContextActions(textCell);
    }
}