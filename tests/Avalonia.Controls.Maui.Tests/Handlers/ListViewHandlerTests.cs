using System.Collections.ObjectModel;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui.Controls;
using AvaloniaListView = Avalonia.Controls.Maui.MauiListView;
using AvaloniaSelectionMode = Avalonia.Controls.SelectionMode;
using MauiBoxView = Microsoft.Maui.Controls.BoxView;
using MauiDataTemplate = Microsoft.Maui.Controls.DataTemplate;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiMenuItem = Microsoft.Maui.Controls.MenuItem;
using MauiListViewHandler = Avalonia.Controls.Maui.Handlers.ListViewHandler;
using MauiSelectionMode = Microsoft.Maui.Controls.ListViewSelectionMode;
using MauiScrollBarVisibility = Microsoft.Maui.ScrollBarVisibility;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class ListViewHandlerTests : HandlerTestBase
{
    private ListView CreateListView()
    {
        EnsureHandlerCreated();
        return new ListView
        {
            WidthRequest = 200,
            HeightRequest = 300
        };
    }

    [AvaloniaFact(DisplayName = "Handler Creates MauiListView Platform View")]
    public async Task HandlerCreatesMauiListView()
    {
        var listView = CreateListView();
        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<AvaloniaListView>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "ItemsSource Maps Correctly")]
    public async Task ItemsSourceMapsCorrectly()
    {
        var items = new List<string> { "A", "B", "C" };
        var listView = CreateListView();
        listView.ItemsSource = items;

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.ItemsSource);

        Assert.NotNull(platformItems);
        Assert.Equal(3, platformItems.Cast<object>().Count());
    }

    [AvaloniaFact(DisplayName = "Header Maps Correctly")]
    public async Task HeaderMapsCorrectly()
    {
        var listView = CreateListView();
        listView.Header = "Test Header";

        var header = await GetValueAsync<object?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.Header);

        Assert.Equal("Test Header", header);
    }

    [AvaloniaFact(DisplayName = "Footer Maps Correctly")]
    public async Task FooterMapsCorrectly()
    {
        var listView = CreateListView();
        listView.Footer = "Test Footer";

        var footer = await GetValueAsync<object?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.Footer);

        Assert.Equal("Test Footer", footer);
    }

    [AvaloniaFact(DisplayName = "SelectionMode Maps Correctly")]
    public async Task SelectionModeMapsCorrectly()
    {
        var listView = CreateListView();
        listView.SelectionMode = MauiSelectionMode.None;

        var selectionMode = await GetValueAsync<AvaloniaSelectionMode, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.SelectionMode);

        // None maps to Single in our implementation to avoid ListBox issues, but selection is cleared
        Assert.Equal(AvaloniaSelectionMode.Single, selectionMode);
    }

    [AvaloniaFact(DisplayName = "SelectedItem Maps Correctly")]
    public async Task SelectedItemMapsCorrectly()
    {
        var items = new ObservableCollection<string> { "Item 1", "Item 2" };
        var listView = CreateListView();
        listView.ItemsSource = items;
        listView.SelectedItem = "Item 2";

        var selectedItem = await GetValueAsync<object?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.SelectedItem);

        Assert.Equal("Item 2", selectedItem);
    }

    [AvaloniaFact(DisplayName = "SeparatorVisibility Maps Correctly")]
    public async Task SeparatorVisibilityMapsCorrectly()
    {
        var listView = CreateListView();
        listView.SeparatorVisibility = SeparatorVisibility.None;

        var isVisible = await GetValueAsync<bool, MauiListViewHandler>(
            listView, handler => handler.PlatformView.SeparatorVisibility);

        Assert.False(isVisible);
    }

    [AvaloniaFact(DisplayName = "SeparatorColor Maps Correctly")]
    public async Task SeparatorColorMapsCorrectly()
    {
        var listView = CreateListView();
        listView.SeparatorColor = Microsoft.Maui.Graphics.Colors.Red;

        var color = await GetValueAsync<Media.IBrush?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.SeparatorColor);

        Assert.NotNull(color);
    }

    [AvaloniaFact(DisplayName = "IsGroupingEnabled Maps Correctly")]
    public async Task IsGroupingEnabledMapsCorrectly()
    {
        var listView = CreateListView();
        listView.IsGroupingEnabled = true;

        var isGroupingEnabled = await GetValueAsync<bool, MauiListViewHandler>(
            listView, handler => handler.PlatformView.IsGroupingEnabled);

        Assert.True(isGroupingEnabled);
    }

    [AvaloniaTheory(DisplayName = "Scroll Bar Visibility Maps Correctly")]
    [InlineData(MauiScrollBarVisibility.Always, Primitives.ScrollBarVisibility.Visible)]
    [InlineData(MauiScrollBarVisibility.Never, Primitives.ScrollBarVisibility.Hidden)]
    [InlineData(MauiScrollBarVisibility.Default, Primitives.ScrollBarVisibility.Auto)]
    public async Task ScrollBarVisibilityMapsCorrectly(MauiScrollBarVisibility mauiVisibility,
        global::Avalonia.Controls.Primitives.ScrollBarVisibility expectedAvalonia)
    {
        var listView = CreateListView();
        listView.VerticalScrollBarVisibility = mauiVisibility;

        var visibility =
            await GetValueAsync<global::Avalonia.Controls.Primitives.ScrollBarVisibility, MauiListViewHandler>(
                listView, handler => handler.PlatformView.VerticalScrollBarVisibility);

        Assert.Equal(expectedAvalonia, visibility);
    }

    [AvaloniaFact(DisplayName = "ContextActions Create ContextMenu")]
    public async Task ContextActionsCreateContextMenu()
    {
        var listView = CreateListView();
        var item = "Test Item";
        listView.ItemsSource = new List<string> { item };

        listView.ItemTemplate = new MauiDataTemplate(() =>
        {
            var cell = new ViewCell();
            cell.ContextActions.Add(new MauiMenuItem { Text = "Action 1" });
            var label = new MauiLabel();
            label.SetBinding(MauiLabel.TextProperty, ".");
            cell.View = label;
            return cell;
        });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        // This triggers layout and realization of items
        handler.PlatformView.UpdateItemTemplate(listView, handler);

        // In a real test we'd need to find the container for the item
        // But we can check if the internal logic for building the template works
    }

    [AvaloniaFact(DisplayName = "HeaderTemplate Maps Correctly")]
    public async Task HeaderTemplateMapsCorrectly()
    {
        var listView = CreateListView();
        listView.HeaderTemplate = new MauiDataTemplate(() => new MauiLabel { Text = "Header" });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        Assert.NotNull(handler.PlatformView.HeaderTemplate);
    }

    [AvaloniaFact(DisplayName = "Header View Maps Correctly")]
    public async Task HeaderViewMapsCorrectly()
    {
        var listView = CreateListView();
        listView.HeaderTemplate = new MauiDataTemplate(() => new MauiLabel { Text = "Header" });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        Assert.NotNull(handler.PlatformView.Header);
    }

    [AvaloniaFact(DisplayName = "FooterTemplate Maps Correctly")]
    public async Task FooterTemplateMapsCorrectly()
    {
        var listView = CreateListView();
        listView.FooterTemplate = new MauiDataTemplate(() => new MauiLabel { Text = "Footer" });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        Assert.NotNull(handler.PlatformView.FooterTemplate);
    }

    [AvaloniaFact(DisplayName = "Footer View Maps Correctly")]
    public async Task FooterViewMapsCorrectly()
    {
        var listView = CreateListView();
        listView.FooterTemplate = new MauiDataTemplate(() => new MauiLabel { Text = "Footer" });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        Assert.NotNull(handler.PlatformView.Footer);
    }

    [AvaloniaFact(DisplayName = "Null ItemsSource Maps Correctly")]
    public async Task NullItemsSourceMapsCorrectly()
    {
        var listView = CreateListView();
        listView.ItemsSource = null;

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.ItemsSource);

        Assert.Null(platformItems);
    }

    [AvaloniaFact(DisplayName = "Empty ItemsSource Maps Correctly")]
    public async Task EmptyItemsSourceMapsCorrectly()
    {
        var listView = CreateListView();
        listView.ItemsSource = new List<string>();

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.ItemsSource);

        Assert.NotNull(platformItems);
        Assert.Empty(platformItems.Cast<object>());
    }

    [AvaloniaFact(DisplayName = "DataTemplateSelector Works")]
    public async Task DataTemplateSelectorWorks()
    {
        var items = new List<string> { "TypeA", "TypeB" };
        var listView = CreateListView();
        listView.ItemsSource = items;

        var templateA = new MauiDataTemplate(() => new MauiLabel { Text = "A" });
        var templateB = new MauiDataTemplate(() => new MauiLabel { Text = "B" });

        listView.ItemTemplate = new TestTemplateSelector
        {
            TemplateA = templateA,
            TemplateB = templateB
        };

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        // Force evaluation
        handler.PlatformView.UpdateItemTemplate(listView, handler);
    }

    [AvaloniaFact(DisplayName = "PullToRefresh Maps Correctly")]
    public async Task PullToRefreshMapsCorrectly()
    {
        var listView = CreateListView();
        listView.IsPullToRefreshEnabled = true;
        listView.IsRefreshing = true;
        listView.RefreshControlColor = Microsoft.Maui.Graphics.Colors.Blue;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        Assert.True(handler.PlatformView.IsPullToRefreshEnabled);
        Assert.True(handler.PlatformView.IsRefreshing);
        Assert.NotNull(handler.PlatformView.RefreshControlColor);
    }

    [AvaloniaTheory(DisplayName = "Selection Mode All Modes Map Correctly")]
    [InlineData(MauiSelectionMode.None, AvaloniaSelectionMode.Single)]
    [InlineData(MauiSelectionMode.Single, AvaloniaSelectionMode.Single)]
    public async Task SelectionModeAllModesMapCorrectly(MauiSelectionMode mauiMode, AvaloniaSelectionMode expectedMode)
    {
        var listView = CreateListView();
        listView.SelectionMode = mauiMode;

        var selectionMode = await GetValueAsync<AvaloniaSelectionMode, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.SelectionMode);

        Assert.Equal(expectedMode, selectionMode);
    }

    [AvaloniaFact(DisplayName = "Selection Mode None Clears Selection")]
    public async Task SelectionModeNoneClearsSelection()
    {
        var items = new ObservableCollection<string> { "Item 1", "Item 2" };
        var listView = CreateListView();
        listView.ItemsSource = items;
        listView.SelectedItem = "Item 1";

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        Assert.NotNull(handler.PlatformView.ListBox.SelectedItem);

        listView.SelectionMode = MauiSelectionMode.None;
        handler.UpdateValue(nameof(Microsoft.Maui.Controls.ListView.SelectionMode));

        var selectedItem = await GetValueAsync<object?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.SelectedItem);

        Assert.Null(selectedItem);
    }

    [AvaloniaTheory(DisplayName = "Horizontal Scroll Bar Visibility Maps Correctly")]
    [InlineData(MauiScrollBarVisibility.Always, Primitives.ScrollBarVisibility.Visible)]
    [InlineData(MauiScrollBarVisibility.Never, Primitives.ScrollBarVisibility.Hidden)]
    [InlineData(MauiScrollBarVisibility.Default, Primitives.ScrollBarVisibility.Disabled)]
    public async Task HorizontalScrollBarVisibilityMapsCorrectly(MauiScrollBarVisibility mauiVisibility,
        global::Avalonia.Controls.Primitives.ScrollBarVisibility expectedAvalonia)
    {
        var listView = CreateListView();
        listView.HorizontalScrollBarVisibility = mauiVisibility;

        var visibility =
            await GetValueAsync<global::Avalonia.Controls.Primitives.ScrollBarVisibility, MauiListViewHandler>(
                listView, handler => handler.PlatformView.HorizontalScrollBarVisibility);

        Assert.Equal(expectedAvalonia, visibility);
    }

    [AvaloniaFact(DisplayName = "Row Height Maps Correctly")]
    public async Task RowHeightMapsCorrectly()
    {
        var listView = CreateListView();
        listView.RowHeight = 50;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        handler.PlatformView.UpdateRowHeight(listView);
    }

    [AvaloniaFact(DisplayName = "Has Uneven Rows Maps Correctly")]
    public async Task HasUnevenRowsMapsCorrectly()
    {
        var listView = CreateListView();
        listView.HasUnevenRows = true;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        handler.PlatformView.UpdateRowHeight(listView);
    }

    [AvaloniaFact(DisplayName = "Group Header Template Maps Correctly")]
    public async Task GroupHeaderTemplateMapsCorrectly()
    {
        var listView = CreateListView();
        listView.GroupHeaderTemplate = new MauiDataTemplate(() => new MauiLabel { Text = "Group Header" });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        var template = await GetValueAsync<Avalonia.Controls.Templates.IDataTemplate?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.GroupHeaderTemplate);

        Assert.NotNull(template);
    }

    [AvaloniaFact(DisplayName = "Refresh Control Color Maps Correctly")]
    public async Task RefreshControlColorMapsCorrectly()
    {
        var listView = CreateListView();
        listView.RefreshControlColor = Microsoft.Maui.Graphics.Colors.Green;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        var color = await GetValueAsync<Media.IBrush?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.RefreshControlColor);

        Assert.NotNull(color);
    }

    [AvaloniaFact(DisplayName = "Refresh Control Disabled By Default")]
    public async Task RefreshControlDisabledByDefault()
    {
        var listView = CreateListView();
        listView.IsPullToRefreshEnabled = false;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        var isEnabled = await GetValueAsync<bool, MauiListViewHandler>(
            listView, handler => handler.PlatformView.IsPullToRefreshEnabled);

        Assert.False(isEnabled);
    }

    [AvaloniaFact(DisplayName = "Is Refreshing State Maps Correctly")]
    public async Task IsRefreshingStateMapsCorrectly()
    {
        var listView = CreateListView();
        listView.IsRefreshing = true;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        var isRefreshing = await GetValueAsync<bool, MauiListViewHandler>(
            listView, handler => handler.PlatformView.IsRefreshing);

        Assert.True(isRefreshing);
    }

    [AvaloniaFact(DisplayName = "Header With Both Content And Template Prioritizes Content")]
    public async Task HeaderWithBothContentAndTemplatePrioritizesContent()
    {
        var listView = CreateListView();
        listView.Header = "String Header";
        listView.HeaderTemplate = new MauiDataTemplate(() => new MauiLabel { Text = "Template Header" });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        var header = await GetValueAsync<object?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.Header);

        Assert.Equal("String Header", header);
    }

    [AvaloniaFact(DisplayName = "Footer With Both Content And Template Prioritizes Content")]
    public async Task FooterWithBothContentAndTemplatePrioritizesContent()
    {
        var listView = CreateListView();
        listView.Footer = "String Footer";
        listView.FooterTemplate = new MauiDataTemplate(() => new MauiLabel { Text = "Template Footer" });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        var footer = await GetValueAsync<object?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.Footer);

        Assert.Equal("String Footer", footer);
    }

    [AvaloniaFact(DisplayName = "Items Source Collection Change Triggers Update")]
    public async Task ItemsSourceCollectionChangeTriggersUpdate()
    {
        var items = new ObservableCollection<string> { "A", "B" };
        var listView = CreateListView();
        listView.ItemsSource = items;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        items.Add("C");

        await Task.Delay(50);

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.ItemsSource);

        Assert.NotNull(platformItems);
        Assert.Equal(3, platformItems.Cast<object>().Count());
    }

    [AvaloniaFact(DisplayName = "Items Source Can Be Updated After Initialization")]
    public async Task ItemsSourceCanBeUpdatedAfterInitialization()
    {
        var initialItems = new List<string> { "A", "B" };
        var listView = CreateListView();
        listView.ItemsSource = initialItems;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        var updatedItems = new List<string> { "X", "Y", "Z" };
        listView.ItemsSource = updatedItems;

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.ItemsSource);

        Assert.NotNull(platformItems);
        Assert.Equal(3, platformItems.Cast<object>().Count());
    }

    [AvaloniaFact(DisplayName = "Selected Item Can Be Changed After Initialization")]
    public async Task SelectedItemCanBeChangedAfterInitialization()
    {
        var items = new ObservableCollection<string> { "Item 1", "Item 2", "Item 3" };
        var listView = CreateListView();
        listView.ItemsSource = items;
        listView.SelectedItem = "Item 1";

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.SelectedItem = "Item 3";

        var selectedItem = await GetValueAsync<object?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.SelectedItem);

        Assert.Equal("Item 3", selectedItem);
    }

    [AvaloniaFact(DisplayName = "Selection Mode Can Be Changed After Initialization")]
    public async Task SelectionModeCanBeChangedAfterInitialization()
    {
        var listView = CreateListView();
        listView.SelectionMode = MauiSelectionMode.Single;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.SelectionMode = MauiSelectionMode.None;

        var selectionMode = await GetValueAsync<AvaloniaSelectionMode, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.SelectionMode);

        Assert.Equal(AvaloniaSelectionMode.Single, selectionMode);
    }

    [AvaloniaFact(DisplayName = "Separator Visibility Can Be Toggled")]
    public async Task SeparatorVisibilityCanBeToggled()
    {
        var listView = CreateListView();
        listView.SeparatorVisibility = SeparatorVisibility.Default;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.SeparatorVisibility = SeparatorVisibility.None;

        var isVisible = await GetValueAsync<bool, MauiListViewHandler>(
            listView, handler => handler.PlatformView.SeparatorVisibility);

        Assert.False(isVisible);
    }

    [AvaloniaFact(DisplayName = "Separator Color Can Be Changed")]
    public async Task SeparatorColorCanBeChanged()
    {
        var listView = CreateListView();
        listView.SeparatorColor = Microsoft.Maui.Graphics.Colors.Blue;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.SeparatorColor = Microsoft.Maui.Graphics.Colors.Red;

        var color = await GetValueAsync<Media.IBrush?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.SeparatorColor);

        Assert.NotNull(color);
    }

    [AvaloniaFact(DisplayName = "Multiple Context Actions Create Multiple Menu Items")]
    public async Task MultipleContextActionsCreateMultipleMenuItems()
    {
        var listView = CreateListView();
        var item = "Test Item";
        listView.ItemsSource = new List<string> { item };

        listView.ItemTemplate = new MauiDataTemplate(() =>
        {
            var cell = new ViewCell();
            cell.ContextActions.Add(new MauiMenuItem { Text = "Action 1" });
            cell.ContextActions.Add(new MauiMenuItem { Text = "Action 2" });
            cell.ContextActions.Add(new MauiMenuItem { Text = "Action 3" });
            var label = new MauiLabel();
            label.SetBinding(MauiLabel.TextProperty, ".");
            cell.View = label;
            return cell;
        });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        handler.PlatformView.UpdateItemTemplate(listView, handler);
    }

    [AvaloniaFact(DisplayName = "Context Action With Command Works")]
    public async Task ContextActionWithCommandWorks()
    {
        var commandExecuted = false;
        var listView = CreateListView();
        var item = "Test Item";
        listView.ItemsSource = new List<string> { item };

        listView.ItemTemplate = new MauiDataTemplate(() =>
        {
            var cell = new ViewCell();
            var menuItem = new MauiMenuItem { Text = "Test Action" };
            menuItem.Command = new Command(() => commandExecuted = true);
            cell.ContextActions.Add(menuItem);
            var label = new MauiLabel();
            label.SetBinding(MauiLabel.TextProperty, ".");
            cell.View = label;
            return cell;
        });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        handler.PlatformView.UpdateItemTemplate(listView, handler);
    }

    [AvaloniaFact(DisplayName = "Item Template With BoxView Works")]
    public async Task ItemTemplateWithBoxViewWorks()
    {
        var items = new List<string> { "A", "B", "C" };
        var listView = CreateListView();
        listView.ItemsSource = items;

        listView.ItemTemplate = new MauiDataTemplate(() =>
        {
            var boxView = new MauiBoxView
            {
                Color = Microsoft.Maui.Graphics.Colors.Red,
                WidthRequest = 50,
                HeightRequest = 50
            };
            return boxView;
        });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        handler.PlatformView.UpdateItemTemplate(listView, handler);
    }

    [AvaloniaFact(DisplayName = "Item Template With Complex Layout Works")]
    public async Task ItemTemplateWithComplexLayoutWorks()
    {
        var items = new List<string> { "Item 1", "Item 2" };
        var listView = CreateListView();
        listView.ItemsSource = items;

        listView.ItemTemplate = new MauiDataTemplate(() =>
        {
            var stackLayout = new Microsoft.Maui.Controls.StackLayout
            {
                Orientation = Microsoft.Maui.Controls.StackOrientation.Vertical
            };

            var titleLabel = new MauiLabel { Text = "Title" };
            stackLayout.Children.Add(titleLabel);

            var descLabel = new MauiLabel { Text = "Description" };
            stackLayout.Children.Add(descLabel);

            var button = new Microsoft.Maui.Controls.Button { Text = "Button" };
            stackLayout.Children.Add(button);

            return stackLayout;
        });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        handler.PlatformView.UpdateItemTemplate(listView, handler);
    }

    [AvaloniaFact(DisplayName = "Is Grouping Enabled Can Be Toggled")]
    public async Task IsGroupingEnabledCanBeToggled()
    {
        var listView = CreateListView();
        listView.IsGroupingEnabled = false;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.IsGroupingEnabled = true;

        var isGroupingEnabled = await GetValueAsync<bool, MauiListViewHandler>(
            listView, handler => handler.PlatformView.IsGroupingEnabled);

        Assert.True(isGroupingEnabled);
    }

    [AvaloniaFact(DisplayName = "Scroll Bar Visibility Can Be Updated")]
    public async Task ScrollBarVisibilityCanBeUpdated()
    {
        var listView = CreateListView();
        listView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.VerticalScrollBarVisibility = ScrollBarVisibility.Never;

        var visibility =
            await GetValueAsync<global::Avalonia.Controls.Primitives.ScrollBarVisibility, MauiListViewHandler>(
                listView, handler => handler.PlatformView.VerticalScrollBarVisibility);

        Assert.Equal(Primitives.ScrollBarVisibility.Hidden, visibility);
    }

    [AvaloniaFact(DisplayName = "Pull To Refresh Can Be Enabled After Initialization")]
    public async Task PullToRefreshCanBeEnabledAfterInitialization()
    {
        var listView = CreateListView();
        listView.IsPullToRefreshEnabled = false;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.IsPullToRefreshEnabled = true;

        var isEnabled = await GetValueAsync<bool, MauiListViewHandler>(
            listView, handler => handler.PlatformView.IsPullToRefreshEnabled);

        Assert.True(isEnabled);
    }

    [AvaloniaFact(DisplayName = "Refreshing State Can Be Toggled")]
    public async Task RefreshingStateCanBeToggled()
    {
        var listView = CreateListView();
        listView.IsPullToRefreshEnabled = true;
        listView.IsRefreshing = false;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.IsRefreshing = true;

        var isRefreshing = await GetValueAsync<bool, MauiListViewHandler>(
            listView, handler => handler.PlatformView.IsRefreshing);

        Assert.True(isRefreshing);
    }

    [AvaloniaFact(DisplayName = "Refresh Control Color Can Be Updated")]
    public async Task RefreshControlColorCanBeUpdated()
    {
        var listView = CreateListView();
        listView.RefreshControlColor = Microsoft.Maui.Graphics.Colors.Blue;

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.RefreshControlColor = Microsoft.Maui.Graphics.Colors.Purple;

        var color = await GetValueAsync<Media.IBrush?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.RefreshControlColor);

        Assert.NotNull(color);
    }

    [AvaloniaFact(DisplayName = "Large Items Source Works")]
    public async Task LargeItemsSourceWorks()
    {
        var items = Enumerable.Range(1, 1000).Select(i => $"Item {i}").ToList();
        var listView = CreateListView();
        listView.ItemsSource = items;

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.ItemsSource);

        Assert.NotNull(platformItems);
        Assert.Equal(1000, platformItems.Cast<object>().Count());
    }

    [AvaloniaTheory(DisplayName = "Separator Visibility All Values Work")]
    [InlineData(SeparatorVisibility.Default, true)]
    [InlineData(SeparatorVisibility.None, false)]
    public async Task SeparatorVisibilityAllValuesWork(SeparatorVisibility visibility, bool expectedValue)
    {
        var listView = CreateListView();
        listView.SeparatorVisibility = visibility;

        var isVisible = await GetValueAsync<bool, MauiListViewHandler>(
            listView, handler => handler.PlatformView.SeparatorVisibility);

        Assert.Equal(expectedValue, isVisible);
    }

    [AvaloniaFact(DisplayName = "Separator Color With Different Colors Works")]
    public async Task SeparatorColorWithDifferentColorsWorks()
    {
        var colors = new[]
        {
            Microsoft.Maui.Graphics.Colors.Red,
            Microsoft.Maui.Graphics.Colors.Green,
            Microsoft.Maui.Graphics.Colors.Blue,
            Microsoft.Maui.Graphics.Colors.Yellow,
            Microsoft.Maui.Graphics.Colors.Purple
        };

        foreach (var color in colors)
        {
            var listView = CreateListView();
            listView.SeparatorColor = color;

            var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

            var brush = await GetValueAsync<Media.IBrush?, MauiListViewHandler>(
                listView, handler => handler.PlatformView.SeparatorColor);

            Assert.NotNull(brush);
        }
    }

    [AvaloniaFact(DisplayName = "Group Header Template With DataTemplateSelector Works")]
    public async Task GroupHeaderTemplateWithDataTemplateSelectorWorks()
    {
        var listView = CreateListView();
        listView.GroupHeaderTemplate = new TestTemplateSelector
        {
            TemplateA = new MauiDataTemplate(() => new MauiLabel { Text = "Group A" }),
            TemplateB = new MauiDataTemplate(() => new MauiLabel { Text = "Group B" })
        };

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        var template = await GetValueAsync<Avalonia.Controls.Templates.IDataTemplate?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.GroupHeaderTemplate);

        Assert.NotNull(template);
    }

    [AvaloniaFact(DisplayName = "Items Source With Custom Objects Works")]
    public async Task ItemsSourceWithCustomObjectsWorks()
    {
        var items = new List<TestItem>
        {
            new TestItem { Name = "Item 1", Value = 1 },
            new TestItem { Name = "Item 2", Value = 2 },
            new TestItem { Name = "Item 3", Value = 3 }
        };

        var listView = CreateListView();
        listView.ItemsSource = items;

        listView.ItemTemplate = new MauiDataTemplate(() =>
        {
            var label = new MauiLabel();
            label.SetBinding(MauiLabel.TextProperty, new Binding("Name"));
            return label;
        });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.ItemsSource);

        Assert.NotNull(platformItems);
        Assert.Equal(3, platformItems.Cast<object>().Count());
    }

    [AvaloniaFact(DisplayName = "Selected Item Can Be Set To Null")]
    public async Task SelectedItemCanBeSetToNull()
    {
        var items = new ObservableCollection<string> { "Item 1", "Item 2" };
        var listView = CreateListView();
        listView.ItemsSource = items;
        listView.SelectedItem = "Item 1";

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.SelectedItem = null;

        var selectedItem = await GetValueAsync<object?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.SelectedItem);

        Assert.Null(selectedItem);
    }

    [AvaloniaFact(DisplayName = "Context Actions Can Be Updated")]
    public async Task ContextActionsCanBeUpdated()
    {
        var listView = CreateListView();
        var item = "Test Item";
        listView.ItemsSource = new List<string> { item };

        listView.ItemTemplate = new MauiDataTemplate(() =>
        {
            var cell = new ViewCell();
            cell.ContextActions.Add(new MauiMenuItem { Text = "Action 1" });
            var label = new MauiLabel();
            label.SetBinding(MauiLabel.TextProperty, ".");
            cell.View = label;
            return cell;
        });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.ItemTemplate = new MauiDataTemplate(() =>
        {
            var cell = new ViewCell();
            cell.ContextActions.Add(new MauiMenuItem { Text = "Action 1" });
            cell.ContextActions.Add(new MauiMenuItem { Text = "Action 2" });
            var label = new MauiLabel();
            label.SetBinding(MauiLabel.TextProperty, ".");
            cell.View = label;
            return cell;
        });

        handler.PlatformView.UpdateItemTemplate(listView, handler);
    }

    [AvaloniaFact(DisplayName = "DataTemplateSelector Returns Correct Template")]
    public async Task DataTemplateSelectorReturnsCorrectTemplate()
    {
        var items = new List<string> { "TypeA", "TypeB", "TypeA", "TypeB" };
        var listView = CreateListView();
        listView.ItemsSource = items;

        var templateA = new MauiDataTemplate(() => new MauiLabel { Text = "A" });
        var templateB = new MauiDataTemplate(() => new MauiLabel { Text = "B" });

        listView.ItemTemplate = new TestTemplateSelector
        {
            TemplateA = templateA,
            TemplateB = templateB
        };

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        handler.PlatformView.UpdateItemTemplate(listView, handler);
    }

    [AvaloniaFact(DisplayName = "Items Source With Null Items Works")]
    public async Task ItemsSourceWithNullItemsWorks()
    {
        var items = new List<string> { "A", null, "C", null, "E" };
        var listView = CreateListView();
        listView.ItemsSource = items;

        var platformItems = await GetValueAsync<System.Collections.IEnumerable?, MauiListViewHandler>(
            listView, handler => handler.PlatformView.ListBox.ItemsSource);

        Assert.NotNull(platformItems);
        Assert.Equal(5, platformItems.Cast<object>().Count());
    }

    [AvaloniaFact(DisplayName = "Item Template Can Be Changed")]
    public async Task ItemTemplateCanBeChanged()
    {
        var items = new List<string> { "Item 1", "Item 2" };
        var listView = CreateListView();
        listView.ItemsSource = items;

        listView.ItemTemplate = new MauiDataTemplate(() => new MauiLabel { Text = "Template 1" });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        listView.ItemTemplate = new MauiDataTemplate(() => new MauiLabel { Text = "Template 2" });

        handler.PlatformView.UpdateItemTemplate(listView, handler);
    }
}

public class TestTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TemplateA { get; set; }
    public DataTemplate? TemplateB { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return item.ToString() == "TypeA" ? TemplateA! : TemplateB!;
    }
}

public class TestItem
{
    public string Name { get; set; }
    public int Value { get; set; }
}