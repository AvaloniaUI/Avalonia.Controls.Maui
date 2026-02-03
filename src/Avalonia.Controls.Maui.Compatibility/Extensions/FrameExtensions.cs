using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Microsoft.Maui;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Provides extension methods for updating Avalonia <see cref="Border"/> properties from cross-platform
/// .NET MAUI Frame interfaces. These methods handle the platform-specific implementation details
/// for rendering the legacy Frame control on Avalonia-based platforms.
/// </summary>
/// <remarks>
/// Frame is marked as obsolete in .NET MAUI 9 and will be removed in a future release.
/// Consider using Border instead for new applications. Frame maps directly to Avalonia Border.
/// </remarks>
public static class FrameExtensions
{
    /// <summary>
    /// Updates the content of the frame.
    /// </summary>
    /// <param name="platformView">The platform border control.</param>
    /// <param name="frame">The cross-platform frame view.</param>
    /// <param name="mauiContext">The MAUI context for converting views.</param>
    public static void UpdateContent(this Border platformView, Microsoft.Maui.Controls.Frame frame, IMauiContext? mauiContext)
    {
        if (mauiContext == null)
            return;

        platformView.Child = null;

        if (frame.Content is IView view)
        {
            platformView.Child = (Control)view.ToPlatform(mauiContext);
        }
    }

    /// <summary>
    /// Updates the border color of the frame.
    /// </summary>
    /// <param name="platformView">The platform border control.</param>
    /// <param name="frame">The cross-platform frame.</param>
    /// <remarks>
    /// In Frame, BorderColor is a simple Color value that sets the stroke around the frame.
    /// This is equivalent to the BorderBrush property in Avalonia Border.
    /// </remarks>
    public static void UpdateBorderColor(this Border platformView, Microsoft.Maui.Controls.Frame frame)
    {
        var borderColor = frame.BorderColor;
        
        if (borderColor == null || borderColor == Microsoft.Maui.Graphics.Colors.Transparent)
        {
            platformView.BorderBrush = null;
            platformView.BorderThickness = new Thickness(0);
        }
        else
        {
            platformView.BorderBrush = new ImmutableSolidColorBrush(Color.FromArgb(
                (byte)(borderColor.Alpha * 255),
                (byte)(borderColor.Red * 255),
                (byte)(borderColor.Green * 255),
                (byte)(borderColor.Blue * 255)));
            
            if (platformView.BorderThickness == default || platformView.BorderThickness == new Thickness(0))
            {
                platformView.BorderThickness = new Thickness(1);
            }
        }
    }

    /// <summary>
    /// Updates the corner radius of the frame.
    /// </summary>
    /// <param name="platformView">The platform border control.</param>
    /// <param name="frame">The cross-platform frame.</param>
    /// <remarks>
    /// Frame uses a single float value for CornerRadius which is applied uniformly to all corners.
    /// </remarks>
    public static void UpdateCornerRadius(this Border platformView, Microsoft.Maui.Controls.Frame frame)
    {
        var radius = frame.CornerRadius;
        platformView.CornerRadius = new CornerRadius(Math.Max(0, radius));
    }

    /// <summary>
    /// Updates the shadow effect of the frame.
    /// </summary>
    /// <param name="platformView">The platform border control.</param>
    /// <param name="frame">The cross-platform frame.</param>
    /// <remarks>
    /// The HasShadow property applies a subtle drop shadow effect to the frame.
    /// This is a simplified shadow compared to the full Shadow property available in .NET MAUI.
    /// </remarks>
    public static void UpdateHasShadow(this Border platformView, Microsoft.Maui.Controls.Frame frame)
    {
        if (frame.HasShadow)
        {
            platformView.BoxShadow = new BoxShadows(new BoxShadow
            {
                OffsetX = 2,
                OffsetY = 2,
                Blur = 6,
                Color = Color.FromArgb(80, 0, 0, 0)
            });
        }
        else
        {
            platformView.BoxShadow = default;
        }
    }

    /// <summary>
    /// Updates the background brush of the frame.
    /// </summary>
    /// <param name="platformView">The platform border control.</param>
    /// <param name="frame">The cross-platform frame.</param>
    public static void UpdateBackground(this Border platformView, Microsoft.Maui.Controls.Frame frame)
    {
        // Check BackgroundColor first (more common for Frame usage)
        if (frame.BackgroundColor != null && frame.BackgroundColor != Microsoft.Maui.Graphics.Colors.Transparent)
        {
            platformView.Background = new ImmutableSolidColorBrush(Color.FromArgb(
                (byte)(frame.BackgroundColor.Alpha * 255),
                (byte)(frame.BackgroundColor.Red * 255),
                (byte)(frame.BackgroundColor.Green * 255),
                (byte)(frame.BackgroundColor.Blue * 255)));
        }
        else if (frame.Background != null && !frame.Background.IsEmpty)
        {
            platformView.Background = frame.Background.ToPlatform();
        }
        else
        {
            // Default Frame background is white
            platformView.Background = Brushes.White;
        }
    }

    /// <summary>
    /// Updates the padding of the frame.
    /// </summary>
    /// <param name="platformView">The platform border control.</param>
    /// <param name="frame">The cross-platform frame.</param>
    /// <remarks>
    /// Padding represents the space between the frame border and its content.
    /// The default Frame padding in MAUI is (20, 20, 20, 20).
    /// </remarks>
    public static void UpdatePadding(this Border platformView, Microsoft.Maui.Controls.Frame frame)
    {
        var padding = frame.Padding;
        platformView.Padding = new Thickness(
            padding.Left,
            padding.Top,
            padding.Right,
            padding.Bottom);
    }

    /// <summary>
    /// Updates the clipping behavior of the frame content.
    /// </summary>
    /// <param name="platformView">The platform border control.</param>
    /// <param name="frame">The cross-platform frame.</param>
    /// <remarks>
    /// When IsClippedToBounds is true, content that extends beyond the frame's bounds
    /// will be clipped. This is useful for creating circular images or other clipped content.
    /// </remarks>
    public static void UpdateIsClippedToBounds(this Border platformView, Microsoft.Maui.Controls.Frame frame)
    {
        platformView.ClipToBounds = frame.IsClippedToBounds;
    }
}
