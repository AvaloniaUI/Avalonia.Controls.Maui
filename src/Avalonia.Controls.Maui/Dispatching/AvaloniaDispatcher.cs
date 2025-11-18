using System;
using Microsoft.Maui.Dispatching;

namespace Avalonia.Controls.Maui.Dispatching;

/// <summary>
/// Avalonia implementation of MAUI's IDispatcher
/// </summary>
public class AvaloniaDispatcher : IDispatcher
{
    private readonly Threading.Dispatcher _avaloniaDispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaDispatcher"/> class.
    /// </summary>
    /// <param name="avaloniaDispatcher">The Avalonia dispatcher instance to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="avaloniaDispatcher"/> is null.</exception>
    public AvaloniaDispatcher(Threading.Dispatcher avaloniaDispatcher)
    {
        _avaloniaDispatcher = avaloniaDispatcher ?? throw new ArgumentNullException(nameof(avaloniaDispatcher));
    }

    /// <summary>
    /// Gets a value indicating whether a dispatch is required to execute code on the UI thread.
    /// </summary>
    public bool IsDispatchRequired => !_avaloniaDispatcher.CheckAccess();

    /// <summary>
    /// Dispatches the specified action to be executed on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>Always returns true.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public bool Dispatch(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        _avaloniaDispatcher.Post(action);
        return true;
    }

    /// <summary>
    /// Dispatches the specified action to be executed on the UI thread after the specified delay.
    /// </summary>
    /// <param name="delay">The time delay before executing the action.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>Always returns true.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
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

    /// <summary>
    /// Creates a new dispatcher timer.
    /// </summary>
    /// <returns>A new instance of <see cref="IDispatcherTimer"/>.</returns>
    public IDispatcherTimer CreateTimer()
    {
        return new AvaloniaDispatcherTimer(_avaloniaDispatcher);
    }
}