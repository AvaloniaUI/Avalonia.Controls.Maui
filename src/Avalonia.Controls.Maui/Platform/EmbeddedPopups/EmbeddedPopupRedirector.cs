using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace Avalonia.Controls.Maui.Platforms.EmbeddedPopups
{
    /// <summary>
    /// A window-sized overlay that hosts popups for embedded Avalonia views.
    /// </summary>
    internal interface IEmbeddedPopupOverlay
    {
        /// <summary>The overlay root. Popup anchors are placed here in overlay coordinates.</summary>
        Canvas AnchorLayer { get; }

        /// <summary>Shows the overlay above the window that contains <paramref name="host"/>.</summary>
        void Show(IEmbeddedPopupHost host);

        /// <summary>Hides the overlay.</summary>
        void Hide();
    }

    /// <summary>
    /// A platform view that embeds an Avalonia <see cref="TopLevel"/> in a native page.
    /// </summary>
    internal interface IEmbeddedPopupHost
    {
        /// <summary>Gets the overlay for the window of this host.</summary>
        IEmbeddedPopupOverlay GetOverlay();

        /// <summary>Gets the top-left of this host in overlay coordinates, or null if it is not in a window.</summary>
        Point? GetOriginInOverlay();
    }

    /// <summary>
    /// Opens the popups of embedded Avalonia views in a window-sized overlay.
    /// </summary>
    /// <remarks>
    /// On Android, iOS and Windows (WinUI), Avalonia draws popups inside the surface of their <see cref="TopLevel"/>.
    /// An embedded view is often small (a picker, for example), so its popups are clipped. Each popup
    /// that targets a control in an embedded view gets an invisible anchor control in the overlay as its
    /// <see cref="Popup.PlacementTarget"/>. The anchor is moved over the real target when the popup opens.
    /// </remarks>
    internal static class EmbeddedPopupRedirector
    {
        private static readonly ConditionalWeakTable<TopLevel, HostState> s_hosts = new();
        private static readonly ConditionalWeakTable<Popup, PopupState> s_popups = new();
        private static readonly List<PopupState> s_open = new();
        private static readonly Dictionary<IEmbeddedPopupOverlay, int> s_overlayUsers = new();
        private static bool s_initialized;

        /// <summary>
        /// Registers an embedded view. Popups that target controls in <paramref name="topLevel"/> open in the overlay.
        /// </summary>
        public static void Register(TopLevel topLevel, IEmbeddedPopupHost host)
        {
            EnsureInitialized();

            if (s_hosts.TryGetValue(topLevel, out _))
                return;

            var state = new HostState(host);
            s_hosts.Add(topLevel, state);

            // Pointer placement uses the last pointer position, which the overlay never receives.
            topLevel.AddHandler(
                InputElement.PointerPressedEvent,
                (_, e) => state.LastPointer = e.GetPosition(topLevel),
                RoutingStrategies.Tunnel,
                handledEventsToo: true);
            topLevel.AddHandler(
                InputElement.PointerMovedEvent,
                (_, e) => state.LastPointer = e.GetPosition(topLevel),
                RoutingStrategies.Tunnel,
                handledEventsToo: true);

            // Where the overlay does not cover the window (Windows), a press in the embedded view never reaches
            // the light-dismiss layer of the open popups. Presses in the popups are routed through their logical
            // parents in the embedded view too, so only presses on the view's own visuals dismiss.
            topLevel.AddHandler(
                InputElement.PointerPressedEvent,
                (_, e) =>
                {
                    if (e.Source is Visual source && TopLevel.GetTopLevel(source) == topLevel
                        && LightDismiss(host.GetOverlay()))
                        e.Handled = true;
                },
                RoutingStrategies.Tunnel);
        }

        /// <summary>
        /// Closes the open light-dismiss popups of <paramref name="overlay"/> after a press outside of them.
        /// </summary>
        /// <returns>True if the press should be handled, because a closed popup does not pass it through.</returns>
        public static bool LightDismiss(IEmbeddedPopupOverlay overlay)
        {
            var handled = false;
            foreach (var state in s_open.ToArray())
            {
                if (state.Overlay != overlay || !state.Popup.IsLightDismissEnabled)
                    continue;

                handled |= !state.Popup.OverlayDismissEventPassThrough;
                state.Popup.Close();
            }

            return handled;
        }

        /// <summary>Returns true if <paramref name="topLevel"/> has open light-dismiss popups in the overlay.</summary>
        public static bool HasOpenLightDismissPopups(TopLevel topLevel)
        {
            foreach (var state in s_open)
            {
                if (state.HostTopLevel == topLevel && state.Popup.IsLightDismissEnabled)
                    return true;
            }

            return false;
        }

        /// <summary>Closes the open light-dismiss popups of <paramref name="topLevel"/>.</summary>
        public static void CloseLightDismissPopups(TopLevel topLevel)
        {
            foreach (var state in s_open.ToArray())
            {
                if (state.HostTopLevel == topLevel && state.Popup.IsLightDismissEnabled)
                    state.Popup.Close();
            }
        }

        /// <summary>
        /// Moves the open popups of <paramref name="topLevel"/> after its host view moved or resized.
        /// </summary>
        public static void OnHostLayoutChanged(TopLevel? topLevel)
        {
            if (topLevel is null || s_open.Count == 0)
                return;

            foreach (var state in s_open.ToArray())
            {
                if (state.HostTopLevel == topLevel)
                    state.UpdatePosition();
            }
        }

        /// <summary>
        /// Moves the closed popups of <paramref name="topLevel"/> to the current overlay of its host, for example
        /// after the host view moved to another window. Open popups move when they close.
        /// </summary>
        public static void OnHostOverlayChanged(TopLevel? topLevel)
        {
            if (topLevel is null)
                return;

            foreach (var (_, state) in s_popups)
            {
                if (state.HostTopLevel == topLevel)
                    state.Redirect();
            }
        }

        /// <summary>
        /// Moves all open popups of <paramref name="overlay"/>, for example after the overlay resized.
        /// </summary>
        public static void OnOverlayLayoutChanged(IEmbeddedPopupOverlay overlay)
        {
            // Called during a layout pass, so the anchors can only be arranged after it.
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var state in s_open.ToArray())
                {
                    if (state.Overlay == overlay)
                        state.UpdatePosition();
                }
            });
        }

        private static void EnsureInitialized()
        {
            if (s_initialized)
                return;

            s_initialized = true;
            Popup.PlacementTargetProperty.Changed.AddClassHandler<Popup>(OnPlacementTargetChanged);
            Control.LoadedEvent.AddClassHandler<Popup>((popup, _) => GetState(popup).Redirect());
            Control.UnloadedEvent.AddClassHandler<Popup>((popup, _) =>
            {
                if (s_popups.TryGetValue(popup, out var state))
                    state.Release();
            });
        }

        private static PopupState GetState(Popup popup) => s_popups.GetValue(popup, p => new PopupState(p));

        private static void OnPlacementTargetChanged(Popup popup, AvaloniaPropertyChangedEventArgs e)
        {
            var state = GetState(popup);
            if (state.IsUpdatingTarget)
                return;

            state.OriginalTarget = e.NewValue as Control;
            state.Redirect();
            state.ReopenIfClosedForNewTarget();
        }

        private static void AddOverlayUser(IEmbeddedPopupOverlay overlay, IEmbeddedPopupHost host)
        {
            s_overlayUsers.TryGetValue(overlay, out var count);
            s_overlayUsers[overlay] = count + 1;
            if (count == 0)
                overlay.Show(host);
        }

        private static void RemoveOverlayUser(IEmbeddedPopupOverlay overlay)
        {
            if (!s_overlayUsers.TryGetValue(overlay, out var count))
                return;

            s_overlayUsers[overlay] = count - 1;
            if (count > 1)
                return;

            // Hide after the current input event, which may be the tap that closed the popup.
            Dispatcher.UIThread.Post(() =>
            {
                if (s_overlayUsers.TryGetValue(overlay, out var users) && users == 0)
                {
                    s_overlayUsers.Remove(overlay);
                    overlay.Hide();
                }
            });
        }

        private sealed class HostState(IEmbeddedPopupHost host)
        {
            public IEmbeddedPopupHost Host { get; } = host;

            public Point? LastPointer { get; set; }
        }

        /// <summary>An invisible control in the overlay that stands in for the placement target of a popup.</summary>
        private sealed class OverlayAnchor(Popup popup) : Control
        {
            public WeakReference<Popup> Popup { get; } = new(popup);
        }

        private sealed class PopupState
        {
            private readonly Popup _popup;
            private readonly OverlayAnchor _anchor;
            private Control? _openTarget;
            private bool _closedForNewTarget;
            private bool _placementOverridden;
            private PlacementMode _savedPlacement;
            private PopupAnchor _savedAnchor;
            private PopupGravity _savedGravity;

            public PopupState(Popup popup)
            {
                _popup = popup;
                _anchor = new OverlayAnchor(popup) { IsHitTestVisible = false };
                OriginalTarget = popup.PlacementTarget;
                popup.Opened += OnOpened;
                popup.Closed += OnClosed;
            }

            public Popup Popup => _popup;

            /// <summary>The placement target set by the app or the control, before the redirect.</summary>
            public Control? OriginalTarget { get; set; }

            public bool IsUpdatingTarget { get; private set; }

            public IEmbeddedPopupOverlay? Overlay { get; private set; }

            public TopLevel? HostTopLevel { get; private set; }

            private Control? EffectiveTarget => OriginalTarget ?? _popup.FindLogicalAncestorOfType<Control>();

            /// <summary>
            /// Points the popup at the overlay anchor when its target is in an embedded view,
            /// and back at the original target when it is not.
            /// </summary>
            public void Redirect()
            {
                if (_openTarget is not null)
                    return;

                var target = EffectiveTarget;
                if (target is not null
                    && TopLevel.GetTopLevel(target) is { } topLevel
                    && s_hosts.TryGetValue(topLevel, out var host))
                {
                    var overlay = host.Host.GetOverlay();
                    if (_anchor.Parent is Canvas oldLayer && oldLayer != overlay.AnchorLayer)
                        oldLayer.Children.Remove(_anchor);
                    if (_anchor.Parent is null)
                    {
                        RemoveCollectedAnchors(overlay.AnchorLayer);
                        overlay.AnchorLayer.Children.Add(_anchor);
                    }

                    Overlay = overlay;
                    HostTopLevel = topLevel;
                    SetTarget(_anchor);
                }
                else
                {
                    Release();
                }
            }

            /// <summary>
            /// Removes the anchor from the overlay and points the popup back at its original target. The popup is
            /// redirected again when it is loaded or gets a new target.
            /// </summary>
            public void Release()
            {
                // An open popup is released when it closes.
                if (_openTarget is not null)
                    return;

                if (_anchor.Parent is Canvas layer)
                    layer.Children.Remove(_anchor);

                Overlay = null;
                HostTopLevel = null;
                if (_popup.PlacementTarget == _anchor)
                    SetTarget(OriginalTarget);
            }

            /// <summary>Opens the popup again if it closed because it got a new placement target.</summary>
            /// <remarks>
            /// A popup closes itself when its new placement target is in another top level than the one it is open
            /// in. A redirected popup is open in the overlay, so that is always the case.
            /// </remarks>
            public void ReopenIfClosedForNewTarget()
            {
                if (!_closedForNewTarget)
                    return;

                _closedForNewTarget = false;
                if (!_popup.IsOpen)
                    _popup.Open();
            }

            /// <summary>Removes the anchors of popups that no longer exist from <paramref name="layer"/>.</summary>
            private static void RemoveCollectedAnchors(Canvas layer)
            {
                for (var i = layer.Children.Count - 1; i >= 0; i--)
                {
                    if (layer.Children[i] is OverlayAnchor anchor && !anchor.Popup.TryGetTarget(out _))
                        layer.Children.RemoveAt(i);
                }
            }

            public void UpdatePosition()
            {
                if (_openTarget is null || HostTopLevel is null || !s_hosts.TryGetValue(HostTopLevel, out var host))
                    return;

                if (host.Host.GetOriginInOverlay() is not { } origin
                    || _openTarget.TranslatePoint(default, HostTopLevel) is not { } targetOrigin)
                    return;

                var bounds = new Rect(origin + targetOrigin, _openTarget.Bounds.Size);
                if (_placementOverridden)
                {
                    var pointer = host.LastPointer ?? targetOrigin;
                    bounds = new Rect(origin + pointer, new Size(1, 1));
                }

                Canvas.SetLeft(_anchor, bounds.X);
                Canvas.SetTop(_anchor, bounds.Y);
                _anchor.Width = bounds.Width;
                _anchor.Height = bounds.Height;
                _anchor.UpdateLayout();

                // Popup only recomputes its position when a placement property changes.
                var offset = _popup.HorizontalOffset;
                _popup.SetCurrentValue(Popup.HorizontalOffsetProperty, offset + 1);
                _popup.SetCurrentValue(Popup.HorizontalOffsetProperty, offset);
            }

            private void SetTarget(Control? target)
            {
                if (_popup.PlacementTarget == target)
                    return;

                IsUpdatingTarget = true;
                try
                {
                    _popup.SetCurrentValue(Popup.PlacementTargetProperty, target);
                }
                finally
                {
                    IsUpdatingTarget = false;
                }
            }

            private void OnOpened(object? sender, EventArgs e)
            {
                if (_popup.PlacementTarget != _anchor || Overlay is null || HostTopLevel is null
                    || !s_hosts.TryGetValue(HostTopLevel, out var host) || EffectiveTarget is not { } target)
                    return;

                _openTarget = target;
                _openTarget.DetachedFromVisualTree += OnTargetDetached;
                s_open.Add(this);
                AddOverlayUser(Overlay, host.Host);

                if (_popup.Placement == PlacementMode.Pointer)
                {
                    _placementOverridden = true;
                    _savedPlacement = _popup.Placement;
                    _savedAnchor = _popup.PlacementAnchor;
                    _savedGravity = _popup.PlacementGravity;
                    _popup.SetCurrentValue(Popup.PlacementAnchorProperty, PopupAnchor.TopLeft);
                    _popup.SetCurrentValue(Popup.PlacementGravityProperty, PopupGravity.BottomRight);
                    _popup.SetCurrentValue(Popup.PlacementProperty, PlacementMode.AnchorAndGravity);
                }

                UpdatePosition();
            }

            private void OnClosed(object? sender, EventArgs e)
            {
                if (_openTarget is null)
                    return;

                _openTarget.DetachedFromVisualTree -= OnTargetDetached;
                _openTarget = null;
                s_open.Remove(this);

                // The popup was redirected, so a target other than the anchor means that it got a new one.
                _closedForNewTarget = _popup.PlacementTarget != _anchor;

                if (_placementOverridden)
                {
                    _placementOverridden = false;
                    _popup.SetCurrentValue(Popup.PlacementProperty, _savedPlacement);
                    _popup.SetCurrentValue(Popup.PlacementAnchorProperty, _savedAnchor);
                    _popup.SetCurrentValue(Popup.PlacementGravityProperty, _savedGravity);
                }

                if (Overlay is not null)
                    RemoveOverlayUser(Overlay);

                // The target may have moved to another view while the popup was open. A new target is redirected
                // when its change is handled.
                if (!_closedForNewTarget)
                    Redirect();
            }

            private void OnTargetDetached(object? sender, VisualTreeAttachmentEventArgs e) => _popup.Close();
        }
    }
}
