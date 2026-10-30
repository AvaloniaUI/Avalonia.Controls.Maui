using Microsoft.Maui.Dispatching;

namespace Avalonia.Controls.Maui.Dispatching;

/// <summary>
/// Avalonia implementation of MAUI's IDispatcherProvider
/// </summary>
public class AvaloniaDispatcherProvider : IDispatcherProvider
{
    [ThreadStatic]
    private static AvaloniaDispatcher? _dispatcher;

    /// <summary>
    /// Gets the dispatcher for the current thread.
    /// </summary>
    /// <returns>The dispatcher for the current thread, or null if none is available.</returns>
    public IDispatcher? GetForCurrentThread()
    {
        // Avalonia can replace its UI thread dispatcher, as the headless test platform does between
        // tests. Work posted to the old one never runs, so wrap the current one.
        var avaloniaDispatcher = Threading.Dispatcher.UIThread;
        if (_dispatcher?.AvaloniaDispatcherInstance != avaloniaDispatcher)
            _dispatcher = new AvaloniaDispatcher(avaloniaDispatcher);

        return _dispatcher;
    }
}
