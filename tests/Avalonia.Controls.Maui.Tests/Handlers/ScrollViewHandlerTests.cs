using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ScrollViewHandlerTests : HandlerTestBase<ScrollViewHandler, ScrollViewStub>
{
    protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder)
    {
        return base.ConfigureBuilder(mauiAppBuilder)
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<ButtonStub, ButtonHandler>();
            });
    }

    [AvaloniaFact(DisplayName = "Content Initializes Correctly")]
    public async Task ContentInitializesCorrectly()
    {
        var scrollView = new ScrollViewStub
        {
            Content = new ButtonStub { Text = "Item" }
        };

        var platformContent = await GetValueAsync(scrollView, handler => handler.PlatformView.Content);

        Assert.NotNull(platformContent);
    }

    [AvaloniaFact(DisplayName = "Content Alignment Updates When Its Options Change")]
    public async Task ContentAlignmentUpdatesWhenOptionsChange()
    {
        var content = new Microsoft.Maui.Controls.Label
        {
            Text = "Item",
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
            VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
        };
        var scrollView = new Microsoft.Maui.Controls.ScrollView
        {
            Content = content
        };

        var platformContent = await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<ScrollViewHandler>(scrollView);
            return Assert.IsAssignableFrom<Control>(handler.PlatformView.Content);
        });

        await InvokeOnMainThreadAsync(() =>
        {
            Assert.Equal(Avalonia.Layout.HorizontalAlignment.Left, platformContent.HorizontalAlignment);
            Assert.Equal(Avalonia.Layout.VerticalAlignment.Top, platformContent.VerticalAlignment);

            content.HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Center;
            content.VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.End;

            Assert.Equal(Avalonia.Layout.HorizontalAlignment.Center, platformContent.HorizontalAlignment);
            Assert.Equal(Avalonia.Layout.VerticalAlignment.Bottom, platformContent.VerticalAlignment);
        });
    }

    [AvaloniaFact(DisplayName = "Scroll Bars Update Correctly")]
    public async Task ScrollBarsUpdateCorrectly()
    {
        var scrollView = new ScrollViewStub
        {
            Orientation = ScrollOrientation.Both,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never
        };

        var handler = await CreateHandlerAsync(scrollView);

        Assert.Equal(Primitives.ScrollBarVisibility.Visible, handler.PlatformView.HorizontalScrollBarVisibility);
        Assert.Equal(Primitives.ScrollBarVisibility.Hidden, handler.PlatformView.VerticalScrollBarVisibility);
    }

    [AvaloniaFact(DisplayName = "Orientation Disables Opposite Scroll Bar")]
    public async Task OrientationDisablesOppositeScrollBar()
    {
        var scrollView = new ScrollViewStub
        {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
            VerticalScrollBarVisibility = ScrollBarVisibility.Always
        };

        var handler = await CreateHandlerAsync(scrollView);

        Assert.Equal(Primitives.ScrollBarVisibility.Visible, handler.PlatformView.HorizontalScrollBarVisibility);
        Assert.Equal(Primitives.ScrollBarVisibility.Disabled, handler.PlatformView.VerticalScrollBarVisibility);
    }

    [AvaloniaFact(DisplayName = "ScrollChanged Updates Offsets And Signals Completion", Skip = "ScrollViewStub is not Microsoft.Maui.Controls.ScrollView, which means it can't get the right type.")]
    public async Task ScrollChangedUpdatesOffsetsAndSignalsCompletion()
    {
        var scrollView = new ScrollViewStub();
        var handler = await CreateHandlerAsync(scrollView);

        var request = new ScrollToRequest(5, 15, true);
        ScrollViewHandler.MapRequestScrollTo(handler, scrollView, request);

        Assert.Equal(5, scrollView.HorizontalOffset);
        Assert.Equal(15, scrollView.VerticalOffset);
        Assert.Equal(5, scrollView.ScrollX);
        Assert.Equal(15, scrollView.ScrollY);
        Assert.True(scrollView.ScrollFinishedInvoked);
    }
}
