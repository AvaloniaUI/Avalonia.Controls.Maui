using Avalonia.Controls.Embedding;
using Avalonia.Controls.Maui.Platforms.EmbeddedPopups;
using Avalonia.Input;
using Avalonia.VisualTree;
using System;
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

        /// <summary>Gets the window that contains the host.</summary>
        public UIWindow? Window => _view.Window;

        /// <summary>Call when the host view was laid out.</summary>
        public void OnLayout()
        {
            // The window may have been resized.
            EmbeddedPopupOverlay.Instance.Prepare(_view.Window);
            EmbeddedPopupRedirector.OnHostLayoutChanged(_topLevel);
        }

        /// <summary>Call when the host view was added to or removed from a window.</summary>
        /// <remarks>MAUI can lay out the host before it is in a window, and not again after.</remarks>
        public void OnMovedToWindow() => EmbeddedPopupOverlay.Instance.Prepare(_view.Window);

        // The host may not be in a window yet when its popups are redirected, so all hosts share one
        // overlay. The overlay moves to the window of the host that shows it.
        public IEmbeddedPopupOverlay GetOverlay() => EmbeddedPopupOverlay.Instance;

        public Point? GetOriginInOverlay() => EmbeddedPopupOverlay.Instance.GetOrigin(_view);
    }

    /// <summary>
    /// A transparent view on top of the window that shows the popups of embedded Avalonia views.
    /// </summary>
    internal sealed class EmbeddedPopupOverlay : IEmbeddedPopupOverlay
    {
        private static EmbeddedPopupOverlay? s_instance;

        private readonly OverlayView _view;
        private readonly EmbeddableControlRoot _root;
        private readonly AvaloniaViewHover? _hover;
        private bool _hidePending;

        private EmbeddedPopupOverlay()
        {
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
            // HitsPopup ignores the root, so input outside the popups still goes to the views below.
            _root.Background = Avalonia.Media.Brushes.Transparent;
            _hover = AvaloniaViewHover.Attach(_view, _root);

            // A Metal layer that is not in a window never returns its drawables. Rendering it would block
            // the shared render loop, and with it all other Avalonia views. Render only while shown.
            _root.StopRendering();
        }

        public static EmbeddedPopupOverlay Instance => s_instance ??= new EmbeddedPopupOverlay();

        public Canvas AnchorLayer { get; }

        public void Show(IEmbeddedPopupHost host)
        {
            if (((EmbeddedPopupHost)host).Window is not { } window)
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

        /// <summary>Gives the hidden overlay the size and scale of <paramref name="window"/>.</summary>
        /// <remarks>
        /// A popup is laid out while it opens, before the overlay is shown. Without this, the first popup is laid
        /// out for an overlay with no size, and controls such as the date picker keep the wrong layout.
        /// </remarks>
        public void Prepare(UIWindow? window)
        {
            if (window is null || _view.Superview is not null)
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

        /// <summary>Returns true when the point is over popup content, including light-dismiss layers.</summary>
        private bool HitsPopup(CGPoint point)
        {
            // UIKit points are Avalonia device-independent units.
            return _root.InputHitTest(new Point(point.X, point.Y)) is Visual hit
                && hit != _root
                && !AnchorLayer.IsVisualAncestorOf(hit)
                && !hit.IsVisualAncestorOf(AnchorLayer);
        }

        private sealed class OverlayView(EmbeddedPopupOverlay overlay) : Avalonia.iOS.AvaloniaView
        {
            private int _activeTouches;

            public bool HasActiveTouches => _activeTouches > 0;

            // Touches that are not over a popup go to the views below the overlay.
            public override UIView? HitTest(CGPoint point, UIEvent? uievent) =>
                overlay.HitsPopup(point) ? base.HitTest(point, uievent) : null;

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
