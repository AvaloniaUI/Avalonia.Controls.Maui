using Avalonia.Threading;
using Microsoft.Maui.Networking;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of <see cref="IConnectivity"/> with platform-specific
/// backends for desktop (System.Net.NetworkInformation) and browser (Navigator API).
/// </summary>
public sealed partial class AvaloniaConnectivity : IConnectivity
{
    readonly object _eventLock = new();
    EventHandler<ConnectivityChangedEventArgs>? _connectivityChanged;
    bool _isListening;

    /// <inheritdoc/>
    public NetworkAccess NetworkAccess => PlatformGetNetworkAccess();

    /// <inheritdoc/>
    public IEnumerable<ConnectionProfile> ConnectionProfiles => PlatformGetConnectionProfiles();

    /// <inheritdoc/>
    public event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged
    {
        add
        {
            lock (_eventLock)
            {
                var wasNull = _connectivityChanged is null;
                _connectivityChanged += value;
                if (wasNull && _connectivityChanged is not null && !_isListening)
                {
                    _isListening = true;
                    PlatformStartListening();
                }
            }
        }
        remove
        {
            lock (_eventLock)
            {
                _connectivityChanged -= value;
                if (_connectivityChanged is null && _isListening)
                {
                    _isListening = false;
                    PlatformStopListening();
                }
            }
        }
    }

    void RaiseConnectivityChanged()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            var args = new ConnectivityChangedEventArgs(NetworkAccess, ConnectionProfiles);
            _connectivityChanged?.Invoke(this, args);
        }
        else
        {
            Dispatcher.UIThread.Post(RaiseConnectivityChanged);
        }
    }

    private partial NetworkAccess PlatformGetNetworkAccess();
    private partial IEnumerable<ConnectionProfile> PlatformGetConnectionProfiles();
    private partial void PlatformStartListening();
    private partial void PlatformStopListening();
}
