using Avalonia.Controls;
using Avalonia.Media;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Platform;

internal class BorderView : Decorator
{
    /// <summary>
    /// Defines the <see cref="Background"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<BorderView, IBrush?>(nameof(Background));

    /// <summary>
    /// Defines the <see cref="Stroke"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<BorderView, IBrush?>(nameof(Stroke));

    /// <summary>
    /// Defines the <see cref="StrokeThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<BorderView, double>(nameof(StrokeThickness), 0.0);

    /// <summary>
    /// Defines the <see cref="Shape"/> property.
    /// </summary>
    public static readonly StyledProperty<MauiGraphics.IShape?> ShapeProperty =
        AvaloniaProperty.Register<BorderView, MauiGraphics.IShape?>(nameof(Shape));

    /// <summary>
    /// Defines the <see cref="StrokeDashPattern"/> property.
    /// </summary>
    public static readonly StyledProperty<float[]?> StrokeDashPatternProperty =
        AvaloniaProperty.Register<BorderView, float[]?>(nameof(StrokeDashPattern));

    /// <summary>
    /// Defines the <see cref="StrokeDashOffset"/> property.
    /// </summary>
    public static readonly StyledProperty<float> StrokeDashOffsetProperty =
        AvaloniaProperty.Register<BorderView, float>(nameof(StrokeDashOffset), 0f);

    /// <summary>
    /// Defines the <see cref="StrokeLineCap"/> property.
    /// </summary>
    public static readonly StyledProperty<MauiGraphics.LineCap> StrokeLineCapProperty =
        AvaloniaProperty.Register<BorderView, MauiGraphics.LineCap>(nameof(StrokeLineCap), MauiGraphics.LineCap.Butt);

    /// <summary>
    /// Defines the <see cref="StrokeLineJoin"/> property.
    /// </summary>
    public static readonly StyledProperty<MauiGraphics.LineJoin> StrokeLineJoinProperty =
        AvaloniaProperty.Register<BorderView, MauiGraphics.LineJoin>(nameof(StrokeLineJoin), MauiGraphics.LineJoin.Miter);

    /// <summary>
    /// Defines the <see cref="StrokeMiterLimit"/> property.
    /// </summary>
    public static readonly StyledProperty<float> StrokeMiterLimitProperty =
        AvaloniaProperty.Register<BorderView, float>(nameof(StrokeMiterLimit), 10f);

    private Geometry? _cachedGeometry;
    private Size _lastRenderSize;

    static BorderView()
    {
        AffectsRender<BorderView>(
            BackgroundProperty,
            StrokeProperty,
            StrokeThicknessProperty,
            ShapeProperty,
            StrokeDashPatternProperty,
            StrokeDashOffsetProperty,
            StrokeLineCapProperty,
            StrokeLineJoinProperty,
            StrokeMiterLimitProperty);

        AffectsMeasure<BorderView>(
            StrokeThicknessProperty,
            ShapeProperty);
    }

    /// <summary>
    /// Gets or sets the background brush.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke brush.
    /// </summary>
    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke thickness.
    /// </summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the MAUI shape.
    /// </summary>
    public MauiGraphics.IShape? Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke dash pattern.
    /// </summary>
    public float[]? StrokeDashPattern
    {
        get => GetValue(StrokeDashPatternProperty);
        set => SetValue(StrokeDashPatternProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke dash offset.
    /// </summary>
    public float StrokeDashOffset
    {
        get => GetValue(StrokeDashOffsetProperty);
        set => SetValue(StrokeDashOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke line cap.
    /// </summary>
    public MauiGraphics.LineCap StrokeLineCap
    {
        get => GetValue(StrokeLineCapProperty);
        set => SetValue(StrokeLineCapProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke line join.
    /// </summary>
    public MauiGraphics.LineJoin StrokeLineJoin
    {
        get => GetValue(StrokeLineJoinProperty);
        set => SetValue(StrokeLineJoinProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke miter limit.
    /// </summary>
    public float StrokeMiterLimit
    {
        get => GetValue(StrokeMiterLimitProperty);
        set => SetValue(StrokeMiterLimitProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ShapeProperty || change.Property == BoundsProperty)
        {
            _cachedGeometry = null;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var strokeThickness = StrokeThickness;
        var padding = Padding;
        var constraint = availableSize.Deflate(new Thickness(strokeThickness) + padding);

        if (Child != null)
        {
            Child.Measure(constraint);
            var childSize = Child.DesiredSize;
            return new Size(
                childSize.Width + strokeThickness * 2 + padding.Left + padding.Right,
                childSize.Height + strokeThickness * 2 + padding.Top + padding.Bottom);
        }

        return new Size(strokeThickness * 2 + padding.Left + padding.Right,
                       strokeThickness * 2 + padding.Top + padding.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Child != null)
        {
            var strokeThickness = StrokeThickness;
            var padding = Padding;
            var childRect = new Rect(finalSize).Deflate(new Thickness(strokeThickness) + padding);
            Child.Arrange(childRect);
        }

        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);

        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        // Update cached geometry if size changed or geometry is null
        if (_cachedGeometry == null || _lastRenderSize != bounds.Size)
        {
            _cachedGeometry = CreateGeometry(bounds.Size);
            _lastRenderSize = bounds.Size;
        }

        if (_cachedGeometry == null)
            return;

        // Draw background
        if (Background != null)
        {
            context.DrawGeometry(Background, null, _cachedGeometry);
        }

        // Draw stroke
        if (Stroke != null && StrokeThickness > 0)
        {
            var pen = CreatePen();
            context.DrawGeometry(null, pen, _cachedGeometry);
        }
    }

    private Geometry? CreateGeometry(Size size)
    {
        if (Shape != null)
        {
            // Convert MAUI shape to Avalonia geometry
            var bounds = new MauiGraphics.Rect(0, 0, size.Width, size.Height);
            var pathF = Shape.PathForBounds(bounds);
            return ShapeExtensions.ToAvaloniaGeometry(pathF);
        }

        // Default to rectangle
        return new RectangleGeometry(new Rect(size));
    }

    private Pen CreatePen()
    {
        var pen = new Pen(Stroke, StrokeThickness)
        {
            LineCap = ShapeExtensions.ToAvaloniaPenLineCap(StrokeLineCap),
            LineJoin = ShapeExtensions.ToAvaloniaPenLineJoin(StrokeLineJoin),
            MiterLimit = StrokeMiterLimit
        };

        // Set dash style if pattern is provided
        if (StrokeDashPattern != null && StrokeDashPattern.Length > 0)
        {
            var dashes = new double[StrokeDashPattern.Length];
            for (int i = 0; i < StrokeDashPattern.Length; i++)
            {
                dashes[i] = StrokeDashPattern[i];
            }
            pen.DashStyle = new DashStyle(dashes, StrokeDashOffset);
        }

        return pen;
    }
}