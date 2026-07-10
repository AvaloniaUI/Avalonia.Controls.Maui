using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Bridges a single-view (browser/embedded) Avalonia <see cref="Control"/>'s lifecycle to the MAUI
/// <see cref="IWindow"/> lifecycle.
/// </summary>
/// <remarks>
/// Single-view lifetimes have no Avalonia <see cref="Window"/>, so there is no open/close or minimize/restore
/// signal. Instead the root content's <see cref="Control.Loaded"/>/<see cref="Control.Unloaded"/> events are used:
/// loading the view maps to <see cref="IWindow.Created"/> (which starts the MAUI application) and unloading maps to
/// <see cref="IWindow.Destroying"/>. Resume/sleep transitions (for example browser tab visibility) are not wired,
/// State tracking and ordering live in <see cref="MauiWindowLifecycleDispatcher"/>.
/// </remarks>
internal sealed class MauiSingleViewLifecycleManager : MauiWindowLifecycleDispatcher
{
    private readonly Control _platformView;

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiSingleViewLifecycleManager"/> class and subscribes
    /// to the lifecycle events of the supplied single-view content control.
    /// </summary>
    /// <param name="virtualView">The MAUI window to forward lifecycle calls to.</param>
    /// <param name="platformView">The single-view content control whose lifecycle drives the MAUI window.</param>
    public MauiSingleViewLifecycleManager(IWindow virtualView, Control platformView)
        : base(virtualView)
    {
        _platformView = platformView;

        platformView.Loaded += OnLoaded;
        platformView.Unloaded += OnUnloaded;

        // The handler may connect after the content is already loaded; treat that as creation.
        if (platformView.IsLoaded)
        {
            NotifyCreated();
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) => NotifyCreated();

    private void OnUnloaded(object? sender, RoutedEventArgs e) => NotifyDestroying();

    /// <inheritdoc/>
    public override void Dispose()
    {
        _platformView.Loaded -= OnLoaded;
        _platformView.Unloaded -= OnUnloaded;
    }
}
