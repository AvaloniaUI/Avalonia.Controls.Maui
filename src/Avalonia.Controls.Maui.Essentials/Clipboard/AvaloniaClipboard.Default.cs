using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Desktop-specific clipboard implementation using Avalonia's <c>TopLevel.Clipboard</c> API.
/// Monitors <c>Window.Activated</c> to detect external clipboard changes.
/// </summary>
public partial class AvaloniaClipboard
{
    bool _isSubscribed;

    private partial async Task<string?> PlatformGetTextAsync()
    {
        var clipboard = GetClipboard();
        if (clipboard is null)
            return null;

        if (!Dispatcher.UIThread.CheckAccess())
            return await Dispatcher.UIThread.InvokeAsync(async () => await clipboard.TryGetTextAsync());

        return await clipboard.TryGetTextAsync();
    }

    private partial async Task<bool> PlatformSetTextAsync(string? text)
    {
        var clipboard = GetClipboard();
        if (clipboard is null)
            return false;

        if (!Dispatcher.UIThread.CheckAccess())
            await Dispatcher.UIThread.InvokeAsync(async () => await clipboard.SetTextAsync(text));
        else
            await clipboard.SetTextAsync(text);

        return true;
    }

    IClipboard? GetClipboard()
    {
        EnsureSubscribedToWindowActivation();
        return _platformProvider.GetTopLevel()?.Clipboard;
    }

    void EnsureSubscribedToWindowActivation()
    {
        if (_isSubscribed)
            return;

        if (_platformProvider.GetTopLevel() is Window window)
        {
            window.Activated += OnWindowActivated;
            _isSubscribed = true;
        }
    }

    async void OnWindowActivated(object? sender, EventArgs e)
    {
        try
        {
            var clipboard = _platformProvider.GetTopLevel()?.Clipboard;
            if (clipboard is null)
                return;

            var currentText = await clipboard.TryGetTextAsync();
            var changed = currentText != _lastKnownText;
            UpdateState(currentText);

            if (changed)
                ClipboardContentChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // Clipboard access can fail on some platforms.
        }
    }
}
