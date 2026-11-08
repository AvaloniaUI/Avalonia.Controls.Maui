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
        public static void Attach(UIView view, TopLevel? topLevel)
        {
            if (topLevel is null || !(OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalyst()))
                return;

            var hover = new AvaloniaViewHover(view, topLevel);
            view.AddGestureRecognizer(new UIHoverGestureRecognizer(hover.OnHover)
            {
                CancelsTouchesInView = false,
                DelaysTouchesBegan = false,
                DelaysTouchesEnded = false,
            });
        }

        private void OnHover(UIHoverGestureRecognizer recognizer)
        {
            var type = recognizer.State switch
            {
                UIGestureRecognizerState.Began or UIGestureRecognizerState.Changed => RawPointerEventType.Move,
                UIGestureRecognizerState.Ended or UIGestureRecognizerState.Cancelled
                    or UIGestureRecognizerState.Failed => RawPointerEventType.LeaveWindow,
                _ => (RawPointerEventType?)null,
            };

            if (type is null
                || _topLevel.PlatformImpl is not { } impl
                || GetInput(impl) is not { } input
                || _topLevel.GetPresentationSource() is not IInputRoot root)
                return;

            // UIKit points are Avalonia device-independent units.
            var location = recognizer.LocationInView(_view);
            var timestamp = (ulong)(NSProcessInfo.ProcessInfo.SystemUptime * 1000);

            input(CreatePointerEvent(
                _device,
                timestamp,
                root,
                type.Value,
                new Point(location.X, location.Y),
                GetModifiers(recognizer)));
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
