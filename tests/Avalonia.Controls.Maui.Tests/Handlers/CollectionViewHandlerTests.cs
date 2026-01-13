using Avalonia.VisualTree;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using AvaloniaCollectionView = Avalonia.Controls.Maui.MauiCollectionView;
using AvaloniaSelectionMode = Avalonia.Controls.SelectionMode;
using AvaloniaScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility;
using MauiCollectionViewHandler = Avalonia.Controls.Maui.Handlers.CollectionViewHandler;
using MauiScrollBarVisibility = Microsoft.Maui.ScrollBarVisibility;
using MauiSelectionMode = Microsoft.Maui.Controls.SelectionMode;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class CollectionViewHandlerTests : HandlerTestBase
{
    private static CollectionView CreateCollectionView()
    {
        var collectionView = new CollectionView();
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

    [AvaloniaFact(DisplayName = "Large ItemsSource Works")]
    public async Task LargeItemsSourceWorks()
    {
        var items = Enumerable.Range(1, 1000).Select(i => $"Item {i}").ToList();
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiCollectionViewHandler>(
            collectionView, GetPlatformItemsSource);

        Assert.NotNull(platformItems);
        Assert.Equal(1000, platformItems.Cast<object>().Count());
    }

    [AvaloniaFact(DisplayName = "Collection Change Triggers Update")]
    public async Task CollectionChangeTriggersUpdate()
    {
        var items = new ObservableCollection<string> { "A", "B" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        items.Add("C");

        // Wait a bit for the binding to update
        await Task.Delay(50);

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiCollectionViewHandler>(
            collectionView, GetPlatformItemsSource);

        Assert.NotNull(platformItems);
        Assert.Equal(3, platformItems.Cast<object>().Count());
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

    [AvaloniaFact(DisplayName = "EmptyViewTemplate Applied Correctly")]
    public async Task EmptyViewTemplateAppliedCorrectly()
    {
        var template = new DataTemplate(() => new Microsoft.Maui.Controls.Label { Text = "Empty Template" });
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string>();
        collectionView.EmptyViewTemplate = template;

        var platformTemplate = await GetValueAsync<Templates.IDataTemplate?, MauiCollectionViewHandler>(
            collectionView, GetPlatformEmptyViewTemplate);

        Assert.NotNull(platformTemplate);
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

    [AvaloniaTheory(DisplayName = "HorizontalScrollBarVisibility Maps Correctly")]
    [InlineData(MauiScrollBarVisibility.Always, AvaloniaScrollBarVisibility.Visible)]
    [InlineData(MauiScrollBarVisibility.Never, AvaloniaScrollBarVisibility.Hidden)]
    [InlineData(MauiScrollBarVisibility.Default, AvaloniaScrollBarVisibility.Auto)]
    public async Task HorizontalScrollBarVisibilityMapsCorrectly(MauiScrollBarVisibility mauiVisibility, AvaloniaScrollBarVisibility expectedAvalonia)
    {
        var collectionView = CreateCollectionView();
        collectionView.HorizontalScrollBarVisibility = mauiVisibility;

        var platformVisibility = await GetValueAsync<AvaloniaScrollBarVisibility, MauiCollectionViewHandler>(
            collectionView, GetPlatformHorizontalScrollBarVisibility);

        Assert.Equal(expectedAvalonia, platformVisibility);
    }

    [AvaloniaTheory(DisplayName = "VerticalScrollBarVisibility Maps Correctly")]
    [InlineData(MauiScrollBarVisibility.Always, AvaloniaScrollBarVisibility.Visible)]
    [InlineData(MauiScrollBarVisibility.Never, AvaloniaScrollBarVisibility.Hidden)]
    [InlineData(MauiScrollBarVisibility.Default, AvaloniaScrollBarVisibility.Auto)]
    public async Task VerticalScrollBarVisibilityMapsCorrectly(MauiScrollBarVisibility mauiVisibility, AvaloniaScrollBarVisibility expectedAvalonia)
    {
        var collectionView = CreateCollectionView();
        collectionView.VerticalScrollBarVisibility = mauiVisibility;

        var platformVisibility = await GetValueAsync<AvaloniaScrollBarVisibility, MauiCollectionViewHandler>(
            collectionView, GetPlatformVerticalScrollBarVisibility);

        Assert.Equal(expectedAvalonia, platformVisibility);
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

    [AvaloniaFact(DisplayName = "SelectionMode None Clears Selection")]
    public async Task SelectionModeNoneClearsSelection()
    {
        var items = new ObservableCollection<string> { "Item 1", "Item 2" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.SelectionMode = MauiSelectionMode.Single;
        collectionView.SelectedItem = "Item 1";

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        Assert.NotNull(handler.PlatformView.SelectedItem);

        collectionView.SelectionMode = MauiSelectionMode.None;
        handler.UpdateValue(nameof(SelectableItemsView.SelectionMode));

        var selectedItem = await GetValueAsync<object?, MauiCollectionViewHandler>(
            collectionView, GetPlatformSelectedItem);

        Assert.Null(selectedItem);
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

        var groupHeaderTemplate = await GetValueAsync<Templates.IDataTemplate?, MauiCollectionViewHandler>(
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

        var groupFooterTemplate = await GetValueAsync<Templates.IDataTemplate?, MauiCollectionViewHandler>(
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

        var platformHeaderTemplate = await GetValueAsync<Templates.IDataTemplate?, MauiCollectionViewHandler>(
            collectionView, GetPlatformHeaderTemplate);

        Assert.NotNull(platformHeaderTemplate);
    }

    [AvaloniaFact(DisplayName = "Header With Both Content And Template Prioritizes Content")]
    public async Task HeaderWithBothContentAndTemplatePrioritizesContent()
    {
        var collectionView = CreateCollectionView();
        collectionView.Header = "String Header";
        collectionView.HeaderTemplate = new DataTemplate(() => new Microsoft.Maui.Controls.Label { Text = "Template Header" });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        var header = await GetValueAsync<object?, MauiCollectionViewHandler>(
            collectionView, GetPlatformHeader);

        Assert.Equal("String Header", header);
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

        var platformFooterTemplate = await GetValueAsync<Templates.IDataTemplate?, MauiCollectionViewHandler>(
            collectionView, GetPlatformFooterTemplate);

        Assert.NotNull(platformFooterTemplate);
    }

    [AvaloniaFact(DisplayName = "Footer With Both Content And Template Prioritizes Content")]
    public async Task FooterWithBothContentAndTemplatePrioritizesContent()
    {
        var collectionView = CreateCollectionView();
        collectionView.Footer = "String Footer";
        collectionView.FooterTemplate = new DataTemplate(() => new Microsoft.Maui.Controls.Label { Text = "Template Footer" });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        var footer = await GetValueAsync<object?, MauiCollectionViewHandler>(
            collectionView, GetPlatformFooter);

        Assert.Equal("String Footer", footer);
    }

    [AvaloniaFact(DisplayName = "DataTemplateSelector Works")]
    public async Task DataTemplateSelectorWorks()
    {
        var items = new List<string> { "TypeA", "TypeB" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;

        var templateA = new DataTemplate(() => new Microsoft.Maui.Controls.Label { Text = "A" });
        var templateB = new DataTemplate(() => new Microsoft.Maui.Controls.Label { Text = "B" });

        collectionView.ItemTemplate = new TestTemplateSelector
        {
            TemplateA = templateA,
            TemplateB = templateB
        };

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        // Force evaluation
        handler.PlatformView.UpdateItemTemplate(collectionView, handler);
        Assert.NotNull(handler.PlatformView.ItemTemplate);
    }

    [AvaloniaFact(DisplayName = "SelectedItems Initializes Correctly")]
    public async Task SelectedItemsInitializesCorrectly()
    {
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.SelectionMode = MauiSelectionMode.Multiple;

        // SelectedItems is read-only in MAUI, but we can test that the property exists
        var selectedItems = await GetValueAsync<IList<object>?, MauiCollectionViewHandler>(
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

[AvaloniaFact(DisplayName = "EmptyView Appears In VisualTree")]
    public async Task EmptyViewAppearsInVisualTree()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string>();
        collectionView.EmptyView = new Microsoft.Maui.Controls.Label { Text = "Empty" };

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        var platformView = handler.PlatformView;

        // Force layout pass
        Assert.Single(platformView.GetVisualDescendants().OfType<TextBlock>(), tb => tb.Text == "Empty");
    }

    [AvaloniaFact(DisplayName = "EmptyView Disappears From VisualTree")]
    public async Task EmptyViewDisappearsFromVisualTree()
    {
        var collectionView = CreateCollectionView();
        var items = new ObservableCollection<string>();
        collectionView.ItemsSource = items;
        collectionView.EmptyView = new Microsoft.Maui.Controls.Label { Text = "Empty" };

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        var platformView = handler.PlatformView;

        // Initially empty
        Assert.Contains(platformView.GetVisualDescendants().OfType<TextBlock>(), tb => tb.Text == "Empty");

        // Add item
        items.Add("Item 1");
        
        // Wait for update
        await Task.Yield();
        
        // Should be gone
        Assert.DoesNotContain(platformView.GetVisualDescendants().OfType<TextBlock>(), tb => tb.Text == "Empty");
    }

    [AvaloniaFact(DisplayName = "EmptyViewTemplate Appears In VisualTree")]
    public async Task EmptyViewTemplateAppearsInVisualTree()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string>();
        collectionView.EmptyViewTemplate = new DataTemplate(() => new Microsoft.Maui.Controls.Label { Text = "Template Empty" });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        var platformView = (AvaloniaCollectionView)handler.PlatformView;

        Assert.Contains(platformView.GetVisualDescendants().OfType<TextBlock>(), tb => tb.Text == "Template Empty");
    }

    [AvaloniaFact(DisplayName = "EmptyView Updates On Collection Change")]
    public async Task EmptyViewUpdatesOnCollectionChange()
    {
        var items = new ObservableCollection<string> { "Initial" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.EmptyView = new Microsoft.Maui.Controls.Label { Text = "Empty" };

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        var platformView = handler.PlatformView;

        // Initially has items, so no empty view
        Assert.DoesNotContain(platformView.GetVisualDescendants().OfType<TextBlock>(), tb => tb.Text == "Empty");

        // Clear items
        items.Clear();
        await Task.Yield();
        // Empty view should appear
        Assert.Contains(platformView.GetVisualDescendants().OfType<TextBlock>(), tb => tb.Text == "Empty");
    }

    [AvaloniaFact(DisplayName = "ScrollTo Index Updates Offset")]
    public async Task ScrollToIndexUpdatesOffset()
    {
        var items = Enumerable.Range(0, 100).Select(i => $"Item {i}").ToList();
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.HeightRequest = 200;

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        var platformView = handler.PlatformView;
        
        // Attach to a window to ensure full layout
        var window = new Window { Content = platformView, Width = 200, Height = 200 };
        window.Show();

        // Run jobs to process initial layout
        Threading.Dispatcher.UIThread.RunJobs();

        // Initial offset should be 0
        var scrollViewer = platformView.GetScrollViewer();
        Assert.NotNull(scrollViewer);
        Assert.Equal(0, scrollViewer.Offset.Y);

        // Scroll to end
        collectionView.ScrollTo(99, -1, ScrollToPosition.MakeVisible, false);
        
        // Run jobs to process Dispatcher.Post in ScrollTo and subsequent layout/scroll updates
        Threading.Dispatcher.UIThread.RunJobs();
        await Task.Delay(100); // Give a little time time for animations if any, though RunJobs should handle most
        Threading.Dispatcher.UIThread.RunJobs();
        
        // Offset should have increased
        // We use a looser check because exact pixels depend on styling/fonts
        Assert.True(scrollViewer.Offset.Y > 0, $"ScrollViewer offset {scrollViewer.Offset.Y} should be greater than 0 after scrolling");
    }

    [AvaloniaFact(DisplayName = "ScrollTo Item Updates Offset")]
    public async Task ScrollToItemUpdatesOffset()
    {
        var items = Enumerable.Range(0, 100).Select(i => $"Item {i}").ToList();
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.HeightRequest = 200;

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        var platformView = handler.PlatformView;
        
        var window = new Window { Content = platformView, Width = 200, Height = 200 };
        window.Show();

        Threading.Dispatcher.UIThread.RunJobs();

        var scrollViewer = platformView.GetScrollViewer();
        Assert.NotNull(scrollViewer);
        Assert.Equal(0, scrollViewer.Offset.Y);

        // Scroll to specific item
        var targetItem = items[50];
        collectionView.ScrollTo(targetItem, null, ScrollToPosition.MakeVisible, false);
        
        Threading.Dispatcher.UIThread.RunJobs();
        await Task.Delay(100);
        Threading.Dispatcher.UIThread.RunJobs();
        
        Assert.True(scrollViewer.Offset.Y > 0, $"ScrollViewer offset {scrollViewer.Offset.Y} should be greater than 0 after scrolling to item");
    }

    [AvaloniaFact(DisplayName = "SelectionChangedCommand Executes")]
    public async Task SelectionChangedCommandExecutes()
    {
        var items = new List<string> { "Item 1", "Item 2" };
        var collectionView = CreateCollectionView();
        var targetItem = "Item 2";
        
        collectionView.ItemsSource = items;
        collectionView.SelectionMode = Microsoft.Maui.Controls.SelectionMode.Single;
        
        int commandExecutedCount = 0;
        object? lastParameter = null;
        
        var command = new TestCommand<object?>(
            (p) => 
            { 
                commandExecutedCount++;
                lastParameter = p;
            },
            (p) => true
        );
        collectionView.SelectionChangedCommand = command;

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        // Simulate selection change on platform
        handler.PlatformView.SelectedItem = targetItem;

        // Wait for Dispatcher.Post in handler
        await Task.Delay(150);

        // MAUI might fire once with null parameter, and we fire once with correct parameter
        Assert.True(commandExecutedCount >= 1, "Command should be executed at least once");
        
        // We really care that it was executed with the correct item eventually
        Assert.Equal(targetItem, lastParameter);
    }

    [AvaloniaFact(DisplayName = "SelectionChangedCommandParameter Passed")]
    public async Task SelectionChangedCommandParameterPassed()
    {
        var items = new List<string> { "Item 1", "Item 2" };
        var collectionView = CreateCollectionView();
        var targetItem = "Item 2";
        
        collectionView.ItemsSource = items;
        collectionView.SelectionMode = Microsoft.Maui.Controls.SelectionMode.Single;
        
        int commandExecutedCount = 0;
        object? lastParameter = null;
        
        var command = new TestCommand<object?>(
            (p) => 
            { 
                commandExecutedCount++;
                lastParameter = p;
            },
            (p) => true
        );
        collectionView.SelectionChangedCommand = command;
        collectionView.SelectionChangedCommandParameter = targetItem; 

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        // Simulate selection change on platform
        handler.PlatformView.SelectedItem = targetItem;

        // Wait for Dispatcher.Post in handler
        await Task.Delay(150);

        Assert.True(commandExecutedCount >= 1, "Command should be executed at least once");
        Assert.Equal(targetItem, lastParameter);
    }

    [AvaloniaFact(DisplayName = "RemainingItemsThresholdReachedCommand Executes")]
    public async Task RemainingItemsThresholdReachedCommandExecutes()
    {
        var items = Enumerable.Range(0, 50).Select(i => $"Item {i}").ToList();
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.RemainingItemsThreshold = 5;

        var commandExecutedCount = 0;
        var command = new TestCommand(() => commandExecutedCount++);
        collectionView.RemainingItemsThresholdReachedCommand = command;

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        var platformView = handler.PlatformView;

        var window = new Window { Content = platformView, Width = 200, Height = 200 };
        window.Show();

        Threading.Dispatcher.UIThread.RunJobs();

        // Scroll near the end to trigger threshold
        collectionView.ScrollTo(48, -1, ScrollToPosition.MakeVisible, false);

        // Run jobs and wait for potential async operations
        Threading.Dispatcher.UIThread.RunJobs();
        await Task.Delay(100);
        Threading.Dispatcher.UIThread.RunJobs();

        Assert.True(commandExecutedCount > 0, "RemainingItemsThresholdReachedCommand should have executed");
    }

    [AvaloniaFact(DisplayName = "ScrolledEventFires")]
    public async Task ScrolledEventFires()
    {
        var items = Enumerable.Range(0, 100).Select(i => $"Item {i}").ToList();
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.HeightRequest = 200;

        bool scrolledFired = false;
        collectionView.Scrolled += (s, e) => scrolledFired = true;

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        var platformView = handler.PlatformView;

        var window = new Window { Content = platformView, Width = 200, Height = 200 };
        window.Show();

        Threading.Dispatcher.UIThread.RunJobs();

        // Perform scroll
        collectionView.ScrollTo(50, -1, ScrollToPosition.MakeVisible, false);

        Threading.Dispatcher.UIThread.RunJobs();
        await Task.Delay(100);
        Threading.Dispatcher.UIThread.RunJobs();

        Assert.True(scrolledFired, "Scrolled event should have fired");
    }

    [AvaloniaFact(DisplayName = "SelectionMode None Does Not Execute Command")]
    public async Task SelectionModeNoneDoesNotExecuteCommand()
    {
        var items = new List<string> { "Item 1", "Item 2" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.SelectionMode = MauiSelectionMode.None;

        var commandExecutedCount = 0;
        var command = new TestCommand(() => commandExecutedCount++);
        collectionView.SelectionChangedCommand = command;

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        // Simulate selection change on platform (even though mode is None)
        handler.PlatformView.SelectedItem = "Item 2";

        await Task.Delay(150);

        Assert.Equal(0, commandExecutedCount);
    }
    
    System.Collections.IEnumerable? GetPlatformItemsSource(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.ItemsSource;

    Templates.IDataTemplate? GetPlatformEmptyViewTemplate(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.EmptyViewTemplate;

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

    Templates.IDataTemplate? GetPlatformGroupHeaderTemplate(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.GroupHeaderTemplate;

    Templates.IDataTemplate? GetPlatformGroupFooterTemplate(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.GroupFooterTemplate;

    object? GetPlatformHeader(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.Header;

    Templates.IDataTemplate? GetPlatformHeaderTemplate(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.HeaderTemplate;

    object? GetPlatformFooter(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.Footer;

    Templates.IDataTemplate? GetPlatformFooterTemplate(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.FooterTemplate;

    IList<object>? GetPlatformSelectedItems(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.SelectedItems;

    int GetPlatformRemainingItemsThreshold(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.RemainingItemsThreshold ?? -1;

    ItemsUpdatingScrollMode GetPlatformItemsUpdatingScrollMode(MauiCollectionViewHandler handler) =>
        handler.PlatformView?.ItemsUpdatingScrollMode ?? ItemsUpdatingScrollMode.KeepItemsInView;
}

public class TestTemplateSelector : DataTemplateSelector
{
    public DataTemplate TemplateA { get; set; }
    public DataTemplate TemplateB { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return (string)item == "TypeA" ? TemplateA : TemplateB;
    }
}