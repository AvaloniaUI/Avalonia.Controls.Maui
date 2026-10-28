using Avalonia.Controls.Maui.SkiaSharp.Views.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using SkiaSharp.Views.Maui;

namespace Avalonia.Controls.Maui.Tests.Handlers.SkiaSharp;

public class SKViewControlTouchTests
{
    public static TheoryData<string> Controls => new() { nameof(SKCanvasViewControl), nameof(SKGLViewControl) };

    [AvaloniaTheory(DisplayName = "Mouse hover raises Entered, Moved and Exited without contact")]
    [MemberData(nameof(Controls))]
    public void MouseHoverRaisesEnteredMovedAndExitedWithoutContact(string controlType)
    {
        var touches = HostAndRecord(controlType, window =>
        {
            window.MouseMove(new Point(20, 20));
            window.MouseMove(new Point(30, 30));
            window.MouseMove(new Point(150, 150));
        });

        Assert.True(touches.Count >= 3);
        Assert.Equal(SKTouchAction.Entered, touches[0].ActionType);
        Assert.Equal(SKTouchAction.Exited, touches[^1].ActionType);
        Assert.All(touches.Skip(1).SkipLast(1), t => Assert.Equal(SKTouchAction.Moved, t.ActionType));
        Assert.All(touches, t => Assert.False(t.InContact));
        Assert.All(touches, t => Assert.Equal(SKTouchDeviceType.Mouse, t.DeviceType));
    }

    [AvaloniaTheory(DisplayName = "Mouse drag still raises Pressed, Moved and Released in contact")]
    [MemberData(nameof(Controls))]
    public void MouseDragStillRaisesPressedMovedAndReleasedInContact(string controlType)
    {
        var touches = HostAndRecord(controlType, window =>
        {
            window.MouseMove(new Point(20, 20));
            window.MouseDown(new Point(20, 20), MouseButton.Left);
            window.MouseMove(new Point(30, 30), RawInputModifiers.LeftMouseButton);
            window.MouseUp(new Point(30, 30), MouseButton.Left);
        });

        var contact = touches.Where(t => t.ActionType is SKTouchAction.Pressed or SKTouchAction.Released || t.InContact).ToList();
        Assert.Equal(new[] { SKTouchAction.Pressed, SKTouchAction.Moved, SKTouchAction.Released }, contact.Select(t => t.ActionType));
        Assert.True(contact[1].InContact);
    }

    [AvaloniaTheory(DisplayName = "Exited and Entered during a drag have the drag's touch ID")]
    [MemberData(nameof(Controls))]
    public void ExitedAndEnteredDuringDragHaveDragTouchId(string controlType)
    {
        var touches = HostAndRecord(controlType, window =>
        {
            window.MouseMove(new Point(20, 20));
            window.MouseDown(new Point(20, 20), MouseButton.Left);
            window.MouseMove(new Point(150, 150), RawInputModifiers.LeftMouseButton);
            window.MouseMove(new Point(30, 30), RawInputModifiers.LeftMouseButton);
            window.MouseUp(new Point(30, 30), MouseButton.Left);
        });

        var pressed = Assert.Single(touches, t => t.ActionType == SKTouchAction.Pressed);
        var drag = touches.SkipWhile(t => t != pressed).ToList();
        Assert.Contains(drag, t => t.ActionType == SKTouchAction.Exited);
        Assert.Contains(drag, t => t.ActionType == SKTouchAction.Entered);
        Assert.All(drag, t => Assert.Equal(pressed.Id, t.Id));
    }

    [AvaloniaTheory(DisplayName = "Every touch action of a pointer has the pointer's ID")]
    [MemberData(nameof(Controls))]
    public void EveryTouchActionOfPointerHasPointerId(string controlType)
    {
        // Hover and contact share one ID policy, so a hovering mouse and a touch contact cannot share an ID
        var touches = HostAndRecord(controlType, window =>
        {
            window.MouseMove(new Point(20, 20));
            window.MouseDown(new Point(20, 20), MouseButton.Left);
            window.MouseMove(new Point(30, 30), RawInputModifiers.LeftMouseButton);
            window.MouseUp(new Point(30, 30), MouseButton.Left);
            window.MouseMove(new Point(40, 40));
            window.MouseDown(new Point(40, 40), MouseButton.Left);
            window.MouseUp(new Point(40, 40), MouseButton.Left);
        });

        Assert.Equal(2, touches.Count(t => t.ActionType == SKTouchAction.Pressed));
        Assert.Single(touches.Select(t => t.Id).Distinct());
    }

    [AvaloniaTheory(DisplayName = "No touch actions are raised while touch events are disabled")]
    [MemberData(nameof(Controls))]
    public void NoTouchActionsAreRaisedWhileTouchEventsAreDisabled(string controlType)
    {
        var touches = HostAndRecord(controlType, window =>
        {
            window.MouseMove(new Point(20, 20));
            window.MouseMove(new Point(150, 150));
        }, enableTouchEvents: false);

        Assert.Empty(touches);
    }

    [AvaloniaTheory(DisplayName = "Touch locations are in pixels at a render scaling of 2")]
    [InlineData(nameof(SKCanvasViewControl), false, 40f)]
    [InlineData(nameof(SKCanvasViewControl), true, 20f)]
    [InlineData(nameof(SKGLViewControl), false, 40f)]
    [InlineData(nameof(SKGLViewControl), true, 20f)]
    public void TouchLocationsAreInPixelsAtScaling2(string controlType, bool ignorePixelScaling, float expected)
    {
        var touches = HostAndRecord(controlType, window =>
        {
            window.MouseMove(new Point(20, 20));
            window.MouseDown(new Point(20, 20), MouseButton.Left);
            window.MouseUp(new Point(20, 20), MouseButton.Left);
        }, scaling: 2, ignorePixelScaling: ignorePixelScaling);

        Assert.NotEmpty(touches);
        Assert.All(touches, t => Assert.Equal(new global::SkiaSharp.SKPoint(expected, expected), t.Location));
    }

    static List<SKTouchEventArgs> HostAndRecord(string controlType, Action<Window> input, bool enableTouchEvents = true,
        double scaling = 1, bool ignorePixelScaling = false)
    {
        var touches = new List<SKTouchEventArgs>();
        Func<SKTouchEventArgs, bool> record = args =>
        {
            touches.Add(args);
            return true;
        };

        // The handlers always set a paint action; without one the control draws nothing to hit test
        Control control = controlType == nameof(SKCanvasViewControl)
            ? new SKCanvasViewControl { EnableTouchEvents = enableTouchEvents, TouchAction = record, PaintSurfaceAction = _ => { }, IgnorePixelScaling = ignorePixelScaling }
            : new SKGLViewControl { EnableTouchEvents = enableTouchEvents, TouchAction = record, PaintSurfaceAction = _ => { }, IgnorePixelScaling = ignorePixelScaling };
        control.Width = 100;
        control.Height = 100;
        control.HorizontalAlignment = Layout.HorizontalAlignment.Left;
        control.VerticalAlignment = Layout.VerticalAlignment.Top;

        var window = new Window { Width = 200, Height = 200, Content = control };
        window.Show();
        try
        {
            if (scaling != 1)
                window.SetRenderScaling(scaling);
            window.UpdateLayout();
            // Input is hit tested against the composited scene, so render a frame first
            window.CaptureRenderedFrame();
            input(window);

            // Closing the window raises an Exited of its own, which is not part of the test input
            return touches.ToList();
        }
        finally
        {
            window.Close();
        }
    }
}
