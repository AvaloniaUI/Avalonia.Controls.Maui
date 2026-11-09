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
        public FakeOverlay()
        {
            Window = new Window { Width = 800, Height = 600, Content = AnchorLayer };
            Window.Show();
        }

        public Window Window { get; }

        public Canvas AnchorLayer { get; } = new();

        public int ShowCount { get; private set; }

        public int HideCount { get; private set; }

        public void Show(IEmbeddedPopupHost host) => ShowCount++;

        public void Hide() => HideCount++;
    }

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
}
