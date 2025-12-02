using System;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Media = Avalonia.Media;

namespace Avalonia.Controls.Maui.Extensions;

internal static class ShadowExtensions
{
    public static IEffect? ToAvalonia(this IShadow? shadow)
    {
        if (shadow is null)
            return null;

        var color = shadow.Paint.ToAvaloniaColor(shadow.Opacity);
        var offset = shadow.Offset;

        return new DropShadowEffect
        {
            Color = color,
            OffsetX = offset.X,
            OffsetY = offset.Y,
            BlurRadius = shadow.Radius
        };
    }

    static Media.Color ToAvaloniaColor(this Paint? paint, float opacity)
    {
        var baseColor = paint is SolidPaint solid && solid.Color is not null
            ? solid.Color.ToAvaloniaColor()
            : Media.Colors.Black;

        var clampedOpacity = Math.Clamp(opacity, 0f, 1f);
        var alpha = (byte)(baseColor.A * clampedOpacity);
        return Media.Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
    }

    static Media.Color ToAvaloniaColor(this Microsoft.Maui.Graphics.Color color) =>
        Media.Color.FromArgb(
            (byte)(color.Alpha * 255),
            (byte)(color.Red * 255),
            (byte)(color.Green * 255),
            (byte)(color.Blue * 255));
}
