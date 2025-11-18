#nullable disable
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Media;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Custom Avalonia control to represent MAUI BoxView
/// </summary>
public class MauiBoxView : Control
{
    public static readonly StyledProperty<IBrush> FillProperty =
        AvaloniaProperty.Register<MauiBoxView, IBrush>(nameof(Fill));

    public static readonly StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<MauiBoxView, IBrush>(nameof(Stroke));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<MauiBoxView, double>(nameof(StrokeThickness), 1.0);

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<MauiBoxView, CornerRadius>(nameof(CornerRadius));

    static MauiBoxView()
    {
        AffectsRender<MauiBoxView>(FillProperty, StrokeProperty, StrokeThicknessProperty, CornerRadiusProperty);
    }

    public IBrush Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public IBrush Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var cornerRadius = CornerRadius;

        // Draw filled rectangle with rounded corners
        if (Fill != null)
        {
            if (cornerRadius != default)
            {
                var rect = new RoundedRect(bounds,
                    cornerRadius.TopLeft,
                    cornerRadius.TopRight,
                    cornerRadius.BottomRight,
                    cornerRadius.BottomLeft);
                context.DrawRectangle(Fill, null, rect);
            }
            else
            {
                context.DrawRectangle(Fill, null, bounds);
            }
        }

        // Draw stroke if specified
        if (Stroke != null && StrokeThickness > 0)
        {
            var pen = new Pen(Stroke, StrokeThickness);
            var strokeBounds = bounds.Deflate(StrokeThickness / 2);

            if (cornerRadius != default)
            {
                var rect = new RoundedRect(strokeBounds,
                    cornerRadius.TopLeft,
                    cornerRadius.TopRight,
                    cornerRadius.BottomRight,
                    cornerRadius.BottomLeft);
                context.DrawRectangle(null, pen, rect);
            }
            else
            {
                context.DrawRectangle(null, pen, strokeBounds);
            }
        }
    }
}