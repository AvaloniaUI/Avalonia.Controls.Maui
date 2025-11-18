using Microsoft.Maui.Dispatching;

namespace Avalonia.Controls.Maui.Dispatching;

/// <summary>
/// Avalonia implementation of MAUI's IDispatcherProvider
/// </summary>
public class AvaloniaDispatcherProvider : IDispatcherProvider
{
    [ThreadStatic]
    private static IDispatcher? _dispatcher;

    /// <summary>
    /// Gets the dispatcher for the current thread.
    /// </summary>
    /// <returns>The dispatcher for the current thread, or null if none is available.</returns>
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
