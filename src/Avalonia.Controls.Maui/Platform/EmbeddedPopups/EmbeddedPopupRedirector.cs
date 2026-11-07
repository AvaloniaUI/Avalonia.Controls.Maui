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
    /// On Android and iOS, Avalonia draws popups inside the surface of their <see cref="TopLevel"/>.
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
        }

        private static PopupState GetState(Popup popup) => s_popups.GetValue(popup, p => new PopupState(p));

        private static void OnPlacementTargetChanged(Popup popup, AvaloniaPropertyChangedEventArgs e)
        {
            var state = GetState(popup);
            if (state.IsUpdatingTarget)
                return;

            state.OriginalTarget = e.NewValue as Control;
            state.Redirect();
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

        private sealed class PopupState
        {
            private readonly Popup _popup;
            private readonly Control _anchor = new() { IsHitTestVisible = false };
            private Control? _openTarget;
            private bool _placementOverridden;
            private PlacementMode _savedPlacement;
            private PopupAnchor _savedAnchor;
            private PopupGravity _savedGravity;

            public PopupState(Popup popup)
            {
                _popup = popup;
                OriginalTarget = popup.PlacementTarget;
                popup.Opened += OnOpened;
                popup.Closed += OnClosed;
            }

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
                        overlay.AnchorLayer.Children.Add(_anchor);

                    Overlay = overlay;
                    HostTopLevel = topLevel;
                    SetTarget(_anchor);
                }
                else if (_popup.PlacementTarget == _anchor)
                {
                    Overlay = null;
                    HostTopLevel = null;
                    SetTarget(OriginalTarget);
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

                if (_placementOverridden)
                {
                    _placementOverridden = false;
                    _popup.SetCurrentValue(Popup.PlacementProperty, _savedPlacement);
                    _popup.SetCurrentValue(Popup.PlacementAnchorProperty, _savedAnchor);
                    _popup.SetCurrentValue(Popup.PlacementGravityProperty, _savedGravity);
                }

                if (Overlay is not null)
                    RemoveOverlayUser(Overlay);

                // The target may have moved to another view while the popup was open.
                Redirect();
            }

            private void OnTargetDetached(object? sender, VisualTreeAttachmentEventArgs e) => _popup.Close();
        }
    }
}
