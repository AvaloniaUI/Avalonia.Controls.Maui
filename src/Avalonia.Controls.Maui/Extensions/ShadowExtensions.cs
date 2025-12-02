using System;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Media = Avalonia.Media;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for converting MAUI shadow definitions to Avalonia drop shadow effects.
/// </summary>
/// <remarks>
/// This class provides the conversion layer between MAUI's IShadow interface and
/// Avalonia's DropShadowEffect implementation. It handles color space conversion,
/// opacity blending, and property mapping.
/// </remarks>
internal static class ShadowExtensions
{
    /// <summary>
    /// Converts a MAUI IShadow to an Avalonia DropShadowEffect.
    /// </summary>
    /// <param name="shadow">The MAUI shadow definition to convert.</param>
    /// <returns>
    /// An Avalonia DropShadowEffect with mapped properties, or null if the input shadow is null.
    /// </returns>
    /// <remarks>
    /// <para><b>Property Mapping:</b></para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>MAUI Property</term>
    ///     <description>Avalonia Property</description>
    ///   </listheader>
    ///   <item>
    ///     <term>Shadow.Paint (SolidPaint)</term>
    ///     <description>DropShadowEffect.Color</description>
    ///   </item>
    ///   <item>
    ///     <term>Shadow.Opacity (0.0-1.0)</term>
    ///     <description>Combined with color alpha channel</description>
    ///   </item>
    ///   <item>
    ///     <term>Shadow.Offset (Point)</term>
    ///     <description>DropShadowEffect.OffsetX/OffsetY</description>
    ///   </item>
    ///   <item>
    ///     <term>Shadow.Radius (double)</term>
    ///     <description>DropShadowEffect.BlurRadius</description>
    ///   </item>
    /// </list>
    /// <para><b>Color Handling:</b></para>
    /// <list type="bullet">
    ///   <item>Only SolidPaint is supported; other paint types default to black</item>
    ///   <item>Opacity is multiplied with the paint's alpha channel</item>
    ///   <item>RGBA values are converted from 0.0-1.0 range to 0-255 byte range</item>
    /// </list>
    /// </remarks>
    public static IEffect? ToAvalonia(this IShadow? shadow)
    {
        if (shadow is null)
            return null;

        // Convert the paint color with applied opacity
        // This combines the shadow's opacity with the paint's inherent alpha
        var color = shadow.Paint.ToAvaloniaColor(shadow.Opacity);
        var offset = shadow.Offset;

        // Create Avalonia's drop shadow effect with mapped properties
        return new DropShadowEffect
        {
            Color = color,              // Shadow color with blended opacity
            OffsetX = offset.X,         // Horizontal shadow displacement
            OffsetY = offset.Y,         // Vertical shadow displacement
            BlurRadius = shadow.Radius  // Gaussian blur radius for soft edges
        };
    }

    /// <summary>
    /// Converts a MAUI Paint to an Avalonia Color with applied opacity.
    /// </summary>
    /// <param name="paint">The MAUI paint to convert (typically SolidPaint).</param>
    /// <param name="opacity">The opacity to apply (0.0-1.0 range).</param>
    /// <returns>
    /// An Avalonia Color with the paint's RGB values and opacity blended into the alpha channel.
    /// Returns black if the paint is not a SolidPaint or has no color.
    /// </returns>
    /// <remarks>
    /// <para><b>Opacity Blending:</b></para>
    /// The final alpha channel is calculated as:
    /// <c>FinalAlpha = BaseAlpha * Opacity</c>
    /// <para>This allows for two levels of transparency control:</para>
    /// <list type="number">
    ///   <item>The paint's inherent alpha (e.g., semi-transparent red)</item>
    ///   <item>The shadow's opacity setting (e.g., 50% visible shadow)</item>
    /// </list>
    /// <para><b>Fallback Behavior:</b></para>
    /// Non-solid paints (gradients, patterns) are not supported for shadows
    /// and will render as black. This matches typical platform behavior where
    /// shadows are solid colors only.
    /// </remarks>
    static Media.Color ToAvaloniaColor(this Paint? paint, float opacity)
    {
        // Extract base color from paint (only SolidPaint supported)
        // Gradient/pattern paints default to black
        var baseColor = paint is SolidPaint solid && solid.Color is not null
            ? solid.Color.ToAvaloniaColor()
            : Media.Colors.Black;

        // Clamp opacity to valid range and blend with base alpha
        var clampedOpacity = Math.Clamp(opacity, 0f, 1f);
        var alpha = (byte)(baseColor.A * clampedOpacity);

        // Return new color with blended alpha, preserving RGB channels
        return Media.Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
    }

    /// <summary>
    /// Converts a MAUI Graphics Color to an Avalonia Media Color.
    /// </summary>
    /// <param name="color">The MAUI color with 0.0-1.0 component values.</param>
    /// <returns>
    /// An Avalonia Color with 0-255 byte component values.
    /// </returns>
    /// <remarks>
    /// <para><b>Color Space Conversion:</b></para>
    /// MAUI uses floating-point RGBA (0.0-1.0 range) while Avalonia uses
    /// byte ARGB (0-255 range). This method performs the conversion by
    /// multiplying each component by 255 and casting to byte.
    /// <para><b>Component Order:</b></para>
    /// Note the different component order between platforms:
    /// <list type="bullet">
    ///   <item>MAUI: Red, Green, Blue, Alpha (RGBA)</item>
    ///   <item>Avalonia: Alpha, Red, Green, Blue (ARGB)</item>
    /// </list>
    /// </remarks>
    static Media.Color ToAvaloniaColor(this Microsoft.Maui.Graphics.Color color) =>
        Media.Color.FromArgb(
            (byte)(color.Alpha * 255),  // Convert alpha: 0.0-1.0 → 0-255
            (byte)(color.Red * 255),    // Convert red: 0.0-1.0 → 0-255
            (byte)(color.Green * 255),  // Convert green: 0.0-1.0 → 0-255
            (byte)(color.Blue * 255));  // Convert blue: 0.0-1.0 → 0-255
}
