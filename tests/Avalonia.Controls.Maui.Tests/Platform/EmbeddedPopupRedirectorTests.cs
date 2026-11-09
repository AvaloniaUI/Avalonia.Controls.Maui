using Avalonia.Controls;
using Avalonia.Controls.Maui.Platforms.EmbeddedPopups;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Maui.Tests.Platform;

public class EmbeddedPopupRedirectorTests
{
    private sealed class FakeOverlay : IEmbeddedPopupOverlay
    {
        public FakeOverlay(bool ready = true)
        {
            Window = new Window { Width = 800, Height = 600, Content = ready ? AnchorLayer : null };
            Window.Show();
        }

        public Window Window { get; }

        public Canvas AnchorLayer { get; } = new();

        public int ShowCount { get; private set; }

        public int HideCount { get; private set; }

        public bool IsShown => ShowCount > HideCount;

        public void Show(IEmbeddedPopupHost host) => ShowCount++;

        public void Hide() => HideCount++;
    }

    /// <summary>A host whose popups each open in an idle overlay, like on Windows.</summary>
    private sealed class OverlayPerPopupHost(params FakeOverlay[] overlays) : IEmbeddedPopupHost
    {
        public IEmbeddedPopupOverlay GetOverlay() => overlays.First(o => !o.IsShown);

        public IEnumerable<IEmbeddedPopupOverlay> GetWindowOverlays() => overlays;

        public Point? GetOriginInOverlay() => default(Point);
    }

    private static Popup CreatePopup(Control target) => new()
    {
        PlacementTarget = target,
        Placement = PlacementMode.Bottom,
        ShouldUseOverlayLayer = true,
        Child = new Border { Width = 50, Height = 100 },
    };

    private sealed class FakeHost(FakeOverlay overlay, Point origin) : IEmbeddedPopupHost
    {
        public FakeOverlay Overlay { get; set; } = overlay;

        public IEmbeddedPopupOverlay GetOverlay() => Overlay;

        public Point? GetOriginInOverlay() => origin;
    }

    private static (Window Host, Button Target, Popup Popup, FakeOverlay Overlay) CreateEmbeddedPopup(bool register = true) =>
        CreateEmbeddedPopup(out _, register);

    private static (Window Host, Button Target, Popup Popup, FakeOverlay Overlay) CreateEmbeddedPopup(
        out FakeHost fakeHost, bool register = true)
    {
        var target = new Button { Width = 100, Height = 30, Margin = new Thickness(5, 40, 0, 0) };
        var popup = new Popup
        {
            PlacementTarget = target,
            Placement = PlacementMode.Bottom,
            ShouldUseOverlayLayer = true,
            Child = new Border { Width = 50, Height = 200 },
        };
        var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Left, Children = { target, popup } };
        var host = new Window { Width = 200, Height = 100, Content = panel };
        var overlay = new FakeOverlay();

        fakeHost = new FakeHost(overlay, new Point(10, 20));
        if (register)
            EmbeddedPopupRedirector.Register(host, fakeHost);

        host.Show();
        Dispatcher.UIThread.RunJobs();
        return (host, target, popup, overlay);
    }

    [AvaloniaFact(DisplayName = "Popup in an embedded view opens on an anchor in the overlay")]
    public void Popup_Opens_On_Overlay_Anchor()
    {
        var (_, target, popup, overlay) = CreateEmbeddedPopup();

        popup.Open();

        var anchor = Assert.IsAssignableFrom<Control>(popup.PlacementTarget);
        Assert.NotSame(target, anchor);
        Assert.Same(overlay.AnchorLayer, anchor.Parent);
        Assert.Same(overlay.Window, TopLevel.GetTopLevel(popup.Child));

        var targetOrigin = target.TranslatePoint(default, TopLevel.GetTopLevel(target)!)!.Value;
        Assert.Equal(10 + targetOrigin.X, Canvas.GetLeft(anchor));
        Assert.Equal(20 + targetOrigin.Y, Canvas.GetTop(anchor));
        Assert.Equal(target.Bounds.Size, anchor.Bounds.Size);
        Assert.Equal(1, overlay.ShowCount);
    }

    [AvaloniaFact(DisplayName = "Overlay is hidden after the last popup closes")]
    public void Overlay_Hidden_After_Close()
    {
        var (_, _, popup, overlay) = CreateEmbeddedPopup();

        popup.Open();
        popup.Close();
        Assert.Equal(0, overlay.HideCount);

        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, overlay.HideCount);

        popup.Open();
        Assert.Equal(2, overlay.ShowCount);
    }

    [AvaloniaFact(DisplayName = "Light dismiss closes a popup opened from a popup that is not light-dismiss")]
    public void Light_Dismiss_Closes_Nested_Popup()
    {
        var (_, _, popup, overlay) = CreateEmbeddedPopup();
        var nestedTarget = new Button { Width = 50, Height = 30 };
        var nested = new Popup
        {
            PlacementTarget = nestedTarget,
            IsLightDismissEnabled = true,
            ShouldUseOverlayLayer = true,
            Child = new Border { Width = 50, Height = 100 },
        };
        popup.IsLightDismissEnabled = false;
        popup.Child = new StackPanel { Children = { nestedTarget, nested } };

        popup.Open();
        nested.Open();
        Assert.Same(overlay.Window, TopLevel.GetTopLevel(nested.Child));

        Assert.True(EmbeddedPopupRedirector.LightDismiss(overlay));
        Assert.False(nested.IsOpen);
        Assert.True(popup.IsOpen);
    }

    [AvaloniaFact(DisplayName = "Popup outside an embedded view keeps its placement target")]
    public void Popup_Outside_Embedded_View_Is_Not_Redirected()
    {
        var (host, target, popup, overlay) = CreateEmbeddedPopup(register: false);

        popup.Open();

        Assert.Same(target, popup.PlacementTarget);
        Assert.Same(host, TopLevel.GetTopLevel(popup.Child));
        Assert.Equal(0, overlay.ShowCount);
    }

    [AvaloniaFact(DisplayName = "New placement target in an embedded view is redirected")]
    public void New_Placement_Target_Is_Redirected()
    {
        var (_, _, popup, overlay) = CreateEmbeddedPopup();
        var anchor = popup.PlacementTarget;
        var other = new Button { Width = 40, Height = 20 };
        ((StackPanel)popup.Parent!).Children.Add(other);
        Dispatcher.UIThread.RunJobs();

        popup.PlacementTarget = other;
        popup.Open();

        Assert.Same(anchor, popup.PlacementTarget);
        Assert.Equal(other.Bounds.Size, anchor!.Bounds.Size);
        Assert.Same(overlay.Window, TopLevel.GetTopLevel(popup.Child));
    }

    [AvaloniaFact(DisplayName = "Pointer placement is anchored at the pointer and restored on close")]
    public void Pointer_Placement_Restored_On_Close()
    {
        var (_, _, popup, _) = CreateEmbeddedPopup();
        popup.Placement = PlacementMode.Pointer;

        popup.Open();
        Assert.Equal(PlacementMode.AnchorAndGravity, popup.Placement);
        Assert.Equal(new Size(1, 1), popup.PlacementTarget!.Bounds.Size);

        popup.Close();
        Assert.Equal(PlacementMode.Pointer, popup.Placement);
    }

    [AvaloniaFact(DisplayName = "Popup closes when its target leaves the embedded view")]
    public void Popup_Closes_When_Target_Detached()
    {
        var (_, target, popup, _) = CreateEmbeddedPopup();

        popup.Open();
        ((StackPanel)target.Parent!).Children.Remove(target);

        Assert.False(popup.IsOpen);
    }

    [AvaloniaFact(DisplayName = "ComboBox drop-down in an embedded view opens in the overlay")]
    public void ComboBox_DropDown_Opens_In_Overlay()
    {
        var comboBox = new ComboBox { Width = 150, ItemsSource = new[] { "A", "B", "C" } };
        var host = new Window { Width = 200, Height = 40, Content = comboBox };
        var overlay = new FakeOverlay();
        EmbeddedPopupRedirector.Register(host, new FakeHost(overlay, new Point(0, 300)));
        host.Show();
        Dispatcher.UIThread.RunJobs();

        comboBox.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        var popup = comboBox.GetVisualDescendants().OfType<Popup>().Single();
        Assert.True(popup.IsOpen);
        Assert.Same(overlay.Window, TopLevel.GetTopLevel(popup.Child));
        Assert.Equal(1, overlay.ShowCount);

        comboBox.IsDropDownOpen = false;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, overlay.HideCount);
    }

    [AvaloniaFact(DisplayName = "Open popup follows a new placement target in the same embedded view")]
    public void Open_Popup_Follows_New_Target()
    {
        var (_, target, popup, overlay) = CreateEmbeddedPopup();
        var anchor = popup.PlacementTarget!;
        var other = new Button { Width = 40, Height = 20 };
        ((StackPanel)target.Parent!).Children.Add(other);
        Dispatcher.UIThread.RunJobs();

        popup.Open();
        popup.PlacementTarget = other;

        Assert.True(popup.IsOpen);
        Assert.Same(anchor, popup.PlacementTarget);
        Assert.Same(overlay.Window, TopLevel.GetTopLevel(popup.Child));
        var otherOrigin = other.TranslatePoint(default, TopLevel.GetTopLevel(other)!)!.Value;
        Assert.Equal(20 + otherOrigin.Y, Canvas.GetTop(anchor));
        Assert.Equal(other.Bounds.Size, anchor.Bounds.Size);

        // The old target is no longer tracked, so removing it does not close the popup.
        ((StackPanel)target.Parent!).Children.Remove(target);
        Assert.True(popup.IsOpen);

        // The overlay stays shown while the popup reopens.
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, overlay.HideCount);
    }

    [AvaloniaFact(DisplayName = "Open popup moves out of the overlay for a target outside embedded views")]
    public void Open_Popup_Leaves_Overlay_For_Target_Outside_Embedded_View()
    {
        var (_, _, popup, overlay) = CreateEmbeddedPopup();
        var outsideTarget = new Button { Width = 40, Height = 20 };
        var outside = new Window { Width = 200, Height = 100, Content = outsideTarget };
        outside.Show();
        Dispatcher.UIThread.RunJobs();

        popup.Open();
        var anchor = popup.PlacementTarget!;
        popup.PlacementTarget = outsideTarget;

        Assert.True(popup.IsOpen);
        Assert.Same(outsideTarget, popup.PlacementTarget);
        Assert.Same(outside, TopLevel.GetTopLevel(popup.Child));
        Assert.Null(anchor.Parent);

        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, overlay.HideCount);
    }

    [AvaloniaFact(DisplayName = "Anchor is removed when its popup unloads, and added again when it loads")]
    public void Anchor_Removed_On_Unload()
    {
        var (_, target, popup, overlay) = CreateEmbeddedPopup();
        var anchor = popup.PlacementTarget!;
        var panel = (StackPanel)popup.Parent!;
        Assert.Same(overlay.AnchorLayer, anchor.Parent);

        panel.Children.Remove(popup);
        Dispatcher.UIThread.RunJobs();

        Assert.Null(anchor.Parent);
        Assert.Same(target, popup.PlacementTarget);

        panel.Children.Add(popup);
        Dispatcher.UIThread.RunJobs();

        Assert.Same(anchor, popup.PlacementTarget);
        Assert.Same(overlay.AnchorLayer, anchor.Parent);
    }

    [AvaloniaFact(DisplayName = "Anchor is removed when the popup's target leaves the embedded views")]
    public void Anchor_Removed_When_Target_Not_Embedded()
    {
        var (_, target, popup, _) = CreateEmbeddedPopup();
        var anchor = popup.PlacementTarget!;
        var outsideTarget = new Button();
        var outside = new Window { Content = outsideTarget };
        outside.Show();
        Dispatcher.UIThread.RunJobs();

        popup.PlacementTarget = outsideTarget;

        Assert.Null(anchor.Parent);
        Assert.Same(outsideTarget, popup.PlacementTarget);
    }

    [AvaloniaFact(DisplayName = "Popups move to the new overlay of their host")]
    public void Popups_Move_To_New_Overlay()
    {
        var (host, _, popup, _) = CreateEmbeddedPopup(out var fakeHost);
        var anchor = popup.PlacementTarget!;
        var newOverlay = new FakeOverlay();

        fakeHost.Overlay = newOverlay;
        EmbeddedPopupRedirector.OnHostOverlayChanged(host);

        Assert.Same(newOverlay.AnchorLayer, anchor.Parent);
        popup.Open();
        Assert.Same(newOverlay.Window, TopLevel.GetTopLevel(popup.Child));
        Assert.Equal(1, newOverlay.ShowCount);
    }

    [AvaloniaFact(DisplayName = "Closed popups move to an idle overlay when a popup opens in theirs")]
    public void Closed_Popups_Move_When_Their_Overlay_Shows()
    {
        var first = new FakeOverlay();
        var second = new FakeOverlay();
        var targetA = new Button { Width = 100, Height = 30 };
        var targetB = new Button { Width = 100, Height = 30 };
        var popupA = CreatePopup(targetA);
        var popupB = CreatePopup(targetB);
        var host = new Window
        {
            Width = 200,
            Height = 100,
            Content = new StackPanel { Children = { targetA, targetB, popupA, popupB } },
        };
        EmbeddedPopupRedirector.Register(host, new OverlayPerPopupHost(first, second));
        host.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.Same(first.AnchorLayer, popupB.PlacementTarget!.Parent);

        popupA.Open();

        Assert.Same(first.Window, TopLevel.GetTopLevel(popupA.Child));
        Assert.Same(second.AnchorLayer, popupB.PlacementTarget!.Parent);
        popupB.Open();
        Assert.Same(second.Window, TopLevel.GetTopLevel(popupB.Child));
    }

    [AvaloniaFact(DisplayName = "Light dismiss keeps the pressed popup and the popups it was opened from")]
    public void Light_Dismiss_Keeps_Pressed_Popup_And_Its_Parents()
    {
        FakeOverlay[] overlays = [new(), new(), new()];
        var target = new Button { Width = 100, Height = 30 };
        var nestedTarget = new Button { Width = 50, Height = 30 };
        var nested = CreatePopup(nestedTarget);
        var outer = CreatePopup(target);
        nested.IsLightDismissEnabled = true;
        outer.IsLightDismissEnabled = true;
        outer.Child = new StackPanel { Children = { nestedTarget, nested } };
        var host = new Window { Width = 200, Height = 100, Content = new StackPanel { Children = { target, outer } } };

        // On Windows the overlays are hosts too, so that popups opened from their popups get their own overlay.
        EmbeddedPopupRedirector.Register(host, new OverlayPerPopupHost(overlays));
        foreach (var overlay in overlays)
            EmbeddedPopupRedirector.Register(overlay.Window, new OverlayPerPopupHost(overlays));
        host.Show();
        Dispatcher.UIThread.RunJobs();

        outer.Open();
        Dispatcher.UIThread.RunJobs();
        nested.Open();
        Assert.Same(overlays[0].Window, TopLevel.GetTopLevel(outer.Child));
        Assert.Same(overlays[1].Window, TopLevel.GetTopLevel(nested.Child));

        // A press in the nested popup keeps both.
        Assert.False(EmbeddedPopupRedirector.LightDismiss(overlays, overlays[1].Window));
        Assert.True(nested.IsOpen);
        Assert.True(outer.IsOpen);

        // A press in the outer popup closes the nested one.
        Assert.True(EmbeddedPopupRedirector.LightDismiss(overlays, overlays[0].Window));
        Assert.False(nested.IsOpen);
        Assert.True(outer.IsOpen);

        // A press in the embedded view closes both.
        Dispatcher.UIThread.RunJobs();
        nested.Open();
        Assert.True(EmbeddedPopupRedirector.LightDismiss(overlays, host));
        Assert.False(nested.IsOpen);
        Assert.False(outer.IsOpen);
    }

    [AvaloniaFact(DisplayName = "Closed popups move out of an overlay that is removed, and open popups stay")]
    public void Closed_Popups_Move_Out_Of_Removed_Overlay()
    {
        var (_, _, closed, removed) = CreateEmbeddedPopup(out var fakeHost);
        var openTarget = new Button { Width = 100, Height = 30 };
        var open = CreatePopup(openTarget);
        ((StackPanel)closed.Parent!).Children.AddRange([openTarget, open]);
        Dispatcher.UIThread.RunJobs();
        open.Open();
        var kept = new FakeOverlay();

        fakeHost.Overlay = kept;
        EmbeddedPopupRedirector.MoveClosedPopups(removed);

        Assert.Same(kept.AnchorLayer, closed.PlacementTarget!.Parent);
        Assert.Same(removed.Window, TopLevel.GetTopLevel(open.Child));
        closed.Open();
        Assert.Same(kept.Window, TopLevel.GetTopLevel(closed.Child));
    }

    [AvaloniaFact(DisplayName = "Popup that was asked to open before its overlay was ready opens when it is")]
    public void Popup_Opens_When_Overlay_Ready()
    {
        var overlay = new FakeOverlay(ready: false);
        var target = new Button { Width = 100, Height = 30 };
        var popup = CreatePopup(target);
        var host = new Window { Width = 200, Height = 100, Content = new StackPanel { Children = { target, popup } } };
        EmbeddedPopupRedirector.Register(host, new FakeHost(overlay, default));
        host.Show();
        Dispatcher.UIThread.RunJobs();

        popup.IsOpen = true;
        Assert.Null(TopLevel.GetTopLevel(popup.Child));

        overlay.Window.Content = overlay.AnchorLayer;
        EmbeddedPopupRedirector.OnOverlayReady(overlay);

        Assert.Same(overlay.Window, TopLevel.GetTopLevel(popup.Child));
        Assert.Equal(1, overlay.ShowCount);
    }
}
