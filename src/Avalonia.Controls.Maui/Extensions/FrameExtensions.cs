using Avalonia.Controls.Primitives;
using Microsoft.Maui;
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
    /// <param name="view">The IView (Frame) providing the border color via reflection.</param>
    public static void UpdateBorderColor(this PlatformView platformView, IView view)
    {
        if (platformView == null || view == null)
            return;

        // Get BorderColor property via reflection since Frame doesn't have a specific interface for it
        var borderColorProperty = view.GetType().GetProperty("BorderColor");
        if (borderColorProperty != null)
        {
            var borderColor = borderColorProperty.GetValue(view);
            if (borderColor is Color color && color != null)
            {
                var avaloniaColor = Media.Color.FromArgb(
                    (byte)(color.Alpha * 255),
                    (byte)(color.Red * 255),
                    (byte)(color.Green * 255),
                    (byte)(color.Blue * 255));

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
    }

    /// <summary>
    /// Updates the corner radius of the Border based on the Frame's CornerRadius property.
    /// </summary>
    /// <param name="platformView">The Border control to update.</param>
    /// <param name="view">The IContentView (Frame) providing the corner radius via reflection.</param>
    public static void UpdateCornerRadius(this PlatformView platformView, IView view)
    {
        if (platformView == null || view == null)
            return;

        // Get CornerRadius property via reflection since Frame doesn't have an interface for it
        var cornerRadiusProperty = view.GetType().GetProperty("CornerRadius");
        if (cornerRadiusProperty != null)
        {
            var cornerRadius = cornerRadiusProperty.GetValue(view);
            if (cornerRadius is float floatValue)
            {
                platformView.CornerRadius = new CornerRadius(floatValue);
            }
        }
    }

    /// <summary>
    /// Updates the shadow effect on the Border based on the Frame's HasShadow property.
    /// Creates a visual shadow by adding a semi-transparent background layer behind the border.
    /// Note: This implementation uses BoxShadow which only affects the border, not the content.
    /// </summary>
    /// <param name="platformView">The Border control to update.</param>
    /// <param name="view">The IContentView (Frame) providing the shadow setting via reflection.</param>
    public static void UpdateHasShadow(this PlatformView platformView, IView view)
    {
        if (platformView == null || view == null)
            return;

        // Get HasShadow property via reflection since Frame doesn't have an interface for it
        var hasShadowProperty = view.GetType().GetProperty("HasShadow");
        if (hasShadowProperty != null)
        {
            var hasShadow = hasShadowProperty.GetValue(view);
            if (hasShadow is bool boolValue)
            {
                if (boolValue)
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
        }
    }

    /// <summary>
    /// Updates the background of the Border based on the Frame's Background property.
    /// </summary>
    /// <param name="platformView">The Border control to update.</param>
    /// <param name="view">The IView providing the background paint.</param>
    public static void UpdateBackground(this PlatformView platformView, IView view)
    {
        if (platformView == null || view == null)
            return;

        if (view.Background != null)
        {
            platformView.Background = view.Background.ToPlatform();
        }
        else
        {
            platformView.ClearValue(TemplatedControl.BackgroundProperty);
        }
    }

    /// <summary>
    /// Updates the padding of the Border based on the Frame's Padding property.
    /// </summary>
    /// <param name="platformView">The Border control to update.</param>
    /// <param name="view">The IView providing the padding.</param>
    public static void UpdatePadding(this PlatformView platformView, IView view)
    {
        if (platformView == null || view == null)
            return;

        // Get Padding property via reflection
        var paddingProperty = view.GetType().GetProperty("Padding");
        if (paddingProperty != null)
        {
            var padding = paddingProperty.GetValue(view);
            if (padding is Microsoft.Maui.Thickness mauiPadding)
            {
                platformView.Padding = new Thickness(
                    mauiPadding.Left,
                    mauiPadding.Top,
                    mauiPadding.Right,
                    mauiPadding.Bottom);
            }
        }
    }
}
