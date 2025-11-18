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
        control.AddHandler(global::Avalonia.Controls.Control.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    private void UnsubscribeFromGestureEvents(AvaloniaControl control)
    {
        control.RemoveHandler(global::Avalonia.Controls.Control.PointerPressedEvent, OnPointerPressed);
    }

    private DateTime? _lastTapTime;
    private global::Avalonia.Point? _lastTapPosition;
    private const double DoubleTapThresholdMs = 300;
    private const double DoubleTapDistanceThreshold = 10;

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_view is not View view)
            return;

        var recognizers = view.GetCompositeGestureRecognizers();
        if (recognizers == null || recognizers.Count == 0)
            return;

        var tapRecognizers = recognizers.OfType<TapGestureRecognizer>().ToList();
        if (tapRecognizers.Count == 0)
            return;

        var point = e.GetPosition(sender as global::Avalonia.Visual);
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
            if (recognizer.NumberOfTapsRequired == tapCount)
            {
                recognizer.SendTapped(view, GetPositionFunc(point));
                e.Handled = true;
            }
        }
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
