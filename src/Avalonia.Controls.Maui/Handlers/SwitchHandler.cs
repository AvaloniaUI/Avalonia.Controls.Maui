using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Avalonia.Interactivity;
using PlatformView = Avalonia.Controls.ToggleSwitch;

namespace Avalonia.Controls.Maui.Handlers;

public partial class SwitchHandler : ViewHandler<ISwitch, PlatformView>, ISwitchHandler
{
    public static IPropertyMapper<ISwitch, ISwitchHandler> Mapper = new PropertyMapper<ISwitch, ISwitchHandler>(ViewHandler.ViewMapper)
    {
        [nameof(ISwitch.IsOn)] = MapIsOn,
        [nameof(ISwitch.ThumbColor)] = MapThumbColor,
        [nameof(ISwitch.TrackColor)] = MapTrackColor,
    };

    public static CommandMapper<ISwitch, ISwitchHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
    {
    };

    public SwitchHandler() : base(Mapper, CommandMapper)
    {
    }

    public SwitchHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public SwitchHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView
        {
            OffContent = null,
            OnContent = null
        };
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.IsCheckedChanged += OnIsCheckedChanged;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.IsCheckedChanged -= OnIsCheckedChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        var isChecked = PlatformView.IsChecked ?? false;
        if (VirtualView.IsOn != isChecked)
        {
            VirtualView.IsOn = isChecked;
        }
    }

    public static void MapIsOn(ISwitchHandler handler, ISwitch view)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.IsChecked = view.IsOn;
        }
    }

    public static void MapTrackColor(ISwitchHandler handler, ISwitch view)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        // Map track color using Avalonia's Background property
        if (view.TrackColor != null)
        {
            var color = view.TrackColor;
            var avaloniaColor = global::Avalonia.Media.Color.FromArgb(
                (byte)(color.Alpha * 255),
                (byte)(color.Red * 255),
                (byte)(color.Green * 255),
                (byte)(color.Blue * 255));

            platformView.Background = new global::Avalonia.Media.SolidColorBrush(avaloniaColor);
        }
    }

    public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        // Avalonia's ToggleSwitch thumb color is typically controlled through styles/themes
        // For a basic implementation, we could set the Foreground which affects the knob
        if (view.ThumbColor != null)
        {
            var color = view.ThumbColor;
            var avaloniaColor = global::Avalonia.Media.Color.FromArgb(
                (byte)(color.Alpha * 255),
                (byte)(color.Red * 255),
                (byte)(color.Green * 255),
                (byte)(color.Blue * 255));

            platformView.Foreground = new global::Avalonia.Media.SolidColorBrush(avaloniaColor);
        }
    }

    ISwitch ISwitchHandler.VirtualView => VirtualView;

    object ISwitchHandler.PlatformView => PlatformView;
}
