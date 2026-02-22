using System;
using Avalonia.Threading;
using Microsoft.Maui.Animations;

namespace Avalonia.Controls.Maui.Animations;

/// <summary>
/// An Avalonia-based implementation of <see cref="ITicker"/> that uses <see cref="DispatcherTimer"/>
/// to ensure animation callbacks run on the UI thread.
/// </summary>
public class AvaloniaTicker : Ticker
{
    private DispatcherTimer? _timer;

    /// <inheritdoc/>
    public override void Start()
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
    public override void Stop()
    {
        if (_timer == null)
            return;

        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _timer = null;
    }

    private void OnTimerTick(object? sender, EventArgs e) =>
        Fire?.Invoke();
}
