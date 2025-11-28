using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Avalonia.Interactivity;
using PlatformView = Avalonia.Controls.ToggleSwitch;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler that maps MAUI <see cref="ISwitch"/> to Avalonia <see cref="ToggleSwitch"/>.
/// </summary>
public partial class SwitchHandler : ViewHandler<ISwitch, PlatformView>, ISwitchHandler
{
    private bool _isUpdating;

    public static IPropertyMapper<ISwitch, ISwitchHandler> Mapper = new PropertyMapper<ISwitch, ISwitchHandler>(ViewHandler.ViewMapper)
    {
        [nameof(ISwitch.IsOn)] = MapIsOn,
        [nameof(Microsoft.Maui.Controls.Switch.IsToggled)] = MapIsOn,
        [nameof(ISwitch.ThumbColor)] = MapThumbColor,
        [nameof(ISwitch.TrackColor)] = MapTrackColor,
        [nameof(Microsoft.Maui.Controls.Switch.OnColor)] = MapTrackColor,
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

    public static void MapIsOn(ISwitchHandler handler, ISwitch view)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        var switchHandler = handler as SwitchHandler;
        if (switchHandler != null)
        {
            switchHandler._isUpdating = true;
        }

        try
        {
            platformView.UpdateIsOn(view);
            switchHandler?.UpdateColors();
        }
        finally
        {
            if (switchHandler != null)
            {
                switchHandler._isUpdating = false;
            }
        }
    }

    public static void MapTrackColor(ISwitchHandler handler, ISwitch view)
    {
        if (handler is SwitchHandler switchHandler)
        {
            switchHandler.UpdateColors();
        }
    }

    public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
    {
        if (handler is SwitchHandler switchHandler)
        {
            switchHandler.UpdateColors();
        }
    }
    
    private void OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (VirtualView == null || PlatformView == null || _isUpdating)
            return;

        var isChecked = PlatformView.IsChecked ?? false;
        if (VirtualView.IsOn != isChecked)
        {
            VirtualView.IsOn = isChecked;
        }

        // Ensure correct visual states
        UpdateColors();
    }

    private void UpdateColors()
    {
        if (VirtualView == null || PlatformView == null)
            return;

        var fallbackColor = (VirtualView as Microsoft.Maui.Controls.Switch)?.OnColor;
        PlatformView.UpdateTrackColor(VirtualView, fallbackColor);
        PlatformView.UpdateThumbColor(VirtualView);
    }
    
    ISwitch ISwitchHandler.VirtualView => VirtualView;
    object ISwitchHandler.PlatformView => PlatformView;
}