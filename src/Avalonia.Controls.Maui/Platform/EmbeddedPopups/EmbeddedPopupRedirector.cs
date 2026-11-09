using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;

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
        /// <summary>Gets the overlay for the window of this host, in which its next popup opens.</summary>
        IEmbeddedPopupOverlay GetOverlay();

        /// <summary>Gets all overlays of the window of this host.</summary>
        IEnumerable<IEmbeddedPopupOverlay> GetWindowOverlays() => new[] { GetOverlay() };

        /// <summary>Gets the top-left of this host in overlay coordinates, or null if it is not in a window.</summary>
        Point? GetOriginInOverlay();

        /// <summary>
        /// Gets a value indicating whether this host is in a window. The popups of a host that is not are hidden
        /// until it is added to one.
        /// </summary>
        bool IsInWindow => true;

        /// <summary>
        /// Gets a value indicating whether the position of this host is checked on each frame of the overlay while
        /// it has open popups.
        /// </summary>
        /// <remarks>
        /// For platforms that do not lay out a view when a native scroll view that contains it scrolls.
        /// </remarks>
        bool FollowsMovesEachFrame => false;

        /// <summary>Gets the size of the window of this host, or null if it is not in a window.</summary>
        /// <remarks>Only compared with earlier values, so it can be in any unit.</remarks>
        Size? GetWindowSize() => null;
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
                        && LightDismiss(host.GetWindowOverlays(), topLevel))
                        e.Handled = true;
                },
                RoutingStrategies.Tunnel);
        }

        /// <summary>
        /// Closes the open light-dismiss popups of <paramref name="overlay"/> after a press outside of them.
        /// </summary>
        /// <returns>True if the press should be handled, because a closed popup does not pass it through.</returns>
        public static bool LightDismiss(IEmbeddedPopupOverlay overlay) => LightDismiss(new[] { overlay }, null);

        /// <summary>
        /// Closes the open light-dismiss popups of <paramref name="overlays"/> after a press outside of them.
        /// </summary>
        /// <param name="overlays">The overlays of the window that was pressed.</param>
        /// <param name="pressed">
        /// The top level that was pressed, or null for a press outside of Avalonia. Its popups, and the popups they
        /// were opened from, stay open.
        /// </param>
        /// <returns>True if the press should be handled, because a closed popup does not pass it through.</returns>
        public static bool LightDismiss(IEnumerable<IEmbeddedPopupOverlay> overlays, TopLevel? pressed)
        {
            var popups = new List<Popup>();
            foreach (var overlay in overlays)
            {
                foreach (var state in s_open)
                {
                    if (state.Overlay == overlay)
                        popups.Add(state.Popup);
                }

                // Also the popups opened from a popup in the overlay, which are only tracked where they are redirected.
                foreach (var host in EmbeddedPopupOverlayRoot.GetPopupHosts(overlay.AnchorLayer))
                {
                    if (EmbeddedPopupOverlayRoot.GetPopup(host) is { } popup && !popups.Contains(popup))
                        popups.Add(popup);
                }
            }

            // A press in a popup keeps it open, and the popups it was opened from.
            var keep = new HashSet<TopLevel>();
            var pending = new Stack<TopLevel>();
            if (pressed is not null)
                pending.Push(pressed);
            while (pending.TryPop(out var topLevel))
            {
                if (!keep.Add(topLevel))
                    continue;

                foreach (var popup in popups)
                {
                    if (popup.Child is { } child && TopLevel.GetTopLevel(child) == topLevel
                        && TopLevel.GetTopLevel(popup) is { } owner)
                        pending.Push(owner);
                }
            }

            var handled = false;
            foreach (var popup in popups)
            {
                // Closing a popup can close the popups that were opened from it.
                if (!popup.IsOpen || !popup.IsLightDismissEnabled
                    || (popup.Child is { } child && TopLevel.GetTopLevel(child) is { } shownIn && keep.Contains(shownIn)))
                    continue;

                handled |= !popup.OverlayDismissEventPassThrough;
                popup.Close();
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
        /// Hides the open popups of <paramref name="topLevel"/> after its host view left its window, for example after
        /// a navigation to another page. Closes its light-dismiss popups.
        /// </summary>
        /// <remarks>
        /// The overlay stays in the window, so the popups would stay above the next page. The popups that the app
        /// keeps open are only hidden, like the popups of a hidden Avalonia window, and are shown again when the host
        /// returns.
        /// </remarks>
        public static void OnHostLeftWindow(TopLevel? topLevel)
        {
            if (topLevel is null)
                return;

            if (s_hosts.TryGetValue(topLevel, out var host))
                host.StopFollowing();

            CloseLightDismissPopups(topLevel);
            foreach (var state in s_open.ToArray())
            {
                if (state.HostTopLevel == topLevel)
                    state.Hide();
            }
        }

        /// <summary>
        /// Shows the popups of <paramref name="topLevel"/> again after its host view was added to a window. Popups
        /// that are open in the overlay of another window reopen in the overlay of the new one.
        /// </summary>
        public static void OnHostEnteredWindow(TopLevel? topLevel)
        {
            if (topLevel is null || !s_hosts.TryGetValue(topLevel, out var host))
                return;

            var overlay = host.Host.GetOverlay();
            foreach (var state in s_open.ToArray())
            {
                if (state.HostTopLevel != topLevel)
                    continue;

                if (state.Overlay != overlay)
                    state.Reopen();
                else
                    state.Show(host.Host);
            }

            FollowHost(topLevel, host);
        }

        /// <summary>
        /// Checks the position of the host of <paramref name="topLevel"/> on each frame of its overlay while it has
        /// open popups, if the host asks for it. A move closes its light-dismiss popups and moves the others.
        /// </summary>
        /// <remarks>
        /// A native scroll view that scrolls, or a navigation transition, moves the host without a layout of it.
        /// The popups of an Avalonia window close when the window moves, so light-dismiss popups close here too.
        /// A window resize can move the host without a layout of it too, for example when it is centered. Then
        /// the popups only move, like after a layout.
        /// </remarks>
        private static void FollowHost(TopLevel topLevel, HostState host)
        {
            if (!host.Host.FollowsMovesEachFrame || host.IsFollowing || !host.Host.IsInWindow
                || TopLevel.GetTopLevel(host.Host.GetOverlay().AnchorLayer) is not { } overlayRoot)
                return;

            host.IsFollowing = true;
            var version = host.FollowVersion;
            host.ResetMoveTracker();
            var windowSize = host.Host.GetWindowSize();
            TimeSpan? resizedAt = null;

            void OnFrame(TimeSpan time)
            {
                if (version != host.FollowVersion)
                    return;

                if (!HasOpenPopups(topLevel))
                {
                    host.IsFollowing = false;
                    return;
                }

                // The host can move a few frames after the window resized, when the page was laid out.
                var newWindowSize = host.Host.GetWindowSize();
                if (newWindowSize != windowSize)
                {
                    windowSize = newWindowSize;
                    resizedAt = time;
                }

                if (host.MoveTracker.HasMoved(host.Host.GetOriginInOverlay()))
                {
                    if (resizedAt is not { } resized || time - resized > s_resizeMoveDelay)
                        CloseLightDismissPopups(topLevel);
                    OnHostLayoutChanged(topLevel);
                }

                overlayRoot.RequestAnimationFrame(OnFrame);
            }

            overlayRoot.RequestAnimationFrame(OnFrame);
        }

        /// <summary>How long after a window resize a move of a host counts as caused by the resize.</summary>
        private static readonly TimeSpan s_resizeMoveDelay = TimeSpan.FromMilliseconds(250);

        private static bool HasOpenPopups(TopLevel topLevel)
        {
            foreach (var state in s_open)
            {
                if (state.HostTopLevel == topLevel)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Moves the open popups of <paramref name="topLevel"/> after its host view moved or resized.
        /// </summary>
        public static void OnHostLayoutChanged(TopLevel? topLevel)
        {
            if (topLevel is null || s_open.Count == 0)
                return;

            // A layout moves the popups with the host, and does not close them.
            if (s_hosts.TryGetValue(topLevel, out var host))
                host.ResetMoveTracker();

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

            // Presses in a popup are routed through its logical parents, so they never reach the top level that
            // shows it. Where that top level is registered (the overlays on Windows), handle them on the popup host.
            InputElement.PointerPressedEvent.AddClassHandler<OverlayPopupHost>(OnPopupHostPressed, RoutingStrategies.Tunnel, true);
            InputElement.PointerMovedEvent.AddClassHandler<OverlayPopupHost>(
                (popupHost, e) =>
                {
                    if (GetPressedHost(popupHost, e) is var (topLevel, host))
                        host.LastPointer = e.GetPosition(topLevel);
                },
                RoutingStrategies.Tunnel,
                true);
        }

        private static void OnPopupHostPressed(OverlayPopupHost popupHost, PointerPressedEventArgs e)
        {
            if (GetPressedHost(popupHost, e) is not var (topLevel, host))
                return;

            host.LastPointer = e.GetPosition(topLevel);
            if (!e.Handled && LightDismiss(host.Host.GetWindowOverlays(), topLevel))
                e.Handled = true;
        }

        /// <summary>
        /// Gets the registered top level that shows <paramref name="popupHost"/>, if the pointer event is in it.
        /// </summary>
        /// <remarks>
        /// A press in a popup that was opened from another popup is routed through the host of that one too.
        /// </remarks>
        private static (TopLevel, HostState)? GetPressedHost(OverlayPopupHost popupHost, RoutedEventArgs e) =>
            TopLevel.GetTopLevel(popupHost) is { } topLevel
            && e.Source is Visual source && TopLevel.GetTopLevel(source) == topLevel
            && s_hosts.TryGetValue(topLevel, out var host)
                ? (topLevel, host)
                : null;

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
            {
                overlay.Show(host);

                // Where each popup has its own overlay (Windows), the closed popups move to another overlay.
                MoveClosedPopups(overlay);
            }
        }

        /// <summary>
        /// Moves the closed popups of <paramref name="overlay"/> to the overlay that their hosts now return, for
        /// example before the overlay is removed.
        /// </summary>
        public static void MoveClosedPopups(IEmbeddedPopupOverlay overlay)
        {
            foreach (var (_, state) in s_popups)
            {
                if (state.Overlay == overlay && !state.IsOpen)
                    state.Redirect();
            }
        }

        /// <summary>
        /// Opens the popups of <paramref name="overlay"/> that were asked to open before its top level existed.
        /// </summary>
        public static void OnOverlayReady(IEmbeddedPopupOverlay overlay)
        {
            foreach (var (_, state) in s_popups)
            {
                if (state.Overlay == overlay)
                    state.OpenIfRequested();
            }
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

            /// <summary>Incremented to stop the popups from following the host.</summary>
            public int FollowVersion { get; private set; }

            public bool IsFollowing { get; set; }

            public EmbeddedPopupHostMoveTracker MoveTracker { get; } = new();

            /// <summary>Makes the current position of the host the reference for the next move.</summary>
            public void ResetMoveTracker()
            {
                MoveTracker.Reset();
                MoveTracker.HasMoved(Host.GetOriginInOverlay());
            }

            public void StopFollowing()
            {
                FollowVersion++;
                IsFollowing = false;
            }
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
            private Control? _watchedTarget;
            private Rect? _anchorBounds;
            private List<Control>? _hiddenHosts;
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

            /// <summary>Gets a value indicating whether the popup is open in its overlay.</summary>
            public bool IsOpen => _openTarget is not null;

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
                    ReleaseAnchor();
                }

                Watch(_popup.IsLoaded ? target : null);
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

                Watch(null);
                ReleaseAnchor();
            }

            private void ReleaseAnchor()
            {
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

            /// <summary>Opens the popup if it was asked to open while its anchor was not in a top level.</summary>
            /// <remarks>
            /// Popup only retries to open when it is attached to the visual tree itself, not when its target is.
            /// </remarks>
            public void OpenIfRequested()
            {
                if (_openTarget is null && _popup.IsOpen && _popup.PlacementTarget == _anchor
                    && TopLevel.GetTopLevel(_anchor) is not null)
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
                if (GetAnchorBounds() is not { } bounds)
                    return;

                _anchorBounds = bounds;
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

            /// <summary>
            /// Hides the open popup and the popups that were opened from it, while its host is not in a window.
            /// The popups opened from it that are light-dismiss close.
            /// </summary>
            public void Hide()
            {
                if (_hiddenHosts is not null || Overlay is null)
                    return;

                HideHosts();
                RemoveOverlayUser(Overlay);
            }

            /// <summary>Hides the hosts of the popup and of the popups opened from it, and closes the light-dismiss ones.</summary>
            private void HideHosts()
            {
                _hiddenHosts = new List<Control>();
                if (_popup.Child?.FindAncestorOfType<OverlayPopupHost>() is { } popupHost)
                {
                    popupHost.IsVisible = false;
                    _hiddenHosts.Add(popupHost);
                }

                if (Overlay is null)
                    return;

                // Closing a popup removes its host from the overlay.
                foreach (var (nestedHost, nested) in GetNestedPopups(Overlay))
                {
                    if (nested.IsLightDismissEnabled)
                        nested.Close();
                    else
                    {
                        nestedHost.IsVisible = false;
                        _hiddenHosts.Add(nestedHost);
                    }
                }
            }

            /// <summary>
            /// Gets the popups in <paramref name="overlay"/> that were opened from this popup, or from a popup that
            /// was opened from it, at any depth.
            /// </summary>
            /// <remarks>
            /// A popup is declared in the content of the popup it was opened from, which is shown in another host.
            /// </remarks>
            private List<(OverlayPopupHost Host, Popup Popup)> GetNestedPopups(IEmbeddedPopupOverlay overlay)
            {
                var nestedPopups = new List<(OverlayPopupHost, Popup)>();
                if (_popup.Child is not { } child)
                    return nestedPopups;

                var owners = new List<Visual> { child };
                var candidates = new List<OverlayPopupHost>(EmbeddedPopupOverlayRoot.GetPopupHosts(overlay.AnchorLayer));
                bool found;
                do
                {
                    found = false;
                    for (var i = candidates.Count - 1; i >= 0; i--)
                    {
                        if (EmbeddedPopupOverlayRoot.GetPopup(candidates[i]) is not { } nested || nested == _popup
                            || !owners.Exists(owner => owner.IsVisualAncestorOf(nested)))
                            continue;

                        nestedPopups.Add((candidates[i], nested));
                        if (nested.Child is { } nestedChild)
                            owners.Add(nestedChild);
                        candidates.RemoveAt(i);
                        found = true;
                    }
                }
                while (found);

                return nestedPopups;
            }

            /// <summary>Shows the popups that <see cref="Hide"/> hid.</summary>
            public void Show(IEmbeddedPopupHost host)
            {
                if (_hiddenHosts is not { } hiddenHosts || Overlay is null)
                    return;

                _hiddenHosts = null;
                foreach (var hiddenHost in hiddenHosts)
                    hiddenHost.IsVisible = true;

                AddOverlayUser(Overlay, host);
                UpdatePosition();
            }

            /// <summary>Opens the popup again, in the overlay that its host now returns.</summary>
            public void Reopen()
            {
                _popup.Close();
                _popup.Open();
            }

            /// <summary>Gets the bounds of the open target in overlay coordinates.</summary>
            private Rect? GetAnchorBounds()
            {
                if (_openTarget is null || HostTopLevel is null || !s_hosts.TryGetValue(HostTopLevel, out var host))
                    return null;

                if (host.Host.GetOriginInOverlay() is not { } origin
                    || _openTarget.TranslatePoint(default, HostTopLevel) is not { } targetOrigin)
                    return null;

                if (_placementOverridden)
                {
                    var pointer = host.LastPointer ?? targetOrigin;
                    return new Rect(origin + pointer, new Size(1, 1));
                }

                return new Rect(origin + targetOrigin, _openTarget.Bounds.Size);
            }

            /// <summary>
            /// Follows <paramref name="target"/> into and out of the embedded views while the popup is closed, so
            /// that the popup opens in the overlay of the view that contains the target.
            /// </summary>
            private void Watch(Control? target)
            {
                if (_watchedTarget == target)
                    return;

                if (_watchedTarget is not null)
                {
                    _watchedTarget.AttachedToVisualTree -= OnWatchedTargetChanged;
                    _watchedTarget.DetachedFromVisualTree -= OnWatchedTargetChanged;
                }

                _watchedTarget = target;
                if (target is not null)
                {
                    target.AttachedToVisualTree += OnWatchedTargetChanged;
                    target.DetachedFromVisualTree += OnWatchedTargetChanged;
                }
            }

            // An open popup closes when its target detaches, and is redirected then.
            private void OnWatchedTargetChanged(object? sender, VisualTreeAttachmentEventArgs e) => Redirect();

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
                // The target can move in its view without a layout of the host view, for example when it scrolls.
                _openTarget.LayoutUpdated += OnTargetLayoutUpdated;
                s_open.Add(this);

                // The overlay is not shown for a host that is not in a window, and a shared overlay must not count
                // the popup as shown. The popup shows when the host is added to a window.
                if (host.Host.IsInWindow)
                    AddOverlayUser(Overlay, host.Host);
                else
                    HideHosts();

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
                FollowHost(HostTopLevel, host);
            }

            private void OnClosed(object? sender, EventArgs e)
            {
                if (_openTarget is null)
                    return;

                _openTarget.DetachedFromVisualTree -= OnTargetDetached;
                _openTarget.LayoutUpdated -= OnTargetLayoutUpdated;
                _openTarget = null;
                _anchorBounds = null;
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

                // A hidden popup is not a user of its overlay.
                if (Overlay is not null && _hiddenHosts is null)
                    RemoveOverlayUser(Overlay);
                _hiddenHosts = null;

                // The target may have moved to another view while the popup was open. A new target is redirected
                // when its change is handled.
                if (!_closedForNewTarget)
                    Redirect();
            }

            private void OnTargetDetached(object? sender, VisualTreeAttachmentEventArgs e) => _popup.Close();

            private void OnTargetLayoutUpdated(object? sender, EventArgs e)
            {
                if (GetAnchorBounds() is { } bounds && bounds != _anchorBounds)
                    UpdatePosition();
            }
        }
    }
}
