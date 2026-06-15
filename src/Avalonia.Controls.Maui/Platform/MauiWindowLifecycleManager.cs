using Avalonia.Controls;
using Microsoft.Maui;
using System;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Bridges an Avalonia <see cref="Window"/>'s lifecycle to the MAUI <see cref="IWindow"/> lifecycle.
/// </summary>
internal sealed class MauiWindowLifecycleManager : MauiWindowLifecycleDispatcher
{
    private readonly Window _platformView;
    private bool _isMinimized;

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiWindowLifecycleManager"/> class and subscribes
    /// to the lifecycle events of the supplied Avalonia window.
    /// </summary>
    /// <param name="virtualView">The MAUI window to forward lifecycle calls to.</param>
    /// <param name="platformView">The Avalonia window whose lifecycle drives the MAUI window.</param>
    public MauiWindowLifecycleManager(IWindow virtualView, Window platformView)
        : base(virtualView)
    {
        _platformView = platformView;
        _isMinimized = platformView.WindowState == WindowState.Minimized;

        platformView.Opened += OnOpened;
        platformView.Activated += OnActivated;
        platformView.Deactivated += OnDeactivated;
        platformView.Closed += OnClosed;
        platformView.PropertyChanged += OnPlatformPropertyChanged;

        // The handler may connect after the window is already shown; treat that as creation.
        if (platformView.IsVisible)
        {
            HandleShown();
        }
    }

    private void OnOpened(object? sender, EventArgs e) => HandleShown();

    private void HandleShown()
    {
        var firstCreate = !IsCreated;
        NotifyCreated();

        // A window that opens while already minimized must still go through Stopped so that a later
        // restore produces a matching Resumed (MAUI raises Resumed/Stopped without ordering guards,
        // but emitting Resumed with no preceding Stopped would be an asymmetric lifecycle). Gate on
        // the first creation so the Stopped is not re-sent if HandleShown runs again.
        if (firstCreate && _isMinimized)
        {
            NotifyStopped();
        }
    }

    private void OnActivated(object? sender, EventArgs e) => NotifyActivated();

    private void OnDeactivated(object? sender, EventArgs e) => NotifyDeactivated();

    private void OnClosed(object? sender, EventArgs e) => NotifyDestroying();

    private void OnPlatformPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Window.WindowStateProperty)
        {
            return;
        }

        var minimized = e.GetNewValue<WindowState>() == WindowState.Minimized;
        if (minimized == _isMinimized)
        {
            return;
        }

        _isMinimized = minimized;
        if (minimized)
        {
            NotifyStopped();
        }
        else
        {
            NotifyResumed();
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        _platformView.Opened -= OnOpened;
        _platformView.Activated -= OnActivated;
        _platformView.Deactivated -= OnDeactivated;
        _platformView.Closed -= OnClosed;
        _platformView.PropertyChanged -= OnPlatformPropertyChanged;
    }
}
