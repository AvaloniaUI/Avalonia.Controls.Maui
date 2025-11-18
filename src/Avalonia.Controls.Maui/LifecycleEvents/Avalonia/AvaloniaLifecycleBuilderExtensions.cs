using Microsoft.Maui.LifecycleEvents;

namespace Avalonia.Controls.Maui.LifecycleEvents;

/// <summary>
/// Provides extension methods for registering Avalonia lifecycle events with the <see cref="IAvaloniaLifecycleBuilder"/>.
/// </summary>
public static class AvaloniaLifecycleBuilderExtensions
{
    /// <summary>
    /// Registers a handler for the Activated event.
    /// </summary>
    public static IAvaloniaLifecycleBuilder OnActivated(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnActivated del) => lifecycle.OnEvent(del);

    /// <summary>
    /// Registers a handler for the Closed event.
    /// </summary>
    public static IAvaloniaLifecycleBuilder OnClosed(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnClosed del) => lifecycle.OnEvent(del);

    /// <summary>
    /// Registers a handler for the Launching event.
    /// </summary>
    public static IAvaloniaLifecycleBuilder OnLaunching(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnLaunching del) => lifecycle.OnEvent(del);

    /// <summary>
    /// Registers a handler for the Launched event.
    /// </summary>
    public static IAvaloniaLifecycleBuilder OnLaunched(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnLaunched del) => lifecycle.OnEvent(del);

    /// <summary>
    /// Registers a handler for the VisibilityChanged event.
    /// </summary>
    public static IAvaloniaLifecycleBuilder OnVisibilityChanged(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnVisibilityChanged del) => lifecycle.OnEvent(del);

    /// <summary>
    /// Registers a handler for the WindowCreated event.
    /// </summary>
    public static IAvaloniaLifecycleBuilder OnWindowCreated(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnWindowCreated del) => lifecycle.OnEvent(del);

    /// <summary>
    /// Registers a handler for the Resumed event.
    /// </summary>
    public static IAvaloniaLifecycleBuilder OnResumed(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnResumed del) => lifecycle.OnEvent(del);

    /// <summary>
    /// Registers a handler for the PlatformMessage event.
    /// </summary>
    public static IAvaloniaLifecycleBuilder OnPlatformMessage(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnPlatformMessage del) => lifecycle.OnEvent(del);

    /// <summary>
    /// Registers a handler for the MauiContextCreated event (internal use).
    /// </summary>
    internal static IAvaloniaLifecycleBuilder OnMauiContextCreated(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnMauiContextCreated del) => lifecycle.OnEvent(del);
}