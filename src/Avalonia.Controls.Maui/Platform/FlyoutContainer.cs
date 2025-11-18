using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Animation;
using Avalonia.Styling;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Custom flyout container that manages flyout/detail layout without using SplitView
/// </summary>
public class FlyoutContainer : Panel
{
    private Control? _flyoutContent;
    private Control? _detailContent;
    private Panel? _scrim;
    private TranslateTransform? _flyoutTransform;
    private bool _isAnimating;

    public static readonly StyledProperty<bool> IsFlyoutOpenProperty =
        AvaloniaProperty.Register<FlyoutContainer, bool>(nameof(IsFlyoutOpen), false);

    public static readonly StyledProperty<double> FlyoutWidthProperty =
        AvaloniaProperty.Register<FlyoutContainer, double>(nameof(FlyoutWidth), 320);

    public static readonly StyledProperty<FlyoutBehavior> FlyoutBehaviorProperty =
        AvaloniaProperty.Register<FlyoutContainer, FlyoutBehavior>(nameof(FlyoutBehavior), FlyoutBehavior.Flyout);

    public static readonly StyledProperty<bool> IsGestureEnabledProperty =
        AvaloniaProperty.Register<FlyoutContainer, bool>(nameof(IsGestureEnabled), true);

    public bool IsFlyoutOpen
    {
        get => GetValue(IsFlyoutOpenProperty);
        set => SetValue(IsFlyoutOpenProperty, value);
    }

    public double FlyoutWidth
    {
        get => GetValue(FlyoutWidthProperty);
        set => SetValue(FlyoutWidthProperty, value);
    }

    public FlyoutBehavior FlyoutBehavior
    {
        get => GetValue(FlyoutBehaviorProperty);
        set => SetValue(FlyoutBehaviorProperty, value);
    }

    public bool IsGestureEnabled
    {
        get => GetValue(IsGestureEnabledProperty);
        set => SetValue(IsGestureEnabledProperty, value);
    }

    public event EventHandler? FlyoutOpened;
    public event EventHandler? FlyoutClosed;

    private Point? _gestureStartPoint;
    private double _gestureStartOffset;

    public FlyoutContainer()
    {
        // Create scrim (overlay that dims the detail content when flyout is open)
        _scrim = new Panel
        {
            Background = new SolidColorBrush(Colors.Black, 0.5),
            IsVisible = false,
            ZIndex = 1
        };
        _scrim.PointerPressed += OnScrimPressed;
        Children.Add(_scrim);

        // Setup property change handlers
        IsFlyoutOpenProperty.Changed.AddClassHandler<FlyoutContainer>((x, e) => x.OnIsFlyoutOpenChanged(e));
        FlyoutBehaviorProperty.Changed.AddClassHandler<FlyoutContainer>((x, e) => x.OnFlyoutBehaviorChanged(e));
    }

    public void SetFlyoutContent(Control? content)
    {
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

            // Add gesture support
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

    public void SetDetailContent(Control? content)
    {
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

            // Add gesture support for swiping from edge to open
            if (IsGestureEnabled)
            {
                _detailContent.PointerPressed += OnDetailPointerPressed;
                _detailContent.PointerMoved += OnDetailPointerMoved;
                _detailContent.PointerReleased += OnDetailPointerReleased;
            }

            Children.Add(_detailContent);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var flyoutWidth = FlyoutWidth;
        var behavior = FlyoutBehavior;

        // Measure detail content (always full size)
        _detailContent?.Measure(availableSize);

        // Measure scrim (always full size)
        _scrim?.Measure(availableSize);

        // Measure flyout content
        if (_flyoutContent != null)
        {
            var flyoutSize = new Size(flyoutWidth, availableSize.Height);
            _flyoutContent.Measure(flyoutSize);
        }

        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var flyoutWidth = FlyoutWidth;
        var behavior = FlyoutBehavior;

        // Arrange detail content (always full size)
        if (_detailContent != null)
        {
            _detailContent.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        }

        // Arrange scrim (always full size, but visibility controlled separately)
        if (_scrim != null)
        {
            _scrim.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        }

        // Arrange flyout content
        if (_flyoutContent != null)
        {
            var flyoutRect = new Rect(0, 0, flyoutWidth, finalSize.Height);
            _flyoutContent.Arrange(flyoutRect);
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
        UpdateFlyoutPosition(false);
    }

    private void UpdateFlyoutPosition(bool animate)
    {
        if (_flyoutContent == null || _flyoutTransform == null)
            return;

        var behavior = FlyoutBehavior;
        var isOpen = IsFlyoutOpen;
        var flyoutWidth = FlyoutWidth;

        double targetX;
        bool showScrim;

        switch (behavior)
        {
            case FlyoutBehavior.Disabled:
                targetX = -flyoutWidth;
                showScrim = false;
                break;

            case FlyoutBehavior.Flyout:
                targetX = isOpen ? 0 : -flyoutWidth;
                showScrim = isOpen;
                break;

            case FlyoutBehavior.Locked:
                targetX = 0;
                showScrim = false;
                break;

            default:
                targetX = -flyoutWidth;
                showScrim = false;
                break;
        }

        if (animate && !_isAnimating)
        {
            AnimateToPosition(targetX, showScrim);
        }
        else
        {
            _flyoutTransform.X = targetX;
            if (_scrim != null)
            {
                _scrim.IsVisible = showScrim;
            }
        }
    }

    private async void AnimateToPosition(double targetX, bool showScrim)
    {
        if (_flyoutTransform == null || _isAnimating)
            return;

        _isAnimating = true;

        var startX = _flyoutTransform.X;
        var duration = TimeSpan.FromMilliseconds(250);
        var startTime = DateTime.Now;

        // Show scrim immediately if opening
        if (showScrim && _scrim != null)
        {
            _scrim.IsVisible = true;
            _scrim.Opacity = 0;
        }

        while (DateTime.Now - startTime < duration)
        {
            var progress = (DateTime.Now - startTime).TotalMilliseconds / duration.TotalMilliseconds;
            progress = EaseOut(progress);

            _flyoutTransform.X = startX + (targetX - startX) * progress;

            if (_scrim != null)
            {
                _scrim.Opacity = showScrim ? progress : (1 - progress);
            }

            await System.Threading.Tasks.Task.Delay(16); // ~60fps
        }

        _flyoutTransform.X = targetX;

        if (_scrim != null)
        {
            _scrim.IsVisible = showScrim;
            _scrim.Opacity = showScrim ? 1 : 0;
        }

        _isAnimating = false;
    }

    private double EaseOut(double t)
    {
        return 1 - Math.Pow(1 - t, 3);
    }

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        // Close flyout when scrim is clicked
        if (IsFlyoutOpen && FlyoutBehavior == FlyoutBehavior.Flyout)
        {
            IsFlyoutOpen = false;
        }
    }

    // Gesture handling for detail content (swipe from left edge to open)
    private void OnDetailPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsGestureEnabled || FlyoutBehavior != FlyoutBehavior.Flyout)
            return;

        var point = e.GetPosition(_detailContent);

        // Only start gesture if press is near left edge (within 50 pixels)
        if (point.X <= 50)
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

        // Only allow dragging to the right
        if (deltaX > 0 && _flyoutTransform != null)
        {
            var newX = Math.Max(-FlyoutWidth, Math.Min(0, _gestureStartOffset + deltaX));
            _flyoutTransform.X = newX;

            // Update scrim opacity based on position
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

        // If dragged more than halfway or fast swipe, open the flyout
        if (deltaX > FlyoutWidth / 3)
        {
            IsFlyoutOpen = true;
        }
        else
        {
            // Snap back closed
            UpdateFlyoutPosition(true);
        }

        _gestureStartPoint = null;
    }

    // Gesture handling for flyout content (swipe left to close)
    private void OnFlyoutPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsGestureEnabled || FlyoutBehavior != FlyoutBehavior.Flyout)
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

        // Only allow dragging to the left
        if (deltaX < 0 && _flyoutTransform != null)
        {
            var newX = Math.Max(-FlyoutWidth, Math.Min(0, _gestureStartOffset + deltaX));
            _flyoutTransform.X = newX;

            // Update scrim opacity based on position
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

        // If dragged more than halfway or fast swipe, close the flyout
        if (Math.Abs(deltaX) > FlyoutWidth / 3)
        {
            IsFlyoutOpen = false;
        }
        else
        {
            // Snap back open
            UpdateFlyoutPosition(true);
        }

        _gestureStartPoint = null;
    }
}

public enum FlyoutBehavior
{
    Disabled = 0,
    Flyout = 1,
    Locked = 2
}
