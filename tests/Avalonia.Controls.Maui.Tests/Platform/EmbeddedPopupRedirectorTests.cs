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
        public IEmbeddedPopupOverlay GetOverlay() => overlay;

        public Point? GetOriginInOverlay() => origin;
    }

    private static (Window Host, Button Target, Popup Popup, FakeOverlay Overlay) CreateEmbeddedPopup(bool register = true)
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

        if (register)
            EmbeddedPopupRedirector.Register(host, new FakeHost(overlay, new Point(10, 20)));

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
}
