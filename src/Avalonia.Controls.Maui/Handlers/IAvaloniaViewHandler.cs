using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// A view handler whose platform views are Avalonia controls. Exposes strongly-typed access
/// to the Avalonia platform and container views along with container requirements.
/// </summary>
public interface IAvaloniaViewHandler : IViewHandler
{
    /// <summary>
    /// Gets the Avalonia platform view associated with this handler, or <see langword="null"/> if not created yet.
    /// </summary>
    new PlatformView? PlatformView { get; }

    /// <summary>
    /// Gets the view that acts as a container for the platform view, or <see langword="null"/> if no container is set up.
    /// </summary>
    new PlatformView? ContainerView { get; }

    /// <summary>
    /// Gets a value that indicates whether or not the virtual view needs a container view.
    /// </summary>
    bool NeedsContainer { get; }
}
