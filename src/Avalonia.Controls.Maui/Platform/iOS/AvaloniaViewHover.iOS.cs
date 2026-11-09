using System;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;
using Foundation;
using UIKit;

namespace Avalonia.Controls.Maui.Platforms.iOS.Handlers
{
    /// <summary>
    /// Sends mouse and trackpad hover to the Avalonia content of a UIKit view.
    /// </summary>
    internal sealed class AvaloniaViewHover
    {
        private readonly UIView _view;
        private readonly TopLevel _topLevel;
        private readonly MouseDevice _device = CreateMouseDevice(null);

        private AvaloniaViewHover(UIView view, TopLevel topLevel)
        {
            _view = view;
            _topLevel = topLevel;
        }

        /// <summary>Sends the hover over <paramref name="view"/> to <paramref name="topLevel"/>.</summary>
        /// <returns>The hover handler, or null if hover is not supported.</returns>
        public static AvaloniaViewHover? Attach(UIView view, TopLevel? topLevel)
        {
            if (topLevel is null || !(OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalyst()))
                return null;

            var hover = new AvaloniaViewHover(view, topLevel);
            view.AddGestureRecognizer(new UIHoverGestureRecognizer(hover.OnHover)
            {
                CancelsTouchesInView = false,
                DelaysTouchesBegan = false,
                DelaysTouchesEnded = false,
            });
            return hover;
        }

        /// <summary>
        /// Tells Avalonia that the pointer left the view, for example because the view leaves the window.
        /// </summary>
        /// <remarks>
        /// A view that leaves the window gets no more pointer input, so Avalonia would keep the pointer over its
        /// content. A later render then hit-tests at that old position, which closes tooltips of other views.
        /// </remarks>
        public void Leave() => Send(RawPointerEventType.LeaveWindow, default, RawInputModifiers.None);

        private void OnHover(UIHoverGestureRecognizer recognizer)
        {
            var type = recognizer.State switch
            {
                UIGestureRecognizerState.Began or UIGestureRecognizerState.Changed => RawPointerEventType.Move,
                UIGestureRecognizerState.Ended or UIGestureRecognizerState.Cancelled
                    or UIGestureRecognizerState.Failed => RawPointerEventType.LeaveWindow,
                _ => (RawPointerEventType?)null,
            };

            if (type is null)
                return;

            // UIKit points are Avalonia device-independent units.
            var location = recognizer.LocationInView(_view);
            Send(type.Value, new Point(location.X, location.Y), GetModifiers(recognizer));
        }

        private void Send(RawPointerEventType type, Point position, RawInputModifiers modifiers)
        {
            if (_topLevel.PlatformImpl is not { } impl
                || GetInput(impl) is not { } input
                || _topLevel.GetPresentationSource() is not IInputRoot root)
                return;

            var timestamp = (ulong)(NSProcessInfo.ProcessInfo.SystemUptime * 1000);
            input(CreatePointerEvent(_device, timestamp, root, type, position, modifiers));
        }

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        private static extern MouseDevice CreateMouseDevice(Pointer? pointer);

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
            [UnsafeAccessorType("Avalonia.iOS.AvaloniaView+TopLevelImpl, Avalonia.iOS")] object topLevelImpl);

        private static RawInputModifiers GetModifiers(UIGestureRecognizer recognizer)
        {
            if (!(OperatingSystem.IsIOSVersionAtLeast(13, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 4)))
                return RawInputModifiers.None;

            var flags = recognizer.ModifierFlags;
            var modifiers = RawInputModifiers.None;
            if (flags.HasFlag(UIKeyModifierFlags.Shift))
                modifiers |= RawInputModifiers.Shift;
            if (flags.HasFlag(UIKeyModifierFlags.Control))
                modifiers |= RawInputModifiers.Control;
            if (flags.HasFlag(UIKeyModifierFlags.Alternate))
                modifiers |= RawInputModifiers.Alt;
            if (flags.HasFlag(UIKeyModifierFlags.Command))
                modifiers |= RawInputModifiers.Meta;
            return modifiers;
        }
    }
}
