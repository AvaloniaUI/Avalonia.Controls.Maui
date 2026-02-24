using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace Avalonia.Controls.Maui.Controls.Shell;

/// <summary>
/// Custom flyout container that manages flyout/detail layout without using SplitView
/// </summary>
public class FlyoutContainer : Panel
{
    private Control? _flyoutContent;
    private Control? _detailContent;
    private Panel? _scrim;
    private TranslateTransform? _flyoutTransform;
    private DispatcherTimer? _animationTimer;
    private Stopwatch? _animStopwatch;
    private double _animStartX;
    private double _animTargetX;
    private bool _animShowScrim;

    public const double DefaultFlyoutWidth = 320;
    internal const double GestureEdgeThreshold = 50.0;
    internal const double GestureVelocityRatio = 1.0 / 3.0;
    internal static readonly TimeSpan DefaultTransitionDuration = TimeSpan.FromMilliseconds(250);
    
    public Control? DetailContent => _detailContent;
    
    public static readonly StyledProperty<bool> IsFlyoutOpenProperty =
        AvaloniaProperty.Register<FlyoutContainer, bool>(nameof(IsFlyoutOpen), false);
    
    public static readonly StyledProperty<double> FlyoutWidthProperty =
        AvaloniaProperty.Register<FlyoutContainer, double>(nameof(FlyoutWidth), DefaultFlyoutWidth);
    
    public static readonly StyledProperty<FlyoutBehavior> FlyoutBehaviorProperty =
        AvaloniaProperty.Register<FlyoutContainer, FlyoutBehavior>(nameof(FlyoutBehavior), FlyoutBehavior.Default);
    
    public static readonly StyledProperty<double> FlyoutHeightProperty =
        AvaloniaProperty.Register<FlyoutContainer, double>(nameof(FlyoutHeight), -1);
    
    public static readonly StyledProperty<IBrush?> FlyoutBackdropProperty =
        AvaloniaProperty.Register<FlyoutContainer, IBrush?>(nameof(FlyoutBackdrop));
    
    public static readonly StyledProperty<bool> IsGestureEnabledProperty =
        AvaloniaProperty.Register<FlyoutContainer, bool>(nameof(IsGestureEnabled), true);

    /// <summary>
    /// Gets or sets a value indicating whether the flyout is currently open.
    /// </summary>
    public bool IsFlyoutOpen
    {
        get => GetValue(IsFlyoutOpenProperty);
        set => SetValue(IsFlyoutOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the flyout.
    /// </summary>
    public double FlyoutWidth
    {
        get => GetValue(FlyoutWidthProperty);
        set => SetValue(FlyoutWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the flyout behavior.
    /// </summary>
    public FlyoutBehavior FlyoutBehavior
    {
        get => GetValue(FlyoutBehaviorProperty);
        set => SetValue(FlyoutBehaviorProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the flyout.
    /// </summary>
    public double FlyoutHeight
    {
        get => GetValue(FlyoutHeightProperty);
        set => SetValue(FlyoutHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used for the flyout backdrop.
    /// </summary>
    public IBrush? FlyoutBackdrop
    {
        get => GetValue(FlyoutBackdropProperty);
        set => SetValue(FlyoutBackdropProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gestures are enabled.
    /// </summary>
    public bool IsGestureEnabled
    {
        get => GetValue(IsGestureEnabledProperty);
        set => SetValue(IsGestureEnabledProperty, value);
    }

    /// <summary>
    /// Occurs when the flyout is opened.
    /// </summary>
    public event EventHandler? FlyoutOpened;

    /// <summary>
    /// Occurs when the flyout is closed.
    /// </summary>
    public event EventHandler? FlyoutClosed;

    private Point? _gestureStartPoint;
    private double _gestureStartOffset;
    private bool _isLandscape;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlyoutContainer"/> class.
    /// </summary>
    public FlyoutContainer()
    {
        _scrim = new Panel
        {
            Background = new SolidColorBrush(Colors.Black, 0.5),
            IsVisible = false,
            ZIndex = 1
        };
        _scrim.PointerPressed += OnScrimPressed;
        Children.Add(_scrim);

        IsFlyoutOpenProperty.Changed.AddClassHandler<FlyoutContainer>((x, e) => x.OnIsFlyoutOpenChanged(e));
        FlyoutBehaviorProperty.Changed.AddClassHandler<FlyoutContainer>((x, e) => x.OnFlyoutBehaviorChanged(e));
        FlyoutBackdropProperty.Changed.AddClassHandler<FlyoutContainer>((x, e) => x.OnFlyoutBackdropChanged(e));
    }

    private void OnFlyoutBackdropChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_scrim == null) return;

        if (e.NewValue is IBrush backdrop)
        {
            _scrim.Background = backdrop;
        }
        else
        {
            _scrim.Background = new SolidColorBrush(Colors.Black, 0.5);
        }
    }

    /// <summary>
    /// Sets the content of the flyout.
    /// </summary>
    /// <param name="content">The flyout content.</param>
    public void SetFlyoutContent(Control? content)
    {
        if (content != null && content == _flyoutContent)
            return;

        if (_flyoutContent != null)
        {
            Children.Remove(_flyoutContent);
            _flyoutContent.PointerPressed -= OnFlyoutPointerPressed;
            _flyoutContent.PointerMoved -= OnFlyoutPointerMoved;
            _flyoutContent.PointerReleased -= OnFlyoutPointerReleased;
        }

        _flyoutContent = content;

        if (_flyoutContent != null)
        {
            _flyoutTransform = new TranslateTransform();
            _flyoutContent.RenderTransform = _flyoutTransform;
            _flyoutContent.ZIndex = 2;

            if (IsGestureEnabled)
            {
                _flyoutContent.PointerPressed += OnFlyoutPointerPressed;
                _flyoutContent.PointerMoved += OnFlyoutPointerMoved;
                _flyoutContent.PointerReleased += OnFlyoutPointerReleased;
            }

            Children.Add(_flyoutContent);
            UpdateFlyoutPosition(false);
        }
    }

    /// <summary>
    /// Sets the content of the detail area.
    /// </summary>
    /// <param name="content">The detail content.</param>
    public void SetDetailContent(Control? content)
    {
        if (content != null && content == _detailContent)
            return;

        if (_detailContent != null)
        {
            Children.Remove(_detailContent);
            _detailContent.PointerPressed -= OnDetailPointerPressed;
            _detailContent.PointerMoved -= OnDetailPointerMoved;
            _detailContent.PointerReleased -= OnDetailPointerReleased;
        }

        _detailContent = content;

        if (_detailContent != null)
        {
            _detailContent.ZIndex = 0;

            if (IsGestureEnabled)
            {
                _detailContent.PointerPressed += OnDetailPointerPressed;
                _detailContent.PointerMoved += OnDetailPointerMoved;
                _detailContent.PointerReleased += OnDetailPointerReleased;
            }

            Children.Add(_detailContent);
        }
    }

    /// <summary>
    /// Determines if the current behavior and orientation use split mode
    /// </summary>
    private bool IsSplitMode()
    {
        var behavior = FlyoutBehavior;

        return behavior switch
        {
            FlyoutBehavior.Split => true,
            FlyoutBehavior.Locked => true,
            FlyoutBehavior.SplitOnLandscape => _isLandscape,
            FlyoutBehavior.SplitOnPortrait => !_isLandscape,
            FlyoutBehavior.Default => _isLandscape, // Default behavior: split on landscape
            _ => false
        };
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var flyoutWidth = FlyoutWidth;
        var flyoutHeight = FlyoutHeight > 0 ? FlyoutHeight : availableSize.Height;
        var isSplitMode = IsSplitMode();

        _isLandscape = availableSize.Width > availableSize.Height;

        if (isSplitMode)
        {
            var detailWidth = Math.Max(0, availableSize.Width - flyoutWidth);
            _detailContent?.Measure(new Size(detailWidth, availableSize.Height));
            _scrim?.Measure(new Size(0, 0));

            if (_flyoutContent != null)
            {
                _flyoutContent.Measure(new Size(flyoutWidth, flyoutHeight));
            }
        }
        else
        {
            _detailContent?.Measure(availableSize);
            _scrim?.Measure(availableSize);

            if (_flyoutContent != null)
            {
                _flyoutContent.Measure(new Size(flyoutWidth, flyoutHeight));
            }
        }

        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var flyoutWidth = FlyoutWidth;
        var flyoutHeight = FlyoutHeight > 0 ? FlyoutHeight : finalSize.Height;
        var isSplitMode = IsSplitMode();

        if (isSplitMode)
        {
            var detailWidth = Math.Max(0, finalSize.Width - flyoutWidth);

            _flyoutContent?.Arrange(new Rect(0, 0, flyoutWidth, flyoutHeight));
            _detailContent?.Arrange(new Rect(flyoutWidth, 0, detailWidth, finalSize.Height));

            if (_scrim != null)
            {
                _scrim.Arrange(new Rect(0, 0, 0, 0));
                _scrim.IsVisible = false;
            }
        }
        else
        {
            _detailContent?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            _scrim?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            _flyoutContent?.Arrange(new Rect(0, 0, flyoutWidth, flyoutHeight));
        }

        return finalSize;
    }

    private void OnIsFlyoutOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool isOpen)
        {
            UpdateFlyoutPosition(true);

            if (isOpen)
            {
                FlyoutOpened?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                FlyoutClosed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void OnFlyoutBehaviorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        InvalidateMeasure();
        InvalidateArrange();
        UpdateFlyoutPosition(false);
    }

    private void UpdateFlyoutPosition(bool animate)
    {
        if (_flyoutContent == null || _flyoutTransform == null)
            return;

        var behavior = FlyoutBehavior;
        var isOpen = IsFlyoutOpen;
        var flyoutWidth = FlyoutWidth;
        var isSplitMode = IsSplitMode();

        double targetX;
        bool showScrim;

        if (isSplitMode)
        {
            targetX = 0;
            showScrim = false;
            _flyoutContent.IsVisible = true;
        }
        else
        {
            switch (behavior)
            {
                case FlyoutBehavior.Disabled:
                    targetX = -flyoutWidth;
                    showScrim = false;
                    break;

                default:
                    targetX = isOpen ? 0 : -flyoutWidth;
                    showScrim = isOpen;
                    break;
            }

            if (targetX > -flyoutWidth)
            {
                _flyoutContent.IsVisible = true;
            }
        }

        if (animate)
        {
            AnimateToPosition(targetX, showScrim);
        }
        else
        {
            CancelAnimation();
            _flyoutTransform.X = targetX;
            if (_scrim != null)
            {
                _scrim.IsVisible = showScrim;
            }

            if (!isSplitMode && targetX <= -flyoutWidth)
            {
                _flyoutContent.IsVisible = false;
            }
        }
    }

    private void AnimateToPosition(double targetX, bool showScrim)
    {
        if (_flyoutTransform == null)
            return;

        // Cancel any running animation and start fresh from current position
        CancelAnimation();

        _animStartX = _flyoutTransform.X;
        _animTargetX = targetX;
        _animShowScrim = showScrim;

        if (showScrim && _scrim != null)
        {
            _scrim.IsVisible = true;
            _scrim.Opacity = 0;
        }

        _animStopwatch = Stopwatch.StartNew();
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _animationTimer.Tick += OnAnimationTick;
        _animationTimer.Start();
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        if (_flyoutTransform == null || _animStopwatch == null)
        {
            CompleteAnimation();
            return;
        }

        var elapsedMs = _animStopwatch.Elapsed.TotalMilliseconds;
        var durationMs = DefaultTransitionDuration.TotalMilliseconds;

        if (elapsedMs >= durationMs)
        {
            CompleteAnimation();
            return;
        }

        var progress = EaseOut(elapsedMs / durationMs);
        _flyoutTransform.X = _animStartX + (_animTargetX - _animStartX) * progress;

        if (_scrim != null)
        {
            _scrim.Opacity = _animShowScrim ? progress : (1 - progress);
        }
    }

    private void CompleteAnimation()
    {
        if (_flyoutTransform != null)
        {
            _flyoutTransform.X = _animTargetX;
        }

        if (_scrim != null)
        {
            _scrim.IsVisible = _animShowScrim;
            _scrim.Opacity = _animShowScrim ? 1 : 0;
        }

        if (!IsSplitMode() && _animTargetX <= -FlyoutWidth && _flyoutContent != null)
        {
            _flyoutContent.IsVisible = false;
        }

        CancelAnimation();
    }

    private void CancelAnimation()
    {
        if (_animationTimer != null)
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= OnAnimationTick;
            _animationTimer = null;
        }

        _animStopwatch?.Stop();
        _animStopwatch = null;
    }

    private static double EaseOut(double t)
    {
        return 1 - Math.Pow(1 - t, 3);
    }

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (IsFlyoutOpen && !IsSplitMode())
        {
            IsFlyoutOpen = false;
        }
    }

    private void OnDetailPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsGestureEnabled || IsSplitMode())
            return;

        var point = e.GetPosition(_detailContent);

        if (point.X <= GestureEdgeThreshold)
        {
            _gestureStartPoint = point;
            _gestureStartOffset = _flyoutTransform?.X ?? -FlyoutWidth;
        }
    }

    private void OnDetailPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_gestureStartPoint == null || !IsGestureEnabled)
            return;

        var currentPoint = e.GetPosition(_detailContent);
        var deltaX = currentPoint.X - _gestureStartPoint.Value.X;

        if (deltaX > 0 && _flyoutTransform != null)
        {
            var newX = Math.Max(-FlyoutWidth, Math.Min(0, _gestureStartOffset + deltaX));
            _flyoutTransform.X = newX;

            if (_scrim != null)
            {
                var progress = (newX + FlyoutWidth) / FlyoutWidth;
                _scrim.Opacity = progress;
                _scrim.IsVisible = progress > 0;
            }
        }
    }

    private void OnDetailPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_gestureStartPoint == null || !IsGestureEnabled)
            return;

        var currentPoint = e.GetPosition(_detailContent);
        var deltaX = currentPoint.X - _gestureStartPoint.Value.X;

        if (deltaX > FlyoutWidth / 3)
        {
            IsFlyoutOpen = true;
        }
        else
        {
            UpdateFlyoutPosition(true);
        }

        _gestureStartPoint = null;
    }

    private void OnFlyoutPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsGestureEnabled || IsSplitMode())
            return;

        _gestureStartPoint = e.GetPosition(_flyoutContent);
        _gestureStartOffset = _flyoutTransform?.X ?? 0;
    }

    private void OnFlyoutPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_gestureStartPoint == null || !IsGestureEnabled)
            return;

        var currentPoint = e.GetPosition(_flyoutContent);
        var deltaX = currentPoint.X - _gestureStartPoint.Value.X;

        if (deltaX < 0 && _flyoutTransform != null)
        {
            var newX = Math.Max(-FlyoutWidth, Math.Min(0, _gestureStartOffset + deltaX));
            _flyoutTransform.X = newX;

            if (_scrim != null)
            {
                var progress = (newX + FlyoutWidth) / FlyoutWidth;
                _scrim.Opacity = progress;
                _scrim.IsVisible = progress > 0;
            }
        }
    }

    private void OnFlyoutPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_gestureStartPoint == null || !IsGestureEnabled)
            return;

        var currentPoint = e.GetPosition(_flyoutContent);
        var deltaX = currentPoint.X - _gestureStartPoint.Value.X;

        if (Math.Abs(deltaX) > FlyoutWidth * GestureVelocityRatio)
        {
            IsFlyoutOpen = false;
        }
        else
        {
            UpdateFlyoutPosition(true);
        }

        _gestureStartPoint = null;
    }
}

/// <summary>
/// Flyout behavior that matches MAUI's FlyoutLayoutBehavior enum values.
/// </summary>
public enum FlyoutBehavior
{
    /// <summary>
    /// Platform default behavior
    /// </summary>
    Default = 0,

    /// <summary>
    /// Split layout when in landscape orientation
    /// </summary>
    SplitOnLandscape = 1,

    /// <summary>
    /// Always show split layout (flyout on left, detail on right)
    /// </summary>
    Split = 2,

    /// <summary>
    /// Popover mode - flyout overlays detail page
    /// </summary>
    Popover = 3,

    /// <summary>
    /// Split layout when in portrait orientation
    /// </summary>
    SplitOnPortrait = 4,

    /// <summary>
    /// Flyout is always visible and locked open
    /// </summary>
    Locked = 6,

    /// <summary>
    /// Flyout is disabled
    /// </summary>
    Disabled = 7
}