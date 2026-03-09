using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Avalonia handler for <see cref="EntryCell"/>.
/// </summary>
public class EntryCellHandler : ElementHandler<EntryCell, MauiEntryCell>
{
    /// <summary>
    /// Property mapper for <see cref="EntryCellHandler"/>.
    /// </summary>
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

    /// <summary>
    /// Command mapper for <see cref="EntryCellHandler"/>.
    /// </summary>
    public static CommandMapper<EntryCell, EntryCellHandler> CommandMapper =
        new(ElementCommandMapper);

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryCellHandler"/> class.
    /// </summary>
    public EntryCellHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryCellHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public EntryCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryCellHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public EntryCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform element for this handler.
    /// </summary>
    protected override MauiEntryCell CreatePlatformElement()
    {
        var cell = new MauiEntryCell();
        cell.Input.TextChanged += OnTextChanged;
        cell.Input.KeyDown += OnInputKeyDown;
        cell.AttachedToVisualTree += OnCellAttachedToVisualTree;
        cell.DetachedFromVisualTree += OnCellDetachedFromVisualTree;
        return cell;
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// Maps the Label property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the EntryCell.</param>
    /// <param name="entryCell">The MAUI EntryCell virtual view.</param>
    public static void MapLabel(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateLabel(entryCell);
    }

    /// <summary>
    /// Maps the Text property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the EntryCell.</param>
    /// <param name="entryCell">The MAUI EntryCell virtual view.</param>
    public static void MapText(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateText(entryCell, false);
    }

    /// <summary>
    /// Maps the Placeholder property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the EntryCell.</param>
    /// <param name="entryCell">The MAUI EntryCell virtual view.</param>
    public static void MapPlaceholder(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdatePlaceholder(entryCell);
    }

    /// <summary>
    /// Maps the LabelColor property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the EntryCell.</param>
    /// <param name="entryCell">The MAUI EntryCell virtual view.</param>
    public static void MapLabelColor(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateLabelColor(entryCell);
    }

    /// <summary>
    /// Maps the HorizontalTextAlignment property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the EntryCell.</param>
    /// <param name="entryCell">The MAUI EntryCell virtual view.</param>
    public static void MapHorizontalTextAlignment(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateHorizontalTextAlignment(entryCell);
    }

    /// <summary>
    /// Maps the VerticalTextAlignment property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the EntryCell.</param>
    /// <param name="entryCell">The MAUI EntryCell virtual view.</param>
    public static void MapVerticalTextAlignment(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateVerticalTextAlignment(entryCell);
    }

    /// <summary>
    /// Maps the Keyboard property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the EntryCell.</param>
    /// <param name="entryCell">The MAUI EntryCell virtual view.</param>
    public static void MapKeyboard(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateKeyboard(entryCell);
    }

    /// <summary>
    /// Maps the IsEnabled property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the EntryCell.</param>
    /// <param name="entryCell">The MAUI EntryCell virtual view.</param>
    public static void MapIsEnabled(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateIsEnabled(entryCell);
    }

    /// <summary>
    /// Maps the ContextActions property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the EntryCell.</param>
    /// <param name="entryCell">The MAUI EntryCell virtual view.</param>
    public static void MapContextActions(EntryCellHandler handler, EntryCell entryCell)
    {
        handler.PlatformView.UpdateContextActions(entryCell);
    }
}