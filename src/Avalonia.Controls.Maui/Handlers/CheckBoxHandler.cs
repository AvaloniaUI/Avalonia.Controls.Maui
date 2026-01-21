using Microsoft.Maui;
using Avalonia.Controls.Maui.Platform;
using PlatformView = Avalonia.Controls.CheckBox;

namespace Avalonia.Controls.Maui.Handlers;

public class CheckBoxHandler : ViewHandler<ICheckBox, PlatformView>
{
    public static IPropertyMapper<ICheckBox, CheckBoxHandler> Mapper =
        new PropertyMapper<ICheckBox, CheckBoxHandler>(ViewHandler.ViewMapper)
        {
            [nameof(ICheckBox.Background)] = MapBackground,
            [nameof(ICheckBox.IsChecked)] = MapIsChecked,
            [nameof(ICheckBox.Foreground)] = MapForeground,
            ["Color"] = MapColor, // Color is on CheckBox class, not ICheckBox interface
        };

    public static CommandMapper<ICheckBox, CheckBoxHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public CheckBoxHandler() : base(Mapper, CommandMapper)
    {
    }

    public CheckBoxHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public CheckBoxHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.IsCheckedChanged += OnIsCheckedChanged;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        base.DisconnectHandler(platformView);
        platformView.IsCheckedChanged -= OnIsCheckedChanged;
    }

    void OnIsCheckedChanged(object? sender, Interactivity.RoutedEventArgs e)
    {
        if (sender is PlatformView platformView && VirtualView != null)
        {
            VirtualView.IsChecked = platformView.IsChecked ?? false;
        }
    }

    public override bool NeedsContainer => false;

    public static void MapBackground(CheckBoxHandler handler, ICheckBox checkBox)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        ((PlatformView)handler.PlatformView)?.UpdateBackground(checkBox);
    }

    public static void MapIsChecked(CheckBoxHandler handler, ICheckBox checkBox) =>
        ((PlatformView)handler.PlatformView)?.UpdateIsChecked(checkBox);

    public static void MapForeground(CheckBoxHandler handler, ICheckBox checkBox) =>
        ((PlatformView)handler.PlatformView)?.UpdateForeground(checkBox);

    public static void MapColor(CheckBoxHandler handler, ICheckBox checkBox) =>
        handler.UpdateValue(nameof(ICheckBox.Foreground));
}