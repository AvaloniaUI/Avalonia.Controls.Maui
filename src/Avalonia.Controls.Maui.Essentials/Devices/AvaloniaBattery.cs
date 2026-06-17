using System;
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials.Devices;

/// <summary>
/// Avalonia implementation of <see cref="IBattery"/> that provides battery information
/// and energy-saver status across Linux, browser, and native MAUI platforms.
/// </summary>
public partial class AvaloniaBattery : IBattery
{
    private static readonly Lazy<AvaloniaBattery> _current = new(() => new AvaloniaBattery());

    /// <summary>
    /// Provides the default implementation for static usage of this API.
    /// </summary>
    public static IBattery Default => _current.Value;

    private readonly object _eventSync = new();
    private System.Threading.Timer? _linuxPollingTimer;
    private BatterySnapshot? _lastBatterySnapshot;
    private EnergySaverStatus? _lastEnergySaverStatus;
    private bool _isForwardingBatteryInfoChanged;
    private bool _isForwardingEnergySaverStatusChanged;

    private EventHandler<BatteryInfoChangedEventArgs>? _batteryInfoChanged;
    private EventHandler<EnergySaverStatusChangedEventArgs>? _energySaverStatusChanged;

    /// <inheritdoc/>
    public double ChargeLevel => GetPlatformChargeLevel() ?? 1.0;

    /// <inheritdoc/>
    public BatteryState State => GetPlatformState() ?? BatteryState.Unknown;

    /// <inheritdoc/>
    public BatteryPowerSource PowerSource => GetPlatformPowerSource() ?? BatteryPowerSource.Unknown;

    /// <inheritdoc/>
    public EnergySaverStatus EnergySaverStatus => GetPlatformEnergySaverStatus() ?? EnergySaverStatus.Unknown;

    /// <inheritdoc/>
    public event EventHandler<BatteryInfoChangedEventArgs>? BatteryInfoChanged
    {
        add
        {
            _batteryInfoChanged += value;
            UpdateEventMonitoring();
        }
        remove
        {
            _batteryInfoChanged -= value;
            UpdateEventMonitoring();
        }
    }

    /// <inheritdoc/>
    public event EventHandler<EnergySaverStatusChangedEventArgs>? EnergySaverStatusChanged
    {
        add
        {
            _energySaverStatusChanged += value;
            UpdateEventMonitoring();
        }
        remove
        {
            _energySaverStatusChanged -= value;
            UpdateEventMonitoring();
        }
    }

    /// <summary>
    /// Raises the <see cref="BatteryInfoChanged"/> event.
    /// </summary>
    /// <param name="e">Event data containing the updated battery information.</param>
    protected virtual void OnBatteryInfoChanged(BatteryInfoChangedEventArgs e) => _batteryInfoChanged?.Invoke(this, e);

    /// <summary>
    /// Raises the <see cref="EnergySaverStatusChanged"/> event.
    /// </summary>
    /// <param name="e">Event data containing the updated energy-saver status.</param>
    protected virtual void OnEnergySaverStatusChanged(EnergySaverStatusChangedEventArgs e) => _energySaverStatusChanged?.Invoke(this, e);

    private void UpdateEventMonitoring()
    {
        if (OperatingSystem.IsBrowser())
            return;

        lock (_eventSync)
        {
            if (OperatingSystem.IsLinux())
            {
                UpdateLinuxPolling();
            }
            else
            {
                UpdateNativeForwarding();
            }
        }
    }

    private void UpdateLinuxPolling()
    {
        var hasSubscribers = _batteryInfoChanged is not null || _energySaverStatusChanged is not null;
        if (hasSubscribers)
        {
            if (_linuxPollingTimer is not null)
                return;

            _lastBatterySnapshot = CaptureBatterySnapshot();
            _lastEnergySaverStatus = EnergySaverStatus;
            _linuxPollingTimer = new System.Threading.Timer(PollLinuxState, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5));
        }
        else
        {
            _linuxPollingTimer?.Dispose();
            _linuxPollingTimer = null;
            _lastBatterySnapshot = null;
            _lastEnergySaverStatus = null;
        }
    }

    private void PollLinuxState(object? state)
    {
        var currentBattery = CaptureBatterySnapshot();
        var currentEnergySaverStatus = EnergySaverStatus;

        BatterySnapshot? previousBattery;
        EnergySaverStatus? previousEnergySaverStatus;
        bool raiseBatteryEvent;
        bool raiseEnergySaverEvent;

        lock (_eventSync)
        {
            previousBattery = _lastBatterySnapshot;
            previousEnergySaverStatus = _lastEnergySaverStatus;
            _lastBatterySnapshot = currentBattery;
            _lastEnergySaverStatus = currentEnergySaverStatus;

            raiseBatteryEvent = _batteryInfoChanged is not null && previousBattery is not null && previousBattery.Value != currentBattery;
            raiseEnergySaverEvent = _energySaverStatusChanged is not null &&
                                    previousEnergySaverStatus is not null &&
                                    previousEnergySaverStatus.Value != currentEnergySaverStatus;
        }

        if (raiseBatteryEvent)
            OnBatteryInfoChanged(new BatteryInfoChangedEventArgs(currentBattery.ChargeLevel, currentBattery.State, currentBattery.PowerSource));

        if (raiseEnergySaverEvent)
            OnEnergySaverStatusChanged(new EnergySaverStatusChangedEventArgs(currentEnergySaverStatus));
    }

    private void UpdateNativeForwarding()
    {
        if (_batteryInfoChanged is not null && !_isForwardingBatteryInfoChanged)
            TrySubscribeNativeBatteryInfo();
        else if (_batteryInfoChanged is null && _isForwardingBatteryInfoChanged)
            TryUnsubscribeNativeBatteryInfo();

        if (_energySaverStatusChanged is not null && !_isForwardingEnergySaverStatusChanged)
            TrySubscribeNativeEnergySaverStatus();
        else if (_energySaverStatusChanged is null && _isForwardingEnergySaverStatusChanged)
            TryUnsubscribeNativeEnergySaverStatus();
    }

    private void TrySubscribeNativeBatteryInfo()
    {
        // Skip native forwarding, we are the registered default
        _isForwardingBatteryInfoChanged = false;
    }

    private void TryUnsubscribeNativeBatteryInfo()
    {
        _isForwardingBatteryInfoChanged = false;
    }

    private void TrySubscribeNativeEnergySaverStatus()
    {
        // Skip native forwarding, we are the registered default
        _isForwardingEnergySaverStatusChanged = false;
    }

    private void TryUnsubscribeNativeEnergySaverStatus()
    {
        _isForwardingEnergySaverStatusChanged = false;
    }

    private BatterySnapshot CaptureBatterySnapshot() => new(ChargeLevel, State, PowerSource);

    private readonly record struct BatterySnapshot(double ChargeLevel, BatteryState State, BatteryPowerSource PowerSource);

    private double? GetPlatformChargeLevel()
    {
        double? v = null;
        if (OperatingSystem.IsLinux())
            GetChargeLevelLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetChargeLevelBrowser(ref v);
        return v;
    }

    private BatteryState? GetPlatformState()
    {
        BatteryState? v = null;
        if (OperatingSystem.IsLinux())
            GetStateLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetStateBrowser(ref v);
        return v;
    }

    private BatteryPowerSource? GetPlatformPowerSource()
    {
        BatteryPowerSource? v = null;
        if (OperatingSystem.IsLinux())
            GetPowerSourceLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetPowerSourceBrowser(ref v);
        return v;
    }

    private EnergySaverStatus? GetPlatformEnergySaverStatus()
    {
        EnergySaverStatus? v = null;
        if (OperatingSystem.IsLinux())
            GetEnergySaverStatusLinux(ref v);
        else if (OperatingSystem.IsBrowser())
            GetEnergySaverStatusBrowser(ref v);
        return v;
    }

    partial void GetChargeLevelLinux(ref double? v);
    partial void GetStateLinux(ref BatteryState? v);
    partial void GetPowerSourceLinux(ref BatteryPowerSource? v);
    partial void GetEnergySaverStatusLinux(ref EnergySaverStatus? v);

    partial void GetChargeLevelBrowser(ref double? v);
    partial void GetStateBrowser(ref BatteryState? v);
    partial void GetPowerSourceBrowser(ref BatteryPowerSource? v);
    partial void GetEnergySaverStatusBrowser(ref EnergySaverStatus? v);
}
