using System;
using Avalonia.Threading;
using Microsoft.Maui.Animations;

namespace Avalonia.Controls.Maui.Animations;

/// <summary>
/// An Avalonia-based implementation of <see cref="ITicker"/> that uses <see cref="DispatcherTimer"/>
/// to ensure animation callbacks run on the UI thread.
/// </summary>
public class AvaloniaTicker : ITicker, IDisposable
{
    private DispatcherTimer? _timer;
    private bool _systemEnabled = true;
    private bool _disposed;

    /// <inheritdoc/>
    public virtual int MaxFps { get; set; } = 60;

    /// <inheritdoc/>
    public Action? Fire { get; set; }

    /// <inheritdoc/>
    public virtual bool IsRunning => _timer?.IsEnabled ?? false;

    /// <inheritdoc/>
    public virtual bool SystemEnabled
    {
        get => _systemEnabled;
        protected set
        {
            if (_systemEnabled != value)
            {
                _systemEnabled = value;
                OnSystemEnabledChanged();
            }
        }
    }

    /// <inheritdoc/>
    public virtual void Start()
    {
        if (_timer != null)
            return;

        // Use DispatcherTimer to ensure Fire callback runs on UI thread
        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / MaxFps)
        };

        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    /// <inheritdoc/>
    public virtual void Stop()
    {
        if (_timer == null)
            return;

        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _timer = null;
    }

    private void OnTimerTick(object? sender, EventArgs e) =>
        Fire?.Invoke();

    protected virtual void OnSystemEnabledChanged()
    {
        if (IsRunning && !_systemEnabled)
        {
            // Animations are disabled; fire one last time to allow
            // AnimationManager to force-finish any animations in progress.
            Fire?.Invoke();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Stop();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
