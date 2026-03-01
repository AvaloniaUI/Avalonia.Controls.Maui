using Xunit;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Headless.XUnit;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Maui.Controls;
using AvaloniaDragEventArgs = Avalonia.Input.DragEventArgs;

namespace Avalonia.Controls.Maui.Tests.Gestures
{
    public class DragDropGestureTests : HandlerTestBase<LabelHandler, Microsoft.Maui.Controls.Label>
    {
        // --- Drag Source Tests ---

        [AvaloniaFact(DisplayName = "DragGesture fires DragStarting after threshold move")]
        public async Task DragGesture_FiresDragStarting_AfterThresholdMove()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Drag Me", WidthRequest = 100, HeightRequest = 50 };
            bool dragStartingFired = false;
            var dragGesture = new DragGestureRecognizer();
            dragGesture.DragStarting += (s, e) =>
            {
                dragStartingFired = true;
                e.Cancel = true; // Cancel to prevent DoDragDropAsync (no platform drag source in tests)
            };
            label.GestureRecognizers.Add(dragGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Press at (10,10)
            var pressed = CreatePointerPressedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(pressed);

            // Move past threshold (5px) to (20, 10) - distance = 10px
            var moved = CreatePointerMovedEventArgs(platformView, new Point(20, 10));
            platformView.RaiseEvent(moved);

            // Allow async drag initiation to run
            await Task.Delay(50);

            Assert.True(dragStartingFired, "DragStarting should have fired after moving past threshold");
        }

        [AvaloniaFact(DisplayName = "DragGesture does not fire on simple tap")]
        public async Task DragGesture_DoesNotFire_OnSimpleTap()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Tap Me", WidthRequest = 100, HeightRequest = 50 };
            bool dragStartingFired = false;
            var dragGesture = new DragGestureRecognizer();
            dragGesture.DragStarting += (s, e) => dragStartingFired = true;
            label.GestureRecognizers.Add(dragGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Press and release at same point (no movement)
            var pressed = CreatePointerPressedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(pressed);

            var released = CreatePointerReleasedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(released);

            Assert.False(dragStartingFired, "DragStarting should not fire on simple tap");
        }

        [AvaloniaFact(DisplayName = "DragGesture respects CanDrag=false")]
        public async Task DragGesture_CanDragFalse_DoesNotDrag()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "No Drag", WidthRequest = 100, HeightRequest = 50 };
            bool dragStartingFired = false;
            var dragGesture = new DragGestureRecognizer { CanDrag = false };
            dragGesture.DragStarting += (s, e) => dragStartingFired = true;
            label.GestureRecognizers.Add(dragGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Press at (10,10)
            var pressed = CreatePointerPressedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(pressed);

            // Move past threshold
            var moved = CreatePointerMovedEventArgs(platformView, new Point(20, 10));
            platformView.RaiseEvent(moved);

            await Task.Delay(50);

            Assert.False(dragStartingFired, "DragStarting should not fire when CanDrag=false");
        }

        [AvaloniaFact(DisplayName = "DragGesture Cancel prevents drag operation")]
        public async Task DragGesture_Cancel_PreventsDrag()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Cancel Drag", WidthRequest = 100, HeightRequest = 50 };
            bool dropCompletedFired = false;
            var dragGesture = new DragGestureRecognizer();
            dragGesture.DragStarting += (s, e) => e.Cancel = true;
            dragGesture.DropCompleted += (s, e) => dropCompletedFired = true;
            label.GestureRecognizers.Add(dragGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Press and move past threshold
            var pressed = CreatePointerPressedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(pressed);

            var moved = CreatePointerMovedEventArgs(platformView, new Point(20, 10));
            platformView.RaiseEvent(moved);

            await Task.Delay(50);

            Assert.False(dropCompletedFired, "DropCompleted should not fire when drag is cancelled");
        }

        // --- Drop Target Tests ---

        [AvaloniaFact(DisplayName = "DropGesture fires DragOver on DragEnter")]
        public async Task DropGesture_FiresDragOver()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Drop Here", WidthRequest = 100, HeightRequest = 50 };
            bool dragOverFired = false;
            var dropGesture = new DropGestureRecognizer();
            dropGesture.DragOver += (s, e) => dragOverFired = true;
            label.GestureRecognizers.Add(dropGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Populate the bridge to simulate an active drag
            DragDropDataBridge.ActiveDataPackage = new DataPackage();
            DragDropDataBridge.ActiveDataPackage.Text = "test";

            try
            {
                // Raise DragEnter event
                var dragEnterArgs = CreateDragEventArgs(DragDrop.DragEnterEvent, platformView, new Point(10, 10));
                platformView.RaiseEvent(dragEnterArgs);

                Assert.True(dragOverFired, "DragOver should fire on DragEnter");
            }
            finally
            {
                DragDropDataBridge.Clear();
            }
        }

        [AvaloniaFact(DisplayName = "DropGesture fires DragLeave")]
        public async Task DropGesture_FiresDragLeave()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Drop Here", WidthRequest = 100, HeightRequest = 50 };
            bool dragLeaveFired = false;
            var dropGesture = new DropGestureRecognizer();
            dropGesture.DragLeave += (s, e) => dragLeaveFired = true;
            label.GestureRecognizers.Add(dropGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            DragDropDataBridge.ActiveDataPackage = new DataPackage();

            try
            {
                var dragLeaveArgs = CreateDragEventArgs(DragDrop.DragLeaveEvent, platformView, new Point(10, 10));
                platformView.RaiseEvent(dragLeaveArgs);

                Assert.True(dragLeaveFired, "DragLeave should fire");
            }
            finally
            {
                DragDropDataBridge.Clear();
            }
        }

        [AvaloniaFact(DisplayName = "DropGesture fires Drop with DataPackageView")]
        public async Task DropGesture_FiresDrop()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Drop Here", WidthRequest = 100, HeightRequest = 50 };
            bool dropFired = false;
            string? droppedText = null;
            var dropGesture = new DropGestureRecognizer();
            dropGesture.Drop += async (s, e) =>
            {
                dropFired = true;
                droppedText = await e.Data.GetTextAsync();
            };
            label.GestureRecognizers.Add(dropGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            var dataPackage = new DataPackage();
            dataPackage.Text = "Hello World";
            DragDropDataBridge.ActiveDataPackage = dataPackage;

            try
            {
                var dropArgs = CreateDragEventArgs(DragDrop.DropEvent, platformView, new Point(10, 10));
                platformView.RaiseEvent(dropArgs);

                // Allow async drop handler to complete
                await Task.Delay(100);

                Assert.True(dropFired, "Drop should fire");
                Assert.Equal("Hello World", droppedText);
            }
            finally
            {
                DragDropDataBridge.Clear();
            }
        }

        [AvaloniaFact(DisplayName = "DropGesture respects AllowDrop=false")]
        public async Task DropGesture_AllowDropFalse_DoesNotDrop()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "No Drop", WidthRequest = 100, HeightRequest = 50 };
            bool dropFired = false;
            var dropGesture = new DropGestureRecognizer { AllowDrop = false };
            dropGesture.Drop += (s, e) => dropFired = true;
            label.GestureRecognizers.Add(dropGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            DragDropDataBridge.ActiveDataPackage = new DataPackage();

            try
            {
                var dropArgs = CreateDragEventArgs(DragDrop.DropEvent, platformView, new Point(10, 10));
                platformView.RaiseEvent(dropArgs);

                await Task.Delay(50);

                Assert.False(dropFired, "Drop should not fire when AllowDrop=false");
            }
            finally
            {
                DragDropDataBridge.Clear();
            }
        }

        // --- Integration Tests ---

        [AvaloniaFact(DisplayName = "Tap still fires on drag view without threshold movement")]
        public async Task DragAndTap_TapStillFires_WithoutDrag()
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Tap + Drag", WidthRequest = 100, HeightRequest = 50 };
            bool tapped = false;
            bool dragStarted = false;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => tapped = true;
            label.GestureRecognizers.Add(tapGesture);

            var dragGesture = new DragGestureRecognizer();
            dragGesture.DragStarting += (s, e) => dragStarted = true;
            label.GestureRecognizers.Add(dragGesture);

            var handler = await CreateHandlerAsync(label);
            var platformView = handler.PlatformView!;

            // Simple tap: press and release at same point
            var pressed = CreatePointerPressedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(pressed);

            var released = CreatePointerReleasedEventArgs(platformView, new Point(10, 10));
            platformView.RaiseEvent(released);

            Assert.True(tapped, "Tap should still fire on view with DragGestureRecognizer");
            Assert.False(dragStarted, "DragStarting should not fire on simple tap");
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

        private AvaloniaDragEventArgs CreateDragEventArgs(RoutedEvent<AvaloniaDragEventArgs> routedEvent, Interactive target, Point point)
        {
            var dataTransfer = new DataTransfer();
            dataTransfer.Add(DataTransferItem.CreateText("test"));
            return new AvaloniaDragEventArgs(routedEvent, dataTransfer, target, point, KeyModifiers.None);
        }
    }
}
