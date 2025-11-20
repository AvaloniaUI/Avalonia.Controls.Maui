using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Platform;
using PlatformView = Avalonia.Controls.CheckBox;

namespace Avalonia.Controls.Maui.Handlers;

public class CheckBoxHandler : ViewHandler<ICheckBox, PlatformView>, ICheckBoxHandler
{
    public static IPropertyMapper<ICheckBox, ICheckBoxHandler> Mapper =
        new PropertyMapper<ICheckBox, ICheckBoxHandler>(ViewHandler.ViewMapper)
        {    
            [nameof(ICheckBox.Background)] = MapBackground,
            [nameof(ICheckBox.IsChecked)] = MapIsChecked,
            [nameof(ICheckBox.Foreground)] = MapForeground,
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

    ICheckBox ICheckBoxHandler.VirtualView => VirtualView;

    System.Object ICheckBoxHandler.PlatformView => PlatformView;

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

    public static void MapBackground(ICheckBoxHandler handler, ICheckBox checkBox)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        ((PlatformView)handler.PlatformView)?.UpdateBackground(checkBox);
    }

    public static void MapIsChecked(ICheckBoxHandler handler, ICheckBox checkBox) =>
        ((PlatformView)handler.PlatformView)?.UpdateIsChecked(checkBox);

    public static void MapForeground(ICheckBoxHandler handler, ICheckBox checkBox) =>
        ((PlatformView)handler.PlatformView)?.UpdateForeground(checkBox);
}