using Avalonia.Controls.Primitives;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PlatformView = Avalonia.Controls.Border;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping Frame properties to Avalonia Border control.
/// </summary>
/// <remarks>
/// Frame is a legacy .NET MAUI control (obsolete in MAUI 9+) that wraps content with a border and shadow.
/// It's implemented using Avalonia's Border control with drop shadow effects.
/// Frame implements IBorderElement for border color property.
/// </remarks>
public static class FrameExtensions
{
    /// <summary>
    /// Updates the border color of the Border based on the Frame's BorderColor property.
    /// </summary>
    /// <param name="platformView">The Border control to update.</param>
    /// <param name="frame">The Frame providing the border color.</param>
    public static void UpdateBorderColor(this PlatformView platformView, Frame frame)
    {
        if (platformView == null || frame == null)
            return;

        if (frame.IsSet(Frame.BorderColorProperty) &&
            frame.BorderColor is Color borderColor)
        {
            var avaloniaColor = Media.Color.FromArgb(
                (byte)(borderColor.Alpha * 255),
                (byte)(borderColor.Red * 255),
                (byte)(borderColor.Green * 255),
                (byte)(borderColor.Blue * 255));

            platformView.BorderBrush = new Media.SolidColorBrush(avaloniaColor);

            // Set a default border thickness if not already set
            if (platformView.BorderThickness == default)
            {
                platformView.BorderThickness = new Thickness(1);
            }
        }
        else
        {
            // Clear border when color is null
            platformView.BorderBrush = null;
            platformView.BorderThickness = new Thickness(0);
        }
    }

    /// <summary>
    /// Updates the corner radius of the Border based on the Frame's CornerRadius property.
    /// </summary>
    /// <param name="platformView">The Border control to update.</param>
    /// <param name="frame">The Frame providing the corner radius.</param>
    public static void UpdateCornerRadius(this PlatformView platformView, Frame frame)
    {
        if (platformView == null || frame == null)
            return;

        platformView.CornerRadius = new CornerRadius(frame.CornerRadius);
    }

    /// <summary>
    /// Updates the shadow effect on the Border based on the Frame's HasShadow property.
    /// Creates a visual shadow by adding a semi-transparent background layer behind the border.
    /// Note: This implementation uses BoxShadow which only affects the border, not the content.
    /// </summary>
    /// <param name="platformView">The Border control to update.</param>
    /// <param name="frame">The Frame providing the shadow setting.</param>
    public static void UpdateHasShadow(this PlatformView platformView, Frame frame)
    {
        if (platformView == null || frame == null)
            return;

        if (frame.HasShadow)
        {
            // Use BoxShadows property which applies shadow only to the border, not content
            var boxShadow = new Media.BoxShadow
            {
                Color = Media.Color.FromArgb(76, 0, 0, 0), // 30% opacity black
                OffsetX = 0,
                OffsetY = 2,
                Blur = 6,
                Spread = 0
            };

            platformView.BoxShadow = new Media.BoxShadows(boxShadow);
        }
        else
        {
            // Remove shadow
            platformView.BoxShadow = new Media.BoxShadows();
        }
    }

    /// <summary>
    /// Updates the background of the Border based on the Frame's Background or BackgroundColor properties.
    /// </summary>
    /// <param name="platformView">The Border control to update.</param>
    /// <param name="frame">The Frame providing the background paint.</param>
    public static void UpdateBackground(this PlatformView platformView, Frame frame)
    {
        if (platformView == null || frame == null)
            return;

        var backgroundBrush = frame.Background?.ToPlatform();

        var backgroundColor = frame.BackgroundColor;

        if (backgroundBrush == null && backgroundColor != null)
        {
            backgroundBrush = backgroundColor.ToPlatform();
        }

        if (backgroundBrush != null)
            platformView.Background = backgroundBrush;
        else
            platformView.ClearValue(TemplatedControl.BackgroundProperty);
    }

    /// <summary>
    /// Updates the padding of the Border based on the Frame's Padding property.
    /// </summary>
    /// <param name="platformView">The Border control to update.</param>
    /// <param name="frame">The Frame providing the padding.</param>
    public static void UpdatePadding(this PlatformView platformView, Frame frame)
    {
        if (platformView == null || frame == null)
            return;

        var padding = frame.Padding;
        platformView.Padding = new Thickness(
            padding.Left,
            padding.Top,
            padding.Right,
            padding.Bottom);
    }
}
