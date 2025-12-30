using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AvaloniaControl = Avalonia.Controls.Control;

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

        // The connected Gesture Manager is already setup and watching the correct view
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
        // Gesture recognizers collection changed, no action needed for Avalonia
        // as we handle all gestures through event subscriptions
    }

    private void SubscribeToGestureEvents(AvaloniaControl control)
    {
        // Only subscribe to Bubble to avoid handling the same click twice
        control.AddHandler(global::Avalonia.Controls.Control.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble);
        control.AddHandler(global::Avalonia.Controls.Control.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Bubble);
        control.AddHandler(global::Avalonia.Controls.Control.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble);
    }

    private void UnsubscribeFromGestureEvents(AvaloniaControl control)
    {
        control.RemoveHandler(global::Avalonia.Controls.Control.PointerPressedEvent, OnPointerPressed);
        control.RemoveHandler(global::Avalonia.Controls.Control.PointerMovedEvent, OnPointerMoved);
        control.RemoveHandler(global::Avalonia.Controls.Control.PointerReleasedEvent, OnPointerReleased);
    }

    private System.Threading.CancellationTokenSource? _singleTapCts;
    private const int DoubleTapDelayMs = 300;

    // Pan gesture state
    private bool _isPanning;
    private global::Avalonia.Point _panStartPoint;
    private global::Avalonia.Point _lastPanPoint;

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var point = e.GetPosition(sender as global::Avalonia.Visual);
        int clickCount = e.ClickCount;

        // Handle pan gesture recognizers
        var panRecognizers = recognizers.OfType<PanGestureRecognizer>().ToList();
        if (panRecognizers.Count > 0)
        {
            _isPanning = true;
            _panStartPoint = point;
            _lastPanPoint = point;
            
            // Fire Started event
            foreach (var recognizer in panRecognizers)
            {
                recognizer.SendPanStarted(view, Application.Current?.MainPage?.Id ?? 0);
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

        // Case 1: Only single-tap recognizers - fire on every tap immediately
        if (hasSingleTap && !hasDoubleTap)
        {
            foreach (var recognizer in singleTapRecognizers)
            {
                recognizer.SendTapped(view, GetPositionFunc(point));
            }
            e.Handled = true;
            return;
        }

        // Case 2: Only double-tap recognizers - fire when ClickCount >= 2
        if (hasDoubleTap && !hasSingleTap)
        {
            if (clickCount >= 2)
            {
                foreach (var recognizer in doubleTapRecognizers)
                {
                    recognizer.SendTapped(view, GetPositionFunc(point));
                }
                e.Handled = true;
            }
            return;
        }

        // Case 3: Both single and double-tap recognizers
        // This is the complex case that requires delayed single-tap
        if (clickCount >= 2)
        {
            // Cancel any pending single-tap and fire double-tap
            _singleTapCts?.Cancel();
            _singleTapCts = null;
            
            foreach (var recognizer in doubleTapRecognizers)
            {
                recognizer.SendTapped(view, GetPositionFunc(point));
            }
            e.Handled = true;
        }
        else if (clickCount == 1)
        {
            // Delay single-tap to allow for a potential double-tap
            _singleTapCts?.Cancel();
            _singleTapCts = new System.Threading.CancellationTokenSource();
            var cts = _singleTapCts;
            var capturedPoint = point;
            var capturedView = view;
            var capturedRecognizers = singleTapRecognizers.ToList();
            
            _ = System.Threading.Tasks.Task.Delay(DoubleTapDelayMs, cts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        foreach (var recognizer in capturedRecognizers)
                        {
                            recognizer.SendTapped(capturedView, GetPositionFunc(capturedPoint));
                        }
                    });
                }
            }, System.Threading.Tasks.TaskScheduler.Default);
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPanning)
            return;

        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var panRecognizers = recognizers.OfType<PanGestureRecognizer>().ToList();
        if (panRecognizers.Count == 0)
            return;

        var point = e.GetPosition(sender as global::Avalonia.Visual);
        double totalX = point.X - _panStartPoint.X;
        double totalY = point.Y - _panStartPoint.Y;

        _lastPanPoint = point;

        // Fire Running event
        foreach (var recognizer in panRecognizers)
        {
            recognizer.SendPanRunning(view, totalX, totalY, Application.Current?.MainPage?.Id ?? 0);
        }
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPanning)
            return;

        _isPanning = false;

        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var panRecognizers = recognizers.OfType<PanGestureRecognizer>().ToList();
        if (panRecognizers.Count == 0)
            return;

        var point = e.GetPosition(sender as global::Avalonia.Visual);
        double totalX = point.X - _panStartPoint.X;
        double totalY = point.Y - _panStartPoint.Y;

        // Fire Completed event
        foreach (var recognizer in panRecognizers)
        {
            recognizer.SendPanCompleted(view, totalX, totalY, Application.Current?.MainPage?.Id ?? 0);
        }
        e.Handled = true;
    }

    private static Func<Microsoft.Maui.IElement?, Microsoft.Maui.Graphics.Point?> GetPositionFunc(global::Avalonia.Point point)
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
