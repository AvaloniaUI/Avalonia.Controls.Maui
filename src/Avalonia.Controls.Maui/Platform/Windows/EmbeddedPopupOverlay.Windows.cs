using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Embedding;
using Avalonia.Controls.Maui.Platforms.EmbeddedPopups;
using Avalonia.Input;
using Avalonia.Input.Raw;
using DispatcherTimer = Avalonia.Threading.DispatcherTimer;
using Avalonia.VisualTree;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinUIGrid = Microsoft.UI.Xaml.Controls.Grid;
using WinUIPopup = Microsoft.UI.Xaml.Controls.Primitives.Popup;

namespace Avalonia.Controls.Maui.Platforms.Windows.Handlers
{
    /// <summary>
    /// Connects a WinUI view that embeds Avalonia content to the popup overlays of its window.
    /// </summary>
    /// <remarks>
    /// <see cref="Avalonia.WinUI.AvaloniaSwapChainPanel"/> creates its <see cref="TopLevel"/> when it is loaded
    /// and disposes it when it is unloaded, so the host registers each new top level as it appears.
    /// </remarks>
    internal sealed class EmbeddedPopupHost : IEmbeddedPopupHost
    {
        private readonly FrameworkElement _view;
        private readonly EmbeddedPopupHostMoveTracker _moveTracker = new();
        private TopLevel? _topLevel;
        private global::Windows.Foundation.Size? _windowSize;
        private long _resizedAt = long.MinValue / 2;

        private EmbeddedPopupHost(FrameworkElement view)
        {
            _view = view;
        }

        /// <summary>Registers <paramref name="view"/>, whose Avalonia content root is <paramref name="content"/>.</summary>
        public static EmbeddedPopupHost Attach(FrameworkElement view, Control content)
        {
            var host = new EmbeddedPopupHost(view);
            content.AttachedToVisualTree += (_, _) => host.OnContentAttached(TopLevel.GetTopLevel(content));
            host.OnContentAttached(TopLevel.GetTopLevel(content));

            // LayoutUpdated also reports moves, for example when a page scrolls.
            view.LayoutUpdated += (_, _) => host.OnLayoutUpdated();
            return host;
        }

        public IEmbeddedPopupOverlay GetOverlay() => EmbeddedPopupOverlays.GetIdleOverlay(_view.XamlRoot);

        public IEnumerable<IEmbeddedPopupOverlay> GetWindowOverlays() =>
            EmbeddedPopupOverlays.ForXamlRoot(_view.XamlRoot)?.Overlays ?? (IEnumerable<IEmbeddedPopupOverlay>)Array.Empty<IEmbeddedPopupOverlay>();

        public Point? GetOriginInOverlay() => EmbeddedPopupOverlays.ForXamlRoot(_view.XamlRoot)?.GetOrigin(_view);

        /// <summary>
        /// Closes the light-dismiss popups of the host when it moves, for example because its page scrolled.
        /// </summary>
        /// <remarks>
        /// The overlays cover only the open popups, so the page below can scroll while a popup is open. A
        /// light-dismiss popup closes when its target moves, like a press outside of it. Other popups follow it.
        /// A window resize can move the host too, for example when it is centered. Then the popups only move,
        /// like after a layout of the host.
        /// </remarks>
        private void OnLayoutUpdated()
        {
            if (_topLevel is null)
                return;

            // The host can move a few layout passes after the window resized.
            var windowSize = _view.XamlRoot?.Size;
            if (windowSize != _windowSize)
            {
                _windowSize = windowSize;
                _resizedAt = Environment.TickCount64;
            }

            if (!EmbeddedPopupRedirector.HasOpenLightDismissPopups(_topLevel))
                _moveTracker.Reset();
            else if (_moveTracker.HasMoved(GetOriginInOverlay()))
            {
                _moveTracker.Reset();
                if (Environment.TickCount64 - _resizedAt > EmbeddedPopupRedirector.ResizeMoveDelay.TotalMilliseconds)
                    EmbeddedPopupRedirector.CloseLightDismissPopups(_topLevel);
            }

            EmbeddedPopupRedirector.OnHostLayoutChanged(_topLevel);
        }

        private void OnContentAttached(TopLevel? topLevel)
        {
            if (topLevel is null || topLevel == _topLevel)
                return;

            _topLevel = topLevel;
            EmbeddedPopupRedirector.Register(topLevel, this);

            // Create the overlays now, so that their top levels exist before the first popup opens.
            EmbeddedPopupOverlays.ForXamlRoot(_view.XamlRoot);
        }
    }

    /// <summary>
    /// The popup overlays of a window: window-sized Avalonia views in a WinUI popup, each showing one popup.
    /// </summary>
    /// <remarks>
    /// <para>
    /// WinUI does not draw XAML content under a swap chain panel, even where the swap chain is transparent, and it
    /// can only clip an element to a rectangle. Each overlay is therefore clipped to its popup, and each open popup
    /// has its own overlay, so that the window stays visible and usable between popups. Popups opened from a popup
    /// in an overlay, such as the drop-down of a ComboBox in a flyout, open in another overlay too.
    /// </para>
    /// <para>
    /// An overlay that is not showing a popup is idle. Closed popups have their anchors in an idle overlay, and move
    /// to another one when a popup opens in theirs. There is always an idle overlay, and a new one is created when
    /// the last one starts showing a popup. A while after popups close, the idle overlays but one are removed.
    /// </para>
    /// <para>
    /// The WinUI popup stays open while the window exists, because the Avalonia top level of an overlay only exists
    /// while its panel is loaded, and popup anchors must be in a top level when their popup opens.
    /// </para>
    /// </remarks>
    internal sealed class EmbeddedPopupOverlays
    {
        private static readonly ConditionalWeakTable<XamlRoot, EmbeddedPopupOverlays> s_windows = new();

        // Used while a host is not in a window yet. It is never shown.
        private static EmbeddedPopupOverlay? s_detached;

        private readonly XamlRoot _xamlRoot;
        private readonly WinUIGrid _layers;
        private readonly Microsoft.UI.Xaml.Controls.Border _dismissLayer;
        private static readonly TimeSpan TrimDelay = TimeSpan.FromSeconds(5);

        private readonly List<EmbeddedPopupOverlay> _overlays = new();
        private DispatcherTimer? _trimTimer;
        private int _zIndex;

        private EmbeddedPopupOverlays(XamlRoot xamlRoot)
        {
            _xamlRoot = xamlRoot;
            _layers = new WinUIGrid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            // Below the popups, it takes the presses that close them before they reach the window.
            _dismissLayer = new Microsoft.UI.Xaml.Controls.Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                IsHitTestVisible = false,
            };
            Microsoft.UI.Xaml.Controls.Canvas.SetZIndex(_dismissLayer, -1);
            _dismissLayer.PointerPressed += OnDismissLayerPointerInput;
            // The page below cannot scroll while the layer covers it, so the wheel closes the popups instead.
            _dismissLayer.PointerWheelChanged += OnDismissLayerPointerInput;
            _layers.Children.Add(_dismissLayer);

            // A press in the window outside of the popups and the embedded views (which dismiss for themselves),
            // while the open popups pass it through.
            if (xamlRoot.Content is UIElement content)
                content.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnWindowPointerPressed), true);

            var popup = new WinUIPopup
            {
                Child = _layers,
                XamlRoot = xamlRoot,
                IsLightDismissEnabled = false,
                ShouldConstrainToRootBounds = true,
            };

            UpdateSize();
            xamlRoot.Changed += (_, _) => UpdateSize();
            popup.IsOpen = true;

            AddOverlay();
        }

        /// <summary>Gets the overlays of the window.</summary>
        public IReadOnlyList<EmbeddedPopupOverlay> Overlays => _overlays;

        /// <summary>Gets the overlays of the window with <paramref name="xamlRoot"/>, and creates them if needed.</summary>
        public static EmbeddedPopupOverlays? ForXamlRoot(XamlRoot? xamlRoot) =>
            xamlRoot is null ? null : s_windows.GetValue(xamlRoot, r => new EmbeddedPopupOverlays(r));

        /// <summary>Gets an idle overlay of the window with <paramref name="xamlRoot"/>.</summary>
        public static EmbeddedPopupOverlay GetIdleOverlay(XamlRoot? xamlRoot) =>
            ForXamlRoot(xamlRoot)?.GetIdleOverlay() ?? (s_detached ??= new EmbeddedPopupOverlay(null));

        /// <summary>Gets an idle overlay, preferring one whose top level exists.</summary>
        public EmbeddedPopupOverlay GetIdleOverlay()
        {
            EmbeddedPopupOverlay? idle = null;
            foreach (var overlay in _overlays)
            {
                if (overlay.IsShown)
                    continue;
                if (overlay.IsReady)
                    return overlay;
                idle ??= overlay;
            }

            return idle ?? AddOverlay();
        }

        /// <summary>Gets the top-left of <paramref name="view"/> in overlay coordinates, in DIPs.</summary>
        public Point? GetOrigin(FrameworkElement view)
        {
            if (view.XamlRoot != _xamlRoot || !view.IsLoaded || !_layers.IsLoaded)
                return null;

            try
            {
                var origin = view.TransformToVisual(_layers).TransformPoint(default);
                return new Point(origin.X, origin.Y);
            }
            catch (ArgumentException)
            {
                // The view is not in the visual tree of the window.
                return null;
            }
        }

        /// <summary>Call when <paramref name="overlay"/> starts showing a popup.</summary>
        public void OnShown(EmbeddedPopupOverlay overlay)
        {
            // A popup opened later is above the popups opened before it, such as the one it was opened from.
            if (_zIndex >= 100_000)
            {
                _zIndex = 0;
                foreach (var other in _overlays)
                    Microsoft.UI.Xaml.Controls.Canvas.SetZIndex(other.Panel, other.IsShown ? ++_zIndex : 0);
            }

            Microsoft.UI.Xaml.Controls.Canvas.SetZIndex(overlay.Panel, ++_zIndex);
            PostUpdateDismissLayer();

            // Keep an idle overlay, so that the closed popups of this one have somewhere to go.
            foreach (var other in _overlays)
            {
                if (!other.IsShown)
                    return;
            }

            AddOverlay();
        }

        /// <summary>Call when an overlay stopped showing its popup.</summary>
        /// <remarks>
        /// The extra idle overlays are removed after a delay, so that opening popups again soon does not create
        /// new overlays, whose top levels take a layout pass to exist.
        /// </remarks>
        public void OnHidden()
        {
            PostUpdateDismissLayer();

            if (_trimTimer is null)
            {
                _trimTimer = new DispatcherTimer { Interval = TrimDelay };
                _trimTimer.Tick += (_, _) =>
                {
                    _trimTimer.Stop();
                    RemoveExtraIdleOverlays();
                };
            }

            _trimTimer.Stop();
            _trimTimer.Start();
        }

        /// <summary>Removes the idle overlays but one, after moving their closed popups to it.</summary>
        private void RemoveExtraIdleOverlays()
        {
            var kept = GetIdleOverlay();
            if (!kept.IsReady)
                return;

            foreach (var overlay in _overlays.ToArray())
            {
                // An overlay whose top level does not exist yet may have a popup waiting to open in it.
                if (overlay == kept || !overlay.IsIdle || !overlay.IsReady)
                    continue;

                // Removed from the list first, so that its closed popups move to the kept overlay.
                _overlays.Remove(overlay);
                EmbeddedPopupRedirector.MoveClosedPopups(overlay);

                // The panel disposes its top level and swap chain when it is unloaded.
                _layers.Children.Remove(overlay.Panel);
            }
        }

        private EmbeddedPopupOverlay AddOverlay()
        {
            var overlay = new EmbeddedPopupOverlay(this);
            _overlays.Add(overlay);
            _layers.Children.Add(overlay.Panel);
            return overlay;
        }

        private void UpdateSize()
        {
            _layers.Width = _xamlRoot.Size.Width;
            _layers.Height = _xamlRoot.Size.Height;
        }

        /// <summary>
        /// Makes the dismiss layer take the presses outside of the popups while a popup is open that closes on them
        /// without passing them through, like the light-dismiss layer of an Avalonia window.
        /// </summary>
        public void UpdateDismissLayer()
        {
            var block = EmbeddedPopupRedirector.HasOpenBlockingPopups(_overlays);
            if (_dismissLayer.IsHitTestVisible != block)
                _dismissLayer.IsHitTestVisible = block;
        }

        // A popup opens after its overlay is shown.
        private void PostUpdateDismissLayer() => Avalonia.Threading.Dispatcher.UIThread.Post(UpdateDismissLayer);

        private void OnDismissLayerPointerInput(object sender, PointerRoutedEventArgs e)
        {
            EmbeddedPopupRedirector.LightDismiss(_overlays, null);
            e.Handled = true;
            UpdateDismissLayer();
        }

        private void OnWindowPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            for (var element = e.OriginalSource as DependencyObject; element is not null; element = VisualTreeHelper.GetParent(element))
            {
                if (element is Avalonia.WinUI.AvaloniaSwapChainPanel)
                    return;
            }

            if (EmbeddedPopupRedirector.LightDismiss(_overlays, null))
                e.Handled = true;
        }
    }

    /// <summary>
    /// A window-sized Avalonia view that shows one popup of an embedded Avalonia view, clipped to the popup.
    /// </summary>
    /// <remarks>
    /// The clip also limits hit testing, so pointer input outside the popup goes to the window, and light dismiss
    /// is handled by <see cref="EmbeddedPopupOverlays"/> and the redirector instead of by Avalonia.
    /// </remarks>
    internal sealed partial class EmbeddedPopupOverlay : IEmbeddedPopupOverlay
    {
        private readonly EmbeddedPopupOverlays? _window;
        private readonly OverlayPanel _panel;
        private readonly RectangleGeometry _clip = new();
        private readonly HashSet<uint> _pressedPointers = new();
        private readonly MouseDevice _leaveDevice = CreateMouseDevice(null);
        private bool _hidePending;

        public EmbeddedPopupOverlay(EmbeddedPopupOverlays? window)
        {
            _window = window;

            AnchorLayer = new Canvas();
            TopLevel.SetAutoSafeAreaPadding(AnchorLayer, false);
            AnchorLayer.SizeChanged += (_, _) => EmbeddedPopupRedirector.OnOverlayLayoutChanged(this);
            AnchorLayer.AttachedToVisualTree += (_, _) => OnRootCreated(TopLevel.GetTopLevel(AnchorLayer) as EmbeddableControlRoot);

            _panel = new OverlayPanel
            {
                Content = AnchorLayer,
                // Keep keyboard focus, and with it Avalonia focus, in the embedded view.
                IsTabStop = false,
                Clip = _clip,
            };
            // The panel handles pointer input, so listen for handled events too.
            _panel.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnPanelPointerPressed), true);
            _panel.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnPanelPointerEnded), true);
            _panel.AddHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(OnPanelPointerEnded), true);
            _panel.AddHandler(UIElement.PointerCaptureLostEvent, new PointerEventHandler(OnPanelPointerEnded), true);
            Hide();
        }

        public Canvas AnchorLayer { get; }

        /// <summary>Gets the WinUI view of the overlay.</summary>
        public UIElement Panel => _panel;

        /// <summary>Gets a value indicating whether the overlay is showing a popup.</summary>
        public bool IsShown { get; private set; }

        /// <summary>Gets a value indicating whether the overlay is hidden, and no press on it is in progress.</summary>
        public bool IsIdle => !IsShown && !_hidePending;

        /// <summary>Gets a value indicating whether the Avalonia top level of the overlay exists.</summary>
        public bool IsReady => TopLevel.GetTopLevel(AnchorLayer) is not null;

        public void Show(IEmbeddedPopupHost host)
        {
            if (_window is null)
                return;

            IsShown = true;
            _hidePending = false;
            _panel.Opacity = 1;
            _panel.IsHitTestVisible = true;
            _window.OnShown(this);
            UpdateClip();
        }

        public void Hide()
        {
            IsShown = false;
            _panel.Opacity = 0;
            _clip.Rect = default;

            // The panel stays hit-testable until a press on it ends, so that it gets the end of the press, which it
            // captured. The empty clip lets new input through to the window meanwhile.
            if (_pressedPointers.Count > 0)
            {
                _hidePending = true;
                return;
            }

            _hidePending = false;
            _panel.IsHitTestVisible = false;

            // The pointer is no longer over the overlay, so Avalonia stops updating the tooltip service from it.
            // Hide can run while Avalonia handles a click, so the leave is sent after it.
            Avalonia.Threading.Dispatcher.UIThread.Post(SendPointerLeave);
            _window?.OnHidden();
        }

        private void OnRootCreated(EmbeddableControlRoot? root)
        {
            if (root is null || _window is null)
                return;

            EmbeddedPopupOverlayRoot.MakeTransparent(root);

            // A transparent brush keeps the root hit-testable, so Avalonia tracks the pointer over the root itself,
            // and can clear it when the pointer leaves. The clip of the panel sends input outside the popup to the
            // window.
            root.Background = Avalonia.Media.Brushes.Transparent;

            // Popups opened from the popup of this overlay open in another overlay, and follow it when it moves.
            EmbeddedPopupRedirector.Register(root, new NestedHost(_window));

            // Popups are positioned and sized during Avalonia layout passes.
            root.LayoutUpdated += (_, _) =>
            {
                UpdateClip();
                _window.UpdateDismissLayer();
                EmbeddedPopupRedirector.OnHostLayoutChanged(root);
            };

            EmbeddedPopupRedirector.OnOverlayReady(this);
        }

        /// <summary>Clips the overlay to the bounds of its popup.</summary>
        private void UpdateClip()
        {
            if (!IsShown || TopLevel.GetTopLevel(AnchorLayer) is not { } root)
                return;

            Rect? bounds = null;
            foreach (var host in EmbeddedPopupOverlayRoot.GetPopupHosts(AnchorLayer))
            {
                if (host.TranslatePoint(default, root) is not { } origin)
                    continue;

                var rect = new Rect(origin, host.Bounds.Size);
                bounds = bounds is { } b ? b.Union(rect) : rect;
            }

            var clip = bounds is { } r ? new global::Windows.Foundation.Rect(r.X, r.Y, r.Width, r.Height) : default;
            if (_clip.Rect != clip)
                _clip.Rect = clip;
        }

        private void OnPanelPointerPressed(object sender, PointerRoutedEventArgs e) =>
            _pressedPointers.Add(e.Pointer.PointerId);

        private void OnPanelPointerEnded(object sender, PointerRoutedEventArgs e)
        {
            if (_pressedPointers.Remove(e.Pointer.PointerId) && _pressedPointers.Count == 0 && _hidePending)
                Hide();
        }

        /// <summary>Tells Avalonia that the pointer is no longer over the overlay.</summary>
        private void SendPointerLeave()
        {
            if (IsShown
                || TopLevel.GetTopLevel(AnchorLayer) is not { PlatformImpl: { } impl } root
                || GetInput(impl) is not { } input
                || root.GetPresentationSource() is not IInputRoot inputRoot)
                return;

            var timestamp = (ulong)Environment.TickCount64;
            input(CreatePointerEvent(
                _leaveDevice,
                timestamp,
                inputRoot,
                RawPointerEventType.LeaveWindow,
                default,
                RawInputModifiers.None));
        }

        // The input types of Avalonia are not in its reference assemblies.
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        private static extern MouseDevice CreateMouseDevice(Avalonia.Input.Pointer? pointer);

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        private static extern RawPointerEventArgs CreatePointerEvent(
            IInputDevice device,
            ulong timestamp,
            IInputRoot root,
            RawPointerEventType type,
            Point position,
            RawInputModifiers inputModifiers);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Input")]
        private static extern Action<RawInputEventArgs>? GetInput(
            [UnsafeAccessorType("Avalonia.Win32.SwapChainTopLevelImpl, Avalonia.Win32")] object topLevelImpl);

        /// <summary>The overlay as the host of the popups opened from its popup.</summary>
        private sealed class NestedHost(EmbeddedPopupOverlays window) : IEmbeddedPopupHost
        {
            public IEmbeddedPopupOverlay GetOverlay() => window.GetIdleOverlay();

            public IEnumerable<IEmbeddedPopupOverlay> GetWindowOverlays() => window.Overlays;

            // The overlays are on top of each other.
            public Point? GetOriginInOverlay() => default(Point);
        }

        private sealed partial class OverlayPanel : Avalonia.WinUI.AvaloniaSwapChainPanel
        {
        }
    }
}
