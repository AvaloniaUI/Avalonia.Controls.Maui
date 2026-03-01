using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaDragEventArgs = Avalonia.Input.DragEventArgs;
using AvaloniaTopLevel = Avalonia.Controls.TopLevel;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Manages gesture recognizers for Avalonia.Controls.Maui platform
/// </summary>
internal class GestureManager : IDisposable
{
    private IControlsView? _view;
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
            TearDownDropHandlers(control);
        }

        if (_view is View view && view.GetCompositeGestureRecognizers() is ObservableCollection<IGestureRecognizer> recognizers)
        {
            recognizers.CollectionChanged -= OnGestureRecognizersCollectionChanged;
        }

        _handler = null;
        _didHaveWindow = false;
        _containerView = null;
        _platformView = null;

        // Reset drag state
        _isDragPending = false;
        _dragPointerArgs = null;
    }

    private void SetupGestureManager()
    {
        if (_view == null)
            return;

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

            if (_view is View view)
            {
                SetupDropHandlersIfNeeded(control, view);
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

        // Check if drop recognizers were added or removed
        bool hasDropRecognizers = view.GetCompositeGestureRecognizers()
            ?.OfType<DropGestureRecognizer>()
            .Any(r => r.AllowDrop) == true;

        if (hasDropRecognizers && !_isDropSubscribed)
        {
            SetupDropHandlers(control);
        }
        else if (!hasDropRecognizers && _isDropSubscribed)
        {
            TearDownDropHandlers(control);
        }
    }

    private void SubscribeToGestureEvents(AvaloniaControl control)
    {
        control.AddHandler(AvaloniaControl.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerEnteredEvent, OnPointerEntered, RoutingStrategies.Direct | RoutingStrategies.Bubble);
        control.AddHandler(InputElement.PointerExitedEvent, OnPointerExited, RoutingStrategies.Direct | RoutingStrategies.Bubble);
    }

    private void UnsubscribeFromGestureEvents(AvaloniaControl control)
    {
        control.RemoveHandler(AvaloniaControl.PointerPressedEvent, OnPointerPressed);
        control.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        control.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
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

    // Drag gesture state
    private bool _isDragPending;
    private Point _dragStartPoint;
    private PointerPressedEventArgs? _dragPointerArgs;
    private const double DragThreshold = 5.0;

    // Drop gesture state
    private bool _isDropSubscribed;

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

        // Check for drag gesture recognizers
        var dragRecognizers = recognizers.OfType<DragGestureRecognizer>()
            .Where(r => r.CanDrag)
            .ToList();

        if (dragRecognizers.Count > 0)
        {
            _isDragPending = true;
            _dragStartPoint = point;
            _dragPointerArgs = e;
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

        // 2. Check for drag threshold
        if (_isDragPending && _dragPointerArgs != null)
        {
            var currentPoint = e.GetPosition(sender as Visual);
            double deltaX = currentPoint.X - _dragStartPoint.X;
            double deltaY = currentPoint.Y - _dragStartPoint.Y;
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (distance >= DragThreshold)
            {
                _isDragPending = false;
                var dragArgs = _dragPointerArgs;
                _dragPointerArgs = null;

                // Cancel any pending tap
                _singleTapCts?.Cancel();
                _singleTapCts = null;

                _ = InitiateDragAsync(view, dragArgs, sender as Visual);
                e.Handled = true;
                return;
            }
        }

        // 3. Handle PanGestureRecognizer (Only if panning)
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
        // Reset drag state on release (no drag threshold was reached)
        _isDragPending = false;
        _dragPointerArgs = null;

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

        _panOriginVisual = null;
        _panRootVisual = null;
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

    // --- Drag Source Logic ---

    private async Task InitiateDragAsync(View view, PointerPressedEventArgs pointerArgs, Visual? senderVisual)
    {
        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null)
            return;

        var dragRecognizers = recognizers.OfType<DragGestureRecognizer>()
            .Where(r => r.CanDrag)
            .ToList();

        if (dragRecognizers.Count == 0)
            return;

        // Call SendDragStarting on each recognizer to get the DataPackage
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

        if (cancelled || dataPackage == null)
            return;

        // Populate the bridge for drop targets
        DragDropDataBridge.ActiveDataPackage = dataPackage;
        DragDropDataBridge.ActiveDragSourceView = view;
        DragDropDataBridge.ActiveDragRecognizers = dragRecognizers;

        try
        {
            // Create Avalonia DataTransfer with text content
            var dataTransfer = new DataTransfer();
            var text = dataPackage.Text;
            if (!string.IsNullOrEmpty(text))
            {
                dataTransfer.Add(DataTransferItem.CreateText(text));
            }

            // Initiate Avalonia drag-and-drop
            await DragDrop.DoDragDropAsync(pointerArgs, dataTransfer, DragDropEffects.Copy | DragDropEffects.Move);

            // Notify source recognizers that the drop completed
            var dropCompletedArgs = new DropCompletedEventArgs();
            foreach (var recognizer in dragRecognizers)
            {
                recognizer.SendDropCompleted(dropCompletedArgs);
            }
        }
        finally
        {
            DragDropDataBridge.Clear();
        }
    }

    // --- Drop Target Logic ---

    private void SetupDropHandlersIfNeeded(AvaloniaControl control, View view)
    {
        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null)
            return;

        bool hasDropRecognizers = recognizers.OfType<DropGestureRecognizer>()
            .Any(r => r.AllowDrop);

        if (hasDropRecognizers)
        {
            SetupDropHandlers(control);
        }
    }

    private void SetupDropHandlers(AvaloniaControl control)
    {
        if (_isDropSubscribed)
            return;

        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragEnterEvent, OnDragEnter, RoutingStrategies.Bubble);
        control.AddHandler(DragDrop.DragOverEvent, OnDragOver, RoutingStrategies.Bubble);
        control.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave, RoutingStrategies.Bubble);
        control.AddHandler(DragDrop.DropEvent, OnDrop, RoutingStrategies.Bubble);
        _isDropSubscribed = true;
    }

    private void TearDownDropHandlers(AvaloniaControl control)
    {
        if (!_isDropSubscribed)
            return;

        DragDrop.SetAllowDrop(control, false);
        control.RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
        control.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
        control.RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        control.RemoveHandler(DragDrop.DropEvent, OnDrop);
        _isDropSubscribed = false;
    }

    private void OnDragEnter(object? sender, AvaloniaDragEventArgs e)
    {
        HandleDragOver(e);
    }

    private void OnDragOver(object? sender, AvaloniaDragEventArgs e)
    {
        HandleDragOver(e);
    }

    private void HandleDragOver(AvaloniaDragEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null)
            return;

        var dropRecognizers = recognizers.OfType<DropGestureRecognizer>()
            .Where(r => r.AllowDrop)
            .ToList();

        if (dropRecognizers.Count == 0)
            return;

        var dataPackage = DragDropDataBridge.ActiveDataPackage ?? new DataPackage();
        var dragEventArgs = new Microsoft.Maui.Controls.DragEventArgs(dataPackage);

        foreach (var recognizer in dropRecognizers)
        {
            recognizer.SendDragOver(dragEventArgs);
        }

        e.DragEffects = dragEventArgs.AcceptedOperation == DataPackageOperation.None
            ? DragDropEffects.None
            : DragDropEffects.Copy;
        e.Handled = true;
    }

    private void OnDragLeave(object? sender, AvaloniaDragEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null)
            return;

        var dropRecognizers = recognizers.OfType<DropGestureRecognizer>()
            .Where(r => r.AllowDrop)
            .ToList();

        if (dropRecognizers.Count == 0)
            return;

        var dataPackage = DragDropDataBridge.ActiveDataPackage ?? new DataPackage();
        var dragEventArgs = new Microsoft.Maui.Controls.DragEventArgs(dataPackage);

        foreach (var recognizer in dropRecognizers)
        {
            recognizer.SendDragLeave(dragEventArgs);
        }

        e.Handled = true;
    }

    private async void OnDrop(object? sender, AvaloniaDragEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null)
            return;

        var dropRecognizers = recognizers.OfType<DropGestureRecognizer>()
            .Where(r => r.AllowDrop)
            .ToList();

        if (dropRecognizers.Count == 0)
            return;

        var dataPackage = DragDropDataBridge.ActiveDataPackage ?? new DataPackage();
        var dataPackageView = dataPackage.View;
        var dropEventArgs = new DropEventArgs(dataPackageView);

        foreach (var recognizer in dropRecognizers)
        {
            await recognizer.SendDrop(dropEventArgs);
        }

        e.DragEffects = dropEventArgs.Handled ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    // --- Helpers ---

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
            _view = null;
        }
    }
}
