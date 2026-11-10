using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using ControlsBorder = Microsoft.Maui.Controls.Border;
using MauiCollectionViewHandler = Avalonia.Controls.Maui.Handlers.CollectionViewHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

/// <summary>
/// Tests that CollectionView template items are properly parented in the MAUI logical tree,
/// enabling RelativeSource AncestorType bindings (e.g. for TapGestureRecognizer commands).
/// </summary>
public class CollectionViewLogicalParentTests : HandlerTestBase
{
    private static CollectionView CreateCollectionView()
    {
        var collectionView = new CollectionView();
        collectionView.WidthRequest = 200;
        collectionView.HeightRequest = 300;
        return collectionView;
    }

    [AvaloniaFact(DisplayName = "Template Items Are Logical Children Of CollectionView")]
    public async Task TemplateItemsAreLogicalChildrenOfCollectionView()
    {
        var items = new List<string> { "Item 1", "Item 2" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label();
            label.SetBinding(Microsoft.Maui.Controls.Label.TextProperty, ".");
            return label;
        });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);

        // Trigger template materialization
        handler.PlatformView.UpdateItemTemplate(collectionView, handler);

        var platformTemplate = handler.PlatformView.ItemTemplate;
        Assert.NotNull(platformTemplate);

        // Build an item via the platform template to trigger AddLogicalChild
        var built = platformTemplate.Build("Item 1");
        Assert.NotNull(built);

        // The MAUI view stored in Tag should have the CollectionView as its parent
        var mauiElement = built.Tag as Microsoft.Maui.Controls.Element;
        Assert.NotNull(mauiElement);
        Assert.NotNull(mauiElement.Parent);
        Assert.Same(collectionView, mauiElement.Parent);
    }

    [AvaloniaFact(DisplayName = "Template Item Keeps Its Parent When A Shadow Wraps It")]
    public async Task TemplateItemKeepsItsParentWhenAShadowWrapsIt()
    {
        var itemViews = new List<ControlsBorder>();
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string> { "One", "Two" };
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var border = new ControlsBorder { HeightRequest = 30 };
            itemViews.Add(border);
            return border;
        });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        var window = new Window { Content = handler.PlatformView, Width = 300, Height = 400 };
        window.Show();
        Threading.Dispatcher.UIThread.RunJobs();

        var item = itemViews.First(view => view.Handler?.PlatformView is Control control && TopLevel.GetTopLevel(control) != null);
        item.Shadow = new Shadow { Brush = Brush.Black, Radius = 2, Opacity = 0.5f };
        Threading.Dispatcher.UIThread.RunJobs();

        Assert.Same(collectionView, item.Parent);
    }

    [AvaloniaFact(DisplayName = "Removed Template Item Leaves The Logical Tree")]
    public async Task RemovedTemplateItemLeavesTheLogicalTree()
    {
        var itemViews = new Dictionary<string, ControlsBorder>();
        var items = new ObservableCollection<string> { "One", "Two" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var border = new ControlsBorder { HeightRequest = 30 };
            border.BindingContextChanged += (_, _) =>
            {
                if (border.BindingContext is string text)
                    itemViews[text] = border;
            };
            return border;
        });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        var window = new Window { Content = handler.PlatformView, Width = 300, Height = 400 };
        window.Show();
        Threading.Dispatcher.UIThread.RunJobs();

        var removed = itemViews["One"];
        Assert.Same(collectionView, removed.Parent);

        items.Remove("One");
        Threading.Dispatcher.UIThread.RunJobs();

        Assert.Null(removed.Parent);
    }

    [AvaloniaFact(DisplayName = "Multiple Template Items All Have CollectionView As Parent")]
    public async Task MultipleTemplateItemsAllHaveCollectionViewAsParent()
    {
        var items = new List<string> { "A", "B", "C" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label();
            label.SetBinding(Microsoft.Maui.Controls.Label.TextProperty, ".");
            return label;
        });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        handler.PlatformView.UpdateItemTemplate(collectionView, handler);

        var platformTemplate = handler.PlatformView.ItemTemplate;
        Assert.NotNull(platformTemplate);

        foreach (var item in items)
        {
            var built = platformTemplate.Build(item);
            Assert.NotNull(built);

            var mauiElement = built.Tag as Microsoft.Maui.Controls.Element;
            Assert.NotNull(mauiElement);
            Assert.Same(collectionView, mauiElement.Parent);
        }
    }

    [AvaloniaFact(DisplayName = "Template Item BindingContext Is Data Item Not Parent")]
    public async Task TemplateItemBindingContextIsDataItemNotParent()
    {
        var items = new List<string> { "TestItem" };
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = items;
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label();
            label.SetBinding(Microsoft.Maui.Controls.Label.TextProperty, ".");
            return label;
        });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        handler.PlatformView.UpdateItemTemplate(collectionView, handler);

        var platformTemplate = handler.PlatformView.ItemTemplate;
        Assert.NotNull(platformTemplate);

        var built = platformTemplate.Build("TestItem");
        Assert.NotNull(built);

        // The MAUI view's BindingContext should be the data item, NOT inherited from parent
        var mauiView = built.Tag as BindableObject;
        Assert.NotNull(mauiView);
        Assert.Equal("TestItem", mauiView.GetValue(BindableObject.BindingContextProperty));
    }

    [AvaloniaFact(DisplayName = "Group Header Template Items Are Logical Children")]
    public async Task GroupHeaderTemplateItemsAreLogicalChildren()
    {
        var groups = new ObservableCollection<List<string>>
        {
            new List<string> { "A", "B" },
        };

        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = groups;
        collectionView.IsGrouped = true;
        collectionView.GroupHeaderTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Header" };
            return label;
        });
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label();
            label.SetBinding(Microsoft.Maui.Controls.Label.TextProperty, ".");
            return label;
        });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        handler.PlatformView.UpdateGroupHeaderTemplate(collectionView, handler);

        var groupHeaderTemplate = handler.PlatformView.GroupHeaderTemplate;
        Assert.NotNull(groupHeaderTemplate);

        var built = groupHeaderTemplate.Build(groups[0]);
        Assert.NotNull(built);

        var mauiElement = built.Tag as Microsoft.Maui.Controls.Element;
        Assert.NotNull(mauiElement);
        Assert.NotNull(mauiElement.Parent);
        Assert.Same(collectionView, mauiElement.Parent);
    }

    [AvaloniaFact(DisplayName = "Header Template Items Are Logical Children")]
    public async Task HeaderTemplateItemsAreLogicalChildren()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string> { "Item" };

        // The StructuredItemsView.HeaderTemplate requires going through the handler
        var structuredView = collectionView as StructuredItemsView;
        Assert.NotNull(structuredView);

        structuredView.HeaderTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Header" };
            return label;
        });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        handler.PlatformView.UpdateHeaderTemplate(collectionView, handler);

        var headerTemplate = handler.PlatformView.HeaderTemplate;
        Assert.NotNull(headerTemplate);

        var built = headerTemplate.Build("HeaderData");
        Assert.NotNull(built);

        var mauiElement = built.Tag as Microsoft.Maui.Controls.Element;
        Assert.NotNull(mauiElement);
        Assert.NotNull(mauiElement.Parent);
        Assert.Same(collectionView, mauiElement.Parent);
    }

    [AvaloniaFact(DisplayName = "Footer Template Items Are Logical Children")]
    public async Task FooterTemplateItemsAreLogicalChildren()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string> { "Item" };

        var structuredView = collectionView as StructuredItemsView;
        Assert.NotNull(structuredView);

        structuredView.FooterTemplate = new DataTemplate(() =>
        {
            var label = new Microsoft.Maui.Controls.Label { Text = "Footer" };
            return label;
        });

        var handler = await CreateHandlerAsync<MauiCollectionViewHandler>(collectionView);
        handler.PlatformView.UpdateFooterTemplate(collectionView, handler);

        var footerTemplate = handler.PlatformView.FooterTemplate;
        Assert.NotNull(footerTemplate);

        var built = footerTemplate.Build("FooterData");
        Assert.NotNull(built);

        var mauiElement = built.Tag as Microsoft.Maui.Controls.Element;
        Assert.NotNull(mauiElement);
        Assert.NotNull(mauiElement.Parent);
        Assert.Same(collectionView, mauiElement.Parent);
    }
}
