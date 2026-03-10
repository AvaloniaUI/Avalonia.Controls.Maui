using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using PlatformShape = global::Avalonia.Controls.Shapes.Shape;
using AvaloniaRectangle = global::Avalonia.Controls.Shapes.Rectangle;
using AvaloniaLine = global::Avalonia.Controls.Shapes.Line;
using AvaloniaPolygon = global::Avalonia.Controls.Shapes.Polygon;
using AvaloniaPolyline = global::Avalonia.Controls.Shapes.Polyline;
using AvaloniaPath = global::Avalonia.Controls.Shapes.Path;
using AvaloniaRoundRectangle = Avalonia.Controls.Maui.Platform.MauiRoundRectangle;
using PathShape = Microsoft.Maui.Controls.Shapes.Path;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods used by shape handlers to map IShapeView members to Avalonia shapes.
/// </summary>
public static class ShapeViewExtensions
{
    /// <summary>
    /// Updates the background for the specified shape view.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateBackground(this PlatformShape platformView, IShapeView shapeView)
    {
        _ = platformView;
        _ = shapeView;
    }

    /// <summary>
    /// Updates the shape. Base implementation does not require additional work.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateShape(this PlatformShape platformView, IShapeView shapeView)
    {
        _ = platformView;
        _ = shapeView;
    }

    /// <summary>
    /// Updates the aspect/stretch for the shape.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateAspect(this PlatformShape platformView, IShapeView shapeView)
    {
        platformView.Stretch = shapeView.Aspect switch
        {
            PathAspect.None => global::Avalonia.Media.Stretch.None,
            PathAspect.Center => global::Avalonia.Media.Stretch.None,
            PathAspect.Stretch => global::Avalonia.Media.Stretch.Fill,
            PathAspect.AspectFit => global::Avalonia.Media.Stretch.Uniform,
            PathAspect.AspectFill => global::Avalonia.Media.Stretch.UniformToFill,
            _ => global::Avalonia.Media.Stretch.None
        };
    }

    /// <summary>
    /// Updates the fill brush on the Avalonia shape.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateFill(this PlatformShape platformView, IShapeView shapeView)
    {
        platformView.Fill = shapeView.Fill?.ToAvaloniaBrush();
    }

    /// <summary>
    /// Updates the stroke brush on the Avalonia shape.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateStroke(this PlatformShape platformView, IShapeView shapeView)
    {
        platformView.Stroke = shapeView.Stroke?.ToAvaloniaBrush();
    }

    /// <summary>
    /// Updates stroke thickness on the Avalonia shape.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateStrokeThickness(this PlatformShape platformView, IShapeView shapeView)
    {
        platformView.StrokeThickness = shapeView.StrokeThickness;
    }

    /// <summary>
    /// Updates the stroke dash pattern for the Avalonia shape.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateStrokeDashPattern(this PlatformShape platformView, IShapeView shapeView)
    {
        if (shapeView.StrokeDashPattern is null)
        {
            platformView.StrokeDashArray?.Clear();
            return;
        }

        var dashArray = new global::Avalonia.Collections.AvaloniaList<double>();
        foreach (var value in shapeView.StrokeDashPattern)
        {
            dashArray.Add(value);
        }

        platformView.StrokeDashArray = dashArray;
    }

    /// <summary>
    /// Updates the stroke dash offset for the Avalonia shape.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateStrokeDashOffset(this PlatformShape platformView, IShapeView shapeView)
    {
        platformView.StrokeDashOffset = shapeView.StrokeDashOffset;
    }

    /// <summary>
    /// Updates the line cap for the Avalonia shape stroke.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateStrokeLineCap(this PlatformShape platformView, IShapeView shapeView)
    {
        platformView.StrokeLineCap = shapeView.StrokeLineCap switch
        {
            LineCap.Butt => global::Avalonia.Media.PenLineCap.Flat,
            LineCap.Round => global::Avalonia.Media.PenLineCap.Round,
            LineCap.Square => global::Avalonia.Media.PenLineCap.Square,
            _ => global::Avalonia.Media.PenLineCap.Flat
        };
    }

    /// <summary>
    /// Updates the line join for the Avalonia shape stroke.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateStrokeLineJoin(this PlatformShape platformView, IShapeView shapeView)
    {
        platformView.StrokeJoin = shapeView.StrokeLineJoin switch
        {
            LineJoin.Miter => global::Avalonia.Media.PenLineJoin.Miter,
            LineJoin.Round => global::Avalonia.Media.PenLineJoin.Round,
            LineJoin.Bevel => global::Avalonia.Media.PenLineJoin.Bevel,
            _ => global::Avalonia.Media.PenLineJoin.Miter
        };
    }

    /// <summary>
    /// Updates the stroke miter limit on the platform shape.
    /// </summary>
    /// <param name="platformView">Avalonia shape instance.</param>
    /// <param name="shapeView">The virtual shape view.</param>
    public static void UpdateStrokeMiterLimit(this PlatformShape platformView, IShapeView shapeView)
    {
        platformView.StrokeMiterLimit = shapeView.StrokeMiterLimit;
    }

    /// <summary>
    /// Updates the radius X property for a rectangle.
    /// </summary>
    /// <param name="platformView">Avalonia rectangle instance.</param>
    /// <param name="rectangle">The virtual rectangle view.</param>
    public static void UpdateRadiusX(this AvaloniaRectangle platformView, Rectangle rectangle)
    {
        platformView.RadiusX = rectangle.RadiusX;
    }

    /// <summary>
    /// Updates the radius Y property for a rectangle.
    /// </summary>
    /// <param name="platformView">Avalonia rectangle instance.</param>
    /// <param name="rectangle">The virtual rectangle view.</param>
    public static void UpdateRadiusY(this AvaloniaRectangle platformView, Rectangle rectangle)
    {
        platformView.RadiusY = rectangle.RadiusY;
    }

    /// <summary>
    /// Updates the starting point X coordinate for a line.
    /// </summary>
    /// <param name="platformView">Avalonia line instance.</param>
    /// <param name="line">The virtual line view.</param>
    public static void UpdateX1(this AvaloniaLine platformView, Line line)
    {
        platformView.StartPoint = new global::Avalonia.Point(line.X1, platformView.StartPoint.Y);
    }

    /// <summary>
    /// Updates the starting point Y coordinate for a line.
    /// </summary>
    /// <param name="platformView">Avalonia line instance.</param>
    /// <param name="line">The virtual line view.</param>
    public static void UpdateY1(this AvaloniaLine platformView, Line line)
    {
        platformView.StartPoint = new global::Avalonia.Point(platformView.StartPoint.X, line.Y1);
    }

    /// <summary>
    /// Updates the end point X coordinate for a line.
    /// </summary>
    /// <param name="platformView">Avalonia line instance.</param>
    /// <param name="line">The virtual line view.</param>
    public static void UpdateX2(this AvaloniaLine platformView, Line line)
    {
        platformView.EndPoint = new global::Avalonia.Point(line.X2, platformView.EndPoint.Y);
    }

    /// <summary>
    /// Updates the end point Y coordinate for a line.
    /// </summary>
    /// <param name="platformView">Avalonia line instance.</param>
    /// <param name="line">The virtual line view.</param>
    public static void UpdateY2(this AvaloniaLine platformView, Line line)
    {
        platformView.EndPoint = new global::Avalonia.Point(platformView.EndPoint.X, line.Y2);
    }

    /// <summary>
    /// Updates the points collection for a polygon.
    /// </summary>
    /// <param name="platformView">Avalonia polygon instance.</param>
    /// <param name="polygon">The virtual polygon view.</param>
    public static void UpdatePoints(this AvaloniaPolygon platformView, Polygon polygon)
    {
        if (polygon?.Points == null)
        {
            platformView.Points.Clear();
            return;
        }

        platformView.Points.Clear();
        foreach (var point in polygon.Points)
        {
            platformView.Points.Add(new global::Avalonia.Point(point.X, point.Y));
        }
    }

    /// <summary>
    /// Updates the points collection for a polyline.
    /// </summary>
    /// <param name="platformView">Avalonia polyline instance.</param>
    /// <param name="polyline">The virtual polyline view.</param>
    public static void UpdatePoints(this AvaloniaPolyline platformView, Polyline polyline)
    {
        if (polyline?.Points == null)
        {
            platformView.Points.Clear();
            return;
        }

        platformView.Points.Clear();
        foreach (var point in polyline.Points)
        {
            platformView.Points.Add(new global::Avalonia.Point(point.X, point.Y));
        }
    }

    /// <summary>
    /// Updates the fill rule for a polygon.
    /// </summary>
    /// <param name="platformView">Avalonia polygon instance.</param>
    /// <param name="polygon">The virtual polygon view.</param>
    public static void UpdateFillRule(this AvaloniaPolygon platformView, Polygon polygon)
    {
        platformView.FillRule = polygon.FillRule == FillRule.EvenOdd
            ? global::Avalonia.Media.FillRule.EvenOdd
            : global::Avalonia.Media.FillRule.NonZero;
    }

    /// <summary>
    /// Updates the fill rule for a polyline.
    /// </summary>
    /// <param name="platformView">Avalonia polyline instance.</param>
    /// <param name="polyline">The virtual polyline view.</param>
    public static void UpdateFillRule(this AvaloniaPolyline platformView, Polyline polyline)
    {
        platformView.FillRule = polyline.FillRule == FillRule.EvenOdd
            ? global::Avalonia.Media.FillRule.EvenOdd
            : global::Avalonia.Media.FillRule.NonZero;
    }

    /// <summary>
    /// Updates the path geometry for a MAUI Path.
    /// </summary>
    /// <param name="platformView">Avalonia path instance.</param>
    /// <param name="path">The virtual path view.</param>
    public static void UpdateData(this AvaloniaPath platformView, PathShape path)
    {
        if (path?.Data != null)
        {
            platformView.Data = path.Data.ToPlatform();
        }
        else
        {
            platformView.Data = null;
        }
    }

    /// <summary>
    /// Updates the render transform for a MAUI Path.
    /// </summary>
    /// <param name="platformView">Avalonia path instance.</param>
    /// <param name="path">The virtual path view.</param>
    public static void UpdateRenderTransform(this AvaloniaPath platformView, PathShape path)
    {
        if (platformView is null)
            return;

        if (path?.RenderTransform == null)
        {
            platformView.RenderTransform = null;
            return;
        }

        platformView.RenderTransform = ConvertTransform(path.RenderTransform);
    }

    private static global::Avalonia.Media.Transform? ConvertTransform(Transform transform)
    {
        switch (transform)
        {
            case RotateTransform rotate:
                return new global::Avalonia.Media.RotateTransform
                {
                    Angle = rotate.Angle
                };
            case ScaleTransform scale:
                return new global::Avalonia.Media.ScaleTransform
                {
                    ScaleX = scale.ScaleX,
                    ScaleY = scale.ScaleY
                };
            case TranslateTransform translate:
                return new global::Avalonia.Media.TranslateTransform
                {
                    X = translate.X,
                    Y = translate.Y
                };
            case TransformGroup group:
                var transforms = new global::Avalonia.Media.TransformGroup();
                foreach (var child in group.Children)
                {
                    var converted = ConvertTransform(child);
                    if (converted != null)
                    {
                        transforms.Children.Add(converted);
                    }
                }
                return transforms;
            default:
                var m = transform.Value;
                return new global::Avalonia.Media.MatrixTransform(
                    new global::Avalonia.Matrix(
                        m.M11, m.M12,
                        m.M21, m.M22,
                        m.OffsetX, m.OffsetY));
        }
    }

    /// <summary>
    /// Updates the corner radius for a round rectangle.
    /// </summary>
    /// <param name="platformView">Avalonia round rectangle instance.</param>
    /// <param name="roundRectangle">The virtual round rectangle view.</param>
    public static void UpdateCornerRadius(this AvaloniaRoundRectangle platformView, RoundRectangle roundRectangle)
    {
        var radius = roundRectangle.CornerRadius;
        platformView.CornerRadius = new global::Avalonia.CornerRadius(
            radius.TopLeft,
            radius.TopRight,
            radius.BottomRight,
            radius.BottomLeft);
    }
}
