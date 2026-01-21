using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Avalonia.Controls.Maui.Handlers.Cells;

public class SwitchCellHandler : ElementHandler<SwitchCell, MauiSwitchCell>
{
    public static IPropertyMapper<SwitchCell, SwitchCellHandler> Mapper =
        new PropertyMapper<SwitchCell, SwitchCellHandler>(ElementMapper)
        {
            [nameof(SwitchCell.Text)] = MapText,
            [nameof(SwitchCell.On)] = MapOn,
            [nameof(SwitchCell.OnColor)] = MapOnColor,
            [nameof(Cell.IsEnabled)] = MapIsEnabled,
            ["ContextActions"] = MapContextActions,
        };

    public static CommandMapper<SwitchCell, SwitchCellHandler> CommandMapper =
        new(ElementCommandMapper);

    public SwitchCellHandler() : base(Mapper, CommandMapper)
    {
    }

    public SwitchCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public SwitchCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override MauiSwitchCell CreatePlatformElement()
    {
        var cell = new MauiSwitchCell();
        cell.AttachedToVisualTree += OnCellAttachedToVisualTree;
        cell.DetachedFromVisualTree += OnCellDetachedFromVisualTree;
        return cell;
    }

    protected override void ConnectHandler(MauiSwitchCell platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ToggleSwitch.IsCheckedChanged += OnCheckedChanged;
    }

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

    public static void MapText(SwitchCellHandler handler, SwitchCell switchCell)
    {
        handler.PlatformView.UpdateText(switchCell);
    }

    public static void MapOn(SwitchCellHandler handler, SwitchCell switchCell)
    {
        handler.PlatformView.UpdateOn(switchCell, false);
    }

    public static void MapOnColor(SwitchCellHandler handler, SwitchCell switchCell)
    {
        handler.PlatformView.UpdateOnColor(switchCell);
    }

    public static void MapIsEnabled(SwitchCellHandler handler, SwitchCell switchCell)
    {
        handler.PlatformView.UpdateIsEnabled(switchCell);
    }

    public static void MapContextActions(SwitchCellHandler handler, SwitchCell switchCell)
    {
        handler.PlatformView.UpdateContextActions(switchCell);
    }
}