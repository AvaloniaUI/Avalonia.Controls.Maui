using Avalonia.Headless.XUnit;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Layouts;
using Microsoft.Maui.Controls;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace Avalonia.Controls.Maui.Tests.CommunityToolkit;

public class ToolkitLayoutTests : ToolkitTestBase
{
    [AvaloniaFact(DisplayName = "DockLayout Docks Children To Its Edges")]
    public async Task DockLayoutDocksChildrenToEdges()
    {
        var top = new BoxView { HeightRequest = 30 };
        var bottom = new BoxView { HeightRequest = 20 };
        var left = new BoxView { WidthRequest = 40 };
        var right = new BoxView { WidthRequest = 50 };
        var center = new BoxView();
        var dock = new DockLayout { WidthRequest = 300, HeightRequest = 200, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
        dock.Add(top, DockPosition.Top);
        dock.Add(bottom, DockPosition.Bottom);
        dock.Add(left, DockPosition.Left);
        dock.Add(right, DockPosition.Right);
        dock.Add(center);

        await RunInWindowAsync(dock, window =>
        {
            var origin = BoundsInWindow(window, dock).TopLeft;
            Rect Local(Microsoft.Maui.IView view) => BoundsInWindow(window, view).Translate(-(Vector)origin);

            Assert.Equal(new Rect(0, 0, 300, 30), Local(top));
            Assert.Equal(new Rect(0, 180, 300, 20), Local(bottom));
            Assert.Equal(new Rect(0, 30, 40, 150), Local(left));
            Assert.Equal(new Rect(250, 30, 50, 150), Local(right));
            Assert.Equal(new Rect(40, 30, 210, 150), Local(center));
            return Task.CompletedTask;
        });
    }

    [AvaloniaFact(DisplayName = "DockLayout Moves A Child When Its Dock Position Changes")]
    public async Task DockLayoutMovesChildWhenDockPositionChanges()
    {
        var docked = new BoxView { WidthRequest = 40, HeightRequest = 40 };
        var dock = new DockLayout { WidthRequest = 300, HeightRequest = 200, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
        dock.Add(docked, DockPosition.Left);
        dock.Add(new BoxView());

        await RunInWindowAsync(dock, window =>
        {
            var origin = BoundsInWindow(window, dock).TopLeft;
            Assert.Equal(origin.X, BoundsInWindow(window, docked).Left, 1);

            DockLayout.SetDockPosition(docked, DockPosition.Right);
            window.Platform.UpdateLayout();

            Assert.Equal(origin.X + 260, BoundsInWindow(window, docked).Left, 1);
            return Task.CompletedTask;
        });
    }

    [AvaloniaFact(DisplayName = "UniformItemsLayout Wraps Equal Cells Into Rows")]
    public async Task UniformItemsLayoutWrapsEqualCellsIntoRows()
    {
        var layout = new UniformItemsLayout { WidthRequest = 200, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
        var items = Enumerable.Range(0, 5).Select(_ => new BoxView { WidthRequest = 60, HeightRequest = 40, HorizontalOptions = LayoutOptions.Start }).ToList();
        foreach (var item in items)
            layout.Add(item);

        await RunInWindowAsync(layout, window =>
        {
            var origin = BoundsInWindow(window, layout).TopLeft;
            var cells = items.Select(item => BoundsInWindow(window, item).Translate(-(Vector)origin)).ToList();

            // 200 / 60 fits three columns, each a third of the width, so five items take two rows.
            // Layout rounding places them on whole pixels.
            var column = 200d / 3;
            Assert.Equal(new Point(0, 0), cells[0].TopLeft);
            Assert.Equal(column, cells[1].Left, tolerance: 1);
            Assert.Equal(2 * column, cells[2].Left, tolerance: 1);
            Assert.Equal(new Point(0, 40), cells[3].TopLeft);
            Assert.Equal(column, cells[4].Left, tolerance: 1);
            Assert.Equal(40, cells[4].Top, 1);
            Assert.Equal(80, BoundsInWindow(window, layout).Height, 1);
            return Task.CompletedTask;
        });
    }

    [AvaloniaFact(DisplayName = "UniformItemsLayout Limits Its Columns")]
    public async Task UniformItemsLayoutLimitsColumns()
    {
        var layout = new UniformItemsLayout { WidthRequest = 200, MaxColumns = 2, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
        var items = Enumerable.Range(0, 5).Select(_ => new BoxView { WidthRequest = 60, HeightRequest = 40, HorizontalOptions = LayoutOptions.Start }).ToList();
        foreach (var item in items)
            layout.Add(item);

        await RunInWindowAsync(layout, window =>
        {
            var origin = BoundsInWindow(window, layout).TopLeft;

            Assert.Equal(new Point(100, 0), BoundsInWindow(window, items[1]).Translate(-(Vector)origin).TopLeft);
            Assert.Equal(new Point(0, 40), BoundsInWindow(window, items[2]).Translate(-(Vector)origin).TopLeft);
            Assert.Equal(120, BoundsInWindow(window, layout).Height, 1);
            return Task.CompletedTask;
        });
    }

    [AvaloniaFact(DisplayName = "UniformItemsLayout Grows When Items Are Added")]
    public async Task UniformItemsLayoutGrowsWhenItemsAreAdded()
    {
        var layout = new UniformItemsLayout { WidthRequest = 120, HorizontalOptions = LayoutOptions.Start };
        layout.Add(new BoxView { WidthRequest = 60, HeightRequest = 40 });
        var below = new BoxView { HeightRequest = 10 };
        var page = new VerticalStackLayout { Children = { layout, below } };

        await RunInWindowAsync(page, window =>
        {
            var topBefore = BoundsInWindow(window, below).Top;

            layout.Add(new BoxView { WidthRequest = 60, HeightRequest = 40 });
            layout.Add(new BoxView { WidthRequest = 60, HeightRequest = 40 });
            window.Platform.UpdateLayout();

            Assert.Equal(80, BoundsInWindow(window, layout).Height, 1);
            Assert.Equal(topBefore + 40, BoundsInWindow(window, below).Top, 1);
            return Task.CompletedTask;
        });
    }

    [AvaloniaFact(DisplayName = "StateContainer Swaps Its Content For State Views")]
    public async Task StateContainerSwapsContentForStateViews()
    {
        var content = new MauiLabel { Text = "Content" };
        var loading = new MauiLabel { Text = "Loading" };
        var error = new MauiLabel { Text = "Error" };
        StateView.SetStateKey(loading, "Loading");
        StateView.SetStateKey(error, "Error");

        var container = new VerticalStackLayout { Children = { content } };
        StateContainer.GetStateViews(container).Add(loading);
        StateContainer.GetStateViews(container).Add(error);

        await RunInWindowAsync(container, async window =>
        {
            Assert.True(PlatformViewOf(content).IsEffectivelyVisible);

            StateContainer.SetCurrentState(container, "Loading");
            await WaitUntilAsync(window, () => loading.Handler is not null && PlatformViewOf(loading).IsEffectivelyVisible,
                message: "The Loading state view was not shown.");
            Assert.DoesNotContain(content, container.Children);

            StateContainer.SetCurrentState(container, "Error");
            await WaitUntilAsync(window, () => error.Handler is not null && PlatformViewOf(error).IsEffectivelyVisible,
                message: "The Error state view was not shown.");
            Assert.DoesNotContain(loading, container.Children);

            StateContainer.SetCurrentState(container, null);
            await WaitUntilAsync(window, () => container.Children.Contains(content) && PlatformViewOf(content).IsEffectivelyVisible,
                message: "The content was not shown again.");
        });
    }

    [AvaloniaFact(DisplayName = "StateContainer Changes State With An Animation")]
    public async Task StateContainerChangesStateWithAnimation()
    {
        var content = new MauiLabel { Text = "Content" };
        var empty = new MauiLabel { Text = "Empty" };
        StateView.SetStateKey(empty, "Empty");

        var container = new VerticalStackLayout { Children = { content } };
        StateContainer.GetStateViews(container).Add(empty);

        await RunInWindowAsync(container, async window =>
        {
            var change = StateContainer.ChangeStateWithAnimation(container, "Empty");
            await WaitUntilAsync(window, () => change.IsCompleted, message: "The animated state change did not finish.");
            await change;

            Assert.Contains(empty, container.Children);
            Assert.Equal(1, container.Opacity, 2);
            Assert.True(PlatformViewOf(empty).IsEffectivelyVisible);
            Assert.Equal(1, PlatformViewOf(container).Opacity, 2);
        });
    }
}
