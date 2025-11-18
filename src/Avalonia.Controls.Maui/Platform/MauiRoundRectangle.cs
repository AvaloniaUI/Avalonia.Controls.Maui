#nullable disable
using global::Avalonia;
using global::Avalonia.Controls.Shapes;
using global::Avalonia.Media;

namespace Avalonia.Controls.Maui.Platform;

public class MauiRoundRectangle : Shape
{
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<MauiRoundRectangle, CornerRadius>(nameof(CornerRadius));

    static MauiRoundRectangle()
    {
        AffectsGeometry<MauiRoundRectangle>(CornerRadiusProperty, BoundsProperty);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    protected override Geometry CreateDefiningGeometry()
    {
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

        if (bounds.Width <= 0 || bounds.Height <= 0)
            return new RectangleGeometry(new Rect());

        var cornerRadius = CornerRadius;

        // If all corners are the same and simple, use RectangleGeometry
        if (cornerRadius.TopLeft == cornerRadius.TopRight &&
            cornerRadius.TopLeft == cornerRadius.BottomRight &&
            cornerRadius.TopLeft == cornerRadius.BottomLeft)
        {
            return new RectangleGeometry(bounds, cornerRadius.TopLeft, cornerRadius.TopLeft);
        }

        // Otherwise create a path geometry with individual corner radii
        var pathGeometry = new PathGeometry();
        var figure = new PathFigure { IsClosed = true };

        var topLeft = cornerRadius.TopLeft;
        var topRight = cornerRadius.TopRight;
        var bottomRight = cornerRadius.BottomRight;
        var bottomLeft = cornerRadius.BottomLeft;

        // Start from top-left after the corner
        figure.StartPoint = new Point(bounds.Left + topLeft, bounds.Top);

        // Top edge
        figure.Segments.Add(new LineSegment { Point = new Point(bounds.Right - topRight, bounds.Top) });

        // Top-right corner
        if (topRight > 0)
        {
            figure.Segments.Add(new ArcSegment
            {
                Point = new Point(bounds.Right, bounds.Top + topRight),
                Size = new Size(topRight, topRight),
                SweepDirection = SweepDirection.Clockwise
            });
        }

        // Right edge
        figure.Segments.Add(new LineSegment { Point = new Point(bounds.Right, bounds.Bottom - bottomRight) });

        // Bottom-right corner
        if (bottomRight > 0)
        {
            figure.Segments.Add(new ArcSegment
            {
                Point = new Point(bounds.Right - bottomRight, bounds.Bottom),
                Size = new Size(bottomRight, bottomRight),
                SweepDirection = SweepDirection.Clockwise
            });
        }

        // Bottom edge
        figure.Segments.Add(new LineSegment { Point = new Point(bounds.Left + bottomLeft, bounds.Bottom) });

        // Bottom-left corner
        if (bottomLeft > 0)
        {
            figure.Segments.Add(new ArcSegment
            {
                Point = new Point(bounds.Left, bounds.Bottom - bottomLeft),
                Size = new Size(bottomLeft, bottomLeft),
                SweepDirection = SweepDirection.Clockwise
            });
        }

        // Left edge back to start
        figure.Segments.Add(new LineSegment { Point = new Point(bounds.Left, bounds.Top + topLeft) });

        // Top-left corner
        if (topLeft > 0)
        {
            figure.Segments.Add(new ArcSegment
            {
                Point = new Point(bounds.Left + topLeft, bounds.Top),
                Size = new Size(topLeft, topLeft),
                SweepDirection = SweepDirection.Clockwise
            });
        }

        pathGeometry.Figures.Add(figure);
        return pathGeometry;
    }
}