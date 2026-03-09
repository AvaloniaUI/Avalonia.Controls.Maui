using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Avalonia handler for <see cref="SwitchCell"/>.
/// </summary>
public class SwitchCellHandler : ElementHandler<SwitchCell, MauiSwitchCell>
{
    /// <summary>
    /// Property mapper for <see cref="SwitchCellHandler"/>.
    /// </summary>
    public static IPropertyMapper<SwitchCell, SwitchCellHandler> Mapper =
        new PropertyMapper<SwitchCell, SwitchCellHandler>(ElementMapper)
        {
            [nameof(SwitchCell.Text)] = MapText,
            [nameof(SwitchCell.On)] = MapOn,
            [nameof(SwitchCell.OnColor)] = MapOnColor,
            [nameof(Cell.IsEnabled)] = MapIsEnabled,
            ["ContextActions"] = MapContextActions,
        };

    /// <summary>
    /// Command mapper for <see cref="SwitchCellHandler"/>.
    /// </summary>
    public static CommandMapper<SwitchCell, SwitchCellHandler> CommandMapper =
        new(ElementCommandMapper);

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchCellHandler"/> class.
    /// </summary>
    public SwitchCellHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchCellHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public SwitchCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchCellHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public SwitchCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform element for this handler.
    /// </summary>
    protected override MauiSwitchCell CreatePlatformElement()
    {
        var cell = new MauiSwitchCell();
        cell.AttachedToVisualTree += OnCellAttachedToVisualTree;
        cell.DetachedFromVisualTree += OnCellDetachedFromVisualTree;
        return cell;
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(MauiSwitchCell platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ToggleSwitch.IsCheckedChanged += OnCheckedChanged;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(MauiSwitchCell platformView)
    {
        base.DisconnectHandler(platformView);
        platformView.ToggleSwitch.IsCheckedChanged -= OnCheckedChanged;
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

    private void OnCheckedChanged(object? sender, Interactivity.RoutedEventArgs e)
    {
        if (VirtualView != null)
        {
            if (VirtualView.On != PlatformView.ToggleSwitch.IsChecked)
            {
                VirtualView.On = PlatformView.ToggleSwitch.IsChecked ?? false;
            }
        }
    }

    /// <summary>
    /// Maps the Text property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the SwitchCell.</param>
    /// <param name="switchCell">The MAUI SwitchCell virtual view.</param>
    public static void MapText(SwitchCellHandler handler, SwitchCell switchCell)
    {
        handler.PlatformView.UpdateText(switchCell);
    }

    /// <summary>
    /// Maps the On property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the SwitchCell.</param>
    /// <param name="switchCell">The MAUI SwitchCell virtual view.</param>
    public static void MapOn(SwitchCellHandler handler, SwitchCell switchCell)
    {
        handler.PlatformView.UpdateOn(switchCell, false);
    }

    /// <summary>
    /// Maps the OnColor property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the SwitchCell.</param>
    /// <param name="switchCell">The MAUI SwitchCell virtual view.</param>
    public static void MapOnColor(SwitchCellHandler handler, SwitchCell switchCell)
    {
        handler.PlatformView.UpdateOnColor(switchCell);
    }

    /// <summary>
    /// Maps the IsEnabled property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the SwitchCell.</param>
    /// <param name="switchCell">The MAUI SwitchCell virtual view.</param>
    public static void MapIsEnabled(SwitchCellHandler handler, SwitchCell switchCell)
    {
        handler.PlatformView.UpdateIsEnabled(switchCell);
    }

    /// <summary>
    /// Maps the ContextActions property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the SwitchCell.</param>
    /// <param name="switchCell">The MAUI SwitchCell virtual view.</param>
    public static void MapContextActions(SwitchCellHandler handler, SwitchCell switchCell)
    {
        handler.PlatformView.UpdateContextActions(switchCell);
    }
}