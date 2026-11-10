using System.Diagnostics.CodeAnalysis;
using global::Avalonia.Controls.Shapes;
using global::Avalonia.Media;

namespace Avalonia.Controls.Maui.Platform;

internal static class ShapeAspect
{
    public static double GetStrokeInset(Shape shape) =>
        shape.Stroke is null ? 0 : shape.StrokeThickness;

    [return: NotNullIfNotNull(nameof(source))]
    public static Geometry? Apply(Geometry? source, Size size, Stretch stretch, double strokeInset)
    {
        if (source is null || size.Width <= 0 || size.Height <= 0)
            return source;

        var transform = GetTransform(size, source.Bounds, stretch, strokeInset);
        if (transform.IsIdentity)
            return source;

        var geometry = source.Clone();
        geometry.Transform = new MatrixTransform(source.Transform is { } existing ? existing.Value * transform : transform);
        return geometry;
    }

    public static Matrix GetTransform(Size size, Rect pathBounds, Stretch stretch, double strokeInset)
    {
        var view = new Rect(
            strokeInset / 2,
            strokeInset / 2,
            Math.Max(0, size.Width - strokeInset),
            Math.Max(0, size.Height - strokeInset));

        if (stretch == Stretch.None)
        {
            double translateX = 0;
            double translateY = 0;

            if (view.Left > pathBounds.Left)
                translateX = view.Left - pathBounds.Left;
            else if (pathBounds.Right > view.Right)
                translateX = view.Right - pathBounds.Right;

            if (view.Top > pathBounds.Top)
                translateY = view.Top - pathBounds.Top;
            else if (pathBounds.Bottom > view.Bottom)
                translateY = view.Bottom - pathBounds.Bottom;

            return Matrix.CreateTranslation(translateX, translateY);
        }

        var widthScale = Finite(view.Width / pathBounds.Width);
        var heightScale = Finite(view.Height / pathBounds.Height);

        switch (stretch)
        {
            case Stretch.Fill:
                return Matrix.CreateScale(widthScale, heightScale) *
                    Matrix.CreateTranslation(
                        view.Left - widthScale * pathBounds.Left,
                        view.Top - heightScale * pathBounds.Top);

            case Stretch.Uniform:
                var minScale = Math.Min(widthScale, heightScale);
                return Matrix.CreateScale(minScale, minScale) *
                    Matrix.CreateTranslation(
                        view.Left - minScale * pathBounds.Left + (view.Width - minScale * pathBounds.Width) / 2,
                        view.Top - minScale * pathBounds.Top + (view.Height - minScale * pathBounds.Height) / 2);

            case Stretch.UniformToFill:
                var maxScale = Math.Max(widthScale, heightScale);
                return Matrix.CreateScale(maxScale, maxScale) *
                    Matrix.CreateTranslation(
                        view.Left - maxScale * pathBounds.Left,
                        view.Top - maxScale * pathBounds.Top);

            default:
                return Matrix.Identity;
        }
    }

    public static Size Measure(Geometry? source, Size availableSize, Stretch stretch)
    {
        if (source is null)
            return default;

        var shapeBounds = source.Bounds;
        var shapeSize = stretch == Stretch.None ? new Size(shapeBounds.Right, shapeBounds.Bottom) : shapeBounds.Size;
        var desiredX = double.IsInfinity(availableSize.Width) ? shapeSize.Width : availableSize.Width;
        var desiredY = double.IsInfinity(availableSize.Height) ? shapeSize.Height : availableSize.Height;
        var sx = shapeBounds.Width > 0 ? desiredX / shapeSize.Width : 0;
        var sy = shapeBounds.Height > 0 ? desiredY / shapeSize.Height : 0;

        if (double.IsInfinity(availableSize.Width))
            sx = sy;

        if (double.IsInfinity(availableSize.Height))
            sy = sx;

        switch (stretch)
        {
            case Stretch.Uniform:
                sx = sy = Math.Min(sx, sy);
                break;
            case Stretch.UniformToFill:
                sx = sy = Math.Max(sx, sy);
                break;
            case Stretch.Fill:
                if (double.IsInfinity(availableSize.Width))
                    sx = 1;
                if (double.IsInfinity(availableSize.Height))
                    sy = 1;
                break;
            default:
                sx = sy = 1;
                break;
        }

        return new Size(shapeSize.Width * sx, shapeSize.Height * sy);
    }

    private static double Finite(double value) =>
        double.IsNaN(value) || double.IsInfinity(value) ? 0 : value;
}
