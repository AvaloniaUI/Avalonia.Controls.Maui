using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;

namespace Avalonia.Controls.Maui;

/// <summary>
/// A visual control that renders the <see cref="ProgressRing"/> control.
/// Handles the rendering of both determinate (percentage-based) and indeterminate (spinning) states.
/// </summary>
public class ProgressRingVisual : Control
{
    private Avalonia.Animation.Animation? _spinAnimation;
    
    // Geometry caching for performance
    private Geometry? _cachedGeometry;
    private double _cachedStartAngle;
    private double _cachedSweepAngle;
    private double _cachedRadius;

    static ProgressRingVisual()
    {
        AffectsRender<ProgressRingVisual>(
            ForegroundProperty,
            BackgroundProperty,
            IsIndeterminateProperty,
            IsActiveProperty,
            ValueProperty,
            MinimumProperty,
            MaximumProperty,
            StrokeThicknessProperty,
            AnimationProgressProperty);

        // Use Avalonia's style system for the infinite animation.
        // Styles call Apply() internally, which correctly handles infinite IterationCount
        // (unlike RunAsync which throws for looping animations).
        IsActiveProperty.Changed.AddClassHandler<ProgressRingVisual>((x, _) => x.UpdateSpinningPseudoClass());
        IsIndeterminateProperty.Changed.AddClassHandler<ProgressRingVisual>((x, _) => x.UpdateSpinningPseudoClass());
    }
    
    /// <summary>
    /// Defines the <see cref="Foreground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty = 
        AvaloniaProperty.Register<ProgressRingVisual, IBrush?>(nameof(Foreground));
    
    /// <summary>
    /// Defines the <see cref="Background"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty = 
        AvaloniaProperty.Register<ProgressRingVisual, IBrush?>(nameof(Background));
    
    /// <summary>
    /// Defines the <see cref="IsIndeterminate"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty = 
        AvaloniaProperty.Register<ProgressRingVisual, bool>(nameof(IsIndeterminate), defaultValue: true);
    
    /// <summary>
    /// Defines the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty = 
        AvaloniaProperty.Register<ProgressRingVisual, bool>(nameof(IsActive), defaultValue: true);
    
    /// <summary>
    /// Defines the <see cref="Value"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty = 
        AvaloniaProperty.Register<ProgressRingVisual, double>(nameof(Value), defaultValue: 0.0);
    
    /// <summary>
    /// Defines the <see cref="Minimum"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinimumProperty = 
        AvaloniaProperty.Register<ProgressRingVisual, double>(nameof(Minimum), defaultValue: 0.0);
    
    /// <summary>
    /// Defines the <see cref="Maximum"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaximumProperty = 
        AvaloniaProperty.Register<ProgressRingVisual, double>(nameof(Maximum), defaultValue: 100.0);
    
    /// <summary>
    /// Defines the <see cref="StrokeThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StrokeThicknessProperty = 
        AvaloniaProperty.Register<ProgressRingVisual, double>(nameof(StrokeThickness), defaultValue: 4.0);

    /// <summary>
    /// Defines the <see cref="AnimationProgress"/> property.
    /// </summary>
    public static readonly StyledProperty<double> AnimationProgressProperty = 
        AvaloniaProperty.Register<ProgressRingVisual, double>(nameof(AnimationProgress), defaultValue: 0.0);

    /// <summary>
    /// Gets or sets the foreground brush used to draw the progress indicator.
    /// </summary>
    public IBrush? Foreground 
    { 
        get => GetValue(ForegroundProperty); 
        set => SetValue(ForegroundProperty, value); 
    }
    
    /// <summary>
    /// Gets or sets the background brush used to draw the track.
    /// </summary>
    public IBrush? Background 
    { 
        get => GetValue(BackgroundProperty); 
        set => SetValue(BackgroundProperty, value); 
    }
    
    /// <summary>
    /// Gets or sets a value that indicates whether the progress ring reports generic progress with a repeating pattern (true) or reports progress based on the Value property (false).
    /// </summary>
    public bool IsIndeterminate 
    { 
        get => GetValue(IsIndeterminateProperty); 
        set => SetValue(IsIndeterminateProperty, value); 
    }
    
    /// <summary>
    /// Gets or sets a value that indicates whether the ProgressRing is showing progress.
    /// </summary>
    public bool IsActive 
    { 
        get => GetValue(IsActiveProperty); 
        set => SetValue(IsActiveProperty, value); 
    }
    
    /// <summary>
    /// Gets or sets the current value of the progress ring for determinate mode.
    /// </summary>
    public double Value 
    { 
        get => GetValue(ValueProperty); 
        set => SetValue(ValueProperty, value); 
    }
    
    /// <summary>
    /// Gets or sets the minimum value of the range.
    /// </summary>
    public double Minimum 
    { 
        get => GetValue(MinimumProperty); 
        set => SetValue(MinimumProperty, value); 
    }
    
    /// <summary>
    /// Gets or sets the maximum value of the range.
    /// </summary>
    public double Maximum 
    { 
        get => GetValue(MaximumProperty); 
        set => SetValue(MaximumProperty, value); 
    }
    
    /// <summary>
    /// Gets or sets the thickness of the ring stroke.
    /// </summary>
    public double StrokeThickness 
    { 
        get => GetValue(StrokeThicknessProperty); 
        set => SetValue(StrokeThicknessProperty, value); 
    }

    /// <summary>
    /// Gets or sets the normalized progress of the indeterminate animation (0.0 to 1.0).
    /// </summary>
    public double AnimationProgress 
    { 
        get => GetValue(AnimationProgressProperty); 
        set => SetValue(AnimationProgressProperty, value); 
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateSpinningPseudoClass();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        PseudoClasses.Set(":spinning", false);
        AnimationProgress = 0;
    }

    private void UpdateSpinningPseudoClass()
    {
        bool spinning = IsActive && IsIndeterminate && VisualRoot != null;
        PseudoClasses.Set(":spinning", spinning);
        if (!spinning)
        {
            AnimationProgress = 0;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressRingVisual"/> class.
    /// </summary>
    public ProgressRingVisual()
    {
        // Create the infinite spinning animation.
        _spinAnimation = new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromSeconds(2.0),
            IterationCount = IterationCount.Infinite,
            Easing = new LinearEasing(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters = { new Setter(AnimationProgressProperty, 0.0) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters = { new Setter(AnimationProgressProperty, 1.0) }
                }
            }
        };

        // Register the animation via a style targeting the :spinning pseudo-class.
        // Avalonia's style system uses Apply() internally which correctly handles
        // infinite IterationCount (unlike RunAsync which throws for looping animations).
        Styles.Add(new Avalonia.Styling.Style(x => x.Is<ProgressRingVisual>().Class(":spinning"))
        {
            Animations = { _spinAnimation }
        });
    }

    /// <summary>
    /// Computes the arc size (sweep angle) based on animation progress.
    /// </summary>
    /// <param name="progress">The current normalized animation progress.</param>
    /// <returns>The sweep angle in degrees.</returns>
    private static double ComputeArcSize(double progress)
    {
        if (progress < 0.25)
        {
            // Expanding: 0 -> 180 over first quarter
            return 180.0 * (progress / 0.25);
        }
        else if (progress < 0.75)
        {
            // Hold at max size
            return 180.0;
        }
        else
        {
            // Contracting: 180 -> 0 over last quarter
            return 180.0 * ((1.0 - progress) / 0.25);
        }
    }

    /// <summary>
    /// Computes the arc position (center rotation) based on animation progress.
    /// </summary>
    /// <param name="progress">The current normalized animation progress.</param>
    /// <returns>The rotation angle in degrees.</returns>
    private static double ComputeArcPosition(double progress)
    {
        return 1080.0 * progress;
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        var center = new Point(bounds.Width / 2, bounds.Height / 2);
        var radius = (Math.Min(bounds.Width, bounds.Height) - StrokeThickness) / 2;
        if (radius <= 0) return;

        // Draw background track (only for determinate mode)
        if (Background != null && !IsIndeterminate)
        {
            var bgPen = new Pen(Background, StrokeThickness, lineCap: PenLineCap.Round);
            var bgGeometry = new EllipseGeometry(new Rect(
                center.X - radius, 
                center.Y - radius, 
                radius * 2, 
                radius * 2));
            context.DrawGeometry(null, bgPen, bgGeometry);
        }

        if (!IsActive || Foreground == null) return;

        var pen = new Pen(Foreground, StrokeThickness, lineCap: PenLineCap.Round);

        if (IsIndeterminate)
        {
            var progress = AnimationProgress;
            var arcSize = ComputeArcSize(progress);
            var arcPosition = ComputeArcPosition(progress);
            
            // The arc is centered at the position, so we offset by half the size
            var startAngle = -90.0 + arcPosition - (arcSize / 2.0);
            var sweepAngle = arcSize;

            DrawArc(context, pen, center, radius, startAngle, sweepAngle);
        }
        else
        {
            var val = Math.Clamp(Value, Minimum, Maximum);
            var range = Maximum - Minimum;
            var percent = range <= 0 ? 0 : (val - Minimum) / range;
            DrawArc(context, pen, center, radius, -90, percent * 360);
        }
    }

    private void DrawArc(DrawingContext context, Pen pen, Point center, double radius, 
        double startAngle, double sweepAngle)
    {
        // Skip rendering very small arcs
        if (sweepAngle < 0.5)
        {
            _cachedGeometry = null;
            return;
        }

        // Normalize angles for caching comparison
        var normalizedStart = startAngle % 360;
        
        // Check if we can reuse cached geometry
        if (_cachedGeometry != null &&
            Math.Abs(_cachedStartAngle - normalizedStart) < 0.1 &&
            Math.Abs(_cachedSweepAngle - sweepAngle) < 0.1 &&
            Math.Abs(_cachedRadius - radius) < 0.01)
        {
            context.DrawGeometry(null, pen, _cachedGeometry);
            return;
        }

        Geometry geometry;
        
        if (sweepAngle >= 359.9)
        {
            // Full circle
            geometry = new EllipseGeometry(new Rect(
                center.X - radius, 
                center.Y - radius, 
                radius * 2, 
                radius * 2));
        }
        else
        {
            // Arc
            var startRad = startAngle * (Math.PI / 180.0);
            var endRad = (startAngle + sweepAngle) * (Math.PI / 180.0);

            var startPoint = new Point(
                center.X + radius * Math.Cos(startRad), 
                center.Y + radius * Math.Sin(startRad));
            var endPoint = new Point(
                center.X + radius * Math.Cos(endRad), 
                center.Y + radius * Math.Sin(endRad));

            var streamGeometry = new StreamGeometry();
            using (var ctx = streamGeometry.Open())
            {
                ctx.BeginFigure(startPoint, false);
                ctx.ArcTo(
                    endPoint, 
                    new Size(radius, radius), 
                    0, 
                    sweepAngle > 180, 
                    SweepDirection.Clockwise);
            }
            geometry = streamGeometry;
        }

        context.DrawGeometry(null, pen, geometry);
        
        // Cache for next frame
        _cachedGeometry = geometry;
        _cachedStartAngle = normalizedStart;
        _cachedSweepAngle = sweepAngle;
        _cachedRadius = radius;
    }
}