using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ControlsBorder = Microsoft.Maui.Controls.Border;
using ControlsGrid = Microsoft.Maui.Controls.Grid;
using MauiLayoutHandler = Avalonia.Controls.Maui.Handlers.LayoutHandler;

namespace Avalonia.Controls.Maui.Tests.Gestures;

public class PointerOverReparentTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "PointerOver Holds Through An Exit Raised Before A Reparented View Can Be Hit Tested")]
    public async Task PointerOverHoldsThroughAnExitRaisedBeforeAReparentedViewCanBeHitTested()
    {
        var (border, window, events) = await ShowHoverBorderAsync();
        var platformView = (Control)border.Handler!.PlatformView!;
        var position = platformView.TranslatePoint(new Point(40, 25), window)!.Value;

        RaisePointerEvent(InputElement.PointerEnteredEvent, platformView, window, position);
        RaisePointerEvent(InputElement.PointerExitedEvent, platformView, window, position);
        RaisePointerEvent(InputElement.PointerEnteredEvent, platformView, window, position);
        RenderFrames(window);

        Assert.Equal("PointerOver", CurrentState(border));
        Assert.Equal(["Entered"], events);
    }

    [AvaloniaFact(DisplayName = "PointerOver Ends Once When The Pointer Leaves Before A Reparented View Can Be Hit Tested")]
    public async Task PointerOverEndsOnceWhenThePointerLeavesBeforeAReparentedViewCanBeHitTested()
    {
        var (border, window, events) = await ShowHoverBorderAsync();
        var platformView = (Control)border.Handler!.PlatformView!;
        var position = platformView.TranslatePoint(new Point(40, 25), window)!.Value;

        RaisePointerEvent(InputElement.PointerEnteredEvent, platformView, window, position);
        RaisePointerEvent(InputElement.PointerExitedEvent, platformView, window, position);
        RaisePointerEvent(InputElement.PointerExitedEvent, platformView, window, position);
        window.MouseMove(new Point(10, 10));
        RenderFrames(window);

        Assert.Equal("Normal", CurrentState(border));
        Assert.Equal(["Entered", "Exited"], events);
    }

    [AvaloniaFact(DisplayName = "PointerOver Ends On An Exit Over A View That Was Not Just Reparented")]
    public async Task PointerOverEndsOnAnExitOverAViewThatWasNotJustReparented()
    {
        var (border, window, events) = await ShowHoverBorderAsync();
        var platformView = (Control)border.Handler!.PlatformView!;
        var position = platformView.TranslatePoint(new Point(40, 25), window)!.Value;

        RaisePointerEvent(InputElement.PointerEnteredEvent, platformView, window, position);
        RenderFrames(window);
        RaisePointerEvent(InputElement.PointerExitedEvent, platformView, window, position);
        RenderFrames(window);

        Assert.Equal("Normal", CurrentState(border));
        Assert.Equal(["Entered", "Exited"], events);
    }

    [AvaloniaFact(DisplayName = "PointerOver Ends When The Pointer Leaves A View In A Container")]
    public async Task PointerOverEndsWhenThePointerLeavesAViewInAContainer()
    {
        var (border, window, events) = await ShowHoverBorderAsync();

        window.MouseMove(new Point(150, 120));
        RenderFrames(window);
        window.MouseMove(new Point(10, 10));
        RenderFrames(window);

        Assert.Equal("Normal", CurrentState(border));
        Assert.Equal(["Entered", "Exited"], events);
    }

    private async Task<(ControlsBorder Border, Window Window, List<string> Events)> ShowHoverBorderAsync()
    {
        var border = new ControlsBorder
        {
            WidthRequest = 100,
            HeightRequest = 50,
            BackgroundColor = Colors.White,
        };

        var groups = new VisualStateGroupList();
        var common = new VisualStateGroup { Name = "CommonStates" };
        common.States.Add(new VisualState { Name = "Normal" });
        var pointerOver = new VisualState { Name = "PointerOver" };
        pointerOver.Setters.Add(new Setter
        {
            Property = VisualElement.ShadowProperty,
            Value = new Shadow { Brush = Brush.Black, Radius = 3, Opacity = 0.2f },
        });
        common.States.Add(pointerOver);
        groups.Add(common);
        VisualStateManager.SetVisualStateGroups(border, groups);

        var events = new List<string>();
        var pointer = new Microsoft.Maui.Controls.PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => events.Add("Entered");
        pointer.PointerExited += (_, _) => events.Add("Exited");
        border.GestureRecognizers.Add(pointer);

        var grid = new ControlsGrid { Children = { border } };
        var handler = await CreateHandlerAsync<MauiLayoutHandler>(grid);
        var window = ShowInWindow((Control)handler.PlatformView!);
        RenderFrames(window);

        return (border, window, events);
    }

    private static void RenderFrames(Window window)
    {
        for (var i = 0; i < 4; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            window.CaptureRenderedFrame();
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static void RaisePointerEvent(Interactivity.RoutedEvent routedEvent, Control target, Window window, Point position)
    {
        target.RaiseEvent(new Avalonia.Input.PointerEventArgs(routedEvent, target, new Pointer(1, PointerType.Mouse, true),
            window, position, 0, new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other), KeyModifiers.None));
        window.UpdateLayout();
    }

    private static string? CurrentState(ControlsBorder border) =>
        VisualStateManager.GetVisualStateGroups(border)[0].CurrentState?.Name;
}
