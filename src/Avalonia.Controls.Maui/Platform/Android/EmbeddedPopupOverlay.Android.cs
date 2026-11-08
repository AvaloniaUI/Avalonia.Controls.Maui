using System.Runtime.CompilerServices;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Avalonia.Controls.Embedding;
using Avalonia.Controls.Maui.Platforms.EmbeddedPopups;
using Avalonia.Input;
using Avalonia.VisualTree;
using AndroidColor = Android.Graphics.Color;
using AndroidView = Android.Views.View;

namespace Avalonia.Controls.Maui.Platforms.Android.Handlers
{
    /// <summary>
    /// Connects an Android view that embeds Avalonia content to the popup overlay.
    /// </summary>
    internal sealed class EmbeddedPopupHost : IEmbeddedPopupHost
    {
        private readonly AndroidView _view;
        private readonly TopLevel? _topLevel;

        private EmbeddedPopupHost(AndroidView view, TopLevel? topLevel)
        {
            _view = view;
            _topLevel = topLevel;
        }

        /// <summary>Registers <paramref name="view"/>, whose Avalonia content root is <paramref name="content"/>.</summary>
        public static EmbeddedPopupHost Attach(AndroidView view, Control content)
        {
            var host = new EmbeddedPopupHost(view, TopLevel.GetTopLevel(content));
            if (host._topLevel is not null)
                EmbeddedPopupRedirector.Register(host._topLevel, host);
            return host;
        }

        /// <summary>Gets the root view of the window that contains the host.</summary>
        public AndroidView? WindowRoot => _view.IsAttachedToWindow ? _view.RootView : null;

        /// <summary>Call when the host view was laid out.</summary>
        public void OnLayout() => EmbeddedPopupRedirector.OnHostLayoutChanged(_topLevel);

        public IEmbeddedPopupOverlay GetOverlay() => EmbeddedPopupOverlay.ForContext(_view.Context!);

        public Point? GetOriginInOverlay() =>
            _topLevel is null ? null : ((EmbeddedPopupOverlay)GetOverlay()).GetOrigin(_view, _topLevel.RenderScaling);
    }

    /// <summary>
    /// A transparent window above the activity that shows the popups of embedded Avalonia views.
    /// </summary>
    internal sealed class EmbeddedPopupOverlay : IEmbeddedPopupOverlay
    {
        private static readonly ConditionalWeakTable<Context, EmbeddedPopupOverlay> s_overlays = new();

        private readonly OverlayView _view;
        private PopupWindow? _window;
        private AndroidView? _windowRoot;
        private bool _hidePending;

        private EmbeddedPopupOverlay(Context context)
        {
            AnchorLayer = new Canvas();
            TopLevel.SetAutoSafeAreaPadding(AnchorLayer, false);
            AnchorLayer.SizeChanged += (_, _) => EmbeddedPopupRedirector.OnOverlayLayoutChanged(this);

            _view = new OverlayView(context, this) { Content = AnchorLayer };
            _view.SetBackgroundColor(AndroidColor.Transparent);

            if (TopLevel.GetTopLevel(AnchorLayer) is EmbeddableControlRoot root)
                EmbeddedPopupOverlayRoot.MakeTransparent(root);
        }

        public Canvas AnchorLayer { get; }

        public static EmbeddedPopupOverlay ForContext(Context context) =>
            s_overlays.GetValue(context, c => new EmbeddedPopupOverlay(c));

        public void Show(IEmbeddedPopupHost host)
        {
            if (((EmbeddedPopupHost)host).WindowRoot is not { WindowToken: not null } windowRoot)
                return;

            _hidePending = false;
            _window ??= CreateWindow();
            if (_window.IsShowing && _windowRoot == windowRoot)
                return;

            _window.Dismiss();
            _windowRoot = windowRoot;
            _window.ShowAtLocation(windowRoot, GravityFlags.NoGravity, 0, 0);
        }

        public void Hide()
        {
            // A touch that light-dismissed the last popup is still in progress. A dismissed window does not get
            // the end of its touches, so Avalonia would keep the pointer pressed and captured, and the next touch
            // in the overlay would not reach the popup. Hide when the gesture ends.
            if (_view.IsTouching)
            {
                _hidePending = true;
                return;
            }

            _hidePending = false;
            _window?.Dismiss();
            _windowRoot = null;
        }

        private void OnGestureEnded()
        {
            if (_hidePending)
                Hide();
        }

        /// <summary>Gets the top-left of <paramref name="view"/> in overlay coordinates, in DIPs.</summary>
        public Point? GetOrigin(AndroidView view, double scaling)
        {
            if (!view.IsAttachedToWindow)
                return null;

            // Before the overlay window is laid out, it is at the origin of the window it covers.
            var overlayOrigin = LocationOnScreen(
                _view.IsAttachedToWindow && _view.Width > 0 ? _view : view.RootView!);
            var viewOrigin = LocationOnScreen(view);

            return new Point((viewOrigin.X - overlayOrigin.X) / scaling, (viewOrigin.Y - overlayOrigin.Y) / scaling);
        }

        private PopupWindow CreateWindow()
        {
            var window = new PopupWindow(_view, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            {
                // Keep keyboard focus and the soft keyboard on the page.
                Focusable = false,
                Touchable = true,
                ClippingEnabled = false,
                AnimationStyle = 0,
            };
            window.SetBackgroundDrawable(new ColorDrawable(AndroidColor.Transparent));
            return window;
        }

        /// <summary>Returns true when the touch point is over popup content, including light-dismiss layers.</summary>
        private bool HitsPopup(float x, float y)
        {
            if (TopLevel.GetTopLevel(AnchorLayer) is not { } root)
                return false;

            var scaling = root.RenderScaling;
            return root.InputHitTest(new Point(x / scaling, y / scaling)) is Visual hit
                && hit != root
                && !AnchorLayer.IsVisualAncestorOf(hit)
                && !hit.IsVisualAncestorOf(AnchorLayer);
        }

        /// <summary>Sends a touch that is not over a popup to the window below.</summary>
        private bool Forward(MotionEvent e)
        {
            if (_windowRoot is not { } windowRoot)
                return false;

            var viewOrigin = LocationOnScreen(_view);
            var rootOrigin = LocationOnScreen(windowRoot);
            var copy = MotionEvent.Obtain(e)!;
            copy.OffsetLocation(viewOrigin.X - rootOrigin.X, viewOrigin.Y - rootOrigin.Y);
            var handled = windowRoot.DispatchTouchEvent(copy);
            copy.Recycle();
            return handled;
        }

        private static (int X, int Y) LocationOnScreen(AndroidView view)
        {
            var location = new int[2];
            view.GetLocationOnScreen(location);
            return (location[0], location[1]);
        }

        private sealed class OverlayView(Context context, EmbeddedPopupOverlay overlay) : Avalonia.Android.AvaloniaView(context)
        {
            private bool _forwarding;

            /// <summary>Gets a value indicating whether a touch gesture on the overlay is in progress.</summary>
            public bool IsTouching { get; private set; }

            public override bool DispatchTouchEvent(MotionEvent? e)
            {
                if (e is null)
                    return base.DispatchTouchEvent(e);

                if (e.ActionMasked == MotionEventActions.Down)
                {
                    IsTouching = true;
                    _forwarding = !overlay.HitsPopup(e.GetX(), e.GetY());
                }

                var ended = e.ActionMasked is MotionEventActions.Up or MotionEventActions.Cancel;
                bool handled;
                if (!_forwarding)
                    handled = base.DispatchTouchEvent(e);
                else
                {
                    if (ended)
                        _forwarding = false;
                    handled = overlay.Forward(e);
                }

                if (ended)
                {
                    IsTouching = false;
                    overlay.OnGestureEnded();
                }

                return handled;
            }
        }
    }
}
