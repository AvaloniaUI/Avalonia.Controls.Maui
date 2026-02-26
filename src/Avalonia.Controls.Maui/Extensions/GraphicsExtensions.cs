using AvaloniaSolidColorBrush = Avalonia.Media.SolidColorBrush;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaIBrush = Avalonia.Media.IBrush;
using AvaloniaLinearGradientBrush = Avalonia.Media.LinearGradientBrush;
using AvaloniaGradientStop = Avalonia.Media.GradientStop;
using AvaloniaRadialGradientBrush = Avalonia.Media.RadialGradientBrush;
using MauiGradientPaint = Microsoft.Maui.Graphics.GradientPaint;
using MauiSolidPaint = Microsoft.Maui.Graphics.SolidPaint;
using MauiPaint = Microsoft.Maui.Graphics.Paint;
using MauiColor = Microsoft.Maui.Graphics.Color;
using MauiLinearGradientPaint = Microsoft.Maui.Graphics.LinearGradientPaint;
using MauiRadialGradientPaint = Microsoft.Maui.Graphics.RadialGradientPaint;
using AvaloniaPenLineCap = Avalonia.Media.PenLineCap;
using AvaloniaPenLineJoin = Avalonia.Media.PenLineJoin;
using MauiLineCap = Microsoft.Maui.Graphics.LineCap;
using MauiLineJoin = Microsoft.Maui.Graphics.LineJoin;
using MauiControlsBrush = Microsoft.Maui.Controls.Brush;
using MauiControlsSolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
using MauiControlsLinearGradientBrush = Microsoft.Maui.Controls.LinearGradientBrush;
using MauiControlsRadialGradientBrush = Microsoft.Maui.Controls.RadialGradientBrush;
namespace Avalonia.Controls.Maui;

/// <summary>
/// Provides extension methods for converting .NET MAUI graphics types to their Avalonia equivalents.
/// </summary>
public static class GraphicsExtensions
{
    /// <summary>
    /// Converts a .NET MAUI <see cref="MauiColor"/> to an Avalonia <see cref="AvaloniaSolidColorBrush"/>.
    /// </summary>
    /// <param name="color">The .NET MAUI color to convert.</param>
    /// <returns>A <see cref="AvaloniaSolidColorBrush"/> representing the color.</returns>
    public static AvaloniaSolidColorBrush ToPlatform(this MauiColor color)
    {
        return new AvaloniaSolidColorBrush(color.ToAvaloniaColor());
    }

    /// <summary>
    /// Converts a .NET MAUI <see cref="MauiColor"/> to an Avalonia <see cref="AvaloniaColor"/> struct.
    /// </summary>
    /// <param name="color">The .NET MAUI color to convert.</param>
    /// <returns>An <see cref="AvaloniaColor"/> with equivalent ARGB channel values.</returns>
    public static AvaloniaColor ToAvaloniaColor(this MauiColor color)
    {
        return AvaloniaColor.FromArgb(
            (byte)(color.Alpha * 255),
            (byte)(color.Red * 255),
            (byte)(color.Green * 255),
            (byte)(color.Blue * 255));
    }

    /// <summary>
    /// Converts a nullable .NET MAUI <see cref="MauiPaint"/> to an Avalonia <see cref="AvaloniaIBrush"/>, supporting solid, linear gradient, and radial gradient paints.
    /// </summary>
    /// <param name="paint">The .NET MAUI paint to convert, or <c>null</c>.</param>
    /// <returns>An Avalonia brush representing the paint, or <c>null</c> if the paint is <c>null</c> or not a supported type.</returns>
    public static AvaloniaIBrush? ToPlatform(this MauiPaint? paint)
    {
        if (paint is MauiSolidPaint solidPaint && solidPaint.Color != null)
        {
            return solidPaint.Color.ToPlatform();
        }
        else if (paint is MauiLinearGradientPaint linearGradient)
        {
            var stops = linearGradient.GradientStops?.Select(s => (s.Color, s.Offset));
            return CreateLinearGradientBrush(linearGradient.StartPoint, linearGradient.EndPoint, stops);
        }
        else if (paint is MauiRadialGradientPaint radialGradient)
        {
            var stops = radialGradient.GradientStops?.Select(s => (s.Color, s.Offset));
            return CreateRadialGradientBrush(radialGradient.Center, radialGradient.Radius, stops);
        }

        return null;
    }

    /// <summary>
    /// Converts a .NET MAUI <see cref="MauiLineCap"/> to an Avalonia <see cref="AvaloniaPenLineCap"/>.
    /// </summary>
    /// <param name="lineCap">The .NET MAUI line cap to convert.</param>
    /// <returns>The corresponding Avalonia <see cref="AvaloniaPenLineCap"/> value.</returns>
    public static AvaloniaPenLineCap ToPlatform(this MauiLineCap lineCap)
    {
        return lineCap switch
        {
            MauiLineCap.Butt => AvaloniaPenLineCap.Flat,
            MauiLineCap.Round => AvaloniaPenLineCap.Round,
            MauiLineCap.Square => AvaloniaPenLineCap.Square,
            _ => AvaloniaPenLineCap.Flat
        };
    }

    /// <summary>
    /// Converts a .NET MAUI <see cref="MauiLineJoin"/> to an Avalonia <see cref="AvaloniaPenLineJoin"/>.
    /// </summary>
    /// <param name="lineJoin">The .NET MAUI line join to convert.</param>
    /// <returns>The corresponding Avalonia <see cref="AvaloniaPenLineJoin"/> value.</returns>
    public static AvaloniaPenLineJoin ToPlatform(this MauiLineJoin lineJoin)
    {
        return lineJoin switch
        {
            MauiLineJoin.Miter => AvaloniaPenLineJoin.Miter,
            MauiLineJoin.Round => AvaloniaPenLineJoin.Round,
            MauiLineJoin.Bevel => AvaloniaPenLineJoin.Bevel,
            _ => AvaloniaPenLineJoin.Miter
        };
    }

    /// <summary>
    /// Converts a nullable .NET MAUI Controls <see cref="MauiControlsBrush"/> to an Avalonia <see cref="AvaloniaIBrush"/>, supporting solid color, linear gradient, and radial gradient brushes.
    /// </summary>
    /// <param name="brush">The .NET MAUI Controls brush to convert, or <c>null</c>.</param>
    /// <returns>An Avalonia brush representing the MAUI brush, or <c>null</c> if the brush is <c>null</c>, empty, or not a supported type.</returns>
    public static AvaloniaIBrush? ToPlatform(this MauiControlsBrush? brush)
    {
        if (brush == null || brush.IsEmpty)
            return null;

        if (brush is MauiControlsSolidColorBrush solidBrush && solidBrush.Color != null)
        {
            return solidBrush.Color.ToPlatform();
        }
        else if (brush is MauiControlsLinearGradientBrush linearBrush)
        {
            var stops = linearBrush.GradientStops?.Select(s => (s.Color, s.Offset));
            return CreateLinearGradientBrush(linearBrush.StartPoint, linearBrush.EndPoint, stops);
        }
        else if (brush is MauiControlsRadialGradientBrush radialBrush)
        {
            var stops = radialBrush.GradientStops?.Select(s => (s.Color, s.Offset));
            return CreateRadialGradientBrush(radialBrush.Center, radialBrush.Radius, stops);
        }

        return null;
    }

    private static AvaloniaLinearGradientBrush CreateLinearGradientBrush(Microsoft.Maui.Graphics.Point startPoint, Microsoft.Maui.Graphics.Point endPoint, IEnumerable<(MauiColor Color, float Offset)>? stops)
    {
        var brush = new AvaloniaLinearGradientBrush();

        if (stops != null)
        {
            foreach (var stop in stops)
            {
                if (stop.Color != null)
                {
                    brush.GradientStops.Add(new AvaloniaGradientStop
                    {
                        Offset = stop.Offset,
                        Color = stop.Color.ToAvaloniaColor()
                    });
                }
            }
        }

        brush.StartPoint = new RelativePoint(startPoint.X, startPoint.Y, RelativeUnit.Relative);
        brush.EndPoint = new RelativePoint(endPoint.X, endPoint.Y, RelativeUnit.Relative);

        return brush;
    }

    private static AvaloniaRadialGradientBrush CreateRadialGradientBrush(Microsoft.Maui.Graphics.Point center, double radius, IEnumerable<(MauiColor Color, float Offset)>? stops)
    {
        var avaloniaBrush = new AvaloniaRadialGradientBrush();

        if (stops != null)
        {
            foreach (var stop in stops)
            {
                if (stop.Color != null)
                {
                    avaloniaBrush.GradientStops.Add(new AvaloniaGradientStop
                    {
                        Offset = stop.Offset,
                        Color = stop.Color.ToAvaloniaColor()
                    });
                }
            }
        }

        avaloniaBrush.Center = new RelativePoint(center.X, center.Y, RelativeUnit.Relative);
        // Avalonia separates Center (geometry) and GradientOrigin (focal point). MAUI uses Center for both.
        avaloniaBrush.GradientOrigin = avaloniaBrush.Center;
        // Fix for radius scaling
        avaloniaBrush.RadiusX = new RelativeScalar(radius, RelativeUnit.Relative);
        avaloniaBrush.RadiusY = new RelativeScalar(radius, RelativeUnit.Relative);

        return avaloniaBrush;
    }
}