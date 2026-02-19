using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Devices;

/// <summary>
/// Avalonia implementation of <see cref="IDeviceDisplay"/>.
/// Provides display information using Avalonia's <see cref="Screens"/> API,
/// primarily targeted at Linux and WebAssembly platforms.
/// </summary>
public partial class AvaloniaDeviceDisplay : IDeviceDisplay
{
    private readonly object _displayEventSync = new();
    private bool _keepScreenOn;
    private uint? _linuxInhibitCookie;
    private Screens? _observedScreens;
    private DisplayInfo? _lastMainDisplayInfo;
    private EventHandler<DisplayInfoChangedEventArgs>? _mainDisplayInfoChanged;

    /// <inheritdoc/>
    public bool KeepScreenOn
    {
        get => _keepScreenOn;
        set 
        {
            if (_keepScreenOn != value)
            {
                _keepScreenOn = value;
                UpdateKeepScreenOn();
            }
        }
    }

    private void UpdateKeepScreenOn()
    {
        if (OperatingSystem.IsLinux())
        {
            UpdateKeepScreenOnLinux(_keepScreenOn);
        }
        else if (OperatingSystem.IsBrowser())
        {
            UpdateKeepScreenOnBrowser(_keepScreenOn);
        }
    }

    partial void UpdateKeepScreenOnLinux(bool value);
    partial void UpdateKeepScreenOnBrowser(bool value);


    /// <inheritdoc/>
    public DisplayInfo MainDisplayInfo
    {
        get
        {
            try
            {
                var screens = GetScreens();
                if (screens == null)
                    return DefaultDisplayInfo;

                var primary = screens.Primary ?? (screens.All.Count > 0 ? screens.All[0] : null);
                if (primary == null)
                    return DefaultDisplayInfo;

                var bounds = primary.Bounds;
                var scale = primary.Scaling;
                var width = (double)bounds.Width;
                var height = (double)bounds.Height;
                var orientation = width >= height ? DisplayOrientation.Landscape : DisplayOrientation.Portrait;
                
                return new DisplayInfo(width, height, scale, orientation, DisplayRotation.Rotation0, 60.0f);
            }
            catch
            {
                return DefaultDisplayInfo;
            }
        }
    }

    /// <inheritdoc/>
    public event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChanged
    {
        add
        {
            _mainDisplayInfoChanged += value;
            UpdateDisplayMonitoring();
        }
        remove
        {
            _mainDisplayInfoChanged -= value;
            UpdateDisplayMonitoring();
        }
    }

    private void UpdateDisplayMonitoring()
    {
        lock (_displayEventSync)
        {
            if (_mainDisplayInfoChanged is null)
            {
                DetachFromScreens();
                _lastMainDisplayInfo = null;
                return;
            }

            _lastMainDisplayInfo ??= MainDisplayInfo;
            AttachToScreens();
        }
    }

    private void AttachToScreens()
    {
        var screens = GetScreens();
        if (ReferenceEquals(_observedScreens, screens))
            return;

        DetachFromScreens();
        _observedScreens = screens;
        if (_observedScreens is not null)
            _observedScreens.Changed += OnScreensChanged;
    }

    private void DetachFromScreens()
    {
        if (_observedScreens is not null)
            _observedScreens.Changed -= OnScreensChanged;

        _observedScreens = null;
    }

    private void OnScreensChanged(object? sender, EventArgs e)
    {
        DisplayInfo changedDisplayInfo;
        bool raiseEvent;

        lock (_displayEventSync)
        {
            AttachToScreens();

            changedDisplayInfo = MainDisplayInfo;
            if (_lastMainDisplayInfo is null || _lastMainDisplayInfo.Value == changedDisplayInfo)
                return;

            _lastMainDisplayInfo = changedDisplayInfo;
            raiseEvent = _mainDisplayInfoChanged is not null;
        }

        if (raiseEvent)
            _mainDisplayInfoChanged?.Invoke(this, new DisplayInfoChangedEventArgs(changedDisplayInfo));
    }

    private Screens? GetScreens()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.Screens;
        }
        else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView && singleView.MainView != null)
        {
            return TopLevel.GetTopLevel(singleView.MainView)?.Screens;
        }

        return null;
    }

    private static DisplayInfo DefaultDisplayInfo => new DisplayInfo(1920, 1080, 1.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 60.0f);
}
