using Avalonia.Controls.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.MauiBorder;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods for updating <see cref="MauiBorder"/> properties from cross-platform
/// .NET MAUI border interfaces. These methods handle the platform-specific implementation details
/// for rendering borders on Avalonia-based platforms.
/// </summary>
/// <remarks>
/// This class is primarily used by <see cref="BorderHandler"/> to apply property changes from the
/// cross-platform virtual view to the native Avalonia control. Each method is designed to be
/// lightweight and perform only the necessary updates to maintain optimal performance during
/// property change notifications.
/// </remarks>
public static class BorderExtensions
{
    /// <summary>
    /// Updates the content of the border view.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform border view.</param>
    /// <param name="mauiContext">The MAUI context for converting views.</param>
    public static void UpdateContent(this PlatformView platformView, IBorderView border, IMauiContext? mauiContext)
    {
        if (mauiContext == null)
            return;

        platformView.Child = null;

        if (border.PresentedContent is IView view)
        {
            platformView.Child = (Control)view.ToPlatform(mauiContext);
        }
    }

    /// <summary>
    /// Updates the background brush of the border.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform border view.</param>
    public static void UpdateBackground(this PlatformView platformView, IBorderView border)
    {
        platformView.Background = border.Background?.ToPlatform() ?? Media.Brushes.Transparent;
    }

    /// <summary>
    /// Updates the stroke (border) brush of the border.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform border stroke.</param>
    public static void UpdateStroke(this PlatformView platformView, IBorderStroke border)
    {
        platformView.Stroke = border.Stroke?.ToPlatform();
    }

    /// <summary>
    /// Updates the stroke thickness of the border.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform border stroke.</param>
    public static void UpdateStrokeThickness(this PlatformView platformView, IBorderStroke border)
    {
        platformView.StrokeThickness = border.StrokeThickness;
    }

    /// <summary>
    /// Updates the stroke shape (geometry) of the border.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform border stroke.</param>
    /// <remarks>
    /// The shape can be any <see cref="Microsoft.Maui.Graphics.IShape"/> including:
    /// <list type="bullet">
    /// <item><description>Rectangle - A rectangular border</description></item>
    /// <item><description>RoundRectangle - A rectangle with rounded corners</description></item>
    /// <item><description>Ellipse - An elliptical border</description></item>
    /// <item><description>Polygon - A polygon-shaped border</description></item>
    /// <item><description>Custom shapes implementing IShape</description></item>
    /// </list>
    /// </remarks>
    public static void UpdateStrokeShape(this PlatformView platformView, IBorderStroke border)
    {
        platformView.Shape = border.Shape;
        platformView.InvalidateShape();
    }

    /// <summary>
    /// Updates the stroke dash pattern of the border.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform border stroke.</param>
    /// <remarks>
    /// The dash pattern is an array of float values that specify the lengths of alternating
    /// dashes and gaps. For example, [5, 2, 3, 2] creates a pattern with a 5-unit dash,
    /// 2-unit gap, 3-unit dash, and 2-unit gap.
    /// </remarks>
    public static void UpdateStrokeDashPattern(this PlatformView platformView, IBorderStroke border)
    {
        platformView.StrokeDashPattern = border.StrokeDashPattern;
    }

    /// <summary>
    /// Updates the stroke dash offset of the border.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform border stroke.</param>
    /// <remarks>
    /// The dash offset specifies the distance within the dash pattern where a dash begins.
    /// This can be used to animate dashed borders or offset the starting point of the pattern.
    /// </remarks>
    public static void UpdateStrokeDashOffset(this PlatformView platformView, IBorderStroke border)
    {
        platformView.StrokeDashOffset = border.StrokeDashOffset;
    }

    /// <summary>
    /// Updates the stroke line cap style of the border.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform border stroke.</param>
    /// <remarks>
    /// Line cap describes the shape at the start and end of a stroke line:
    /// <list type="bullet">
    /// <item><description>Butt - Flat cap that ends at the endpoint</description></item>
    /// <item><description>Round - Rounded cap that extends beyond the endpoint</description></item>
    /// <item><description>Square - Square cap that extends beyond the endpoint</description></item>
    /// </list>
    /// </remarks>
    public static void UpdateStrokeLineCap(this PlatformView platformView, IBorderStroke border)
    {
        platformView.StrokeLineCap = border.StrokeLineCap;
    }

    /// <summary>
    /// Updates the stroke line join style of the border.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform border stroke.</param>
    /// <remarks>
    /// Line join specifies the type of join used at the vertices of a stroke shape:
    /// <list type="bullet">
    /// <item><description>Miter - Pointed join (default)</description></item>
    /// <item><description>Round - Rounded join</description></item>
    /// <item><description>Bevel - Beveled (flat) join</description></item>
    /// </list>
    /// </remarks>
    public static void UpdateStrokeLineJoin(this PlatformView platformView, IBorderStroke border)
    {
        platformView.StrokeLineJoin = border.StrokeLineJoin;
    }

    /// <summary>
    /// Updates the stroke miter limit of the border.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform border stroke.</param>
    /// <remarks>
    /// The miter limit specifies the limit on the ratio of the miter length to half the stroke thickness.
    /// When the miter length exceeds this limit, a bevel join is used instead of a miter join.
    /// The default value is 10.0.
    /// </remarks>
    public static void UpdateStrokeMiterLimit(this PlatformView platformView, IBorderStroke border)
    {
        platformView.StrokeMiterLimit = border.StrokeMiterLimit;
    }

    /// <summary>
    /// Updates the padding of the border.
    /// </summary>
    /// <param name="platformView">The platform border view control.</param>
    /// <param name="border">The cross-platform padding provider.</param>
    /// <remarks>
    /// Padding represents the distance between the border and its child element.
    /// </remarks>
    public static void UpdatePadding(this PlatformView platformView, IPadding border)
    {
        var padding = border.Padding;
        platformView.Padding = new Thickness(
            padding.Left,
            padding.Top,
            padding.Right,
            padding.Bottom);
    }
}
