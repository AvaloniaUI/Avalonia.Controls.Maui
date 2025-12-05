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

    [AvaloniaFact(DisplayName = "ScrollChanged Updates Offsets And Signals Completion")]
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
