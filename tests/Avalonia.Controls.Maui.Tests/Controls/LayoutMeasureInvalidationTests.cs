using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Controls.Maui.Platform;
using Avalonia.VisualTree;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Controls;

/// <summary>
/// Tests that validate measure invalidation propagates correctly from child controls
/// to parent MAUI layouts when control sizes change.
/// </summary>
public class LayoutMeasureInvalidationTests : WindowHandlerTestBase
{
    [AvaloniaFact(DisplayName = "BoxView Width Change Invalidates Parent Layout Measure")]
    public async Task BoxViewWidthChangeInvalidatesParentLayoutMeasure()
    {
        var boxView = new BoxView
        {
            WidthRequest = 50,
            HeightRequest = 50,
            Color = Microsoft.Maui.Graphics.Colors.Red
        };

        var stackLayout = new VerticalStackLayout
        {
            Children = { boxView }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(stackLayout);

        var initialResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return new
            {
                LayoutWidth = layoutPanel.DesiredSize.Width,
                LayoutHeight = layoutPanel.DesiredSize.Height
            };
        });

        // Change the BoxView width
        boxView.WidthRequest = 200;

        var updatedResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return new
            {
                LayoutWidth = layoutPanel.DesiredSize.Width,
                LayoutHeight = layoutPanel.DesiredSize.Height
            };
        });

        Assert.True(updatedResult.LayoutWidth > initialResult.LayoutWidth,
            $"Layout width should increase from {initialResult.LayoutWidth} to at least {updatedResult.LayoutWidth} after BoxView width change to 200");
    }

    [AvaloniaFact(DisplayName = "BoxView Height Change Invalidates Parent Layout Measure")]
    public async Task BoxViewHeightChangeInvalidatesParentLayoutMeasure()
    {
        var boxView = new BoxView
        {
            WidthRequest = 50,
            HeightRequest = 50,
            Color = Microsoft.Maui.Graphics.Colors.Blue
        };

        var stackLayout = new VerticalStackLayout
        {
            Children = { boxView }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(stackLayout);

        var initialResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return new
            {
                LayoutWidth = layoutPanel.DesiredSize.Width,
                LayoutHeight = layoutPanel.DesiredSize.Height
            };
        });

        // Change the BoxView height
        boxView.HeightRequest = 200;

        var updatedResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return new
            {
                LayoutWidth = layoutPanel.DesiredSize.Width,
                LayoutHeight = layoutPanel.DesiredSize.Height
            };
        });

        Assert.True(updatedResult.LayoutHeight > initialResult.LayoutHeight,
            $"Layout height should increase from {initialResult.LayoutHeight} to at least {updatedResult.LayoutHeight} after BoxView height change to 200");
    }

    [AvaloniaFact(DisplayName = "Multiple BoxView Size Changes Propagate To Layout")]
    public async Task MultipleBoxViewSizeChangesPropagate()
    {
        var boxView1 = new BoxView
        {
            WidthRequest = 50,
            HeightRequest = 50,
            Color = Microsoft.Maui.Graphics.Colors.Red
        };

        var boxView2 = new BoxView
        {
            WidthRequest = 50,
            HeightRequest = 50,
            Color = Microsoft.Maui.Graphics.Colors.Green
        };

        var stackLayout = new VerticalStackLayout
        {
            Children = { boxView1, boxView2 }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(stackLayout);

        var initialResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return new
            {
                LayoutWidth = layoutPanel.DesiredSize.Width,
                LayoutHeight = layoutPanel.DesiredSize.Height
            };
        });

        // Change both BoxView sizes
        boxView1.WidthRequest = 150;
        boxView2.HeightRequest = 150;

        var updatedResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return new
            {
                LayoutWidth = layoutPanel.DesiredSize.Width,
                LayoutHeight = layoutPanel.DesiredSize.Height
            };
        });

        // Layout width should reflect the wider boxView1
        Assert.True(updatedResult.LayoutWidth >= 150,
            $"Layout width ({updatedResult.LayoutWidth}) should be at least 150 after boxView1 width change");

        // Layout height should reflect original boxView1 height (50) + new boxView2 height (150) = 200
        Assert.True(updatedResult.LayoutHeight >= 200,
            $"Layout height ({updatedResult.LayoutHeight}) should be at least 200 after boxView2 height change");
    }

    [AvaloniaFact(DisplayName = "Nested Layout Measure Invalidation Propagates")]
    public async Task NestedLayoutMeasureInvalidationPropagates()
    {
        var boxView = new BoxView
        {
            WidthRequest = 50,
            HeightRequest = 50,
            Color = Microsoft.Maui.Graphics.Colors.Purple
        };

        var innerLayout = new VerticalStackLayout
        {
            Children = { boxView }
        };

        var outerLayout = new VerticalStackLayout
        {
            Children = { innerLayout }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(outerLayout);

        var initialResult = await InvokeOnMainThreadAsync(() =>
        {
            var outerPanel = (LayoutPanel)layoutHandler.PlatformView;
            var innerPanel = (LayoutPanel)innerLayout.Handler!.PlatformView!;
            return new
            {
                OuterWidth = outerPanel.DesiredSize.Width,
                OuterHeight = outerPanel.DesiredSize.Height,
                InnerWidth = innerPanel.DesiredSize.Width,
                InnerHeight = innerPanel.DesiredSize.Height
            };
        });

        // Change the BoxView size
        boxView.WidthRequest = 200;
        boxView.HeightRequest = 200;

        var updatedResult = await InvokeOnMainThreadAsync(() =>
        {
            var outerPanel = (LayoutPanel)layoutHandler.PlatformView;
            var innerPanel = (LayoutPanel)innerLayout.Handler!.PlatformView!;
            return new
            {
                OuterWidth = outerPanel.DesiredSize.Width,
                OuterHeight = outerPanel.DesiredSize.Height,
                InnerWidth = innerPanel.DesiredSize.Width,
                InnerHeight = innerPanel.DesiredSize.Height
            };
        });

        // Inner layout should reflect the new size
        Assert.True(updatedResult.InnerWidth >= 200,
            $"Inner layout width ({updatedResult.InnerWidth}) should be at least 200");
        Assert.True(updatedResult.InnerHeight >= 200,
            $"Inner layout height ({updatedResult.InnerHeight}) should be at least 200");

        // Outer layout should also reflect the new size
        Assert.True(updatedResult.OuterWidth >= 200,
            $"Outer layout width ({updatedResult.OuterWidth}) should be at least 200");
        Assert.True(updatedResult.OuterHeight >= 200,
            $"Outer layout height ({updatedResult.OuterHeight}) should be at least 200");
    }

    [AvaloniaFact(DisplayName = "Button Size Change Invalidates Parent Layout")]
    public async Task ButtonSizeChangeInvalidatesParentLayout()
    {
        var button = new Microsoft.Maui.Controls.Button
        {
            Text = "Small",
            WidthRequest = 80,
            HeightRequest = 40
        };

        var stackLayout = new VerticalStackLayout
        {
            Children = { button }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(stackLayout);

        var initialResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return new
            {
                LayoutWidth = layoutPanel.DesiredSize.Width,
                LayoutHeight = layoutPanel.DesiredSize.Height
            };
        });

        // Change button size
        button.WidthRequest = 200;
        button.HeightRequest = 100;

        var updatedResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return new
            {
                LayoutWidth = layoutPanel.DesiredSize.Width,
                LayoutHeight = layoutPanel.DesiredSize.Height
            };
        });

        Assert.True(updatedResult.LayoutWidth >= 200,
            $"Layout width ({updatedResult.LayoutWidth}) should be at least 200 after button width change");
        Assert.True(updatedResult.LayoutHeight >= 100,
            $"Layout height ({updatedResult.LayoutHeight}) should be at least 100 after button height change");
    }

    [AvaloniaFact(DisplayName = "Entry Width Change Invalidates Parent Layout")]
    public async Task EntryWidthChangeInvalidatesParentLayout()
    {
        var entry = new Entry
        {
            Text = "Test",
            WidthRequest = 100
        };

        var stackLayout = new VerticalStackLayout
        {
            Children = { entry }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(stackLayout);

        var initialResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return layoutPanel.DesiredSize.Width;
        });

        // Change entry width
        entry.WidthRequest = 300;

        var updatedWidth = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return layoutPanel.DesiredSize.Width;
        });

        Assert.True(updatedWidth > initialResult,
            $"Layout width should increase from {initialResult} after Entry width change to 300, but was {updatedWidth}");
    }

    [AvaloniaFact(DisplayName = "HorizontalStackLayout Width Changes Propagate")]
    public async Task HorizontalStackLayoutWidthChangesPropagates()
    {
        var boxView1 = new BoxView
        {
            WidthRequest = 50,
            HeightRequest = 50,
            Color = Microsoft.Maui.Graphics.Colors.Red
        };

        var boxView2 = new BoxView
        {
            WidthRequest = 50,
            HeightRequest = 50,
            Color = Microsoft.Maui.Graphics.Colors.Blue
        };

        var horizontalLayout = new HorizontalStackLayout
        {
            Children = { boxView1, boxView2 }
        };

        var layoutHandler = await CreateHandlerAsync<Avalonia.Controls.Maui.Handlers.LayoutHandler>(horizontalLayout);

        var initialResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return new
            {
                LayoutWidth = layoutPanel.DesiredSize.Width,
                LayoutHeight = layoutPanel.DesiredSize.Height
            };
        });

        // Initial width should be approximately 100 (50 + 50)
        Assert.True(initialResult.LayoutWidth >= 100,
            $"Initial layout width ({initialResult.LayoutWidth}) should be at least 100 (50 + 50)");

        // Change both BoxView widths
        boxView1.WidthRequest = 150;
        boxView2.WidthRequest = 150;

        var updatedResult = await InvokeOnMainThreadAsync(() =>
        {
            var layoutPanel = (LayoutPanel)layoutHandler.PlatformView;
            return new
            {
                LayoutWidth = layoutPanel.DesiredSize.Width,
                LayoutHeight = layoutPanel.DesiredSize.Height
            };
        });

        // Updated width should be approximately 300 (150 + 150)
        Assert.True(updatedResult.LayoutWidth >= 300,
            $"Updated layout width ({updatedResult.LayoutWidth}) should be at least 300 (150 + 150)");
    }
}
