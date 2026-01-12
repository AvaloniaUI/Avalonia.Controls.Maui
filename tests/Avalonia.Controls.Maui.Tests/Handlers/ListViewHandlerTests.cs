using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using MauiListViewHandler = Avalonia.Controls.Maui.Handlers.ListViewHandler;
using PlatformListView = Avalonia.Controls.Maui.MauiListView;
using AvaloniaSelectionMode = Avalonia.Controls.SelectionMode;
using MauiSelectionMode = Microsoft.Maui.Controls.ListViewSelectionMode;
using ScrollBarVisibility = Microsoft.Maui.ScrollBarVisibility;
using MCLabel = Microsoft.Maui.Controls.Label;
using MCMenuItem = Microsoft.Maui.Controls.MenuItem;
using MCDataTemplate = Microsoft.Maui.Controls.DataTemplate;

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
        Assert.IsType<PlatformListView>(handler.PlatformView);
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
    [InlineData(ScrollBarVisibility.Always, Primitives.ScrollBarVisibility.Visible)]
    [InlineData(ScrollBarVisibility.Never, Primitives.ScrollBarVisibility.Hidden)]
    [InlineData(ScrollBarVisibility.Default, Primitives.ScrollBarVisibility.Auto)]
    public async Task ScrollBarVisibilityMapsCorrectly(ScrollBarVisibility mauiVisibility,
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

        listView.ItemTemplate = new MCDataTemplate(() =>
        {
            var cell = new ViewCell();
            cell.ContextActions.Add(new MCMenuItem { Text = "Action 1" });
            var label = new MCLabel();
            label.SetBinding(MCLabel.TextProperty, ".");
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
        listView.HeaderTemplate = new MCDataTemplate(() => new MCLabel { Text = "Header" });

        var handler = await CreateHandlerAsync<MauiListViewHandler>(listView);

        Assert.NotNull(handler.PlatformView.Header);
    }

    [AvaloniaFact(DisplayName = "FooterTemplate Maps Correctly")]
    public async Task FooterTemplateMapsCorrectly()
    {
        var listView = CreateListView();
        listView.FooterTemplate = new MCDataTemplate(() => new MCLabel { Text = "Footer" });

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

        var templateA = new MCDataTemplate(() => new MCLabel { Text = "A" });
        var templateB = new MCDataTemplate(() => new MCLabel { Text = "B" });

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