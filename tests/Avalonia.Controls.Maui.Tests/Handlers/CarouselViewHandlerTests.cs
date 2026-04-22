using Avalonia.Animation;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using AvaloniaCarousel = Avalonia.Controls.Carousel;
using AvaloniaCarouselViewHandler = Avalonia.Controls.Maui.Handlers.CarouselViewHandler;
using AvaloniaSwipeGestureRecognizer = Avalonia.Input.GestureRecognizers.SwipeGestureRecognizer;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiThickness = Microsoft.Maui.Thickness;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class CarouselViewHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "Handler Creates Avalonia Carousel")]
    public async Task HandlerCreatesAvaloniaCarousel()
    {
        var carouselView = CreateCarouselView();
        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        Assert.NotNull(handler.PlatformView);
        Assert.IsAssignableFrom<AvaloniaCarousel>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "ItemsSource Initializes Correctly")]
    public async Task ItemsSourceInitializesCorrectly()
    {
        var items = CreateItems();
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = items;

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, AvaloniaCarouselViewHandler>(
            carouselView,
            handler => handler.PlatformView.ItemsSource);

        Assert.Same(items, platformItems);
    }

    [AvaloniaFact(DisplayName = "ItemsSource Updates Correctly")]
    public async Task ItemsSourceUpdatesCorrectly()
    {
        var firstItems = CreateItems();
        var secondItems = new List<string> { "Four", "Five", "Six" };
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = firstItems;

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.ItemsSource = secondItems;
            handler.UpdateValue(nameof(CarouselView.ItemsSource));
        });

        Assert.Same(secondItems, handler.PlatformView.ItemsSource);
    }

    [AvaloniaFact(DisplayName = "ItemsSource Cleared Clears CurrentItem")]
    public async Task ItemsSourceClearedClearsCurrentItem()
    {
        var items = CreateItems();
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = items;
        carouselView.CurrentItem = items[1];

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.ItemsSource = null;
            handler.UpdateValue(nameof(ItemsView.ItemsSource));
        });

        Assert.Null(handler.PlatformView.ItemsSource);
        Assert.Null(handler.PlatformView.SelectedItem);
        Assert.Null(carouselView.CurrentItem);
    }

    [AvaloniaFact(DisplayName = "ItemsSource Change Does Not Preserve Stale CurrentItem")]
    public async Task ItemsSourceChangeDoesNotPreserveStaleCurrentItem()
    {
        var firstItems = CreateItems();
        var secondItems = new List<string> { "Four", "Five", "Six" };
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = firstItems;
        carouselView.CurrentItem = firstItems[2];

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.ItemsSource = secondItems;
            handler.UpdateValue(nameof(ItemsView.ItemsSource));
        });

        Assert.Equal(secondItems[2], handler.PlatformView.SelectedItem);
        Assert.Equal(secondItems[2], carouselView.CurrentItem);
    }

    [AvaloniaFact(DisplayName = "ItemTemplate Initializes Correctly")]
    public async Task ItemTemplateInitializesCorrectly()
    {
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = CreateItems();
        carouselView.ItemTemplate = new DataTemplate(() => new MauiLabel { Text = "Templated" });

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        Assert.NotNull(handler.PlatformView.ItemTemplate);
        Assert.IsAssignableFrom<Avalonia.Controls.Control>(handler.PlatformView.ItemTemplate.Build("One"));
    }

    [AvaloniaFact(DisplayName = "ItemTemplate Updates Correctly")]
    public async Task ItemTemplateUpdatesCorrectly()
    {
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = CreateItems();
        carouselView.ItemTemplate = new DataTemplate(() => new MauiLabel { Text = "First" });

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);
        var firstTemplate = handler.PlatformView.ItemTemplate;

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.ItemTemplate = new DataTemplate(() => new MauiLabel { Text = "Second" });
            handler.UpdateValue(nameof(CarouselView.ItemTemplate));
        });

        Assert.NotNull(handler.PlatformView.ItemTemplate);
        Assert.NotSame(firstTemplate, handler.PlatformView.ItemTemplate);
        Assert.IsAssignableFrom<Avalonia.Controls.Control>(handler.PlatformView.ItemTemplate.Build("Two"));
    }

    [AvaloniaFact(DisplayName = "EmptyView String Initializes Correctly")]
    public async Task EmptyViewStringInitializesCorrectly()
    {
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = new List<string>();
        carouselView.EmptyView = "No carousel items";

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);
        var emptyContent = Assert.IsType<AvaloniaTextBlock>(handler.PlatformView.ItemTemplate?.Build(new object()));

        Assert.True(handler.PlatformView.IsShowingEmptyView());
        Assert.Equal("No carousel items", emptyContent.Text);
        Assert.Null(carouselView.CurrentItem);
    }

    [AvaloniaFact(DisplayName = "EmptyViewTemplate Initializes Correctly")]
    public async Task EmptyViewTemplateInitializesCorrectly()
    {
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = new List<string>();
        carouselView.EmptyView = "No carousel items";
        carouselView.EmptyViewTemplate = new DataTemplate(() => new MauiLabel { Text = "Templated empty view" });

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        Assert.True(handler.PlatformView.IsShowingEmptyView());
        Assert.NotNull(handler.PlatformView.ItemTemplate);
        Assert.IsAssignableFrom<Avalonia.Controls.Control>(handler.PlatformView.ItemTemplate.Build(new object()));
    }

    [AvaloniaFact(DisplayName = "EmptyView Updates Correctly")]
    public async Task EmptyViewUpdatesCorrectly()
    {
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = new List<string>();
        carouselView.EmptyView = "Initial empty view";

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.EmptyView = "Updated empty view";
            handler.UpdateValue(nameof(ItemsView.EmptyView));
        });

        var emptyContent = Assert.IsType<AvaloniaTextBlock>(handler.PlatformView.ItemTemplate?.Build(new object()));

        Assert.True(handler.PlatformView.IsShowingEmptyView());
        Assert.Equal("Updated empty view", emptyContent.Text);
    }

    [AvaloniaFact(DisplayName = "EmptyView Tracks Collection Changes")]
    public async Task EmptyViewTracksCollectionChanges()
    {
        var items = new ObservableCollection<string>();
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = items;
        carouselView.EmptyView = "No carousel items";

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        Assert.True(handler.PlatformView.IsShowingEmptyView());

        await InvokeOnMainThreadAsync(() =>
        {
            items.Add("One");
            Dispatcher.UIThread.RunJobs();
        });

        Assert.False(handler.PlatformView.IsShowingEmptyView());
        Assert.Same(items, handler.PlatformView.ItemsSource);
        Assert.Equal("One", handler.PlatformView.SelectedItem);
        Assert.Equal("One", carouselView.CurrentItem);

        await InvokeOnMainThreadAsync(() =>
        {
            items.Clear();
            Dispatcher.UIThread.RunJobs();
        });

        Assert.True(handler.PlatformView.IsShowingEmptyView());
        Assert.Null(carouselView.CurrentItem);
    }

    [AvaloniaFact(DisplayName = "Queued ItemsSource Refresh After Disconnect Does Not Throw")]
    public async Task QueuedItemsSourceRefreshAfterDisconnectDoesNotThrow()
    {
        var items = new ObservableCollection<string>();
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = items;
        carouselView.EmptyView = "No carousel items";

        await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<AvaloniaCarouselViewHandler>(carouselView);

            items.Add("One");
            ((Microsoft.Maui.IElementHandler)handler).DisconnectHandler();

            Dispatcher.UIThread.RunJobs();
        });
    }

    [AvaloniaFact(DisplayName = "Loop Maps To WrapSelection")]
    public async Task LoopMapsToWrapSelection()
    {
        var carouselView = CreateCarouselView();
        carouselView.Loop = true;

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        Assert.True(handler.PlatformView.WrapSelection);

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.Loop = false;
            handler.UpdateValue(nameof(CarouselView.Loop));
        });

        Assert.False(handler.PlatformView.WrapSelection);
    }

    [AvaloniaFact(DisplayName = "Loop Enables Avalonia Carousel Wrap Navigation")]
    public async Task LoopEnablesAvaloniaCarouselWrapNavigation()
    {
        var items = CreateItems();
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = items;
        carouselView.Loop = true;
        carouselView.Position = 2;

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        await InvokeOnMainThreadAsync(() => handler.PlatformView.Next());

        Assert.Equal(0, handler.PlatformView.SelectedIndex);
        Assert.Equal(0, carouselView.Position);
        Assert.Equal(items[0], carouselView.CurrentItem);
    }

    [AvaloniaFact(DisplayName = "IsSwipeEnabled Maps Correctly")]
    public async Task IsSwipeEnabledMapsCorrectly()
    {
        var carouselView = CreateCarouselView();
        carouselView.IsSwipeEnabled = false;

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        Assert.False(handler.PlatformView.IsSwipeEnabled);

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.IsSwipeEnabled = true;
            handler.UpdateValue(nameof(CarouselView.IsSwipeEnabled));
        });

        Assert.True(handler.PlatformView.IsSwipeEnabled);
    }

    [AvaloniaFact(DisplayName = "IsSwipeEnabled Updates Avalonia Swipe Recognizer")]
    public async Task IsSwipeEnabledUpdatesAvaloniaSwipeRecognizer()
    {
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = CreateItems();
        carouselView.IsSwipeEnabled = true;

        await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<AvaloniaCarouselViewHandler>(carouselView);
            var window = new Avalonia.Controls.Window
            {
                Content = handler.PlatformView,
                Width = 300,
                Height = 200
            };

            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                handler.PlatformView.ApplyTemplate();
                handler.PlatformView.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                var panel = Assert.IsType<Avalonia.Controls.VirtualizingCarouselPanel>(handler.PlatformView.ItemsPanelRoot);
                var recognizer = Assert.Single(panel.GestureRecognizers, recognizer => recognizer is AvaloniaSwipeGestureRecognizer);
                var swipeRecognizer = Assert.IsType<AvaloniaSwipeGestureRecognizer>(recognizer);
                Assert.True(swipeRecognizer.IsMouseEnabled);

                carouselView.IsSwipeEnabled = false;
                handler.UpdateValue(nameof(CarouselView.IsSwipeEnabled));

                Assert.False(handler.PlatformView.IsSwipeEnabled);
                Assert.DoesNotContain(
                    panel.GestureRecognizers,
                    recognizer => recognizer is AvaloniaSwipeGestureRecognizer { IsMouseEnabled: true });
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Position Maps To SelectedIndex")]
    public async Task PositionMapsToSelectedIndex()
    {
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = CreateItems();
        carouselView.Position = 1;

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        Assert.Equal(1, handler.PlatformView.SelectedIndex);

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.Position = 2;
            handler.UpdateValue(nameof(CarouselView.Position));
        });

        Assert.Equal(2, handler.PlatformView.SelectedIndex);
    }

    [AvaloniaFact(DisplayName = "CurrentItem Maps To SelectedItem")]
    public async Task CurrentItemMapsToSelectedItem()
    {
        var items = CreateItems();
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = items;
        carouselView.CurrentItem = items[1];

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        Assert.Equal(items[1], handler.PlatformView.SelectedItem);

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.CurrentItem = items[2];
            handler.UpdateValue(nameof(CarouselView.CurrentItem));
        });

        Assert.Equal(items[2], handler.PlatformView.SelectedItem);
    }

    [AvaloniaFact(DisplayName = "CurrentItem Null Clears Platform Selection")]
    public async Task CurrentItemNullClearsPlatformSelection()
    {
        var items = CreateItems();
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = items;
        carouselView.CurrentItem = items[1];

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.CurrentItem = null;
            handler.UpdateValue(nameof(CarouselView.CurrentItem));
        });

        Assert.Equal(-1, handler.PlatformView.SelectedIndex);
        Assert.Null(handler.PlatformView.SelectedItem);
        Assert.Null(carouselView.CurrentItem);
    }

    [AvaloniaFact(DisplayName = "Platform Selection Updates Virtual Selection")]
    public async Task PlatformSelectionUpdatesVirtualSelection()
    {
        var items = CreateItems();
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = items;

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        await InvokeOnMainThreadAsync(() => handler.PlatformView.SelectedIndex = 2);

        Assert.Equal(2, carouselView.Position);
        Assert.Equal(items[2], carouselView.CurrentItem);
    }

    [AvaloniaFact(DisplayName = "Platform Selection Raises Virtual Events")]
    public async Task PlatformSelectionRaisesVirtualEvents()
    {
        var positionChangedCount = 0;
        var currentItemChangedCount = 0;
        var items = CreateItems();
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = items;

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        carouselView.PositionChanged += (_, _) => positionChangedCount++;
        carouselView.CurrentItemChanged += (_, _) => currentItemChangedCount++;

        await InvokeOnMainThreadAsync(() => handler.PlatformView.SelectedIndex = 2);

        Assert.Equal(1, positionChangedCount);
        Assert.Equal(1, currentItemChangedCount);
    }

    [AvaloniaFact(DisplayName = "Platform Selection Executes Commands")]
    public async Task PlatformSelectionExecutesCommands()
    {
        var positionCommandCount = 0;
        var currentItemCommandCount = 0;
        object? positionParameter = null;
        object? currentItemParameter = null;
        var items = CreateItems();
        var carouselView = CreateCarouselView();
        carouselView.ItemsSource = items;
        carouselView.PositionChangedCommandParameter = "position";
        carouselView.CurrentItemChangedCommandParameter = "current";
        carouselView.PositionChangedCommand = new TestCommand<object>(parameter =>
        {
            positionCommandCount++;
            positionParameter = parameter;
        });
        carouselView.CurrentItemChangedCommand = new TestCommand<object>(parameter =>
        {
            currentItemCommandCount++;
            currentItemParameter = parameter;
        });

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);
        positionCommandCount = 0;
        currentItemCommandCount = 0;
        positionParameter = null;
        currentItemParameter = null;

        await InvokeOnMainThreadAsync(() => handler.PlatformView.SelectedIndex = 1);

        Assert.Equal(1, positionCommandCount);
        Assert.Equal("position", positionParameter);
        Assert.Equal(1, currentItemCommandCount);
        Assert.Equal("current", currentItemParameter);
    }

    [AvaloniaFact(DisplayName = "Vertical ItemsLayout Maps To Vertical PageSlide")]
    public async Task VerticalItemsLayoutMapsToVerticalPageSlide()
    {
        var carouselView = CreateCarouselView();
        carouselView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);
        var transition = Assert.IsType<PageSlide>(handler.PlatformView.PageTransition);

        Assert.Equal(PageSlide.SlideAxis.Vertical, transition.Orientation);
    }

    [AvaloniaFact(DisplayName = "IsScrollAnimated False Uses Zero Duration Transition")]
    public async Task IsScrollAnimatedFalseUsesZeroDurationTransition()
    {
        var carouselView = CreateCarouselView();
        carouselView.IsScrollAnimated = false;

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);
        var transition = Assert.IsType<PageSlide>(handler.PlatformView.PageTransition);

        Assert.Equal(TimeSpan.Zero, transition.Duration);
    }

    [AvaloniaFact(DisplayName = "PeekAreaInsets Maps To ViewportFraction")]
    public async Task PeekAreaInsetsMapsToViewportFraction()
    {
        var carouselView = CreateCarouselView();
        carouselView.WidthRequest = 200;
        carouselView.PeekAreaInsets = new MauiThickness(20, 0, 20, 0);

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        Assert.Equal(0.8, handler.PlatformView.ViewportFraction, precision: 3);
    }

    [AvaloniaFact(DisplayName = "PeekAreaInsets Updates ViewportFraction")]
    public async Task PeekAreaInsetsUpdatesViewportFraction()
    {
        var carouselView = CreateCarouselView();
        carouselView.WidthRequest = 200;
        carouselView.PeekAreaInsets = new MauiThickness(10, 0, 10, 0);

        var handler = await CreateHandlerAsync<AvaloniaCarouselViewHandler>(carouselView);

        await InvokeOnMainThreadAsync(() =>
        {
            carouselView.PeekAreaInsets = new MauiThickness(40, 0, 40, 0);
            handler.UpdateValue(nameof(CarouselView.PeekAreaInsets));
        });

        Assert.Equal(0.6, handler.PlatformView.ViewportFraction, precision: 3);
    }

    private static CarouselView CreateCarouselView()
    {
        return new CarouselView
        {
            WidthRequest = 240,
            HeightRequest = 160
        };
    }

    private static List<string> CreateItems()
    {
        return new List<string> { "One", "Two", "Three" };
    }
}
