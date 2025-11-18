using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using AvaloniaCheckBox = Avalonia.Controls.CheckBox;

namespace Avalonia.Controls.Maui.Handlers;

public class CheckBoxHandler : ViewHandler<ICheckBox, AvaloniaCheckBox>, ICheckBoxHandler
{
    public static IPropertyMapper<ICheckBox, ICheckBoxHandler> Mapper = new PropertyMapper<ICheckBox, ICheckBoxHandler>(ViewHandler.ViewMapper)
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

    protected override AvaloniaCheckBox CreatePlatformView()
    {
        return new AvaloniaCheckBox();
    }

    protected override void ConnectHandler(AvaloniaCheckBox platformView)
    {
        base.ConnectHandler(platformView);
        platformView.IsCheckedChanged += OnIsCheckedChanged;
    }

    protected override void DisconnectHandler(AvaloniaCheckBox platformView)
    {
        base.DisconnectHandler(platformView);
        platformView.IsCheckedChanged -= OnIsCheckedChanged;
    }

    void OnIsCheckedChanged(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is AvaloniaCheckBox platformView && VirtualView != null)
        {
            VirtualView.IsChecked = platformView.IsChecked ?? false;
        }
    }

    public override bool NeedsContainer => false;

    public static void MapBackground(ICheckBoxHandler handler, ICheckBox checkBox)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        ((AvaloniaCheckBox)handler.PlatformView)?.UpdateBackground(checkBox);
    }

    public static void MapIsChecked(ICheckBoxHandler handler, ICheckBox checkBox) =>
        ((AvaloniaCheckBox)handler.PlatformView)?.UpdateIsChecked(checkBox);

    public static void MapForeground(ICheckBoxHandler handler, ICheckBox checkBox) =>
        ((AvaloniaCheckBox)handler.PlatformView)?.UpdateForeground(checkBox);
}

public static class CheckBoxExtensions
{
    public static void UpdateIsChecked(this AvaloniaCheckBox checkBox, ICheckBox virtualCheckBox)
    {
        checkBox.IsChecked = virtualCheckBox.IsChecked;
    }

    public static void UpdateForeground(this AvaloniaCheckBox checkBox, ICheckBox virtualCheckBox)
    {
        if (virtualCheckBox.Foreground != null)
        {
            var brush = virtualCheckBox.Foreground.ToPlatform();
            checkBox.BorderBrush = brush;
            checkBox.Foreground = brush;
        }
        else
        {
            checkBox.ClearValue(AvaloniaCheckBox.BorderBrushProperty);
            checkBox.ClearValue(AvaloniaCheckBox.ForegroundProperty);
        }
    }

    internal static void UpdateBackground(this AvaloniaCheckBox checkBox, IView view)
    {
        if (view.Background != null)
        {
            checkBox.Background = view.Background.ToPlatform();
        }
        else
        {
            checkBox.ClearValue(AvaloniaCheckBox.BackgroundProperty);
        }
    }
}