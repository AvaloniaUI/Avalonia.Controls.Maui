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

public static class GraphicsExtensions
{
    public static AvaloniaSolidColorBrush ToPlatform(this MauiColor color)
    {
        return new AvaloniaSolidColorBrush(AvaloniaColor.FromArgb(
            (byte)(color.Alpha * 255),
            (byte)(color.Red * 255),
            (byte)(color.Green * 255),
            (byte)(color.Blue * 255)));
    }

    public static AvaloniaIBrush? ToPlatform(this MauiPaint? paint)
    {
        if (paint is MauiSolidPaint solidPaint && solidPaint.Color != null)
        {
            return new AvaloniaSolidColorBrush(global::Avalonia.Media.Color.FromArgb(
                (byte)(solidPaint.Color.Alpha * 255),
                (byte)(solidPaint.Color.Red * 255),
                (byte)(solidPaint.Color.Green * 255),
                (byte)(solidPaint.Color.Blue * 255)));
        }
        else if (paint is MauiLinearGradientPaint linearGradient)
        {
            var brush = new AvaloniaLinearGradientBrush();

            if (linearGradient.GradientStops != null)
            {
                foreach (var stop in linearGradient.GradientStops)
                {
                    brush.GradientStops.Add(new AvaloniaGradientStop
                    {
                        Offset = stop.Offset,
                        Color = AvaloniaColor.FromArgb(
                            (byte)(stop.Color.Alpha * 255),
                            (byte)(stop.Color.Red * 255),
                            (byte)(stop.Color.Green * 255),
                            (byte)(stop.Color.Blue * 255))
                    });
                }
            }

            brush.StartPoint = new RelativePoint(linearGradient.StartPoint.X, linearGradient.StartPoint.Y, RelativeUnit.Relative);
            brush.EndPoint = new RelativePoint(linearGradient.EndPoint.X, linearGradient.EndPoint.Y, RelativeUnit.Relative);

            return brush;
        }
        else if (paint is MauiRadialGradientPaint radialGradient)
        {
            var brush = new AvaloniaRadialGradientBrush();

            if (radialGradient.GradientStops != null)
            {
                foreach (var stop in radialGradient.GradientStops)
                {
                    brush.GradientStops.Add(new AvaloniaGradientStop
                    {
                        Offset = stop.Offset,
                        Color = global::Avalonia.Media.Color.FromArgb(
                            (byte)(stop.Color.Alpha * 255),
                            (byte)(stop.Color.Red * 255),
                            (byte)(stop.Color.Green * 255),
                            (byte)(stop.Color.Blue * 255))
                    });
                }
            }

            brush.Center = new RelativePoint(radialGradient.Center.X, radialGradient.Center.Y, RelativeUnit.Relative);
            brush.RadiusX = new RelativeScalar(radialGradient.Radius * 100, RelativeUnit.Relative);
            brush.RadiusY = new RelativeScalar(radialGradient.Radius * 100, RelativeUnit.Relative);

            return brush;
        }

        return null;
    }

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

    public static AvaloniaIBrush? ToPlatform(this MauiControlsBrush? brush)
    {
        if (brush == null || brush.IsEmpty)
            return null;

        if (brush is MauiControlsSolidColorBrush solidBrush && solidBrush.Color != null)
        {
            return new AvaloniaSolidColorBrush(AvaloniaColor.FromArgb(
                (byte)(solidBrush.Color.Alpha * 255),
                (byte)(solidBrush.Color.Red * 255),
                (byte)(solidBrush.Color.Green * 255),
                (byte)(solidBrush.Color.Blue * 255)));
        }
        else if (brush is MauiControlsLinearGradientBrush linearBrush)
        {
            var avaloniaBrush = new AvaloniaLinearGradientBrush();

            if (linearBrush.GradientStops != null)
            {
                foreach (var stop in linearBrush.GradientStops)
                {
                    if (stop.Color != null)
                    {
                        avaloniaBrush.GradientStops.Add(new AvaloniaGradientStop
                        {
                            Offset = stop.Offset,
                            Color = AvaloniaColor.FromArgb(
                                (byte)(stop.Color.Alpha * 255),
                                (byte)(stop.Color.Red * 255),
                                (byte)(stop.Color.Green * 255),
                                (byte)(stop.Color.Blue * 255))
                        });
                    }
                }
            }

            avaloniaBrush.StartPoint = new RelativePoint(linearBrush.StartPoint.X, linearBrush.StartPoint.Y, RelativeUnit.Relative);
            avaloniaBrush.EndPoint = new RelativePoint(linearBrush.EndPoint.X, linearBrush.EndPoint.Y, RelativeUnit.Relative);

            return avaloniaBrush;
        }
        else if (brush is MauiControlsRadialGradientBrush radialBrush)
        {
            var avaloniaBrush = new AvaloniaRadialGradientBrush();

            if (radialBrush.GradientStops != null)
            {
                foreach (var stop in radialBrush.GradientStops)
                {
                    if (stop.Color != null)
                    {
                        avaloniaBrush.GradientStops.Add(new AvaloniaGradientStop
                        {
                            Offset = stop.Offset,
                            Color = AvaloniaColor.FromArgb(
                                (byte)(stop.Color.Alpha * 255),
                                (byte)(stop.Color.Red * 255),
                                (byte)(stop.Color.Green * 255),
                                (byte)(stop.Color.Blue * 255))
                        });
                    }
                }
            }

            avaloniaBrush.Center = new RelativePoint(radialBrush.Center.X, radialBrush.Center.Y, RelativeUnit.Relative);
            avaloniaBrush.RadiusX = new RelativeScalar(radialBrush.Radius, RelativeUnit.Relative);
            avaloniaBrush.RadiusY = new RelativeScalar(radialBrush.Radius, RelativeUnit.Relative);

            return avaloniaBrush;
        }

        return null;
    }
}