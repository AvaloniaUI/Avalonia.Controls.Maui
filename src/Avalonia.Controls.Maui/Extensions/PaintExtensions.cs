#nullable disable
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Provides extension methods for converting .NET MAUI <see cref="Paint"/> types to Avalonia brush types.
/// </summary>
public static class PaintExtensions
{
    /// <summary>
    /// Converts a .NET MAUI <see cref="Paint"/> to an Avalonia <see cref="global::Avalonia.Media.IBrush"/>, supporting
    /// <see cref="SolidPaint"/>, <see cref="LinearGradientPaint"/>, and <see cref="RadialGradientPaint"/>.
    /// </summary>
    /// <param name="paint">The .NET MAUI paint to convert.</param>
    /// <returns>An Avalonia brush representing the paint, or <c>null</c> if the paint type is not supported or the input is <c>null</c>.</returns>
    public static global::Avalonia.Media.IBrush ToAvaloniaBrush(this Paint paint)
    {
        if (paint is null)
            return null;

        if (paint is SolidPaint solidPaint)
        {
            return new global::Avalonia.Media.SolidColorBrush(
                global::Avalonia.Media.Color.FromArgb(
                    (byte)(solidPaint.Color.Alpha * 255),
                    (byte)(solidPaint.Color.Red * 255),
                    (byte)(solidPaint.Color.Green * 255),
                    (byte)(solidPaint.Color.Blue * 255)));
        }

        if (paint is LinearGradientPaint linearGradient)
        {
            var brush = new global::Avalonia.Media.LinearGradientBrush
            {
                StartPoint = new global::Avalonia.RelativePoint(linearGradient.StartPoint.X, linearGradient.StartPoint.Y, global::Avalonia.RelativeUnit.Relative),
                EndPoint = new global::Avalonia.RelativePoint(linearGradient.EndPoint.X, linearGradient.EndPoint.Y, global::Avalonia.RelativeUnit.Relative)
            };

            if (linearGradient.GradientStops != null)
            {
                foreach (var stop in linearGradient.GradientStops)
                {
                    brush.GradientStops.Add(new global::Avalonia.Media.GradientStop(
                        global::Avalonia.Media.Color.FromArgb(
                            (byte)(stop.Color.Alpha * 255),
                            (byte)(stop.Color.Red * 255),
                            (byte)(stop.Color.Green * 255),
                            (byte)(stop.Color.Blue * 255)),
                        (float)stop.Offset));
                }
            }

            return brush;
        }

        if (paint is RadialGradientPaint radialGradient)
        {
            var brush = new global::Avalonia.Media.RadialGradientBrush
            {
                Center = new global::Avalonia.RelativePoint(radialGradient.Center.X, radialGradient.Center.Y, global::Avalonia.RelativeUnit.Relative),
                GradientOrigin = new global::Avalonia.RelativePoint(radialGradient.Center.X, radialGradient.Center.Y, global::Avalonia.RelativeUnit.Relative),
                RadiusX = new global::Avalonia.RelativeScalar(radialGradient.Radius, global::Avalonia.RelativeUnit.Relative),
                RadiusY = new global::Avalonia.RelativeScalar(radialGradient.Radius, global::Avalonia.RelativeUnit.Relative)
            };

            if (radialGradient.GradientStops != null)
            {
                foreach (var stop in radialGradient.GradientStops)
                {
                    brush.GradientStops.Add(new global::Avalonia.Media.GradientStop(
                        global::Avalonia.Media.Color.FromArgb(
                            (byte)(stop.Color.Alpha * 255),
                            (byte)(stop.Color.Red * 255),
                            (byte)(stop.Color.Green * 255),
                            (byte)(stop.Color.Blue * 255)),
                        (float)stop.Offset));
                }
            }

            return brush;
        }

        // Default fallback
        return null;
    }
}