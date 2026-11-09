using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Embedding;
using Avalonia.Controls.Maui.Platforms.EmbeddedPopups;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinUIPopup = Microsoft.UI.Xaml.Controls.Primitives.Popup;

namespace Avalonia.Controls.Maui.Platforms.Windows.Handlers
{
    /// <summary>
    /// Connects a WinUI view that embeds Avalonia content to the popup overlay.
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

        /// <summary>Gets the XAML root of the window that contains the host.</summary>
        public XamlRoot? XamlRoot => _view.XamlRoot;

        public IEmbeddedPopupOverlay GetOverlay() => EmbeddedPopupOverlay.ForXamlRoot(_view.XamlRoot);

        public Point? GetOriginInOverlay() => EmbeddedPopupOverlay.ForXamlRoot(_view.XamlRoot).GetOrigin(_view);

        /// <summary>
        /// Closes the light-dismiss popups of the host when it moves, for example because its page scrolled.
        /// </summary>
        /// <remarks>
        /// The overlay covers only the open popups, so the page below can scroll. A popup would follow its target
        /// out of the scroll viewer, and the overlay keeps popups inside the window, so it would be left over the
        /// title bar or header. On iOS and Android the light-dismiss layer covers the window, so pages cannot scroll.
        /// </remarks>
        private void OnLayoutUpdated()
        {
            if (_topLevel is null)
                return;

            if (!EmbeddedPopupRedirector.HasOpenLightDismissPopups(_topLevel))
                _moveTracker.Reset();
            else if (_moveTracker.HasMoved(GetOriginInOverlay()))
            {
                _moveTracker.Reset();
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

            // Create the overlay now, so that its top level exists before the first popup opens.
            if (_view.XamlRoot is { } xamlRoot)
                EmbeddedPopupOverlay.ForXamlRoot(xamlRoot);
        }
    }

    /// <summary>
    /// A window-sized Avalonia view in a WinUI popup that shows the popups of embedded Avalonia views.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The WinUI popup stays open while the window exists, because the Avalonia top level of the overlay only
    /// exists while its panel is loaded, and popup anchors must be in a top level when their popup opens.
    /// </para>
    /// <para>
    /// WinUI does not draw XAML content under a swap chain panel, even where the swap chain is transparent.
    /// The overlay is therefore clipped to its open popups. The clip also limits hit testing, so pointer input
    /// outside the popups goes to the window, and light dismiss is handled here instead of by Avalonia.
    /// </para>
    /// </remarks>
    internal sealed partial class EmbeddedPopupOverlay : IEmbeddedPopupOverlay
    {
        private static readonly ConditionalWeakTable<XamlRoot, EmbeddedPopupOverlay> s_overlays = new();

        // Used while a host is not in a window yet. It is never shown.
        private static EmbeddedPopupOverlay? s_detached;

        private readonly OverlayPanel _panel;
        private readonly RectangleGeometry _clip = new();
        private readonly WinUIPopup? _popup;
        private readonly XamlRoot? _xamlRoot;
        private readonly HashSet<uint> _pressedPointers = new();
        private readonly MouseDevice _leaveDevice = CreateMouseDevice(null);
        private bool _isShown;
        private bool _hidePending;

        private EmbeddedPopupOverlay(XamlRoot? xamlRoot)
        {
            _xamlRoot = xamlRoot;

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
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            // The panel handles pointer input, so listen for handled events too.
            _panel.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnPanelPointerPressed), true);
            _panel.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnPanelPointerEnded), true);
            _panel.AddHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(OnPanelPointerEnded), true);
            _panel.AddHandler(UIElement.PointerCaptureLostEvent, new PointerEventHandler(OnPanelPointerEnded), true);
            Hide();

            if (xamlRoot is null)
                return;

            // A press in the window outside of the popups and the embedded views (which dismiss for themselves).
            if (xamlRoot.Content is UIElement content)
                content.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnWindowPointerPressed), true);

            _popup = new WinUIPopup
            {
                Child = _panel,
                XamlRoot = xamlRoot,
                IsLightDismissEnabled = false,
                ShouldConstrainToRootBounds = true,
            };

            UpdateSize();
            xamlRoot.Changed += (_, _) => UpdateSize();
            _popup.IsOpen = true;
        }

        public Canvas AnchorLayer { get; }

        /// <summary>Gets the overlay of the window with <paramref name="xamlRoot"/>, and creates it if needed.</summary>
        public static EmbeddedPopupOverlay ForXamlRoot(XamlRoot? xamlRoot)
        {
            if (xamlRoot is null)
                return s_detached ??= new EmbeddedPopupOverlay(null);

            return s_overlays.GetValue(xamlRoot, r => new EmbeddedPopupOverlay(r));
        }

        /// <summary>Gets the top-left of <paramref name="view"/> in overlay coordinates, in DIPs.</summary>
        public Point? GetOrigin(FrameworkElement view)
        {
            if (_popup is null || view.XamlRoot != _xamlRoot || !view.IsLoaded || !_panel.IsLoaded)
                return null;

            try
            {
                var origin = view.TransformToVisual(_panel).TransformPoint(default);
                return new Point(origin.X, origin.Y);
            }
            catch (System.ArgumentException)
            {
                // The view is not in the visual tree of the window.
                return null;
            }
        }

        public void Show(IEmbeddedPopupHost host)
        {
            if (_popup is null || ((EmbeddedPopupHost)host).XamlRoot != _xamlRoot)
                return;

            _popup.IsOpen = true;
            _isShown = true;
            _hidePending = false;
            _panel.Opacity = 1;
            _panel.IsHitTestVisible = true;
            UpdateClip();
        }

        public void Hide()
        {
            _isShown = false;
            _panel.Opacity = 0;
            _clip.Rect = default;

            // A press that light-dismissed the last popup is still in progress, for example a press between two
            // popups. The panel captured the pointer, and may not get the end of the press once it is no longer
            // hit-testable, so Avalonia would keep the pointer pressed and captured. The empty clip already lets
            // new input through to the window. Finish hiding when the press ends.
            if (_pressedPointers.Count > 0)
            {
                _hidePending = true;
                return;
            }

            _hidePending = false;
            _panel.IsHitTestVisible = false;

            // After a click in a popup, Avalonia keeps the pointer over the overlay until WinUI reports that it
            // left. Until then, each render of the overlay updates the tooltip service with the control under that
            // old position, which closes the tooltips of the embedded views. Hide can run while Avalonia handles
            // the click, so send the leave after it.
            Avalonia.Threading.Dispatcher.UIThread.Post(SendPointerLeave);
        }

        private void OnRootCreated(EmbeddableControlRoot? root)
        {
            if (root is null)
                return;

            EmbeddedPopupOverlayRoot.MakeTransparent(root);

            // A transparent brush keeps the root hit-testable, so Avalonia tracks the pointer over the root itself.
            // With no background, the pointer is over nothing once the popup under it closes, and Avalonia can no
            // longer clear IsPointerOver of the root. Its renders would then close the tooltips of embedded views.
            // The clip of the panel still sends input outside the popups to the window.
            root.Background = Avalonia.Media.Brushes.Transparent;

            // Popups are positioned and sized during Avalonia layout passes.
            root.LayoutUpdated += (_, _) => UpdateClip();
        }

        private void UpdateSize()
        {
            if (_xamlRoot is null)
                return;

            _panel.Width = _xamlRoot.Size.Width;
            _panel.Height = _xamlRoot.Size.Height;
        }

        /// <summary>Clips the overlay to the union of the bounds of its open popups.</summary>
        private void UpdateClip()
        {
            if (!_isShown || TopLevel.GetTopLevel(AnchorLayer) is not { } root)
                return;

            Rect? bounds = null;
            foreach (var popup in EmbeddedPopupRedirector.GetOpenPopups(this))
            {
                if (popup.Child is not { } child || child.TranslatePoint(default, root) is not { } origin)
                    continue;

                var rect = new Rect(origin, child.Bounds.Size);
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
            if (_isShown
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

        private void OnWindowPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!_isShown)
                return;

            for (var element = e.OriginalSource as DependencyObject; element is not null; element = VisualTreeHelper.GetParent(element))
            {
                if (element is Avalonia.WinUI.AvaloniaSwapChainPanel)
                    return;
            }

            if (EmbeddedPopupRedirector.LightDismiss(this))
                e.Handled = true;
        }

        private sealed partial class OverlayPanel : Avalonia.WinUI.AvaloniaSwapChainPanel
        {
        }
    }
}
