using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Visual component for rendering the <see cref="ProgressRing"/> animation.
/// This is a template part that can be replaced for custom visualizations.
/// </summary>
public class ProgressRingVisual : Control
{
    private Stopwatch? _animationStopwatch;
    private DispatcherTimer? _animationTimer;
    private double _currentRotation;
    private double _currentArcSize;
    
    private Geometry? _cachedGeometry;
    private double _cachedValue = double.NaN;
    private double _cachedRadius = double.NaN;
    private bool _cachedIsRtl;

    static ProgressRingVisual()
    {
        AffectsRender<ProgressRingVisual>(
            RingForegroundProperty,
            RingBackgroundProperty,
            IsIndeterminateProperty,
            ValueProperty,
            MinimumProperty,
            MaximumProperty,
            StrokeThicknessProperty,
            AnimationDurationProperty,
            StartAngleProperty,
            FlowDirectionProperty);
    }

    /// <summary>
    /// Defines the <see cref="RingForeground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> RingForegroundProperty =
        AvaloniaProperty.Register<ProgressRingVisual, IBrush?>(nameof(RingForeground));

    /// <summary>
    /// Defines the <see cref="RingBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> RingBackgroundProperty =
        AvaloniaProperty.Register<ProgressRingVisual, IBrush?>(nameof(RingBackground));

    /// <summary>
    /// Defines the <see cref="IsIndeterminate"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<ProgressRingVisual, bool>(nameof(IsIndeterminate), defaultValue: true);

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
        AvaloniaProperty.Register<ProgressRingVisual, double>(
            nameof(StrokeThickness), 
            defaultValue: 4.0,
            validate: v => v > 0);

    /// <summary>
    /// Defines the <see cref="AnimationDuration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<ProgressRingVisual, TimeSpan>(
            nameof(AnimationDuration),
            defaultValue: TimeSpan.FromSeconds(2),
            validate: v => v > TimeSpan.Zero);

    /// <summary>
    /// Defines the <see cref="StartAngle"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<ProgressRingVisual, double>(nameof(StartAngle), defaultValue: -90.0);

    /// <summary>
    /// Gets or sets the foreground brush for the progress arc.
    /// </summary>
    public IBrush? RingForeground
    {
        get => GetValue(RingForegroundProperty);
        set => SetValue(RingForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush for the track arc.
    /// </summary>
    public IBrush? RingBackground
    {
        get => GetValue(RingBackgroundProperty);
        set => SetValue(RingBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the visual shows indeterminate progress.
    /// </summary>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>
    /// Gets or sets the current value for determinate mode.
    /// </summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke thickness in pixels.
    /// </summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the animation duration for indeterminate mode.
    /// </summary>
    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets the starting angle in degrees.
    /// Default is -90 (top of circle, 12 o'clock position).
    /// </summary>
    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    private bool IsRightToLeft => FlowDirection == FlowDirection.RightToLeft;

    public ProgressRingVisual()
    {
        ClipToBounds = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsIndeterminateProperty)
        {
            _cachedGeometry = null;
            UpdateAnimation();
        }
        else if (change.Property == ValueProperty ||
                 change.Property == MinimumProperty ||
                 change.Property == MaximumProperty ||
                 change.Property == FlowDirectionProperty)
        {
            _cachedGeometry = null;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateAnimation();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopAnimation();
    }

    private void UpdateAnimation()
    {
        if (IsIndeterminate && VisualRoot != null)
            StartAnimation();
        else
            StopAnimation();
    }

    private void StartAnimation()
    {
        if (_animationTimer != null)
            return;

        _animationStopwatch = Stopwatch.StartNew();
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _animationTimer.Tick += OnAnimationTick;
        _animationTimer.Start();
    }

    private void StopAnimation()
    {
        _animationTimer?.Stop();
        if (_animationTimer != null)
        {
            _animationTimer.Tick -= OnAnimationTick;
            _animationTimer = null;
        }
        _animationStopwatch?.Stop();
        _animationStopwatch = null;
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        if (_animationStopwatch == null)
            return;

        var elapsed = _animationStopwatch.Elapsed;
        var progress = (elapsed.TotalSeconds % AnimationDuration.TotalSeconds) / AnimationDuration.TotalSeconds;

        if (progress < 0.25)
        {
            var t = progress / 0.25;
            _currentArcSize = 180 * (1 - Math.Pow(1 - t, 3));
        }
        else if (progress >= 0.75)
        {
            var t = (progress - 0.75) / 0.25;
            _currentArcSize = 180 * (1 - Math.Pow(t, 3));
        }
        else
        {
            _currentArcSize = 180;
        }

        var baseRotation = 1080 * progress;
        _currentRotation = IsRightToLeft ? -baseRotation : baseRotation;

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var size = Math.Min(bounds.Width, bounds.Height);
        var center = new Point(bounds.Width / 2, bounds.Height / 2);
        var radius = (size - StrokeThickness) / 2;

        if (radius <= 0 || RingForeground == null)
            return;

        var pen = new Pen(RingForeground, StrokeThickness, lineCap: PenLineCap.Round);

        if (RingBackground != null && !IsIndeterminate)
        {
            var bgPen = new Pen(RingBackground, StrokeThickness, lineCap: PenLineCap.Round);
            var bgGeometry = CreateArcGeometry(center, radius, 0, 360);
            context.DrawGeometry(null, bgPen, bgGeometry);
        }

        if (IsIndeterminate)
        {
            RenderIndeterminate(context, center, radius, pen);
        }
        else
        {
            RenderDeterminate(context, center, radius, pen);
        }
    }

    /// <summary>
    /// Renders the animated indeterminate arc.
    /// Virtual to allow custom animation strategies.
    /// </summary>
    protected virtual void RenderIndeterminate(DrawingContext context, Point center, double radius, Pen pen)
    {
        var angleRadians = _currentRotation * Math.PI / 180;
        var transform = Matrix.CreateTranslation(-center.X, -center.Y) *
                       Matrix.CreateRotation(angleRadians) *
                       Matrix.CreateTranslation(center.X, center.Y);

        using (context.PushTransform(transform))
        {
            var halfArc = _currentArcSize / 2;
            var geometry = CreateArcGeometry(center, radius, -halfArc, halfArc);
            context.DrawGeometry(null, pen, geometry);
        }
    }

    /// <summary>
    /// Renders the static determinate arc with geometry caching.
    /// Virtual to allow custom rendering strategies.
    /// </summary>
    protected virtual void RenderDeterminate(DrawingContext context, Point center, double radius, Pen pen)
    {
        var progress = Math.Clamp((Value - Minimum) / (Maximum - Minimum), 0, 1);
        var angle = progress * 360;

        if (IsRightToLeft)
            angle = -angle;

        if (_cachedGeometry != null &&
            Math.Abs(_cachedValue - Value) < 0.001 &&
            Math.Abs(_cachedRadius - radius) < 0.001 &&
            _cachedIsRtl == IsRightToLeft)
        {
            context.DrawGeometry(null, pen, _cachedGeometry);
        }
        else
        {
            var geometry = CreateArcGeometry(center, radius, 0, angle);
            _cachedGeometry = geometry;
            _cachedValue = Value;
            _cachedRadius = radius;
            _cachedIsRtl = IsRightToLeft;
            context.DrawGeometry(null, pen, geometry);
        }
    }

    /// <summary>
    /// Creates arc geometry for rendering.
    /// Virtual to allow custom shapes (e.g., square, hexagon).
    /// </summary>
    protected virtual Geometry CreateArcGeometry(Point center, double radius, double startAngle, double endAngle)
    {
        var sweepAngle = endAngle - startAngle;

        if (Math.Abs(sweepAngle) < 0.01)
            return new StreamGeometry();

        if (Math.Abs(sweepAngle) >= 359.9)
            return CreateFullCircleGeometry(center, radius);

        var isNegativeSweep = sweepAngle < 0;
        var absSweep = Math.Abs(sweepAngle);

        var adjustedStart = (startAngle + StartAngle) * Math.PI / 180;
        var adjustedEnd = (endAngle + StartAngle) * Math.PI / 180;

        var startPoint = new Point(
            center.X + radius * Math.Cos(adjustedStart),
            center.Y + radius * Math.Sin(adjustedStart));

        var endPoint = new Point(
            center.X + radius * Math.Cos(adjustedEnd),
            center.Y + radius * Math.Sin(adjustedEnd));

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(startPoint, false);
            ctx.ArcTo(
                endPoint,
                new Size(radius, radius),
                0,
                absSweep > 180,
                isNegativeSweep ? SweepDirection.CounterClockwise : SweepDirection.Clockwise);
        }

        return geometry;
    }

    /// <summary>
    /// Creates a full circle geometry using two semicircles.
    /// Virtual to allow alternative full-circle strategies.
    /// </summary>
    protected virtual Geometry CreateFullCircleGeometry(Point center, double radius)
    {
        var adjustedStart = StartAngle * Math.PI / 180;

        var startPoint = new Point(
            center.X + radius * Math.Cos(adjustedStart),
            center.Y + radius * Math.Sin(adjustedStart));

        var midPoint = new Point(
            center.X + radius * Math.Cos(adjustedStart + Math.PI),
            center.Y + radius * Math.Sin(adjustedStart + Math.PI));

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(startPoint, false);
            ctx.ArcTo(midPoint, new Size(radius, radius), 0, false, SweepDirection.Clockwise);
            ctx.ArcTo(startPoint, new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        }

        return geometry;
    }
}