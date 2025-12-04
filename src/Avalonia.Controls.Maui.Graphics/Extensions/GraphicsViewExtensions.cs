using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping IGraphicsView properties to PlatformTouchGraphicsView.
/// </summary>
public static class GraphicsViewExtensions
{
    /// <summary>
    /// Updates the drawable content of the graphics view.
    /// </summary>
    /// <param name="platformView">The platform graphics view control to update.</param>
    /// <param name="graphicsView">The .NET MAUI GraphicsView providing the drawable.</param>
    public static void UpdateDrawable(this PlatformTouchGraphicsView platformView, IGraphicsView graphicsView)
    {
        platformView.UpdateDrawable(graphicsView);
    }

    /// <summary>
    /// Updates the background of the graphics view.
    /// </summary>
    /// <param name="platformView">The platform graphics view control to update.</param>
    /// <param name="graphicsView">The .NET MAUI GraphicsView providing the background.</param>
    public static void UpdateBackground(this PlatformTouchGraphicsView platformView, IGraphicsView graphicsView)
    {
        if (graphicsView?.Background != null)
        {
            platformView.Background = graphicsView.Background.ToPlatform();
        }
    }

    /// <summary>
    /// Invalidates the graphics view to trigger a redraw.
    /// </summary>
    /// <param name="platformView">The platform graphics view control to invalidate.</param>
    public static void Invalidate(this PlatformTouchGraphicsView platformView)
    {
        platformView.InvalidateDrawable();
    }
}
