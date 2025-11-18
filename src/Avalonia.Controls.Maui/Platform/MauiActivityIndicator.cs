using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Custom activity indicator control that displays an animated circular progress indicator
/// HACK: This is a stub control. It probably doesn't "work".
/// </summary>
public class MauiActivityIndicator : Control
{
    private DispatcherTimer? _animationTimer;
    private double _rotationAngle = 0;
    private const double RotationSpeed = 10; // degrees per tick
    private const int AnimationInterval = 16; // ~60fps

    public static readonly StyledProperty<bool> IsRunningProperty =
        AvaloniaProperty.Register<MauiActivityIndicator, bool>(nameof(IsRunning), defaultValue: true);

    public static readonly StyledProperty<IBrush?> ColorProperty =
        AvaloniaProperty.Register<MauiActivityIndicator, IBrush?>(nameof(Color));

    static MauiActivityIndicator()
    {
        IsRunningProperty.Changed.AddClassHandler<MauiActivityIndicator>((indicator, e) => indicator.OnIsRunningChanged());
        AffectsRender<MauiActivityIndicator>(ColorProperty, IsRunningProperty);
    }

    public MauiActivityIndicator()
    {
        Width = 40;
        Height = 40;
        Color = new SolidColorBrush(Colors.Gray);
    }

    public bool IsRunning
    {
        get => GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public IBrush? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    private void OnIsRunningChanged()
    {
        if (IsRunning)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (IsRunning)
        {
            StartAnimation();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopAnimation();
    }

    private void StartAnimation()
    {
        if (_animationTimer != null)
            return;

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(AnimationInterval)
        };
        _animationTimer.Tick += OnAnimationTick;
        _animationTimer.Start();
    }

    private void StopAnimation()
    {
        if (_animationTimer != null)
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= OnAnimationTick;
            _animationTimer = null;
        }
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        _rotationAngle = (_rotationAngle + RotationSpeed) % 360;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!IsRunning || Color == null)
            return;

        var bounds = Bounds;
        var centerX = bounds.Width / 2;
        var centerY = bounds.Height / 2;
        var radius = Math.Min(centerX, centerY) - 2;

        if (radius <= 0)
            return;

        // Draw spinning arc
        var pen = new Pen(Color, 3, lineCap: PenLineCap.Round);

        // Create arc geometry
        var startAngle = _rotationAngle;
        var sweepAngle = 270; // 3/4 circle

        // Convert to radians
        var startRad = startAngle * Math.PI / 180;
        var endRad = (startAngle + sweepAngle) * Math.PI / 180;

        // Calculate start and end points
        var startPoint = new Point(
            centerX + radius * Math.Cos(startRad),
            centerY + radius * Math.Sin(startRad)
        );

        var endPoint = new Point(
            centerX + radius * Math.Cos(endRad),
            centerY + radius * Math.Sin(endRad)
        );

        // Create arc segment
        var arcSegment = new ArcSegment
        {
            Point = endPoint,
            Size = new Size(radius, radius),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = sweepAngle > 180
        };

        var pathFigure = new PathFigure
        {
            StartPoint = startPoint,
            IsClosed = false
        };
        pathFigure.Segments!.Add(arcSegment);

        var pathGeometry = new PathGeometry();
        pathGeometry.Figures!.Add(pathFigure);

        context.DrawGeometry(null, pen, pathGeometry);
    }
}
