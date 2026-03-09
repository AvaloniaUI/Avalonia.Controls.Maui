using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Avalonia.Interactivity;
using PlatformView = Avalonia.Controls.ToggleSwitch;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="ISwitch"/>.</summary>
public partial class SwitchHandler : ViewHandler<ISwitch, PlatformView>
{
    private bool _isUpdating;

    /// <summary>Property mapper for <see cref="SwitchHandler"/>.</summary>
    public static IPropertyMapper<ISwitch, SwitchHandler> Mapper = new PropertyMapper<ISwitch, SwitchHandler>(ViewHandler.ViewMapper)
    {
        [nameof(ISwitch.IsOn)] = MapIsOn,
        [nameof(Microsoft.Maui.Controls.Switch.IsToggled)] = MapIsOn,
        [nameof(ISwitch.ThumbColor)] = MapThumbColor,
        [nameof(ISwitch.TrackColor)] = MapTrackColor,
        [nameof(Microsoft.Maui.Controls.Switch.OnColor)] = MapTrackColor,
    };

    /// <summary>Command mapper for <see cref="SwitchHandler"/>.</summary>
    public static CommandMapper<ISwitch, SwitchHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="SwitchHandler"/>.</summary>
    public SwitchHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SwitchHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public SwitchHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SwitchHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public SwitchHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView
        {
            OffContent = null,
            OnContent = null
        };
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.IsCheckedChanged += OnIsCheckedChanged;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.IsCheckedChanged -= OnIsCheckedChanged;
        base.DisconnectHandler(platformView);
    }

    /// <summary>Maps the IsOn property to the platform view.</summary>
    /// <param name="handler">The handler for the switch.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapIsOn(SwitchHandler handler, ISwitch view)
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

    /// <summary>Maps the TrackColor property to the platform view.</summary>
    /// <param name="handler">The handler for the switch.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapTrackColor(SwitchHandler handler, ISwitch view)
    {
        if (handler is SwitchHandler switchHandler)
        {
            switchHandler.UpdateColors();
        }
    }

    /// <summary>Maps the ThumbColor property to the platform view.</summary>
    /// <param name="handler">The handler for the switch.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapThumbColor(SwitchHandler handler, ISwitch view)
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
}