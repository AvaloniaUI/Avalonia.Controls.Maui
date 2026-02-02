using Microsoft.Maui.Dispatching;

namespace Avalonia.Controls.Maui.Dispatching;

/// <summary>
/// Avalonia implementation of MAUI's IDispatcherTimer
/// </summary>
public class AvaloniaDispatcherTimer : IDispatcherTimer
{
    private readonly Threading.DispatcherTimer _avaloniaTimer;
    private readonly Threading.Dispatcher _dispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaDispatcherTimer"/> class.
    /// </summary>
    /// <param name="dispatcher">The Avalonia dispatcher instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dispatcher"/> is null.</exception>
    public AvaloniaDispatcherTimer(Threading.Dispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _avaloniaTimer = new Threading.DispatcherTimer(Threading.DispatcherPriority.Normal);
        _avaloniaTimer.Tick += OnTick;
    }

    /// <summary>
    /// Gets or sets the interval between timer ticks.
    /// </summary>
    public TimeSpan Interval
    {
        get => _avaloniaTimer.Interval;
        set => _avaloniaTimer.Interval = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the timer repeats after each tick.
    /// </summary>
    public bool IsRepeating { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether the timer is currently running.
    /// </summary>
    public bool IsRunning => _avaloniaTimer.IsEnabled;

    /// <summary>
    /// Occurs when the timer interval has elapsed.
    /// </summary>
    public event EventHandler? Tick;

    /// <summary>
    /// Starts the timer.
    /// </summary>
    public void Start()
    {
        _avaloniaTimer.Start();
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
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
