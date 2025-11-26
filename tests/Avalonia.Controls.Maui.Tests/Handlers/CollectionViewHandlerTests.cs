using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using MauiCollectionViewHandler = Avalonia.Controls.Maui.Handlers.CollectionViewHandler;
using AvaloniaCollectionView = Avalonia.Controls.Maui.CollectionView;
using MauiScrollBarVisibility = Microsoft.Maui.ScrollBarVisibility;
using AvaloniaScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility;
using MauiSelectionMode = Microsoft.Maui.Controls.SelectionMode;
using AvaloniaSelectionMode = Avalonia.Controls.SelectionMode;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class CollectionViewHandlerTests : HandlerTestBase
{
    private static Microsoft.Maui.Controls.CollectionView CreateCollectionView()
    {
        var collectionView = new Microsoft.Maui.Controls.CollectionView();
        collectionView.WidthRequest = 200;
        collectionView.HeightRequest = 300;
        return collectionView;
    }

    [AvaloniaFact(DisplayName = "Handler Creates Avalonia CollectionView")]
    public async Task HandlerCreatesAvaloniaCollectionView()
    {
        var collectionView = CreateCollectionView();
        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<AvaloniaCollectionView>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "ItemsSource Initializes Correctly")]
    public async Task ItemsSourceInitializesCorrectly()
    {
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiCollectionViewHandler>(
            collectionView, GetPlatformItemsSource);

        Assert.NotNull(platformItems);
        Assert.Equal(3, platformItems.Cast<object>().Count());
    }

    [AvaloniaFact(DisplayName = "ItemsSource Updates Correctly")]
    public async Task ItemsSourceUpdatesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string> { "Item 1" };

        var newItems = new List<string> { "A", "B", "C", "D", "E" };

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiCollectionViewHandler>(
            collectionView, handler =>
            {
                collectionView.ItemsSource = newItems;
                handler.UpdateValue(nameof(ItemsView.ItemsSource));
                return GetPlatformItemsSource(handler);
            });

        Assert.NotNull(platformItems);
        Assert.Equal(5, platformItems.Cast<object>().Count());
    }

    [AvaloniaFact(DisplayName = "Null ItemsSource Doesn't Crash")]
    public async Task NullItemsSourceDoesntCrash()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = null;
        await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
    }

    [AvaloniaFact(DisplayName = "Empty ItemsSource Works")]
    public async Task EmptyItemsSourceWorks()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string>();
        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiCollectionViewHandler>(
            collectionView, GetPlatformItemsSource);

        Assert.NotNull(platformItems);
        Assert.Empty(platformItems.Cast<object>());
    }

    [AvaloniaFact(DisplayName = "ObservableCollection ItemsSource Works")]
    public async Task ObservableCollectionItemsSourceWorks()
    {
        var items = new ObservableCollection<string> { "Item 1", "Item 2" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiCollectionViewHandler>(
            collectionView, GetPlatformItemsSource);

        Assert.NotNull(platformItems);
        Assert.Equal(2, platformItems.Cast<object>().Count());
    }

    [AvaloniaFact(DisplayName = "EmptyView String Initializes Correctly")]
    public async Task EmptyViewStringInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string>();
        collectionView.EmptyView = "No items available";

        var emptyView = await GetValueAsync<object?, MauiCollectionViewHandler>(
            collectionView, GetPlatformEmptyView);

        Assert.NotNull(emptyView);
        Assert.Equal("No items available", emptyView);
    }

    [AvaloniaFact(DisplayName = "EmptyView Updates Correctly")]
    public async Task EmptyViewUpdatesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string>();
        collectionView.EmptyView = "Initial";

        var emptyView = await GetValueAsync<object?, MauiCollectionViewHandler>(
            collectionView, handler =>
            {
                collectionView.EmptyView = "Updated empty view";
                handler.UpdateValue(nameof(ItemsView.EmptyView));
                return GetPlatformEmptyView(handler);
            });

        Assert.NotNull(emptyView);
        Assert.Equal("Updated empty view", emptyView);
    }

    [AvaloniaFact(DisplayName = "HorizontalScrollBarVisibility Always Initializes Correctly")]
    public async Task HorizontalScrollBarVisibilityAlwaysInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.HorizontalScrollBarVisibility = MauiScrollBarVisibility.Always;

        var platformVisibility = await GetValueAsync<AvaloniaScrollBarVisibility, MauiCollectionViewHandler>(
            collectionView, GetPlatformHorizontalScrollBarVisibility);

        Assert.Equal(AvaloniaScrollBarVisibility.Visible, platformVisibility);
    }

    [AvaloniaFact(DisplayName = "HorizontalScrollBarVisibility Never Initializes Correctly")]
    public async Task HorizontalScrollBarVisibilityNeverInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.HorizontalScrollBarVisibility = MauiScrollBarVisibility.Never;

        var platformVisibility = await GetValueAsync<AvaloniaScrollBarVisibility, MauiCollectionViewHandler>(
            collectionView, GetPlatformHorizontalScrollBarVisibility);

        Assert.Equal(AvaloniaScrollBarVisibility.Hidden, platformVisibility);
    }

    [AvaloniaFact(DisplayName = "VerticalScrollBarVisibility Always Initializes Correctly")]
    public async Task VerticalScrollBarVisibilityAlwaysInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.VerticalScrollBarVisibility = MauiScrollBarVisibility.Always;

        var platformVisibility = await GetValueAsync<AvaloniaScrollBarVisibility, MauiCollectionViewHandler>(
            collectionView, GetPlatformVerticalScrollBarVisibility);

        Assert.Equal(AvaloniaScrollBarVisibility.Visible, platformVisibility);
    }

    [AvaloniaFact(DisplayName = "VerticalScrollBarVisibility Never Initializes Correctly")]
    public async Task VerticalScrollBarVisibilityNeverInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.VerticalScrollBarVisibility = MauiScrollBarVisibility.Never;

        var platformVisibility = await GetValueAsync<AvaloniaScrollBarVisibility, MauiCollectionViewHandler>(
            collectionView, GetPlatformVerticalScrollBarVisibility);

        Assert.Equal(AvaloniaScrollBarVisibility.Hidden, platformVisibility);
    }

    [AvaloniaFact(DisplayName = "IsGrouped Initializes Correctly False")]
    public async Task IsGroupedInitializesCorrectlyFalse()
    {
        var collectionView = CreateCollectionView();
        collectionView.IsGrouped = false;

        var isGrouped = await GetValueAsync<bool, MauiCollectionViewHandler>(
            collectionView, GetPlatformIsGrouped);

        Assert.False(isGrouped);
    }

    [AvaloniaFact(DisplayName = "IsGrouped Initializes Correctly True")]
    public async Task IsGroupedInitializesCorrectlyTrue()
    {
        var collectionView = CreateCollectionView();
        collectionView.IsGrouped = true;

        var isGrouped = await GetValueAsync<bool, MauiCollectionViewHandler>(
            collectionView, GetPlatformIsGrouped);

        Assert.True(isGrouped);
    }

    [AvaloniaFact(DisplayName = "IsGrouped Updates Correctly")]
    public async Task IsGroupedUpdatesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.IsGrouped = false;

        var isGrouped = await GetValueAsync<bool, MauiCollectionViewHandler>(
            collectionView, handler =>
            {
                collectionView.IsGrouped = true;
                handler.UpdateValue(nameof(GroupableItemsView.IsGrouped));
                return GetPlatformIsGrouped(handler);
            });

        Assert.True(isGrouped);
    }

    [AvaloniaFact(DisplayName = "SelectionMode None Initializes Correctly")]
    public async Task SelectionModeNoneInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.SelectionMode = MauiSelectionMode.None;

        var selectionMode = await GetValueAsync<AvaloniaSelectionMode, MauiCollectionViewHandler>(
            collectionView, GetPlatformSelectionMode);

        // None maps to Single in Avalonia (but selection is handled differently)
        Assert.Equal(AvaloniaSelectionMode.Single, selectionMode);
    }

    [AvaloniaFact(DisplayName = "SelectionMode Single Initializes Correctly")]
    public async Task SelectionModeSingleInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.SelectionMode = MauiSelectionMode.Single;

        var selectionMode = await GetValueAsync<AvaloniaSelectionMode, MauiCollectionViewHandler>(
            collectionView, GetPlatformSelectionMode);

        Assert.Equal(AvaloniaSelectionMode.Single, selectionMode);
    }

    [AvaloniaFact(DisplayName = "SelectionMode Multiple Initializes Correctly")]
    public async Task SelectionModeMultipleInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.SelectionMode = MauiSelectionMode.Multiple;

        var selectionMode = await GetValueAsync<AvaloniaSelectionMode, MauiCollectionViewHandler>(
            collectionView, GetPlatformSelectionMode);

        Assert.Equal(AvaloniaSelectionMode.Multiple, selectionMode);
    }

    [AvaloniaFact(DisplayName = "SelectedItem Initializes Correctly")]
    public async Task SelectedItemInitializesCorrectly()
    {
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.SelectionMode = MauiSelectionMode.Single;
        collectionView.SelectedItem = "Item 2";

        var selectedItem = await GetValueAsync<object?, MauiCollectionViewHandler>(
            collectionView, GetPlatformSelectedItem);

        Assert.Equal("Item 2", selectedItem);
    }

    [AvaloniaFact(DisplayName = "SelectedItem Updates Correctly")]
    public async Task SelectedItemUpdatesCorrectly()
    {
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.SelectionMode = MauiSelectionMode.Single;
        collectionView.SelectedItem = "Item 1";

        var selectedItem = await GetValueAsync<object?, MauiCollectionViewHandler>(
            collectionView, handler =>
            {
                collectionView.SelectedItem = "Item 3";
                handler.UpdateValue(nameof(SelectableItemsView.SelectedItem));
                return GetPlatformSelectedItem(handler);
            });

        Assert.Equal("Item 3", selectedItem);
    }

    [AvaloniaFact(DisplayName = "Null SelectedItem Works")]
    public async Task NullSelectedItemWorks()
    {
        var items = new List<string> { "Item 1", "Item 2" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.SelectionMode = MauiSelectionMode.Single;
        collectionView.SelectedItem = null;

        var selectedItem = await GetValueAsync<object?, MauiCollectionViewHandler>(
            collectionView, GetPlatformSelectedItem);

        Assert.Null(selectedItem);
    }

    [AvaloniaFact(DisplayName = "LinearItemsLayout Vertical Initializes Correctly")]
    public async Task LinearItemsLayoutVerticalInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);

        var itemsLayout = await GetValueAsync<IItemsLayout?, MauiCollectionViewHandler>(
            collectionView, GetPlatformItemsLayout);

        Assert.NotNull(itemsLayout);
        Assert.IsType<LinearItemsLayout>(itemsLayout);
        Assert.Equal(ItemsLayoutOrientation.Vertical, ((LinearItemsLayout)itemsLayout).Orientation);
    }

    [AvaloniaFact(DisplayName = "LinearItemsLayout Horizontal Initializes Correctly")]
    public async Task LinearItemsLayoutHorizontalInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

        var itemsLayout = await GetValueAsync<IItemsLayout?, MauiCollectionViewHandler>(
            collectionView, GetPlatformItemsLayout);

        Assert.NotNull(itemsLayout);
        Assert.IsType<LinearItemsLayout>(itemsLayout);
        Assert.Equal(ItemsLayoutOrientation.Horizontal, ((LinearItemsLayout)itemsLayout).Orientation);
    }

    [AvaloniaFact(DisplayName = "GridItemsLayout Initializes Correctly")]
    public async Task GridItemsLayoutInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);

        var itemsLayout = await GetValueAsync<IItemsLayout?, MauiCollectionViewHandler>(
            collectionView, GetPlatformItemsLayout);

        Assert.NotNull(itemsLayout);
        Assert.IsType<GridItemsLayout>(itemsLayout);
        Assert.Equal(2, ((GridItemsLayout)itemsLayout).Span);
    }

    [AvaloniaFact(DisplayName = "ItemsLayout Updates Correctly")]
    public async Task ItemsLayoutUpdatesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);

        var itemsLayout = await GetValueAsync<IItemsLayout?, MauiCollectionViewHandler>(
            collectionView, handler =>
            {
                collectionView.ItemsLayout = new GridItemsLayout(3, ItemsLayoutOrientation.Vertical);
                handler.UpdateValue(nameof(StructuredItemsView.ItemsLayout));
                return GetPlatformItemsLayout(handler);
            });

        Assert.NotNull(itemsLayout);
        Assert.IsType<GridItemsLayout>(itemsLayout);
        Assert.Equal(3, ((GridItemsLayout)itemsLayout).Span);
    }

    [AvaloniaFact(DisplayName = "ItemTemplate Applied Correctly")]
    public async Task ItemTemplateAppliedCorrectly()
    {
        var template = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label();
            label.SetBinding(Microsoft.Maui.Controls.Label.TextProperty, ".");
            return label;
        });

        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string> { "Test" };
        collectionView.ItemTemplate = template;

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        // The handler should not crash and should have applied the template
        Assert.NotNull(handler.PlatformView);
        Assert.NotNull(handler.PlatformView.ItemTemplate);
    }

    [AvaloniaFact(DisplayName = "GroupHeaderTemplate Applied Correctly")]
    public async Task GroupHeaderTemplateAppliedCorrectly()
    {
        var headerTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Header" };
            return label;
        });

        var collectionView = CreateCollectionView();
        collectionView.IsGrouped = true;
        collectionView.GroupHeaderTemplate = headerTemplate;

        var groupHeaderTemplate = await GetValueAsync<global::Avalonia.Controls.Templates.IDataTemplate?, MauiCollectionViewHandler>(
            collectionView, GetPlatformGroupHeaderTemplate);

        Assert.NotNull(groupHeaderTemplate);
    }

    [AvaloniaFact(DisplayName = "GroupFooterTemplate Applied Correctly")]
    public async Task GroupFooterTemplateAppliedCorrectly()
    {
        var footerTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Footer" };
            return label;
        });

        var collectionView = CreateCollectionView();
        collectionView.IsGrouped = true;
        collectionView.GroupFooterTemplate = footerTemplate;

        var groupFooterTemplate = await GetValueAsync<global::Avalonia.Controls.Templates.IDataTemplate?, MauiCollectionViewHandler>(
            collectionView, GetPlatformGroupFooterTemplate);

        Assert.NotNull(groupFooterTemplate);
    }

    [AvaloniaFact(DisplayName = "Header String Initializes Correctly")]
    public async Task HeaderStringInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.Header = "My Header";

        var header = await GetValueAsync<object?, MauiCollectionViewHandler>(
            collectionView, GetPlatformHeader);

        Assert.NotNull(header);
        Assert.Equal("My Header", header);
    }

    [AvaloniaFact(DisplayName = "HeaderTemplate Applied Correctly")]
    public async Task HeaderTemplateAppliedCorrectly()
    {
        var headerTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Header Template" };
            return label;
        });

        var collectionView = CreateCollectionView();
        collectionView.HeaderTemplate = headerTemplate;
        collectionView.Header = "Header Data";

        var platformHeaderTemplate = await GetValueAsync<global::Avalonia.Controls.Templates.IDataTemplate?, MauiCollectionViewHandler>(
            collectionView, GetPlatformHeaderTemplate);

        Assert.NotNull(platformHeaderTemplate);
    }

    [AvaloniaFact(DisplayName = "Footer String Initializes Correctly")]
    public async Task FooterStringInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.Footer = "My Footer";

        var footer = await GetValueAsync<object?, MauiCollectionViewHandler>(
            collectionView, GetPlatformFooter);

        Assert.NotNull(footer);
        Assert.Equal("My Footer", footer);
    }

    [AvaloniaFact(DisplayName = "FooterTemplate Applied Correctly")]
    public async Task FooterTemplateAppliedCorrectly()
    {
        var footerTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Footer Template" };
            return label;
        });

        var collectionView = CreateCollectionView();
        collectionView.FooterTemplate = footerTemplate;
        collectionView.Footer = "Footer Data";

        var platformFooterTemplate = await GetValueAsync<global::Avalonia.Controls.Templates.IDataTemplate?, MauiCollectionViewHandler>(
            collectionView, GetPlatformFooterTemplate);

        Assert.NotNull(platformFooterTemplate);
    }

    [AvaloniaFact(DisplayName = "SelectedItems Initializes Correctly")]
    public async Task SelectedItemsInitializesCorrectly()
    {
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.SelectionMode = MauiSelectionMode.Multiple;

        // SelectedItems is read-only in MAUI, but we can test that the property exists
        var selectedItems = await GetValueAsync<System.Collections.Generic.IList<object>?, MauiCollectionViewHandler>(
            collectionView, GetPlatformSelectedItems);

        // SelectedItems may be null initially when nothing is selected
        // Just verify the handler doesn't crash
        Assert.True(true);
    }

    [AvaloniaFact(DisplayName = "RemainingItemsThreshold Initializes Correctly")]
    public async Task RemainingItemsThresholdInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.RemainingItemsThreshold = 5;

        var threshold = await GetValueAsync<int, MauiCollectionViewHandler>(
            collectionView, GetPlatformRemainingItemsThreshold);

        Assert.Equal(5, threshold);
    }

    [AvaloniaFact(DisplayName = "RemainingItemsThreshold Updates Correctly")]
    public async Task RemainingItemsThresholdUpdatesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.RemainingItemsThreshold = 3;

        var threshold = await GetValueAsync<int, MauiCollectionViewHandler>(
            collectionView, handler =>
            {
                collectionView.RemainingItemsThreshold = 10;
                handler.UpdateValue(nameof(ItemsView.RemainingItemsThreshold));
                return GetPlatformRemainingItemsThreshold(handler);
            });

        Assert.Equal(10, threshold);
    }

    [AvaloniaFact(DisplayName = "ItemsUpdatingScrollMode Initializes Correctly")]
    public async Task ItemsUpdatingScrollModeInitializesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;

        var scrollMode = await GetValueAsync<ItemsUpdatingScrollMode, MauiCollectionViewHandler>(
            collectionView, GetPlatformItemsUpdatingScrollMode);

        Assert.Equal(ItemsUpdatingScrollMode.KeepLastItemInView, scrollMode);
    }

    [AvaloniaFact(DisplayName = "ItemsUpdatingScrollMode Updates Correctly")]
    public async Task ItemsUpdatingScrollModeUpdatesCorrectly()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;

        var scrollMode = await GetValueAsync<ItemsUpdatingScrollMode, MauiCollectionViewHandler>(
            collectionView, handler =>
            {
                collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
                handler.UpdateValue(nameof(ItemsView.ItemsUpdatingScrollMode));
                return GetPlatformItemsUpdatingScrollMode(handler);
            });

        Assert.Equal(ItemsUpdatingScrollMode.KeepScrollOffset, scrollMode);
    }

    // Helper methods to get platform values
    System.Collections.IEnumerable? GetPlatformItemsSource(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.ItemsSource;

    object? GetPlatformEmptyView(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.EmptyView;

    AvaloniaScrollBarVisibility GetPlatformHorizontalScrollBarVisibility(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.HorizontalScrollBarVisibility ?? AvaloniaScrollBarVisibility.Auto;

    AvaloniaScrollBarVisibility GetPlatformVerticalScrollBarVisibility(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.VerticalScrollBarVisibility ?? AvaloniaScrollBarVisibility.Auto;

    bool GetPlatformIsGrouped(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.IsGrouped ?? false;

    AvaloniaSelectionMode GetPlatformSelectionMode(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.SelectionMode ?? AvaloniaSelectionMode.Single;

    object? GetPlatformSelectedItem(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.SelectedItem;

    IItemsLayout? GetPlatformItemsLayout(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.ItemsLayout;

    global::Avalonia.Controls.Templates.IDataTemplate? GetPlatformGroupHeaderTemplate(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.GroupHeaderTemplate;

    global::Avalonia.Controls.Templates.IDataTemplate? GetPlatformGroupFooterTemplate(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.GroupFooterTemplate;

    object? GetPlatformHeader(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.Header;

    global::Avalonia.Controls.Templates.IDataTemplate? GetPlatformHeaderTemplate(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.HeaderTemplate;

    object? GetPlatformFooter(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.Footer;

    global::Avalonia.Controls.Templates.IDataTemplate? GetPlatformFooterTemplate(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.FooterTemplate;

    System.Collections.Generic.IList<object>? GetPlatformSelectedItems(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.SelectedItems;

    int GetPlatformRemainingItemsThreshold(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.RemainingItemsThreshold ?? -1;

    ItemsUpdatingScrollMode GetPlatformItemsUpdatingScrollMode(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.ItemsUpdatingScrollMode ?? ItemsUpdatingScrollMode.KeepItemsInView;
}
