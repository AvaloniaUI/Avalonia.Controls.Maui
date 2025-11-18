#nullable disable
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System.Text;

namespace Avalonia.Controls.Maui.Extensions;

public static class GeometryExtensions
{
    public static global::Avalonia.Media.Geometry ToAvaloniaGeometry(this Geometry geometry)
    {
        if (geometry is null)
            return null;

        if (geometry is LineGeometry lineGeometry)
        {
            return new global::Avalonia.Media.LineGeometry(
                new global::Avalonia.Point(lineGeometry.StartPoint.X, lineGeometry.StartPoint.Y),
                new global::Avalonia.Point(lineGeometry.EndPoint.X, lineGeometry.EndPoint.Y));
        }

        if (geometry is RectangleGeometry rectGeometry)
        {
            return new global::Avalonia.Media.RectangleGeometry(
                new global::Avalonia.Rect(
                    rectGeometry.Rect.X,
                    rectGeometry.Rect.Y,
                    rectGeometry.Rect.Width,
                    rectGeometry.Rect.Height));
        }

        if (geometry is EllipseGeometry ellipseGeometry)
        {
            return new global::Avalonia.Media.EllipseGeometry(
                new global::Avalonia.Rect(
                    ellipseGeometry.Center.X - ellipseGeometry.RadiusX,
                    ellipseGeometry.Center.Y - ellipseGeometry.RadiusY,
                    ellipseGeometry.RadiusX * 2,
                    ellipseGeometry.RadiusY * 2));
        }

        if (geometry is PathGeometry pathGeometry)
        {
            return global::Avalonia.Media.Geometry.Parse(pathGeometry.ToDefinitionString());
        }

        if (geometry is GeometryGroup geometryGroup)
        {
            var avaloniaGroup = new global::Avalonia.Media.GeometryGroup
            {
                FillRule = geometryGroup.FillRule == FillRule.EvenOdd
                    ? global::Avalonia.Media.FillRule.EvenOdd
                    : global::Avalonia.Media.FillRule.NonZero
            };

            foreach (var child in geometryGroup.Children)
            {
                var avaloniaChild = child.ToAvaloniaGeometry();
                if (avaloniaChild != null)
                    avaloniaGroup.Children.Add(avaloniaChild);
            }

            return avaloniaGroup;
        }

        // Fallback: try to parse as path data string if available
        return null;
    }

    private static string ToDefinitionString(this PathGeometry pathGeometry)
    {
        var sb = new StringBuilder();

        foreach (var figure in pathGeometry.Figures)
        {
            sb.Append($"M {figure.StartPoint.X},{figure.StartPoint.Y} ");

            foreach (var segment in figure.Segments)
            {
                if (segment is LineSegment lineSegment)
                {
                    sb.Append($"L {lineSegment.Point.X},{lineSegment.Point.Y} ");
                }
                else if (segment is BezierSegment bezierSegment)
                {
                    sb.Append($"C {bezierSegment.Point1.X},{bezierSegment.Point1.Y} ");
                    sb.Append($"{bezierSegment.Point2.X},{bezierSegment.Point2.Y} ");
                    sb.Append($"{bezierSegment.Point3.X},{bezierSegment.Point3.Y} ");
                }
                else if (segment is QuadraticBezierSegment quadSegment)
                {
                    sb.Append($"Q {quadSegment.Point1.X},{quadSegment.Point1.Y} ");
                    sb.Append($"{quadSegment.Point2.X},{quadSegment.Point2.Y} ");
                }
                else if (segment is ArcSegment arcSegment)
                {
                    sb.Append($"A {arcSegment.Size.Width},{arcSegment.Size.Height} ");
                    sb.Append($"{arcSegment.RotationAngle} ");
                    sb.Append($"{(arcSegment.IsLargeArc ? 1 : 0)} ");
                    sb.Append($"{(arcSegment.SweepDirection == Microsoft.Maui.Controls.SweepDirection.Clockwise ? 1 : 0)} ");
                    sb.Append($"{arcSegment.Point.X},{arcSegment.Point.Y} ");
                }
                else if (segment is PolyLineSegment polyLineSegment)
                {
                    foreach (var point in polyLineSegment.Points)
                    {
                        sb.Append($"L {point.X},{point.Y} ");
                    }
                }
                else if (segment is PolyBezierSegment polyBezierSegment)
                {
                    for (int i = 0; i < polyBezierSegment.Points.Count; i += 3)
                    {
                        if (i + 2 < polyBezierSegment.Points.Count)
                        {
                            sb.Append($"C {polyBezierSegment.Points[i].X},{polyBezierSegment.Points[i].Y} ");
                            sb.Append($"{polyBezierSegment.Points[i + 1].X},{polyBezierSegment.Points[i + 1].Y} ");
                            sb.Append($"{polyBezierSegment.Points[i + 2].X},{polyBezierSegment.Points[i + 2].Y} ");
                        }
                    }
                }
                else if (segment is PolyQuadraticBezierSegment polyQuadSegment)
                {
                    for (int i = 0; i < polyQuadSegment.Points.Count; i += 2)
                    {
                        if (i + 1 < polyQuadSegment.Points.Count)
                        {
                            sb.Append($"Q {polyQuadSegment.Points[i].X},{polyQuadSegment.Points[i].Y} ");
                            sb.Append($"{polyQuadSegment.Points[i + 1].X},{polyQuadSegment.Points[i + 1].Y} ");
                        }
                    }
                }
            }

            if (figure.IsClosed)
            {
                sb.Append("Z ");
            }
        }

        return sb.ToString().Trim();
    }
}