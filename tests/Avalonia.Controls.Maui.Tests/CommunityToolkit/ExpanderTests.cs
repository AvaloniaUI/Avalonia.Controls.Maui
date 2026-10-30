using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using MauiExpander = CommunityToolkit.Maui.Views.Expander;
using MauiLayoutHandler = Avalonia.Controls.Maui.Handlers.LayoutHandler;

namespace Avalonia.Controls.Maui.Tests.CommunityToolkit;

public class ExpanderTests : HandlerTestBase<MauiLayoutHandler, LayoutStub>
{
    [AvaloniaFact(DisplayName = "Border Grows And Shrinks With An Expander")]
    public async Task BorderGrowsAndShrinksWithExpander()
    {
        var expander = CreateExpander(contentHeight: 50);
        var border = new Microsoft.Maui.Controls.Border
        {
            Padding = new Microsoft.Maui.Thickness(10),
            Content = expander,
        };
        var page = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            Children = { border }
        };

        await RunInWindowAsync(page, window =>
        {
            var platformBorder = (Control)border.Handler!.PlatformView!;
            var collapsedHeight = platformBorder.Bounds.Height;

            expander.IsExpanded = true;
            window.UpdateLayout();

            Assert.Equal(collapsedHeight + 50, platformBorder.Bounds.Height, 1);

            expander.IsExpanded = false;
            window.UpdateLayout();

            Assert.Equal(collapsedHeight, platformBorder.Bounds.Height, 1);
        });
    }

    [AvaloniaFact(DisplayName = "Views Below An Expander Move When It Expands")]
    public async Task ViewsBelowExpanderMoveWhenItExpands()
    {
        var expander = CreateExpander(contentHeight: 50);
        var below = new Microsoft.Maui.Controls.BoxView { HeightRequest = 20 };
        var page = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            Children =
            {
                new Microsoft.Maui.Controls.Border { Content = expander },
                below,
            }
        };

        await RunInWindowAsync(page, window =>
        {
            var platformBelow = (Control)below.Handler!.PlatformView!;
            var collapsedTop = platformBelow.Bounds.Top;

            expander.IsExpanded = true;
            window.UpdateLayout();

            Assert.Equal(collapsedTop + 50, platformBelow.Bounds.Top, 1);
        });
    }

    [AvaloniaFact(DisplayName = "Nested Expanders Resize Every Ancestor")]
    public async Task NestedExpandersResizeEveryAncestor()
    {
        var inner = CreateExpander(contentHeight: 40);
        var outer = new MauiExpander
        {
            Header = new Microsoft.Maui.Controls.BoxView { HeightRequest = 20 },
            Content = new Microsoft.Maui.Controls.Border { Content = inner },
        };
        var outerBorder = new Microsoft.Maui.Controls.Border { Content = outer };
        var page = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            Children = { outerBorder }
        };

        await RunInWindowAsync(page, window =>
        {
            var platformBorder = (Control)outerBorder.Handler!.PlatformView!;
            var collapsedHeight = platformBorder.Bounds.Height;

            outer.IsExpanded = true;
            window.UpdateLayout();
            var outerExpandedHeight = platformBorder.Bounds.Height;

            Assert.True(outerExpandedHeight > collapsedHeight);

            inner.IsExpanded = true;
            window.UpdateLayout();

            Assert.Equal(outerExpandedHeight + 40, platformBorder.Bounds.Height, 1);
        });
    }

    [AvaloniaFact(DisplayName = "Expander In A Border Grows Upward")]
    public async Task ExpanderInBorderGrowsUpward()
    {
        var expander = CreateExpander(contentHeight: 50);
        expander.Direction = global::CommunityToolkit.Maui.Core.ExpandDirection.Up;
        var border = new Microsoft.Maui.Controls.Border { Content = expander };
        var page = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            Children = { border }
        };

        await RunInWindowAsync(page, window =>
        {
            var platformBorder = (Control)border.Handler!.PlatformView!;
            var platformHeader = (Control)((Microsoft.Maui.Controls.View)expander.Header!).Handler!.PlatformView!;
            var collapsedHeight = platformBorder.Bounds.Height;

            expander.IsExpanded = true;
            window.UpdateLayout();

            Assert.Equal(collapsedHeight + 50, platformBorder.Bounds.Height, 1);
            // The content shows above the header, inside the border's default stroke of 1
            Assert.Equal(1 + 50, platformHeader.TranslatePoint(default, platformBorder)!.Value.Y, 1);
        });
    }

    static MauiExpander CreateExpander(double contentHeight) => new()
    {
        Header = new Microsoft.Maui.Controls.BoxView { HeightRequest = 20 },
        Content = new Microsoft.Maui.Controls.BoxView { HeightRequest = contentHeight },
    };

    Task RunInWindowAsync(Microsoft.Maui.Controls.Layout page, Action<Window> test) =>
        InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<MauiLayoutHandler>(page);
            var window = new Window { Width = 200, Height = 400, Content = handler.PlatformView };
            window.Show();
            try
            {
                window.UpdateLayout();
                test(window);
            }
            finally
            {
                window.Close();
            }
        });
}
