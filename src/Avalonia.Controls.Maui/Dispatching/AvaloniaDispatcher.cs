using System;
using Microsoft.Maui.Dispatching;

namespace Avalonia.Controls.Maui.Dispatching;

/// <summary>
/// Avalonia implementation of MAUI's IDispatcherProvider
/// </summary>
public class AvaloniaDispatcherProvider : IDispatcherProvider
{
    [ThreadStatic]
    private static IDispatcher? _dispatcher;

    public IDispatcher? GetForCurrentThread()
    {
        if (_dispatcher == null)
        {
            var avaloniaDispatcher = Threading.Dispatcher.UIThread;
            _dispatcher = new AvaloniaDispatcher(avaloniaDispatcher);
        }
        return _dispatcher;
    }
}

/// <summary>
/// Avalonia implementation of MAUI's IDispatcher
/// </summary>
public class AvaloniaDispatcher : IDispatcher
{
    private readonly Threading.Dispatcher _avaloniaDispatcher;

    public AvaloniaDispatcher(Threading.Dispatcher avaloniaDispatcher)
    {
        _avaloniaDispatcher = avaloniaDispatcher ?? throw new ArgumentNullException(nameof(avaloniaDispatcher));
    }

    public bool IsDispatchRequired => !_avaloniaDispatcher.CheckAccess();

    public bool Dispatch(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        _avaloniaDispatcher.Post(action);
        return true;
    }

    public bool DispatchDelayed(TimeSpan delay, Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        _avaloniaDispatcher.Post(() =>
        {
            Threading.DispatcherTimer.RunOnce(action, delay, Threading.DispatcherPriority.Normal);
        }, Threading.DispatcherPriority.Normal);
        return true;
    }

    public IDispatcherTimer CreateTimer()
    {
        return new AvaloniaDispatcherTimer(_avaloniaDispatcher);
    }
}

/// <summary>
/// Avalonia implementation of MAUI's IDispatcherTimer
/// </summary>
public class AvaloniaDispatcherTimer : IDispatcherTimer
{
    private readonly Threading.DispatcherTimer _avaloniaTimer;
    private readonly Threading.Dispatcher _dispatcher;

    public AvaloniaDispatcherTimer(Threading.Dispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _avaloniaTimer = new Threading.DispatcherTimer(Threading.DispatcherPriority.Normal);
        _avaloniaTimer.Tick += OnTick;
    }

    public TimeSpan Interval
    {
        get => _avaloniaTimer.Interval;
        set => _avaloniaTimer.Interval = value;
    }

    public bool IsRepeating { get; set; } = true;

    public bool IsRunning => _avaloniaTimer.IsEnabled;

    public event EventHandler? Tick;

    public void Start()
    {
        _avaloniaTimer.Start();
    }

    public void Stop()
    {
        _avaloniaTimer.Stop();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        Tick?.Invoke(this, e);

        if (!IsRepeating)
        {
            Stop();
        }
    }
}
