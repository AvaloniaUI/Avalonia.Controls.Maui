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

    static List<SKTouchEventArgs> HostAndRecord(string controlType, Action<Window> input, bool enableTouchEvents = true)
    {
        var touches = new List<SKTouchEventArgs>();
        Func<SKTouchEventArgs, bool> record = args =>
        {
            touches.Add(args);
            return true;
        };

        // The handlers always set a paint action; without one the control draws nothing to hit test
        Control control = controlType == nameof(SKCanvasViewControl)
            ? new SKCanvasViewControl { EnableTouchEvents = enableTouchEvents, TouchAction = record, PaintSurfaceAction = _ => { } }
            : new SKGLViewControl { EnableTouchEvents = enableTouchEvents, TouchAction = record, PaintSurfaceAction = _ => { } };
        control.Width = 100;
        control.Height = 100;
        control.HorizontalAlignment = Layout.HorizontalAlignment.Left;
        control.VerticalAlignment = Layout.VerticalAlignment.Top;

        var window = new Window { Width = 200, Height = 200, Content = control };
        window.Show();
        try
        {
            window.UpdateLayout();
            // Input is hit tested against the composited scene, so render a frame first
            window.CaptureRenderedFrame();
            input(window);
        }
        finally
        {
            window.Close();
        }

        return touches;
    }
}
