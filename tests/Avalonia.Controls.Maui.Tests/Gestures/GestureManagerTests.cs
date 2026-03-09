using Xunit;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Headless.XUnit;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.Generic;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Tests.Gestures
{
    public class GestureManagerTests : HandlerTestBase<LabelHandler, Microsoft.Maui.Controls.Label>
    {
        [AvaloniaFact(DisplayName = "TapGestureRecognizer fires Tapped event")]
        public async Task TapGesture_Fires()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Tap Me", WidthRequest = 100, HeightRequest = 50 };
            int tapCount = 0;
            var tapGesture = new Microsoft.Maui.Controls.TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => tapCount++;
            label.GestureRecognizers.Add(tapGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Simulate Tap (Pressed + Released)
            var pressed = CreatePointerPressedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(pressed);

            var released = CreatePointerReleasedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(released);

            Assert.Equal(1, tapCount);
        }

        [AvaloniaFact(DisplayName = "DoubleTapGestureRecognizer fires Tapped event")]
        public async Task DoubleTapGesture_Fires()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Double Tap Me", WidthRequest = 100, HeightRequest = 50 };
            int tapCount = 0;
            var tapGesture = new Microsoft.Maui.Controls.TapGestureRecognizer { NumberOfTapsRequired = 2 };
            tapGesture.Tapped += (s, e) => tapCount++;
            label.GestureRecognizers.Add(tapGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Simulate Double Tap (Press/Release x2)
            var p1 = CreatePointerPressedEventArgs(platformView, new Point(10, 10), clickCount: 1);
            platformView.RaiseEvent(p1);
            var r1 = CreatePointerReleasedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(r1);
            
            var p2 = CreatePointerPressedEventArgs(platformView, new Point(10, 10), clickCount: 2);
            platformView.RaiseEvent(p2);
            var r2 = CreatePointerReleasedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(r2);

            Assert.Equal(1, tapCount);
        }

        [AvaloniaFact(DisplayName = "SwipeGestureRecognizer triggers Swiped event")]
        public async Task SwipeGesture_Right_Fires()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Swipe Me", WidthRequest = 200, HeightRequest = 200 };
            bool swiped = false;
            Microsoft.Maui.SwipeDirection? direction = null;
            
            // Default threshold is ~48, but in headless test environment the calculated delta might vary.
            // Using a low threshold ensures the event logic is verified.
            var swipeGesture = new Microsoft.Maui.Controls.SwipeGestureRecognizer { Direction = Microsoft.Maui.SwipeDirection.Right, Threshold = 5 };
            swipeGesture.Swiped += (s, e) => 
            {
                swiped = true;
                direction = e.Direction;
            };
            label.GestureRecognizers.Add(swipeGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Simulate Swipe Right: Press at 10,10 -> Release at 110,10
            var pressed = CreatePointerPressedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(pressed);

            var released = CreatePointerReleasedEventArgs(platformView, new Point(110, 10)); // +100 X
            platformView.RaiseEvent(released);

            Assert.True(swiped, "Swipe Right should have fired");
            Assert.Equal(Microsoft.Maui.SwipeDirection.Right, direction);
        }

        [AvaloniaFact(DisplayName = "PanGestureRecognizer triggers PanUpdated events")]
        public async Task PanGesture_Fires()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Pan Me", WidthRequest = 200, HeightRequest = 200 };
            var statuses = new List<GestureStatus>();
            var panGesture = new Microsoft.Maui.Controls.PanGestureRecognizer();
            panGesture.PanUpdated += (s, e) => statuses.Add(e.StatusType);
            label.GestureRecognizers.Add(panGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // 1. Started (Pressed + Moved?)
            var pressed = CreatePointerPressedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(pressed);
            
            // 2. Running (Moved)
            var moved = CreatePointerMovedEventArgs(platformView, new Point(20, 10));
            platformView.RaiseEvent(moved);

            // 3. Completed (Released)
            var released = CreatePointerReleasedEventArgs(platformView, new Point(20, 10));
            platformView.RaiseEvent(released);

            Assert.Contains(GestureStatus.Started, statuses);
            Assert.Contains(GestureStatus.Running, statuses);
            Assert.Contains(GestureStatus.Completed, statuses);
        }

        [AvaloniaFact(DisplayName = "PointerGestureRecognizer triggers events")]
        public async Task PointerGesture_Fires()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Pointer Me", WidthRequest = 100, HeightRequest = 50 };
            bool entered = false, exited = false, moved = false, pressed = false, released = false;

            var ptrGesture = new Microsoft.Maui.Controls.PointerGestureRecognizer();
            ptrGesture.PointerEntered += (s, e) => entered = true;
            ptrGesture.PointerExited += (s, e) => exited = true;
            ptrGesture.PointerMoved += (s, e) => moved = true;
            ptrGesture.PointerPressed += (s, e) => pressed = true;
            ptrGesture.PointerReleased += (s, e) => released = true;
            label.GestureRecognizers.Add(ptrGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Enter
            platformView.RaiseEvent(CreatePointerEventArgs(platformView, InputElement.PointerEnteredEvent, new Point(10, 10)));
            // Wait for dispatch?
            
            // Moved
            platformView.RaiseEvent(CreatePointerMovedEventArgs(platformView, new Point(15, 15)));

            // Pressed
            platformView.RaiseEvent(CreatePointerPressedEventArgs(platformView, new Point(15, 15)));

            // Released
            platformView.RaiseEvent(CreatePointerReleasedEventArgs(platformView, new Point(15, 15)));
            
            // Exit
            platformView.RaiseEvent(CreatePointerEventArgs(platformView, InputElement.PointerExitedEvent, new Point(0, 0)));

            Assert.True(entered, "PointerEntered failed");
            Assert.True(moved, "PointerMoved failed");
            Assert.True(pressed, "PointerPressed failed");
            Assert.True(released, "PointerReleased failed");
            Assert.True(exited, "PointerExited failed");
        }

        [AvaloniaFact(DisplayName = "PinchGestureRecognizer triggers PinchUpdated events")]
        public async Task PinchGesture_Fires()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Pinch Me", WidthRequest = 200, HeightRequest = 200 };
            var statuses = new List<GestureStatus>();
            var pinchGesture = new Microsoft.Maui.Controls.PinchGestureRecognizer();
            pinchGesture.PinchUpdated += (s, e) => statuses.Add(e.Status);
            label.GestureRecognizers.Add(pinchGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Pinch event (fires Started + Running)
            var pinchArgs = new PinchEventArgs(1.5, new Point(100, 100));
            platformView.RaiseEvent(pinchArgs);

            // Pinch ended (fires Completed)
            var pinchEndedArgs = new PinchEndedEventArgs();
            platformView.RaiseEvent(pinchEndedArgs);

            Assert.Contains(GestureStatus.Started, statuses);
            Assert.Contains(GestureStatus.Running, statuses);
            Assert.Contains(GestureStatus.Completed, statuses);
        }

        [AvaloniaFact(DisplayName = "PinchGestureRecognizer computes correct scale deltas")]
        public async Task PinchGesture_ScaleDelta_IsCorrect()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Pinch Me", WidthRequest = 200, HeightRequest = 200 };
            var scales = new List<double>();
            var pinchGesture = new Microsoft.Maui.Controls.PinchGestureRecognizer();
            pinchGesture.PinchUpdated += (s, e) =>
            {
                if (e.Status == GestureStatus.Running)
                    scales.Add(e.Scale);
            };
            label.GestureRecognizers.Add(pinchGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // First pinch: cumulative scale 1.5 → delta = 1.5 / 1.0 = 1.5
            platformView.RaiseEvent(new PinchEventArgs(1.5, new Point(100, 100)));

            // Second pinch: cumulative scale 3.0 → delta = 3.0 / 1.5 = 2.0
            platformView.RaiseEvent(new PinchEventArgs(3.0, new Point(100, 100)));

            // End pinch
            platformView.RaiseEvent(new PinchEndedEventArgs());

            Assert.Equal(2, scales.Count);
            Assert.Equal(1.5, scales[0], 3);
            Assert.Equal(2.0, scales[1], 3);
        }

        // --- Helpers ---

        private PointerPressedEventArgs CreatePointerPressedEventArgs(Visual target, Point point, int clickCount = 1)
        {
            var pointer = new Pointer(1, PointerType.Mouse, true);
            var properties = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonPressed);

            return new PointerPressedEventArgs(
                target,
                pointer,
                target,
                point,
                0,
                properties,
                KeyModifiers.None,
                clickCount);
        }

        private PointerReleasedEventArgs CreatePointerReleasedEventArgs(Visual target, Point point)
        {
            var pointer = new Pointer(1, PointerType.Mouse, true);
            var properties = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased);

            return new PointerReleasedEventArgs(
                target,
                pointer,
                target,
                point,
                0,
                properties,
                KeyModifiers.None,
                MouseButton.Left);
        }

        private Avalonia.Input.PointerEventArgs CreatePointerMovedEventArgs(Visual target, Point point)
        {
            return CreatePointerEventArgs(target, InputElement.PointerMovedEvent, point);
        }

        private Avalonia.Input.PointerEventArgs CreatePointerEventArgs(Visual target, RoutedEvent routedEvent, Point point)
        {
             var pointer = new Pointer(1, PointerType.Mouse, true);
             return new Avalonia.Input.PointerEventArgs(routedEvent, target, pointer, target, point, 0, new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other), KeyModifiers.None);
        }
    }
}
