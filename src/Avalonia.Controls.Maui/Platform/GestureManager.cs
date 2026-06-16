using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaDragEventArgs = Avalonia.Input.DragEventArgs;
using AvaloniaPinchGestureRecognizer = Avalonia.Input.PinchGestureRecognizer;
using AvaloniaTopLevel = Avalonia.Controls.TopLevel;
using AvaloniaTappedEventArgs = Avalonia.Input.TappedEventArgs;

namespace Avalonia.Controls.Maui.Platform;

internal class GestureManager : IDisposable
{
    private IControlsView? _view;
    private object? _containerView;
    private object? _platformView;
    private object? _handler;
    private bool _didHaveWindow;
    private bool _disposed;

    private ScrollGestureRecognizer? _scrollRecognizer;
    private Avalonia.Input.GestureRecognizers.SwipeGestureRecognizer? _swipeRecognizer;

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
            TearDownScrollSwipeRecognizers(control);
            TearDownDropHandlers(control);
            TearDownPinchHandlers(control);
        }

        if (_view is View view && view.GetCompositeGestureRecognizers() is ObservableCollection<IGestureRecognizer> recognizers)
        {
            recognizers.CollectionChanged -= OnGestureRecognizersCollectionChanged;
        }

        _handler = null;
        _didHaveWindow = false;
        _containerView = null;
        _platformView = null;

        _isDragPending = false;
        _dragPointerArgs = null;
        _isPanning = false;
        _isScrollActive = false;
    }

    private void SetupGestureManager()
    {
        if (_view == null)
            return;

        var handler = _view.Handler;

        if (handler == null ||
            (_didHaveWindow && _view.Window == null))
        {
            if (handler != null && _view.Window == null)
            {
                _didHaveWindow = false;
                return;
            }
            DisconnectGestures();
            return;
        }

        if (_containerView != handler.ContainerView ||
            _platformView != handler.PlatformView ||
            _handler != handler)
        {
            DisconnectGestures();
        }

        if (IsConnected)
            return;

        if (handler.PlatformView is AvaloniaControl control)
        {
            _platformView = control;
            SubscribeToGestureEvents(control);
            SetupScrollSwipeRecognizers(control);

            if (_view is View view)
            {
                SetupDropHandlersIfNeeded(control, view);
                SetupPinchHandlersIfNeeded(control, view);
            }
        }

        if (_view is View v && v.GetCompositeGestureRecognizers() is ObservableCollection<IGestureRecognizer> recognizers)
        {
            recognizers.CollectionChanged += OnGestureRecognizersCollectionChanged;
        }

        _handler = handler;
        _containerView = handler.ContainerView;
        _didHaveWindow = _view.Window != null;
    }

    private void OnGestureRecognizersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_platformView is not AvaloniaControl control || _view is not View view)
            return;

        bool hasDropRecognizers = view.GetCompositeGestureRecognizers()
            ?.OfType<DropGestureRecognizer>()
            .Any(r => r.AllowDrop) == true;

        if (hasDropRecognizers && !_isDropSubscribed)
            SetupDropHandlers(control);
        else if (!hasDropRecognizers && _isDropSubscribed)
            TearDownDropHandlers(control);

        bool hasPinchRecognizers = view.GetCompositeGestureRecognizers()
            ?.OfType<Microsoft.Maui.Controls.PinchGestureRecognizer>()
            .Any() == true;

        if (hasPinchRecognizers && !_isPinchSubscribed)
            SetupPinchHandlers(control);
        else if (!hasPinchRecognizers && _isPinchSubscribed)
            TearDownPinchHandlers(control);
    }

    // ── Subscriptions ──────────────────────────────────────────────────────

    private void SubscribeToGestureEvents(AvaloniaControl control)
    {
        // Tapped works for ALL pointer types (touch AND mouse) — scroll-safe
        control.AddHandler(InputElement.TappedEvent, OnTapped, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.DoubleTappedEvent, OnDoubleTapped, RoutingStrategies.Bubble);

        // Scroll & swipe events for touch (via Avalonia gesture recognizers)
        control.AddHandler(InputElement.ScrollGestureEvent, OnScrollGesture, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.ScrollGestureEndedEvent, OnScrollGestureEnded, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.SwipeGestureEvent, OnSwipe, RoutingStrategies.Bubble);

        // Manual pointer tracking for mouse pan/swipe (touch uses ScrollGestureRecognizer/SwipeGestureRecognizer instead)
        control.AddHandler(AvaloniaControl.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble);

        // PointerGestureRecognizer (no Avalonia equivalent)
        control.AddHandler(InputElement.PointerEnteredEvent, OnPointerEntered, RoutingStrategies.Direct | RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerExitedEvent, OnPointerExited, RoutingStrategies.Direct | RoutingStrategies.Bubble);
    }

    private void UnsubscribeFromGestureEvents(AvaloniaControl control)
    {
        control.RemoveHandler(InputElement.TappedEvent, OnTapped);
        control.RemoveHandler(InputElement.DoubleTappedEvent, OnDoubleTapped);
        control.RemoveHandler(InputElement.ScrollGestureEvent, OnScrollGesture);
        control.RemoveHandler(InputElement.ScrollGestureEndedEvent, OnScrollGestureEnded);
        control.RemoveHandler(InputElement.SwipeGestureEvent, OnSwipe);
        control.RemoveHandler(AvaloniaControl.PointerPressedEvent, OnPointerPressed);
        control.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        control.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        control.RemoveHandler(InputElement.PointerEnteredEvent, OnPointerEntered);
        control.RemoveHandler(InputElement.PointerExitedEvent, OnPointerExited);
    }

    private void SetupScrollSwipeRecognizers(AvaloniaControl control)
    {
        var recognizers = _view is View v ? v.GetCompositeGestureRecognizers() : null;
        if (recognizers == null) return;

        if (recognizers.OfType<Microsoft.Maui.Controls.PanGestureRecognizer>().Any())
        {
            _scrollRecognizer = new ScrollGestureRecognizer
            {
                CanHorizontallyScroll = true,
                CanVerticallyScroll = true,
            };
            control.GestureRecognizers.Add(_scrollRecognizer);
        }

        if (recognizers.OfType<Microsoft.Maui.Controls.SwipeGestureRecognizer>().Any())
        {
            _swipeRecognizer = new Avalonia.Input.GestureRecognizers.SwipeGestureRecognizer();
            control.GestureRecognizers.Add(_swipeRecognizer);
        }
    }

    private void TearDownScrollSwipeRecognizers(AvaloniaControl control)
    {
        if (_scrollRecognizer != null)
        {
            control.GestureRecognizers.Remove(_scrollRecognizer);
            _scrollRecognizer = null;
        }
        if (_swipeRecognizer != null)
        {
            control.GestureRecognizers.Remove(_swipeRecognizer);
            _swipeRecognizer = null;
        }
    }

    // ── Tap (Avalonia routed events — scroll-safe, works on all input types) ──

    private void OnTapped(object? sender, AvaloniaTappedEventArgs e)
    {
        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var point = e.GetPosition(sender as Visual);
        var singleTapRecognizers = recognizers.OfType<TapGestureRecognizer>()
            .Where(r => r.NumberOfTapsRequired == 1)
            .ToList();

        foreach (var recognizer in singleTapRecognizers)
            recognizer.SendTapped(view, GetPositionFunc(point));
    }

    private void OnDoubleTapped(object? sender, AvaloniaTappedEventArgs e)
    {
        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var point = e.GetPosition(sender as Visual);
        var doubleTapRecognizers = recognizers.OfType<TapGestureRecognizer>()
            .Where(r => r.NumberOfTapsRequired == 2)
            .ToList();

        foreach (var recognizer in doubleTapRecognizers)
            recognizer.SendTapped(view, GetPositionFunc(point));
    }

    // ── Touch pan/swipe (via Avalonia ScrollGestureRecognizer/SwipeGestureRecognizer) ──

    private bool _isScrollActive;
    private double _scrollTotalX;
    private double _scrollTotalY;

    private void OnScrollGesture(object? sender, ScrollGestureEventArgs e)
    {
        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var panRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PanGestureRecognizer>().ToList();
        if (panRecognizers.Count == 0) return;

        if (!_isScrollActive)
        {
            _isScrollActive = true;
            _scrollTotalX = 0;
            _scrollTotalY = 0;
            foreach (var recognizer in panRecognizers)
            {
                if (recognizer is IPanGestureController controller)
                    controller.SendPanStarted(view, 0);
            }
        }

        _scrollTotalX += e.Delta.X;
        _scrollTotalY += e.Delta.Y;

        foreach (var recognizer in panRecognizers)
        {
            if (recognizer is IPanGestureController controller)
                controller.SendPan(view, _scrollTotalX, _scrollTotalY, 0);
        }
    }

    private void OnScrollGestureEnded(object? sender, ScrollGestureEndedEventArgs e)
    {
        if (!_isScrollActive) return;
        _isScrollActive = false;

        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var panRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PanGestureRecognizer>().ToList();
        foreach (var recognizer in panRecognizers)
        {
            if (recognizer is IPanGestureController controller)
                controller.SendPanCompleted(view, 0);
        }
    }

    private void OnSwipe(object? sender, SwipeGestureEventArgs e)
    {
        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var swipeRecognizers = recognizers.OfType<Microsoft.Maui.Controls.SwipeGestureRecognizer>().ToList();
        if (swipeRecognizers.Count == 0) return;

        Microsoft.Maui.SwipeDirection direction = e.SwipeDirection switch
        {
            Avalonia.Input.SwipeDirection.Left => Microsoft.Maui.SwipeDirection.Left,
            Avalonia.Input.SwipeDirection.Right => Microsoft.Maui.SwipeDirection.Right,
            Avalonia.Input.SwipeDirection.Up => Microsoft.Maui.SwipeDirection.Up,
            Avalonia.Input.SwipeDirection.Down => Microsoft.Maui.SwipeDirection.Down,
            _ => Microsoft.Maui.SwipeDirection.Left,
        };

        foreach (var recognizer in swipeRecognizers)
        {
            if ((recognizer.Direction & direction) == direction)
                recognizer.SendSwiped(view, direction);
        }
    }

    // ── Mouse pan/swipe (manual pointer tracking — works on desktop) ──

    private bool _isPanning;
    private Point _panStartPoint;
    private Visual? _panOriginVisual;
    private Visual? _panRootVisual;

    private bool _isDragPending;
    private Point _dragStartPoint;
    private PointerPressedEventArgs? _dragPointerArgs;
    private const double DragThreshold = 5.0;

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled) return;

        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0) return;

        var point = e.GetPosition(sender as Visual);

        // PointerGestureRecognizer (always)
        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        if (pointerRecognizers.Count > 0)
        {
            var args = GetPointerArgs(point);
            foreach (var recognizer in pointerRecognizers)
                recognizer.SendPointerPressed(view, args.GetPosition, null, args.Buttons);
        }

        // DragGestureRecognizer (always)
        var dragRecognizers = recognizers.OfType<DragGestureRecognizer>()
            .Where(r => r.CanDrag).ToList();
        if (dragRecognizers.Count > 0)
        {
            _isDragPending = true;
            _dragStartPoint = point;
            _dragPointerArgs = e;
        }

        // Touch → skip manual pan/swipe (use ScrollGestureRecognizer/SwipeGestureRecognizer instead)
        if (e.Pointer.Type == PointerType.Touch)
            return;

        // Mouse → manual pan/swipe tracking with e.Handled
        var panRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PanGestureRecognizer>().ToList();
        var swipeRecognizers = recognizers.OfType<Microsoft.Maui.Controls.SwipeGestureRecognizer>().ToList();

        if (panRecognizers.Count > 0 || swipeRecognizers.Count > 0)
        {
            e.Handled = true;

            _isPanning = true;
            _panOriginVisual = sender as Visual;
            _panRootVisual = (_panOriginVisual as AvaloniaControl) != null
                ? AvaloniaTopLevel.GetTopLevel(_panOriginVisual as AvaloniaControl)
                : _panOriginVisual;
            if (_panRootVisual == null) _panRootVisual = _panOriginVisual;
            _panStartPoint = e.GetPosition(_panRootVisual);

            if (sender is IInputElement inputElement)
                e.Pointer.Capture(inputElement);

            foreach (var recognizer in panRecognizers)
            {
                if (recognizer is IPanGestureController controller)
                    controller.SendPanStarted(view, 0);
            }
        }
    }

    private void OnPointerMoved(object? sender, Input.PointerEventArgs e)
    {
        if (e.Handled) return;

        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0) return;

        // PointerGestureRecognizer
        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        if (pointerRecognizers.Count > 0)
        {
            var point = e.GetPosition(sender as Visual);
            var args = GetPointerArgs(point);
            foreach (var recognizer in pointerRecognizers)
                recognizer.SendPointerMoved(view, args.GetPosition, null, args.Buttons);
        }

        // Drag threshold
        if (_isDragPending && _dragPointerArgs != null)
        {
            var currentPoint = e.GetPosition(sender as Visual);
            double distance = Math.Sqrt(
                Math.Pow(currentPoint.X - _dragStartPoint.X, 2) +
                Math.Pow(currentPoint.Y - _dragStartPoint.Y, 2));

            if (distance >= DragThreshold)
            {
                _isDragPending = false;
                var dragArgs = _dragPointerArgs;
                _dragPointerArgs = null;
                _ = InitiateDragAsync(view, dragArgs, sender as Visual);
                e.Handled = true;
                return;
            }
        }

        // Mouse pan
        if (!_isPanning) return;
        if (sender as Visual != _panOriginVisual) return;

        var panRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PanGestureRecognizer>().ToList();
        if (panRecognizers.Count == 0) return;

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
        _isDragPending = false;
        _dragPointerArgs = null;

        if (e.Handled) return;

        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0) return;

        // PointerGestureRecognizer
        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        if (pointerRecognizers.Count > 0)
        {
            var point = e.GetPosition(sender as Visual);
            var args = GetPointerArgs(point);
            foreach (var recognizer in pointerRecognizers)
                recognizer.SendPointerReleased(view, args.GetPosition, null, args.Buttons);
        }

        // Mouse pan/swipe completion
        if (!_isPanning) return;
        if (sender as Visual != _panOriginVisual) return;

        _isPanning = false;
        e.Pointer.Capture(null);

        var panRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PanGestureRecognizer>().ToList();
        foreach (var recognizer in panRecognizers)
        {
            if (recognizer is IPanGestureController controller)
                controller.SendPanCompleted(view, 0);
        }

        var swipeRecognizers = recognizers.OfType<Microsoft.Maui.Controls.SwipeGestureRecognizer>().ToList();
        if (swipeRecognizers.Count > 0)
        {
            var releasedPoint = e.GetPosition(_panRootVisual);
            double totalX = releasedPoint.X - _panStartPoint.X;
            double totalY = releasedPoint.Y - _panStartPoint.Y;

            foreach (var recognizer in swipeRecognizers)
            {
                bool isHorizontal = Math.Abs(totalX) > Math.Abs(totalY);
                double threshold = recognizer.Threshold;
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

                if (detectedDirection.HasValue &&
                    (recognizer.Direction & detectedDirection.Value) == detectedDirection.Value)
                {
                    recognizer.SendSwiped(view, detectedDirection.Value);
                }
            }
        }

        _panOriginVisual = null;
        _panRootVisual = null;
        e.Handled = true;
    }

    // ── PointerGestureRecognizer enter/exit ────────────────────────────────

    private void OnPointerEntered(object? sender, Input.PointerEventArgs e)
    {
        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        if (pointerRecognizers.Count == 0) return;

        var point = e.GetPosition(sender as Visual);
        var args = GetPointerArgs(point);

        foreach (var recognizer in pointerRecognizers)
            recognizer.SendPointerEntered(view, args.GetPosition, null, args.Buttons);
        e.Handled = true;
    }

    private void OnPointerExited(object? sender, Input.PointerEventArgs e)
    {
        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var pointerRecognizers = recognizers.OfType<PointerGestureRecognizer>().ToList();
        if (pointerRecognizers.Count == 0) return;

        var point = e.GetPosition(sender as Visual);
        var args = GetPointerArgs(point);

        foreach (var recognizer in pointerRecognizers)
            recognizer.SendPointerExited(view, args.GetPosition, null, args.Buttons);
        e.Handled = true;
    }

    // ── Drag Source Logic ──────────────────────────────────────────────────

    private async Task InitiateDragAsync(View view, PointerPressedEventArgs pointerArgs, Visual? senderVisual)
    {
        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var dragRecognizers = recognizers.OfType<DragGestureRecognizer>()
            .Where(r => r.CanDrag).ToList();

        if (dragRecognizers.Count == 0) return;

        DataPackage? dataPackage = null;
        bool cancelled = false;

        foreach (var recognizer in dragRecognizers)
        {
            var point = pointerArgs.GetPosition(senderVisual);
            var dragStartingArgs = recognizer.SendDragStarting(view, GetPositionFunc(point));

            if (dragStartingArgs.Cancel)
            {
                cancelled = true;
                break;
            }

            dataPackage ??= dragStartingArgs.Data;
        }

        if (cancelled || dataPackage == null) return;

        DragDropDataBridge.ActiveDataPackage = dataPackage;
        DragDropDataBridge.ActiveDragSourceView = view;
        DragDropDataBridge.ActiveDragRecognizers = dragRecognizers;

        try
        {
            var dataTransfer = new DataTransfer();
            var text = dataPackage.Text;
            if (!string.IsNullOrEmpty(text))
                dataTransfer.Add(DataTransferItem.CreateText(text));

            await DragDrop.DoDragDropAsync(pointerArgs, dataTransfer, DragDropEffects.Copy | DragDropEffects.Move);

            var dropCompletedArgs = new DropCompletedEventArgs();
            foreach (var recognizer in dragRecognizers)
                recognizer.SendDropCompleted(dropCompletedArgs);
        }
        finally
        {
            DragDropDataBridge.Clear();
        }
    }

    // ── Drop Target Logic ──────────────────────────────────────────────────

    private bool _isDropSubscribed;

    private void SetupDropHandlersIfNeeded(AvaloniaControl control, View view)
    {
        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        bool hasDropRecognizers = recognizers.OfType<DropGestureRecognizer>().Any(r => r.AllowDrop);
        if (hasDropRecognizers) SetupDropHandlers(control);
    }

    private void SetupDropHandlers(AvaloniaControl control)
    {
        if (_isDropSubscribed) return;

        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragEnterEvent, OnDragEnter, RoutingStrategies.Bubble);
        control.AddHandler(DragDrop.DragOverEvent, OnDragOver, RoutingStrategies.Bubble);
        control.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave, RoutingStrategies.Bubble);
        control.AddHandler(DragDrop.DropEvent, OnDrop, RoutingStrategies.Bubble);
        _isDropSubscribed = true;
    }

    private void TearDownDropHandlers(AvaloniaControl control)
    {
        if (!_isDropSubscribed) return;

        DragDrop.SetAllowDrop(control, false);
        control.RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
        control.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
        control.RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        control.RemoveHandler(DragDrop.DropEvent, OnDrop);
        _isDropSubscribed = false;
    }

    // ── Pinch (unchanged) ──────────────────────────────────────────────────

    private bool _isPinchSubscribed;
    private bool _isPinchActive;
    private double _previousPinchScale;

    private void SetupPinchHandlersIfNeeded(AvaloniaControl control, View view)
    {
        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        bool hasPinchRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PinchGestureRecognizer>().Any();
        if (hasPinchRecognizers) SetupPinchHandlers(control);
    }

    private void SetupPinchHandlers(AvaloniaControl control)
    {
        if (_isPinchSubscribed) return;

        control.GestureRecognizers.Add(new AvaloniaPinchGestureRecognizer());
        control.AddHandler(InputElement.PinchEvent, OnPinch, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PinchEndedEvent, OnPinchEnded, RoutingStrategies.Bubble);
        _isPinchSubscribed = true;
    }

    private void TearDownPinchHandlers(AvaloniaControl control)
    {
        if (!_isPinchSubscribed) return;

        control.RemoveHandler(InputElement.PinchEvent, OnPinch);
        control.RemoveHandler(InputElement.PinchEndedEvent, OnPinchEnded);

        _isPinchSubscribed = false;
        _isPinchActive = false;
        _previousPinchScale = 1.0;
    }

    private void OnPinch(object? sender, PinchEventArgs e)
    {
        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0) return;

        var pinchRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PinchGestureRecognizer>().ToList();
        if (pinchRecognizers.Count == 0) return;

        var origin = e.ScaleOrigin;
        double viewWidth = view.Width;
        double viewHeight = view.Height;
        var normalizedOrigin = new Microsoft.Maui.Graphics.Point(
            viewWidth > 0 ? origin.X / viewWidth : 0,
            viewHeight > 0 ? origin.Y / viewHeight : 0);

        if (!_isPinchActive)
        {
            foreach (var recognizer in pinchRecognizers)
            {
                if (recognizer is IPinchGestureController controller)
                    controller.SendPinchStarted(view, normalizedOrigin);
            }
            _isPinchActive = true;
            _previousPinchScale = 1.0;
        }

        double delta = _previousPinchScale > 0 ? e.Scale / _previousPinchScale : e.Scale;
        _previousPinchScale = e.Scale;

        foreach (var recognizer in pinchRecognizers)
        {
            if (recognizer is IPinchGestureController controller)
                controller.SendPinch(view, delta, normalizedOrigin);
        }
    }

    private void OnPinchEnded(object? sender, PinchEndedEventArgs e)
    {
        if (!_isPinchActive) return;
        if (_view is not View view) return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0) return;

        var pinchRecognizers = recognizers.OfType<Microsoft.Maui.Controls.PinchGestureRecognizer>().ToList();
        foreach (var recognizer in pinchRecognizers)
        {
            if (recognizer is IPinchGestureController controller)
                controller.SendPinchEnded(view);
        }

        _isPinchActive = false;
        _previousPinchScale = 1.0;
    }

    // ── Drop events (unchanged) ────────────────────────────────────────────

    private void OnDragEnter(object? sender, AvaloniaDragEventArgs e) => HandleDragOver(e);

    private void OnDragOver(object? sender, AvaloniaDragEventArgs e) => HandleDragOver(e);

    private void HandleDragOver(AvaloniaDragEventArgs e)
    {
        if (_view is not View view) return;
        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var dropRecognizers = recognizers.OfType<DropGestureRecognizer>()
            .Where(r => r.AllowDrop).ToList();

        if (dropRecognizers.Count == 0) return;

        var dataPackage = GetOrCreateDataPackage(e.DataTransfer);
        var dragEventArgs = new Microsoft.Maui.Controls.DragEventArgs(dataPackage);

        foreach (var recognizer in dropRecognizers)
            recognizer.SendDragOver(dragEventArgs);

        e.DragEffects = dragEventArgs.AcceptedOperation == DataPackageOperation.None
            ? DragDropEffects.None : DragDropEffects.Copy;
        e.Handled = true;
    }

    private void OnDragLeave(object? sender, AvaloniaDragEventArgs e)
    {
        if (_view is not View view) return;
        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var dropRecognizers = recognizers.OfType<DropGestureRecognizer>()
            .Where(r => r.AllowDrop).ToList();

        if (dropRecognizers.Count == 0) return;

        var dataPackage = GetOrCreateDataPackage(e.DataTransfer);
        var dragEventArgs = new Microsoft.Maui.Controls.DragEventArgs(dataPackage);

        foreach (var recognizer in dropRecognizers)
            recognizer.SendDragLeave(dragEventArgs);

        e.Handled = true;
    }

    private async void OnDrop(object? sender, AvaloniaDragEventArgs e)
    {
        if (_view is not View view) return;
        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null) return;

        var dropRecognizers = recognizers.OfType<DropGestureRecognizer>()
            .Where(r => r.AllowDrop).ToList();

        if (dropRecognizers.Count == 0) return;

        var dataPackage = GetOrCreateDataPackage(e.DataTransfer);
        var dataPackageView = dataPackage.View;
        var dropEventArgs = new DropEventArgs(dataPackageView);

        foreach (var recognizer in dropRecognizers)
            await recognizer.SendDrop(dropEventArgs);

        e.DragEffects = dropEventArgs.Handled ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private static DataPackage GetOrCreateDataPackage(IDataTransfer? dataTransfer)
    {
        if (DragDropDataBridge.ActiveDataPackage is { } bridgePackage)
            return bridgePackage;

        var dataPackage = new DataPackage();
        if (dataTransfer == null) return dataPackage;

        var text = dataTransfer.TryGetText();
        if (!string.IsNullOrEmpty(text))
            dataPackage.Text = text;

        var files = dataTransfer.TryGetFiles();
        if (files != null && files.Length > 0)
        {
            var filePaths = new List<string>();
            foreach (var file in files)
            {
                var localPath = file.TryGetLocalPath();
                if (localPath != null)
                    filePaths.Add(localPath);
                else
                    filePaths.Add(file.Path.ToString());
            }

            if (string.IsNullOrEmpty(dataPackage.Text))
                dataPackage.Text = string.Join(Environment.NewLine, filePaths);

            dataPackage.Properties["FilePaths"] = filePaths;
        }

        return dataPackage;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

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
        if (_disposed) return;

        _disposed = true;
        DisconnectGestures();

        if (_view != null)
        {
            _view.HandlerChanging -= OnHandlerChanging;
            _view.HandlerChanged -= OnHandlerChanged;
            _view.WindowChanged -= OnWindowChanged;
            _view.PlatformContainerViewChanged -= OnPlatformContainerViewChanged;
            _view = null;
        }
    }
}
