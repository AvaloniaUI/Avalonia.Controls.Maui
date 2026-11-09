using Avalonia.Controls.Embedding;
using Avalonia.Controls.Maui.Platforms.EmbeddedPopups;
using Avalonia.Input;
using Avalonia.VisualTree;
using System;
using System.Runtime.CompilerServices;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Avalonia.Controls.Maui.Platforms.iOS.Handlers
{
    /// <summary>
    /// Connects a UIKit view that embeds Avalonia content to the popup overlay.
    /// </summary>
    internal sealed class EmbeddedPopupHost : IEmbeddedPopupHost
    {
        private readonly UIView _view;
        private readonly TopLevel? _topLevel;

        private EmbeddedPopupHost(UIView view, TopLevel? topLevel)
        {
            _view = view;
            _topLevel = topLevel;
        }

        /// <summary>Registers <paramref name="view"/>, whose Avalonia content root is <paramref name="content"/>.</summary>
        public static EmbeddedPopupHost Attach(UIView view, Control content)
        {
            var host = new EmbeddedPopupHost(view, TopLevel.GetTopLevel(content));
            if (host._topLevel is not null)
                EmbeddedPopupRedirector.Register(host._topLevel, host);
            return host;
        }

        /// <summary>Call when the host view was laid out.</summary>
        public void OnLayout()
        {
            // The window may have been resized.
            EmbeddedPopupOverlay.ForWindow(_view.Window).Prepare();
            EmbeddedPopupRedirector.OnHostLayoutChanged(_topLevel);
        }

        /// <summary>Call when the host view was added to or removed from a window.</summary>
        /// <remarks>MAUI can lay out the host before it is in a window, and not again after.</remarks>
        public void OnMovedToWindow()
        {
            // The overlay of the old window would keep the open popups above the next page, for example after a
            // navigation.
            if (_view.Window is null)
                EmbeddedPopupRedirector.OnHostLeftWindow(_topLevel);

            EmbeddedPopupOverlay.ForWindow(_view.Window).Prepare();
            // The closed popups of the host were redirected to the overlay of the window it was in before, if any.
            // A popup that opens while the host is not in a window opens in the detached overlay, which is not shown.
            EmbeddedPopupRedirector.OnHostOverlayChanged(_topLevel);

            if (_view.Window is not null)
                EmbeddedPopupRedirector.OnHostEnteredWindow(_topLevel);
        }

        public IEmbeddedPopupOverlay GetOverlay() => EmbeddedPopupOverlay.ForWindow(_view.Window);

        public bool IsInWindow => _view.Window is not null;

        // A scroll view that scrolls changes its bounds, which does not lay out the views in it.
        public bool FollowsMovesEachFrame => true;

        public Size? GetWindowSize() => _view.Window is { } window
            ? new Size(window.Bounds.Width, window.Bounds.Height)
            : null;

        public Point? GetOriginInOverlay() => EmbeddedPopupOverlay.ForWindow(_view.Window).GetOrigin(_view);
    }

    /// <summary>
    /// A transparent view on top of a window that shows the popups of embedded Avalonia views.
    /// </summary>
    /// <remarks>
    /// Each window has its own overlay, so that popups can be open in several windows at once. Hosts that are not
    /// in a window yet share a detached overlay, which is never shown. Their popups move to the overlay of the
    /// window when the host is added to one.
    /// </remarks>
    internal sealed class EmbeddedPopupOverlay : IEmbeddedPopupOverlay
    {
        private static readonly ConditionalWeakTable<UIWindow, EmbeddedPopupOverlay> s_overlays = new();
        private static EmbeddedPopupOverlay? s_detached;

        private readonly WeakReference<UIWindow>? _window;
        private readonly OverlayView _view;
        private readonly EmbeddableControlRoot _root;
        private readonly AvaloniaViewHover? _hover;
        private bool _hidePending;

        private EmbeddedPopupOverlay(UIWindow? window)
        {
            _window = window is null ? null : new WeakReference<UIWindow>(window);
            AnchorLayer = new Canvas();
            TopLevel.SetAutoSafeAreaPadding(AnchorLayer, false);
            AnchorLayer.SizeChanged += (_, _) => EmbeddedPopupRedirector.OnOverlayLayoutChanged(this);

            _view = new OverlayView(this)
            {
                Content = AnchorLayer,
                BackgroundColor = UIColor.Clear,
                Opaque = false,
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
            };

            _root = (EmbeddableControlRoot)TopLevel.GetTopLevel(AnchorLayer)!;
            EmbeddedPopupOverlayRoot.MakeTransparent(_root);
            // A transparent brush keeps the root hit-testable. Avalonia can then track the mouse over the root,
            // and clears that when the pointer leaves. With no background, the root keeps IsPointerOver after a
            // click in a popup, and its next render closes the tooltips of the embedded views.
            // HitTestOverlay ignores the root, so input outside the popups still goes to the views below.
            _root.Background = Avalonia.Media.Brushes.Transparent;
            _hover = AvaloniaViewHover.Attach(_view, _root);

            // A Metal layer that is not in a window never returns its drawables. Rendering it would block
            // the shared render loop, and with it all other Avalonia views. Render only while shown.
            _root.StopRendering();
        }

        /// <summary>Gets the overlay of <paramref name="window"/>, or the detached overlay if it is null.</summary>
        public static EmbeddedPopupOverlay ForWindow(UIWindow? window) =>
            window is null
                ? s_detached ??= new EmbeddedPopupOverlay(null)
                : s_overlays.GetValue(window, w => new EmbeddedPopupOverlay(w));

        public Canvas AnchorLayer { get; }

        public void Show(IEmbeddedPopupHost host)
        {
            if (_window is null || !_window.TryGetTarget(out var window))
                return;

            _hidePending = false;
            if (_view.Superview != window)
            {
                _view.RemoveFromSuperview();
                window.AddSubview(_view);
            }

            _view.Frame = window.Bounds;
            // Stay above views that were added to the window later, for example modal pages.
            window.BringSubviewToFront(_view);
            _root.StartRendering();
        }

        /// <summary>Gives the hidden overlay the size and scale of its window.</summary>
        /// <remarks>
        /// A popup is laid out while it opens, before the overlay is shown. Without this, the first popup is laid
        /// out for an overlay with no size, and controls such as the date picker keep the wrong layout.
        /// </remarks>
        public void Prepare()
        {
            if (_window is null || !_window.TryGetTarget(out var window) || _view.Superview is not null)
                return;

            // A view that is not in a window has a scale of 1. Avalonia gets the scaling from it, so set the scale
            // of the window. Otherwise the scaling changes when the overlay is shown, after the popup was laid out.
            var scale = window.Screen.Scale;
            if (_view.Frame == window.Bounds && _view.ContentScaleFactor == scale)
                return;

            _view.Frame = window.Bounds;
            _view.ContentScaleFactor = scale;
            // Avalonia gets the new size and scaling in LayoutSubviews.
            _view.SetNeedsLayout();
            _view.LayoutIfNeeded();
        }

        public void Hide()
        {
            // A press that light-dismissed the last popup is still in progress. A view that leaves the window
            // does not get the end of its touches, so Avalonia would keep the pointer pressed and captured,
            // and the next press in the overlay would not reach the popup. Hide when the touches end.
            if (_view.HasActiveTouches)
            {
                _hidePending = true;
                return;
            }

            _hidePending = false;
            _hover?.Leave();
            _root.StopRendering();
            _view.RemoveFromSuperview();
        }

        private void OnTouchesEnded()
        {
            if (_hidePending && !_view.HasActiveTouches)
                Hide();
        }

        /// <summary>Gets the top-left of <paramref name="view"/> in overlay coordinates, in points.</summary>
        public Point? GetOrigin(UIView view)
        {
            if (view.Window is not { } window)
                return null;

            // Before the overlay is in the window, it will cover the window from its origin.
            UIView target = _view.Window == window ? _view : window;
            var origin = view.ConvertPointToView(CGPoint.Empty, target);
            return new Point(origin.X, origin.Y);
        }

        /// <summary>Gets what <paramref name="point"/> is over.</summary>
        // UIKit points are Avalonia device-independent units.
        private EmbeddedPopupOverlayHit HitTestOverlay(CGPoint point) =>
            EmbeddedPopupOverlayRoot.HitTest(_root, AnchorLayer, new Point(point.X, point.Y));

        private sealed class OverlayView(EmbeddedPopupOverlay overlay) : Avalonia.iOS.AvaloniaView
        {
            private int _activeTouches;

            public bool HasActiveTouches => _activeTouches > 0;

            private double _passedThroughAt = -1;

            // Touches that are not over a popup go to the views below the overlay.
            public override UIView? HitTest(CGPoint point, UIEvent? uievent)
            {
                // UIKit can hit-test a touch more than once. The light-dismiss layer is still in the rendered scene
                // after the popups closed, so answer the same for the same touch.
                if (uievent is { Type: UIEventType.Touches } && uievent.Timestamp == _passedThroughAt)
                    return null;

                switch (overlay.HitTestOverlay(point))
                {
                    case EmbeddedPopupOverlayHit.Popup:
                        return base.HitTest(point, uievent);

                    case EmbeddedPopupOverlayHit.LightDismissLayer:
                        // Avalonia passes a press that closes popups to what is below the light-dismiss layer, if
                        // they let it. Here, that is the view below the overlay.
                        if (uievent is { Type: UIEventType.Touches }
                            && EmbeddedPopupRedirector.TryDismissPassingThrough(overlay))
                        {
                            _passedThroughAt = uievent.Timestamp;
                            return null;
                        }

                        // Avalonia closes the popups when the overlay gets the press, and does not pass it on.
                        return base.HitTest(point, uievent);

                    default:
                        return null;
                }
            }

            public override void TouchesBegan(NSSet touches, UIEvent? evt)
            {
                _activeTouches += (int)touches.Count;
                base.TouchesBegan(touches, evt);
            }

            public override void TouchesEnded(NSSet touches, UIEvent? evt)
            {
                base.TouchesEnded(touches, evt);
                EndTouches(touches);
            }

            public override void TouchesCancelled(NSSet touches, UIEvent? evt)
            {
                base.TouchesCancelled(touches, evt);
                EndTouches(touches);
            }

            private void EndTouches(NSSet touches)
            {
                _activeTouches = Math.Max(0, _activeTouches - (int)touches.Count);
                overlay.OnTouchesEnded();
            }
        }
    }
}
