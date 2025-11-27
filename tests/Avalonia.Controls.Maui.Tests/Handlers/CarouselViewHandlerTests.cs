using Avalonia.Animation;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Tests.Stubs;
using MauiScrollBarVisibility = Microsoft.Maui.ScrollBarVisibility;
using MauiCarouselHandler = Avalonia.Controls.Maui.Handlers.CarouselViewHandler;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiGrid = Microsoft.Maui.Controls.Grid;
using PlatformScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class CarouselViewHandlerTests : HandlerTestBase<MauiCarouselHandler, CarouselViewStub>
{
    [AvaloniaFact(DisplayName = "ItemsSource initializes correctly")]
    public async Task ItemsSourceInitializesCorrectly()
    {
        var items = new[] { "One", "Two", "Three" };
        var carousel = new CarouselViewStub { ItemsSource = items };

        var handler = await CreateHandlerAsync(carousel);

        Assert.Same(items, handler.PlatformView.ItemsSource);
    }

    [AvaloniaFact(DisplayName = "ItemTemplate initializes correctly")]
    public async Task ItemTemplateInitializesCorrectly()
    {
        var carousel = new CarouselViewStub
        {
            ItemsSource = new[] { "One" },
            ItemTemplate = new DataTemplate(() => new MauiLabel { Text = "Template" })
        };

        var handler = await CreateHandlerAsync(carousel);

        Assert.NotNull(handler.PlatformView.ItemTemplate);
    }

    [AvaloniaFact(DisplayName = "CurrentItem initializes correctly")]
    public async Task CurrentItemInitializesCorrectly()
    {
        var items = new[] { "One", "Two", "Three" };
        var carousel = new CarouselViewStub
        {
            ItemsSource = items,
            CurrentItem = items[1]
        };

        var handler = await CreateHandlerAsync(carousel);

        await InvokeOnMainThreadAsync(() =>
        {
            carousel.CurrentItem = items[1];
            handler.UpdateValue(nameof(ItemsView.ItemsSource));
            handler.UpdateValue(nameof(CarouselView.CurrentItem));
        });

        Assert.Equal(1, handler.PlatformView.SelectedIndex);
    }

    [AvaloniaFact(DisplayName = "Position initializes correctly")]
    public async Task PositionInitializesCorrectly()
    {
        var items = new[] { "One", "Two", "Three" };
        var carousel = new CarouselViewStub
        {
            ItemsSource = items,
            Position = 2
        };

        var handler = await CreateHandlerAsync(carousel);

        await InvokeOnMainThreadAsync(() =>
        {
            carousel.Position = 2;
            handler.UpdateValue(nameof(ItemsView.ItemsSource));
            handler.UpdateValue(nameof(CarouselView.Position));
        });

        Assert.Equal(2, handler.PlatformView.SelectedIndex);
    }

    [AvaloniaFact(DisplayName = "Loop initializes correctly")]
    public async Task LoopInitializesCorrectly()
    {
        var carousel = CreateCarouselWithItems();
        carousel.Loop = true;

        var handler = await CreateHandlerAsync(carousel);

        Assert.True(handler.PlatformView.IsLoopingEnabled);
    }

    [AvaloniaFact(DisplayName = "IsSwipeEnabled initializes correctly")]
    public async Task IsSwipeEnabledInitializesCorrectly()
    {
        var carousel = CreateCarouselWithItems();
        carousel.IsSwipeEnabled = false;

        var handler = await CreateHandlerAsync(carousel);

        Assert.False(handler.PlatformView.IsGestureEnabled);
    }

    [AvaloniaFact(DisplayName = "ItemsLayout sets orientation")]
    public async Task ItemsLayoutSetsOrientation()
    {
        var carousel = CreateCarouselWithItems();
        var layout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
        carousel.ItemsLayout = layout;

        var handler = await CreateHandlerAsync(carousel);

        Assert.IsType<PageSlide>(handler.PlatformView.PageTransition);
        var slide = (PageSlide)handler.PlatformView.PageTransition!;
        Assert.Equal(PageSlide.SlideAxis.Vertical, slide.Orientation);
    }

    [AvaloniaFact(DisplayName = "EmptyView initializes correctly")]
    public async Task EmptyViewInitializesCorrectly()
    {
        var carousel = new CarouselViewStub
        {
            ItemsSource = Array.Empty<string>(),
            EmptyView = "No items"
        };

        var handler = await CreateHandlerAsync(carousel);

        Assert.Equal("No items", handler.PlatformView.EmptyContent);
    }

    [AvaloniaFact(DisplayName = "EmptyViewTemplate initializes correctly")]
    public async Task EmptyViewTemplateInitializesCorrectly()
    {
        var carousel = new CarouselViewStub
        {
            ItemsSource = Array.Empty<string>(),
            EmptyViewTemplate = new DataTemplate(() => new MauiLabel { Text = "Empty" })
        };

        var handler = await CreateHandlerAsync(carousel);

        Assert.NotNull(handler.PlatformView.EmptyContentTemplate);
    }

    [AvaloniaFact(DisplayName = "Scroll bars initialize disabled by default")]
    public async Task ScrollBarsInitializeDisabled()
    {
        var carousel = CreateCarouselWithItems();

        var handler = await CreateHandlerAsync(carousel);

        Assert.NotNull(handler.PlatformView);
        Assert.Equal(PlatformScrollBarVisibility.Disabled, handler.PlatformView.HorizontalScrollBarVisibility);
        Assert.Equal(PlatformScrollBarVisibility.Disabled, handler.PlatformView.VerticalScrollBarVisibility);
    }

    [AvaloniaTheory(DisplayName = "Scroll bars update correctly")]
    [InlineData(MauiScrollBarVisibility.Always, PlatformScrollBarVisibility.Visible)]
    [InlineData(MauiScrollBarVisibility.Never, PlatformScrollBarVisibility.Hidden)]
    public async Task ScrollBarsUpdateCorrectly(MauiScrollBarVisibility mauiVisibility, PlatformScrollBarVisibility expected)
    {
        var carousel = CreateCarouselWithItems();
        var handler = await CreateHandlerAsync(carousel);

        await InvokeOnMainThreadAsync(() =>
        {
            carousel.HorizontalScrollBarVisibility = mauiVisibility;
            carousel.VerticalScrollBarVisibility = mauiVisibility;

            handler.UpdateValue(nameof(ItemsView.HorizontalScrollBarVisibility));
            handler.UpdateValue(nameof(ItemsView.VerticalScrollBarVisibility));

            Assert.Equal(expected, handler.PlatformView.HorizontalScrollBarVisibility);
            Assert.Equal(expected, handler.PlatformView.VerticalScrollBarVisibility);
        });
    }
    
    private static CarouselViewStub CreateCarouselWithItems()
    {
        var carousel = new CarouselViewStub
        {
            ItemsSource = new[]
            {
                new CarouselItemStub("Item 1"),
                new CarouselItemStub("Item 2"),
                new CarouselItemStub("Item 3"),
            },
            ItemTemplate = new DataTemplate(() =>
            {
                return new MauiGrid { HeightRequest = 100, WidthRequest = 100, BackgroundColor = Colors.Blue };
            })
        };

        return carousel;
    }

    private record CarouselItemStub(string Text);
}
