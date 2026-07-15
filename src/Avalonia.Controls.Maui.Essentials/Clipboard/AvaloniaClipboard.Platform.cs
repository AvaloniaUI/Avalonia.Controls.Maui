using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Platform implementation using Avalonia's <c>TopLevel.Clipboard</c> API.
/// On desktop, monitors <c>Window.Activated</c> to detect external clipboard changes.
/// </summary>
public partial class AvaloniaClipboard
{
    Window? _subscribedWindow;

    private partial async Task<string?> PlatformGetTextAsync()
    {
        var clipboard = _platformProvider.GetTopLevel()?.Clipboard;
        if (clipboard is null)
            return null;

        if (!Dispatcher.UIThread.CheckAccess())
            return await Dispatcher.UIThread.InvokeAsync(async () => await clipboard.TryGetTextAsync());

        return await clipboard.TryGetTextAsync();
    }

    private partial async Task<bool> PlatformSetTextAsync(string? text)
    {
        var clipboard = _platformProvider.GetTopLevel()?.Clipboard;
        if (clipboard is null)
            return false;

        if (!Dispatcher.UIThread.CheckAccess())
            await Dispatcher.UIThread.InvokeAsync(async () => await clipboard.SetTextAsync(text));
        else
            await clipboard.SetTextAsync(text);

        return true;
    }

    partial void EnsureSubscribed()
    {
        if (_subscribedWindow is not null)
            return;

        if (_platformProvider.GetTopLevel() is Window window)
        {
            window.Activated += OnWindowActivated;
            window.Closed += OnWindowClosed;
            _subscribedWindow = window;
        }
    }

    void Unsubscribe()
    {
        if (_subscribedWindow is null)
            return;

        _subscribedWindow.Activated -= OnWindowActivated;
        _subscribedWindow.Closed -= OnWindowClosed;
        _subscribedWindow = null;
    }

    void OnWindowClosed(object? sender, EventArgs e)
    {
        Unsubscribe();
    }

    async void OnWindowActivated(object? sender, EventArgs e)
    {
        try
        {
            var clipboard = _subscribedWindow?.Clipboard;
            if (clipboard is null)
                return;

            var currentText = await clipboard.TryGetTextAsync();
            var changed = currentText != _lastKnownText;
            UpdateState(currentText);

            if (changed)
                _clipboardContentChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
        }
    }
}
