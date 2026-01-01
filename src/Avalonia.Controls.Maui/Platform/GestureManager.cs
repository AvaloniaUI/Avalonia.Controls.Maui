using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaPointerEventArgs = Avalonia.Input.PointerEventArgs;
using MauiPinchGestureRecognizer = Microsoft.Maui.Controls.PinchGestureRecognizer;
using MauiPanGestureRecognizer = Microsoft.Maui.Controls.PanGestureRecognizer;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Manages gesture recognizers for Avalonia.Controls.Maui platform.
/// </summary>
internal class GestureManager : IDisposable
{
    private readonly IControlsView _view;
    private object? _containerView;
    private object? _platformView;
    private object? _handler;
    private bool _didHaveWindow;
    private bool _disposed;

    // Tap gesture tracking
    private DateTime? _lastTapTime;
    private global::Avalonia.Point? _lastTapPosition;
    private const double DoubleTapThresholdMs = 300;
    private const double DoubleTapDistanceThreshold = 10;

    // Pan gesture tracking
    private bool _isPanning;
    private global::Avalonia.Point _panStartPoint;
    private int _currentPanGestureId;
    private static int _nextPanGestureId;
    private IPointer? _panPointer;
    private long _panPointerId;

    // Swipe gesture tracking
    private global::Avalonia.Point _swipeStartPoint;
    private DateTime _swipeStartTime;
    private bool _isSwipeTracking;

    // Pinch gesture tracking
    private readonly Dictionary<long, global::Avalonia.Point> _activeTouchPoints = new();
    private double _initialPinchDistance;
    private global::Avalonia.Point _initialPinchCenter;
    private bool _isPinching;


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

        ResetGestureState();

        _handler = null;
        _didHaveWindow = false;
        _containerView = null;
        _platformView = null;
    }

    private void ResetGestureState()
    {
        _isPanning = false;
        _panPointer = null;
        _isSwipeTracking = false;
        _isPinching = false;
        _activeTouchPoints.Clear();
    }

    private void SetupGestureManager()
    {
        var handler = _view.Handler;

        if (handler == null ||
            (_didHaveWindow && (_view as VisualElement)?.Window == null))
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

        // The connected Gesture Manager is already setup and watching the correct view
        if (IsConnected)
        {
            return;
        }

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
        _didHaveWindow = (_view as VisualElement)?.Window != null;
    }

    private void OnGestureRecognizersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Gesture recognizers collection changed, no action needed for Avalonia
        // as we handle all gestures through event subscriptions
    }

    private void SubscribeToGestureEvents(AvaloniaControl control)
    {
        // Use Tunnel routing to catch events before children handle them
        // Add handledEventsToo: true to ensure we receive events even after children mark them as handled
        control.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        control.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        control.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        control.AddHandler(InputElement.PointerEnteredEvent, OnPointerEntered, RoutingStrategies.Direct);
        control.AddHandler(InputElement.PointerExitedEvent, OnPointerExited, RoutingStrategies.Direct);
        control.AddHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Direct);
    }

    private void UnsubscribeFromGestureEvents(AvaloniaControl control)
    {
        control.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        control.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        control.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        control.RemoveHandler(InputElement.PointerEnteredEvent, OnPointerEntered);
        control.RemoveHandler(InputElement.PointerExitedEvent, OnPointerExited);
        control.RemoveHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var visual = sender as global::Avalonia.Visual;
        var point = e.GetPosition(visual);
        var buttonMask = GetButtonMask(e);

        // Handle PointerGestureRecognizer - PointerPressed
        HandlePointerPressed(view, recognizers, point, e, buttonMask);

        // Handle touch tracking for pinch
        if (e.Pointer.Type == PointerType.Touch)
        {
            var pointerId = e.Pointer.Id;
            _activeTouchPoints[pointerId] = point;

            if (_activeTouchPoints.Count == 2)
            {
                StartPinchGesture(view, recognizers);
            }
        }

        // Handle TapGestureRecognizer
        HandleTapGesture(view, recognizers, point, e, buttonMask);

        // Start Pan/Swipe tracking
        StartPanSwipeTracking(view, recognizers, point, e);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
        {
            ResetGestureState();
            return;
        }

        var visual = sender as global::Avalonia.Visual;
        var point = e.GetPosition(visual);
        var buttonMask = GetButtonMaskFromRelease(e);

        // Handle PointerGestureRecognizer - PointerReleased
        HandlePointerReleased(view, recognizers, point, e, buttonMask);

        // Handle touch release for pinch
        if (e.Pointer.Type == PointerType.Touch)
        {
            var pointerId = e.Pointer.Id;
            if (_activeTouchPoints.Remove(pointerId) && _isPinching)
            {
                EndPinchGesture(view, recognizers);
            }
        }

        // End Pan gesture - use pointer ID for comparison to be robust
        if (_isPanning && e.Pointer.Id == _panPointerId)
        {
            EndPanGesture(view, recognizers);
        }

        // Detect Swipe gesture
        if (_isSwipeTracking)
        {
            DetectSwipe(view, recognizers, point);
            _isSwipeTracking = false;
        }
    }

    private void OnPointerMoved(object? sender, AvaloniaPointerEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var visual = sender as global::Avalonia.Visual;
        var point = e.GetPosition(visual);
        var buttonMask = GetCurrentButtonMask(e);

        // Handle PointerGestureRecognizer - PointerMoved
        HandlePointerMoved(view, recognizers, point, e, buttonMask);

        // Handle touch move for pinch
        if (e.Pointer.Type == PointerType.Touch && _activeTouchPoints.ContainsKey(e.Pointer.Id))
        {
            _activeTouchPoints[e.Pointer.Id] = point;

            if (_isPinching && _activeTouchPoints.Count == 2)
            {
                UpdatePinchGesture(view, recognizers);
            }
        }

        // Handle Pan gesture - use pointer ID for comparison to be robust
        if (_isPanning && e.Pointer.Id == _panPointerId)
        {
            UpdatePanGesture(view, recognizers, point);
        }
    }

    private void OnPointerEntered(object? sender, AvaloniaPointerEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var visual = sender as global::Avalonia.Visual;
        var point = e.GetPosition(visual);
        var buttonMask = GetCurrentButtonMask(e);

        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        foreach (var recognizer in pointerRecognizers)
        {
            if ((recognizer.Buttons & buttonMask) != 0 || buttonMask == ButtonsMask.Primary)
            {
                recognizer.SendPointerEntered(view, GetPositionFunc(point), null, buttonMask);
            }
        }
    }

    private void OnPointerExited(object? sender, AvaloniaPointerEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var visual = sender as global::Avalonia.Visual;
        var point = e.GetPosition(visual);
        var buttonMask = GetCurrentButtonMask(e);

        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        foreach (var recognizer in pointerRecognizers)
        {
            if ((recognizer.Buttons & buttonMask) != 0 || buttonMask == ButtonsMask.Primary)
            {
                recognizer.SendPointerExited(view, GetPositionFunc(point), null, buttonMask);
            }
        }
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();

        // Cancel any ongoing gestures
        if (_isPanning && recognizers != null)
        {
            CancelPanGesture(view, recognizers);
        }

        if (_isPinching && recognizers != null)
        {
            CancelPinchGesture(view, recognizers);
        }

        ResetGestureState();
    }

    #region Tap Gesture

    private void HandleTapGesture(View view, IList<IGestureRecognizer> recognizers, global::Avalonia.Point point, PointerPressedEventArgs e, ButtonsMask buttonMask)
    {
        var tapRecognizers = recognizers.OfType<TapGestureRecognizer>().ToList();
        if (tapRecognizers.Count == 0)
            return;

        var now = DateTime.Now;

        // Determine if this is a double tap
        int tapCount = 1;
        if (_lastTapTime != null && _lastTapPosition != null)
        {
            var timeSinceLastTap = (now - _lastTapTime.Value).TotalMilliseconds;
            var distanceFromLastTap = Math.Sqrt(
                Math.Pow(point.X - _lastTapPosition.Value.X, 2) +
                Math.Pow(point.Y - _lastTapPosition.Value.Y, 2));

            if (timeSinceLastTap <= DoubleTapThresholdMs && distanceFromLastTap <= DoubleTapDistanceThreshold)
            {
                tapCount = 2;
                _lastTapTime = null;
                _lastTapPosition = null;
            }
            else
            {
                _lastTapTime = now;
                _lastTapPosition = point;
            }
        }
        else
        {
            _lastTapTime = now;
            _lastTapPosition = point;
        }

        // Find matching recognizers
        foreach (var recognizer in tapRecognizers)
        {
            if (recognizer.NumberOfTapsRequired == tapCount &&
                (recognizer.Buttons & buttonMask) != 0)
            {
                recognizer.SendTapped(view, GetPositionFunc(point));
                e.Handled = true;
            }
        }
    }

    #endregion

    #region Pan Gesture

    private void StartPanSwipeTracking(View view, IList<IGestureRecognizer> recognizers, global::Avalonia.Point point, PointerPressedEventArgs e)
    {
        // Guard against starting a new pan/swipe if one is already in progress
        if (_isPanning || _isSwipeTracking)
            return;

        var panRecognizers = recognizers.OfType<MauiPanGestureRecognizer>().ToList();
        var swipeRecognizers = recognizers.OfType<SwipeGestureRecognizer>().ToList();

        if (panRecognizers.Count == 0 && swipeRecognizers.Count == 0)
            return;

        // Start tracking for both pan and swipe
        _panStartPoint = point;
        _swipeStartPoint = point;
        _swipeStartTime = DateTime.Now;
        _isSwipeTracking = swipeRecognizers.Count > 0;

        if (panRecognizers.Count > 0)
        {
            _isPanning = true;
            _panPointer = e.Pointer;
            _panPointerId = e.Pointer.Id;
            _currentPanGestureId = ++_nextPanGestureId;

            // Capture pointer for pan gesture
            if (_platformView is AvaloniaControl control)
            {
                e.Pointer.Capture(control);
            }

            // Send PanStarted
            foreach (var recognizer in panRecognizers)
            {
                if (recognizer is IPanGestureController controller)
                {
                    controller.SendPanStarted(view as Element ?? (Element)_view, _currentPanGestureId);
                }
            }
        }
    }

    private void UpdatePanGesture(View view, IList<IGestureRecognizer> recognizers, global::Avalonia.Point currentPoint)
    {
        var panRecognizers = recognizers.OfType<MauiPanGestureRecognizer>().ToList();
        if (panRecognizers.Count == 0)
            return;

        double totalX = currentPoint.X - _panStartPoint.X;
        double totalY = currentPoint.Y - _panStartPoint.Y;

        foreach (var recognizer in panRecognizers)
        {
            if (recognizer is IPanGestureController controller)
            {
                controller.SendPan(view as Element ?? (Element)_view, totalX, totalY, _currentPanGestureId);
            }
        }
    }

    private void EndPanGesture(View view, IList<IGestureRecognizer> recognizers)
    {
        var panRecognizers = recognizers.OfType<MauiPanGestureRecognizer>().ToList();

        foreach (var recognizer in panRecognizers)
        {
            if (recognizer is IPanGestureController controller)
            {
                controller.SendPanCompleted(view as Element ?? (Element)_view, _currentPanGestureId);
            }
        }

        _isPanning = false;
        _panPointer = null;
    }

    private void CancelPanGesture(View view, IList<IGestureRecognizer> recognizers)
    {
        var panRecognizers = recognizers.OfType<MauiPanGestureRecognizer>().ToList();

        foreach (var recognizer in panRecognizers)
        {
            if (recognizer is IPanGestureController controller)
            {
                controller.SendPanCanceled(view as Element ?? (Element)_view, _currentPanGestureId);
            }
        }

        _isPanning = false;
        _panPointer = null;
    }

    #endregion

    #region Swipe Gesture

    private void DetectSwipe(View view, IList<IGestureRecognizer> recognizers, global::Avalonia.Point endPoint)
    {
        var swipeRecognizers = recognizers.OfType<SwipeGestureRecognizer>().ToList();
        if (swipeRecognizers.Count == 0)
            return;

        double totalX = endPoint.X - _swipeStartPoint.X;
        double totalY = endPoint.Y - _swipeStartPoint.Y;

        foreach (var recognizer in swipeRecognizers)
        {
            if (recognizer is ISwipeGestureController controller)
            {
                controller.SendSwipe(view as Element ?? (Element)_view, totalX, totalY);
                controller.DetectSwipe(view, recognizer.Direction);
            }
        }
    }

    #endregion

    #region Pinch Gesture

    private void StartPinchGesture(View view, IList<IGestureRecognizer> recognizers)
    {
        var pinchRecognizers = recognizers.OfType<MauiPinchGestureRecognizer>().ToList();
        if (pinchRecognizers.Count == 0)
            return;

        var points = _activeTouchPoints.Values.ToArray();
        if (points.Length < 2)
            return;

        _initialPinchDistance = GetDistance(points[0], points[1]);
        _initialPinchCenter = GetMidpoint(points[0], points[1]);
        _isPinching = true;

        var scalePoint = new Microsoft.Maui.Graphics.Point(_initialPinchCenter.X, _initialPinchCenter.Y);

        foreach (var recognizer in pinchRecognizers)
        {
            if (recognizer is IPinchGestureController controller)
            {
                controller.SendPinchStarted(view as Element ?? (Element)_view, scalePoint);
            }
        }
    }

    private void UpdatePinchGesture(View view, IList<IGestureRecognizer> recognizers)
    {
        var pinchRecognizers = recognizers.OfType<MauiPinchGestureRecognizer>().ToList();
        if (pinchRecognizers.Count == 0 || !_isPinching)
            return;

        var points = _activeTouchPoints.Values.ToArray();
        if (points.Length < 2)
            return;

        var currentDistance = GetDistance(points[0], points[1]);
        var currentCenter = GetMidpoint(points[0], points[1]);

        double scale = _initialPinchDistance > 0 ? currentDistance / _initialPinchDistance : 1;
        var scalePoint = new Microsoft.Maui.Graphics.Point(currentCenter.X, currentCenter.Y);

        foreach (var recognizer in pinchRecognizers)
        {
            if (recognizer is IPinchGestureController controller)
            {
                controller.SendPinch(view as Element ?? (Element)_view, scale, scalePoint);
            }
        }
    }

    private void EndPinchGesture(View view, IList<IGestureRecognizer> recognizers)
    {
        var pinchRecognizers = recognizers.OfType<MauiPinchGestureRecognizer>().ToList();

        foreach (var recognizer in pinchRecognizers)
        {
            if (recognizer is IPinchGestureController controller)
            {
                controller.SendPinchEnded(view as Element ?? (Element)_view);
            }
        }

        _isPinching = false;
    }

    private void CancelPinchGesture(View view, IList<IGestureRecognizer> recognizers)
    {
        var pinchRecognizers = recognizers.OfType<MauiPinchGestureRecognizer>().ToList();

        foreach (var recognizer in pinchRecognizers)
        {
            if (recognizer is IPinchGestureController controller)
            {
                controller.SendPinchCanceled(view as Element ?? (Element)_view);
            }
        }

        _isPinching = false;
    }

    private static double GetDistance(global::Avalonia.Point p1, global::Avalonia.Point p2)
    {
        return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
    }

    private static global::Avalonia.Point GetMidpoint(global::Avalonia.Point p1, global::Avalonia.Point p2)
    {
        return new global::Avalonia.Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
    }

    #endregion

    #region Pointer Gesture

    private void HandlePointerPressed(View view, IList<IGestureRecognizer> recognizers, global::Avalonia.Point point, PointerPressedEventArgs e, ButtonsMask buttonMask)
    {
        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();

        foreach (var recognizer in pointerRecognizers)
        {
            if ((recognizer.Buttons & buttonMask) != 0)
            {
                recognizer.SendPointerPressed(view, GetPositionFunc(point), null, buttonMask);
            }
        }
    }

    private void HandlePointerReleased(View view, IList<IGestureRecognizer> recognizers, global::Avalonia.Point point, PointerReleasedEventArgs e, ButtonsMask buttonMask)
    {
        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();

        foreach (var recognizer in pointerRecognizers)
        {
            if ((recognizer.Buttons & buttonMask) != 0)
            {
                recognizer.SendPointerReleased(view, GetPositionFunc(point), null, buttonMask);
            }
        }
    }

    private void HandlePointerMoved(View view, IList<IGestureRecognizer> recognizers, global::Avalonia.Point point, AvaloniaPointerEventArgs e, ButtonsMask buttonMask)
    {
        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();

        foreach (var recognizer in pointerRecognizers)
        {
            if ((recognizer.Buttons & buttonMask) != 0 || buttonMask == ButtonsMask.Primary)
            {
                recognizer.SendPointerMoved(view, GetPositionFunc(point), null, buttonMask);
            }
        }
    }

    #endregion

    #region Helper Methods

    private static Func<Microsoft.Maui.IElement?, Microsoft.Maui.Graphics.Point?> GetPositionFunc(global::Avalonia.Point point)
    {
        return (relativeTo) => new Microsoft.Maui.Graphics.Point(point.X, point.Y);
    }

    private static ButtonsMask GetButtonMask(PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(null).Properties;

        if (props.IsRightButtonPressed)
            return ButtonsMask.Secondary;
        // Left button and middle button both map to Primary
        return ButtonsMask.Primary;
    }

    private static ButtonsMask GetButtonMaskFromRelease(PointerReleasedEventArgs e)
    {
        return e.InitialPressMouseButton switch
        {
            MouseButton.Right => ButtonsMask.Secondary,
            // Left, Middle, and other buttons map to Primary
            _ => ButtonsMask.Primary
        };
    }

    private static ButtonsMask GetCurrentButtonMask(AvaloniaPointerEventArgs e)
    {
        var props = e.GetCurrentPoint(null).Properties;

        // MAUI only supports Primary and Secondary buttons
        if (props.IsRightButtonPressed)
            return ButtonsMask.Secondary;

        // Left button, middle button, or no buttons pressed all map to Primary
        return ButtonsMask.Primary;
    }

    #endregion

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
