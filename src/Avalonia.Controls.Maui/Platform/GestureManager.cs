using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaTopLevel = Avalonia.Controls.TopLevel;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Manages gesture recognizers for Avalonia.Controls.Maui platform
/// </summary>
internal class GestureManager : IDisposable
{
    private readonly IControlsView _view;
    private object? _containerView;
    private object? _platformView;
    private object? _handler;
    private bool _didHaveWindow;
    private bool _disposed;

    public bool IsConnected => _platformView != null && _handler != null;

    public GestureManager(IControlsView view)
    {
        _view = view;
        view.HandlerChanging += OnHandlerChanging;
        view.HandlerChanged += OnHandlerChanged;
        view.WindowChanged += OnWindowChanged;
        view.PlatformContainerViewChanged += OnPlatformContainerViewChanged;

        SetupGestureManager();
    }

    private void OnPlatformContainerViewChanged(object? sender, EventArgs e) =>
        SetupGestureManager();

    private void OnWindowChanged(object? sender, EventArgs e) =>
        SetupGestureManager();

    private void OnHandlerChanged(object? sender, EventArgs e) =>
        SetupGestureManager();

    private void OnHandlerChanging(object? sender, HandlerChangingEventArgs e) =>
        DisconnectGestures();

    private void DisconnectGestures()
    {
        if (_platformView is AvaloniaControl control)
        {
            UnsubscribeFromGestureEvents(control);
        }

        if (_view is View view && view.GetCompositeGestureRecognizers() is ObservableCollection<IGestureRecognizer> recognizers)
        {
            recognizers.CollectionChanged -= OnGestureRecognizersCollectionChanged;
        }

        _handler = null;
        _didHaveWindow = false;
        _containerView = null;
        _platformView = null;
    }

    private void SetupGestureManager()
    {
        var handler = _view.Handler;

        if (handler == null ||
            (_didHaveWindow && _view.Window == null))
        {
            DisconnectGestures();
            return;
        }

        if (_containerView != handler.ContainerView ||
            _platformView != handler.PlatformView ||
            _handler != handler)
        {
            DisconnectGestures();
        }

        // Already setup and watching the correct view
        if (IsConnected)
            return;

        if (handler.PlatformView is AvaloniaControl control)
        {
            _platformView = control;
            SubscribeToGestureEvents(control);
        }

        if (_view is View view && view.GetCompositeGestureRecognizers() is ObservableCollection<IGestureRecognizer> recognizers)
        {
            recognizers.CollectionChanged += OnGestureRecognizersCollectionChanged;
        }

        _handler = handler;
        _containerView = handler.ContainerView;
        _didHaveWindow = _view.Window != null;
    }

    private void OnGestureRecognizersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
    }

    private void SubscribeToGestureEvents(AvaloniaControl control)
    {
        control.AddHandler(Control.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerEnteredEvent, OnPointerEntered, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerExitedEvent, OnPointerExited, RoutingStrategies.Bubble);
    }

    private void UnsubscribeFromGestureEvents(AvaloniaControl control)
    {
        control.RemoveHandler(Control.PointerPressedEvent, OnPointerPressed);
        control.RemoveHandler(Control.PointerMovedEvent, OnPointerMoved);
        control.RemoveHandler(Control.PointerReleasedEvent, OnPointerReleased);
        control.RemoveHandler(InputElement.PointerEnteredEvent, OnPointerEntered);
        control.RemoveHandler(InputElement.PointerExitedEvent, OnPointerExited);
    }

    private CancellationTokenSource? _singleTapCts;
    private const int DoubleTapDelayMs = 300;

    // Pan gesture state
    private bool _isPanning;
    private Point _panStartPoint;
    private Visual? _panOriginVisual;
    private Visual? _panRootVisual;

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Skip if already handled by another GestureManager
        if (e.Handled)
            return;
        
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var point = e.GetPosition(sender as Visual);
        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        
        // Handle PointerGestureRecognizer.PointerPressed
        if (pointerRecognizers.Count > 0)
        {
            var args = GetPointerArgs(point);
            foreach (var recognizer in pointerRecognizers)
            {
                recognizer.SendPointerPressed(view, args.GetPosition, null, args.Buttons);
            }
        }

        int clickCount = e.ClickCount;

        // Handle pan & swipe gesture recognizers
        var panRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PanGestureRecognizer>().ToList();
        var swipeRecognizers = recognizers.OfType<Microsoft.Maui.Controls.SwipeGestureRecognizer>().ToList();
        
        if (panRecognizers.Count > 0 || swipeRecognizers.Count > 0)
        {
            // Only set Handled if we have recognizers that need to capture input
            e.Handled = true;
            
            _isPanning = true;
            _panOriginVisual = sender as Visual;
            // Use TopLevel for coordinates
            _panRootVisual = (_panOriginVisual as AvaloniaControl) != null 
                ? AvaloniaTopLevel.GetTopLevel(_panOriginVisual as AvaloniaControl) 
                : _panOriginVisual;
            if (_panRootVisual == null) _panRootVisual = _panOriginVisual;
            _panStartPoint = e.GetPosition(_panRootVisual);
            
            if (sender is IInputElement inputElement)
            {
                e.Pointer.Capture(inputElement);
            }
            
            foreach (var recognizer in panRecognizers)
            {
                if (recognizer is IPanGestureController controller)
                    controller.SendPanStarted(view, 0);
            }
        }

        // Handle tap gesture recognizers
        var tapRecognizers = recognizers.OfType<TapGestureRecognizer>().ToList();
        if (tapRecognizers.Count == 0)
            return;

        // Separate recognizers by type
        var singleTapRecognizers = tapRecognizers.Where(r => r.NumberOfTapsRequired == 1).ToList();
        var doubleTapRecognizers = tapRecognizers.Where(r => r.NumberOfTapsRequired == 2).ToList();

        bool hasSingleTap = singleTapRecognizers.Count > 0;
        bool hasDoubleTap = doubleTapRecognizers.Count > 0;

        // Only single-tap: fire immediately
        if (hasSingleTap && !hasDoubleTap)
        {
            foreach (var recognizer in singleTapRecognizers)
                recognizer.SendTapped(view, GetPositionFunc(point));
            e.Handled = true;
            return;
        }

        // Only double-tap: fire when ClickCount >= 2
        if (hasDoubleTap && !hasSingleTap)
        {
            if (clickCount >= 2)
            {
                foreach (var recognizer in doubleTapRecognizers)
                    recognizer.SendTapped(view, GetPositionFunc(point));
                e.Handled = true;
            }
            return;
        }

        // Both single and double-tap: delay single-tap to detect double-tap
        if (clickCount >= 2)
        {
            _singleTapCts?.Cancel();
            _singleTapCts = null;
            
            foreach (var recognizer in doubleTapRecognizers)
                recognizer.SendTapped(view, GetPositionFunc(point));
            e.Handled = true;
        }
        else if (clickCount == 1)
        {
            _singleTapCts?.Cancel();
            _singleTapCts = new CancellationTokenSource();
            var cts = _singleTapCts;
            var capturedPoint = point;
            var capturedView = view;
            var capturedRecognizers = singleTapRecognizers.ToList();
            
            _ = Task.Delay(DoubleTapDelayMs, cts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    Threading.Dispatcher.UIThread.Post(() =>
                    {
                        foreach (var recognizer in capturedRecognizers)
                        {
                            recognizer.SendTapped(capturedView, GetPositionFunc(capturedPoint));
                        }
                    });
                }
            }, TaskScheduler.Default);
        }
    }

    private void OnPointerMoved(object? sender, Input.PointerEventArgs e)
    {
        if (e.Handled)
            return;

        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        // 1. Handle PointerGestureRecognizer (Always)
        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        if (pointerRecognizers.Count > 0)
        {
             var point = e.GetPosition(sender as Visual);
             var args = GetPointerArgs(point);
             foreach (var recognizer in pointerRecognizers)
             {
                 recognizer.SendPointerMoved(view, args.GetPosition, null, args.Buttons);
             }
        }

        // 2. Handle PanGestureRecognizer (Only if panning)
        if (!_isPanning)
            return;

        // For Pan, we strictly check origin
        if (sender as Visual != _panOriginVisual)
            return;

        var panRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PanGestureRecognizer>().ToList();
        if (panRecognizers.Count == 0)
            return;

        var panPoint = e.GetPosition(_panRootVisual);
        double totalX = panPoint.X - _panStartPoint.X;
        double totalY = panPoint.Y - _panStartPoint.Y;

        foreach (var recognizer in panRecognizers)
        {
            if (recognizer is IPanGestureController controller)
                controller.SendPan(view, totalX, totalY, 0);
        }
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Handled)
            return;

        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        // 1. Handle PointerGestureRecognizer (Always)
        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        if (pointerRecognizers.Count > 0)
        {
             var point = e.GetPosition(sender as Visual);
             var args = GetPointerArgs(point);
             foreach (var recognizer in pointerRecognizers)
             {
                 recognizer.SendPointerReleased(view, args.GetPosition, null, args.Buttons);
             }
        }

        // 2. Handle PanGestureRecognizer (Only if panning)
        if (!_isPanning)
            return;

        if (sender as Visual != _panOriginVisual)
            return;

        _isPanning = false;
        
        // Release pointer capture
        e.Pointer.Capture(null);
        _panOriginVisual = null;
        _panRootVisual = null;

        var panRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PanGestureRecognizer>().ToList();
        foreach (var recognizer in panRecognizers)
        {
            if (recognizer is IPanGestureController controller)
                controller.SendPanCompleted(view, 0);
        }

        var swipeRecognizers = recognizers.OfType<SwipeGestureRecognizer>().ToList();
        if (swipeRecognizers.Count > 0)
        {
            var releasedPoint = e.GetPosition(_panRootVisual);
            double totalX = releasedPoint.X - _panStartPoint.X;
            double totalY = releasedPoint.Y - _panStartPoint.Y;

            foreach (var recognizer in swipeRecognizers)
            {
                // Check if horizontal or vertical
                bool isHorizontal = Math.Abs(totalX) > Math.Abs(totalY);
                double threshold = recognizer.Threshold; // Default is usually 100
                
                // If Threshold is 0, use a reasonable default.
                if (threshold <= 0) threshold = 48;

                Microsoft.Maui.SwipeDirection? detectedDirection = null;

                if (isHorizontal)
                {
                    if (Math.Abs(totalX) > threshold)
                    {
                        if (totalX > 0) detectedDirection = Microsoft.Maui.SwipeDirection.Right;
                        else detectedDirection = Microsoft.Maui.SwipeDirection.Left;
                    }
                }
                else
                {
                    if (Math.Abs(totalY) > threshold)
                    {
                        if (totalY > 0) detectedDirection = Microsoft.Maui.SwipeDirection.Down;
                        else detectedDirection = Microsoft.Maui.SwipeDirection.Up;
                    }
                }

                if (detectedDirection.HasValue)
                {
                    if ((recognizer.Direction & detectedDirection.Value) == detectedDirection.Value)
                    {
                        recognizer.SendSwiped(view, detectedDirection.Value);
                    }
                }
            }
        }
        e.Handled = true;
    }

    private void OnPointerEntered(object? sender, Input.PointerEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        if (pointerRecognizers.Count == 0)
            return;

        var point = e.GetPosition(sender as Visual);
        var args = GetPointerArgs(point);

        foreach (var recognizer in pointerRecognizers)
        {
            recognizer.SendPointerEntered(view, args.GetPosition, null, args.Buttons);
        }
        e.Handled = true;
    }

    private void OnPointerExited(object? sender, Input.PointerEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        if (pointerRecognizers.Count == 0)
            return;

        var point = e.GetPosition(sender as Visual);
        var args = GetPointerArgs(point);

        foreach (var recognizer in pointerRecognizers)
        {
            recognizer.SendPointerExited(view, args.GetPosition, null, args.Buttons);
        }
        e.Handled = true;
    }
    
    private (Func<Microsoft.Maui.IElement?, Microsoft.Maui.Graphics.Point?> GetPosition, ButtonsMask Buttons) GetPointerArgs(Point point)
    {
        return (GetPositionFunc(point), (ButtonsMask)1);
    }


    private static Func<Microsoft.Maui.IElement?, Microsoft.Maui.Graphics.Point?> GetPositionFunc(Point point)
    {
        return (relativeTo) => new Microsoft.Maui.Graphics.Point(point.X, point.Y);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        DisconnectGestures();

        if (_view != null)
        {
            _view.HandlerChanging -= OnHandlerChanging;
            _view.HandlerChanged -= OnHandlerChanged;
            _view.WindowChanged -= OnWindowChanged;
            _view.PlatformContainerViewChanged -= OnPlatformContainerViewChanged;
        }
    }
}