using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Extension methods for converting MAUI shapes and graphics types to Avalonia equivalents.
/// </summary>
public static class ShapeExtensions
{
    /// <summary>
    /// Converts a MAUI PathF to an Avalonia Geometry.
    /// </summary>
    public static Geometry? ToAvaloniaGeometry(this MauiGraphics.PathF pathF)
    {
        if (pathF == null)
            return null;

        var pathGeometry = new PathGeometry();
        PathFigure? currentFigure = null;
        var operations = pathF.SegmentTypes.ToList();
        var points = pathF.Points.ToList();

        int pointIndex = 0;
        int arcIndex = 0;

        for (int i = 0; i < operations.Count; i++)
        {
            var operation = operations[i];

            switch (operation)
            {
                case MauiGraphics.PathOperation.Move:
                    if (pointIndex < points.Count)
                    {
                        var point = points[pointIndex++];
                        currentFigure = new PathFigure
                        {
                            StartPoint = new Point(point.X, point.Y),
                            IsClosed = false
                        };
                        pathGeometry.Figures!.Add(currentFigure);
                    }
                    break;

                case MauiGraphics.PathOperation.Line:
                    if (currentFigure != null && pointIndex < points.Count)
                    {
                        var point = points[pointIndex++];
                        currentFigure.Segments!.Add(new LineSegment
                        {
                            Point = new Point(point.X, point.Y)
                        });
                    }
                    break;

                case MauiGraphics.PathOperation.Quad:
                    if (currentFigure != null && pointIndex + 1 < points.Count)
                    {
                        var controlPoint = points[pointIndex++];
                        var endPoint = points[pointIndex++];
                        currentFigure.Segments!.Add(new QuadraticBezierSegment
                        {
                            Point1 = new Point(controlPoint.X, controlPoint.Y),
                            Point2 = new Point(endPoint.X, endPoint.Y)
                        });
                    }
                    break;

                case MauiGraphics.PathOperation.Cubic:
                    if (currentFigure != null && pointIndex + 2 < points.Count)
                    {
                        var control1 = points[pointIndex++];
                        var control2 = points[pointIndex++];
                        var endPoint = points[pointIndex++];
                        currentFigure.Segments!.Add(new BezierSegment
                        {
                            Point1 = new Point(control1.X, control1.Y),
                            Point2 = new Point(control2.X, control2.Y),
                            Point3 = new Point(endPoint.X, endPoint.Y)
                        });
                    }
                    break;

                case MauiGraphics.PathOperation.Arc:
                    if (currentFigure != null && pointIndex < points.Count)
                    {
                        var endPoint = points[pointIndex++];

                        // For arcs, we need to calculate the radius and other parameters
                        // This is a simplified implementation - MAUI's PathF stores arc data separately
                        // We'll use a reasonable default for the arc
                        var lastPoint = currentFigure.Segments!.Count > 0
                            ? GetLastPoint(currentFigure)
                            : currentFigure.StartPoint;

                        var dx = endPoint.X - lastPoint.X;
                        var dy = endPoint.Y - lastPoint.Y;
                        var distance = Math.Sqrt(dx * dx + dy * dy);
                        var radius = distance / 2;

                        currentFigure.Segments!.Add(new ArcSegment
                        {
                            Point = new Point(endPoint.X, endPoint.Y),
                            Size = new Size(radius, radius),
                            RotationAngle = 0,
                            IsLargeArc = false,
                            SweepDirection = SweepDirection.Clockwise
                        });
                        arcIndex++;
                    }
                    break;

                case MauiGraphics.PathOperation.Close:
                    if (currentFigure != null)
                    {
                        currentFigure.IsClosed = true;
                    }
                    break;
            }
        }

        return pathGeometry;
    }

    /// <summary>
    /// Gets the last point from a path figure.
    /// </summary>
    private static Point GetLastPoint(PathFigure figure)
    {
        if (figure.Segments!.Count == 0)
            return figure.StartPoint;

        var lastSegment = figure.Segments[^1];

        return lastSegment switch
        {
            LineSegment line => line.Point,
            BezierSegment bezier => bezier.Point3,
            QuadraticBezierSegment quad => quad.Point2,
            ArcSegment arc => arc.Point,
            PolyLineSegment polyLine => polyLine.Points![^1],
            PolyBezierSegment polyBezier => polyBezier.Points![^1],
            _ => figure.StartPoint
        };
    }

    /// <summary>
    /// Converts MAUI LineCap to Avalonia PenLineCap.
    /// </summary>
    public static PenLineCap ToAvaloniaPenLineCap(this MauiGraphics.LineCap lineCap)
    {
        return lineCap switch
        {
            MauiGraphics.LineCap.Butt => PenLineCap.Flat,
            MauiGraphics.LineCap.Round => PenLineCap.Round,
            MauiGraphics.LineCap.Square => PenLineCap.Square,
            _ => PenLineCap.Flat
        };
    }

    /// <summary>
    /// Converts MAUI LineJoin to Avalonia PenLineJoin.
    /// </summary>
    public static PenLineJoin ToAvaloniaPenLineJoin(this MauiGraphics.LineJoin lineJoin)
    {
        return lineJoin switch
        {
            MauiGraphics.LineJoin.Miter => PenLineJoin.Miter,
            MauiGraphics.LineJoin.Round => PenLineJoin.Round,
            MauiGraphics.LineJoin.Bevel => PenLineJoin.Bevel,
            _ => PenLineJoin.Miter
        };
    }
}