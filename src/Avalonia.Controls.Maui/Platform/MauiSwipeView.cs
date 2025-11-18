using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// HACK: This is a simplified implementation of MAUI's SwipeView for Avalonia.
/// It's here as a placeholder. It doesn't "work" fully, but provides basic structure.
/// </summary>
public class MauiSwipeView : TemplatedControl
{
    private ContentPresenter? _contentPresenter;
    private Panel? _swipeContainer;
    private Point? _gestureStartPoint;
    private bool _isGestureInProgress;
    private Panel? _leftItemsPanel;
    private Panel? _rightItemsPanel;
    private Panel? _topItemsPanel;
    private Panel? _bottomItemsPanel;

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<MauiSwipeView>();

    public static readonly StyledProperty<double> ThresholdProperty =
        AvaloniaProperty.Register<MauiSwipeView, double>(nameof(Threshold), defaultValue: 50.0);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MauiSwipeView, bool>(nameof(IsOpen), defaultValue: false);

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public double Threshold
    {
        get => GetValue(ThresholdProperty);
        set => SetValue(ThresholdProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public event EventHandler<SwipeEventArgs>? SwipeStarted;
    public event EventHandler<SwipeEventArgs>? SwipeChanging;
    public event EventHandler<SwipeEventArgs>? SwipeEnded;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        _swipeContainer = e.NameScope.Find<Panel>("PART_SwipeContainer");
        _leftItemsPanel = e.NameScope.Find<Panel>("PART_LeftItems");
        _rightItemsPanel = e.NameScope.Find<Panel>("PART_RightItems");
        _topItemsPanel = e.NameScope.Find<Panel>("PART_TopItems");
        _bottomItemsPanel = e.NameScope.Find<Panel>("PART_BottomItems");

        if (_contentPresenter != null)
        {
            _contentPresenter.PointerPressed += OnPointerPressed;
            _contentPresenter.PointerMoved += OnPointerMoved;
            _contentPresenter.PointerReleased += OnPointerReleased;
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        if (!properties.IsLeftButtonPressed)
            return;

        _gestureStartPoint = e.GetPosition(this);
        _isGestureInProgress = true;

        SwipeStarted?.Invoke(this, new SwipeEventArgs(_gestureStartPoint.Value));
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isGestureInProgress || _gestureStartPoint == null)
            return;

        var currentPoint = e.GetPosition(this);
        SwipeChanging?.Invoke(this, new SwipeEventArgs(currentPoint));
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isGestureInProgress || _gestureStartPoint == null)
            return;

        var currentPoint = e.GetPosition(this);
        var deltaX = currentPoint.X - _gestureStartPoint.Value.X;
        var deltaY = currentPoint.Y - _gestureStartPoint.Value.Y;

        // Determine swipe direction based on the larger delta
        if (Math.Abs(deltaX) > Math.Abs(deltaY))
        {
            // Horizontal swipe
            if (Math.Abs(deltaX) >= Threshold)
            {
                if (deltaX > 0)
                {
                    // Swiped right - show left items
                    ShowLeftItems();
                }
                else
                {
                    // Swiped left - show right items
                    ShowRightItems();
                }
            }
        }
        else
        {
            // Vertical swipe
            if (Math.Abs(deltaY) >= Threshold)
            {
                if (deltaY > 0)
                {
                    // Swiped down - show top items
                    ShowTopItems();
                }
                else
                {
                    // Swiped up - show bottom items
                    ShowBottomItems();
                }
            }
        }

        SwipeEnded?.Invoke(this, new SwipeEventArgs(currentPoint));

        _isGestureInProgress = false;
        _gestureStartPoint = null;
    }

    private void ShowLeftItems()
    {
        HideAllItems();
        if (_leftItemsPanel != null)
        {
            _leftItemsPanel.IsVisible = true;
            IsOpen = true;
        }
    }

    private void ShowRightItems()
    {
        HideAllItems();
        if (_rightItemsPanel != null)
        {
            _rightItemsPanel.IsVisible = true;
            IsOpen = true;
        }
    }

    private void ShowTopItems()
    {
        HideAllItems();
        if (_topItemsPanel != null)
        {
            _topItemsPanel.IsVisible = true;
            IsOpen = true;
        }
    }

    private void ShowBottomItems()
    {
        HideAllItems();
        if (_bottomItemsPanel != null)
        {
            _bottomItemsPanel.IsVisible = true;
            IsOpen = true;
        }
    }

    public void HideAllItems()
    {
        if (_leftItemsPanel != null) _leftItemsPanel.IsVisible = false;
        if (_rightItemsPanel != null) _rightItemsPanel.IsVisible = false;
        if (_topItemsPanel != null) _topItemsPanel.IsVisible = false;
        if (_bottomItemsPanel != null) _bottomItemsPanel.IsVisible = false;
        IsOpen = false;
    }

    public void RequestOpen(SwipeDirection direction)
    {
        switch (direction)
        {
            case SwipeDirection.Left:
                ShowRightItems();
                break;
            case SwipeDirection.Right:
                ShowLeftItems();
                break;
            case SwipeDirection.Up:
                ShowBottomItems();
                break;
            case SwipeDirection.Down:
                ShowTopItems();
                break;
        }
    }

    public void RequestClose()
    {
        HideAllItems();
    }
}

public class SwipeEventArgs : EventArgs
{
    public Point Position { get; }

    public SwipeEventArgs(Point position)
    {
        Position = position;
    }
}

public enum SwipeDirection
{
    Left,
    Right,
    Up,
    Down
}
