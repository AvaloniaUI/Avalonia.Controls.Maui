using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using System.Collections;
using System.Collections.ObjectModel;
using MauiSelectionMode = Microsoft.Maui.Controls.SelectionMode;

namespace Avalonia.Controls.Maui.Tests.Controls;

public class MauiCollectionViewTests
{
    private MauiCollectionView CreateCollectionView()
    {
        return new MauiCollectionView();
    }

    [AvaloniaFact(DisplayName = "InitializeDefaultTemplate Creates Core Components")]
    public void InitializeDefaultTemplateCreatesCoreComponents()
    {
        var collectionView = CreateCollectionView();
        
        Assert.NotNull(collectionView.GetItemsControl());
        Assert.NotNull(collectionView.GetScrollViewer());
    }

    [AvaloniaFact(DisplayName = "ItemsSource Maps To Internal ItemsControl")]
    public void ItemsSourceMapsToInternalItemsControl()
    {
        var collectionView = CreateCollectionView();
        var items = new List<string> { "A", "B" };
        
        collectionView.ItemsSource = items;
        
        var internalItemsControl = collectionView.GetItemsControl();
        Assert.Equal(items, internalItemsControl?.ItemsSource);
    }

    [AvaloniaFact(DisplayName = "Null ItemsSource Clears Internal ItemsControl")]
    public void NullItemsSourceClearsInternalItemsControl()
    {
        var collectionView = CreateCollectionView();
        collectionView.ItemsSource = new List<string> { "A", "B" };

        collectionView.ItemsSource = null;

        Assert.Null(collectionView.GetItemsControl()?.ItemsSource);
    }

    [AvaloniaFact(DisplayName = "EmptyView String Displayed When ItemsSource Empty")]
    public void EmptyViewStringDisplayedWhenItemsSourceEmpty()
    {
        var collectionView = CreateCollectionView();
        collectionView.EmptyView = "Empty Message";
        collectionView.ItemsSource = new List<string>(); // Empty

        var textBlock = collectionView.GetVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault(t => t.Text == "Empty Message");

        Assert.NotNull(textBlock);
    }

    [AvaloniaFact(DisplayName = "EmptyView Template Displayed When ItemsSource Empty")]
    public void EmptyViewTemplateDisplayedWhenItemsSourceEmpty()
    {
        var collectionView = CreateCollectionView();
        collectionView.EmptyViewTemplate = new FuncDataTemplate<object?>((_, _) => new Label { Content = "Template Empty" });
        collectionView.ItemsSource = new List<string>();

        var label = collectionView.GetVisualDescendants()
            .OfType<Label>()
            .FirstOrDefault(l => l.Content?.ToString() == "Template Empty");

        Assert.NotNull(label);
    }

    [AvaloniaFact(DisplayName = "EmptyView Hidden When ItemsSource Has Items")]
    public void EmptyViewHiddenWhenItemsSourceHasItems()
    {
        var collectionView = CreateCollectionView();
        collectionView.EmptyView = "Empty Message";
        collectionView.ItemsSource = new List<string> { "Item 1" };

        var textBlock = collectionView.GetVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault(t => t.Text == "Empty Message");

        Assert.Null(textBlock);
        Assert.True(collectionView.GetScrollViewer()?.IsVisible);
    }

    [AvaloniaFact(DisplayName = "EmptyView Reused When Empty State Unchanged")]
    public void EmptyViewReusedWhenEmptyStateUnchanged()
    {
        var collectionView = CreateCollectionView();
        collectionView.EmptyView = "Empty Message";
        collectionView.ItemsSource = new List<string>();

        var firstTextBlock = collectionView.GetVisualDescendants()
            .OfType<TextBlock>()
            .Single(t => t.Text == "Empty Message");

        collectionView.ItemsSource = new List<string>();

        var secondTextBlock = collectionView.GetVisualDescendants()
            .OfType<TextBlock>()
            .Single(t => t.Text == "Empty Message");

        Assert.Same(firstTextBlock, secondTextBlock);
    }

    [AvaloniaFact(DisplayName = "Header String Displayed")]
    public void HeaderStringDisplayed()
    {
        var collectionView = CreateCollectionView();
        collectionView.Header = "My Header";

        // Access via public GetScrollViewer()
        var mainContainer = collectionView.GetScrollViewer()?.Content as StackPanel;

        Assert.NotNull(mainContainer);
        var headerView = mainContainer.Children.FirstOrDefault(c => c is TextBlock t && t.Text == "My Header");
        Assert.NotNull(headerView);
    }

    [AvaloniaFact(DisplayName = "Footer String Displayed")]
    public void FooterStringDisplayed()
    {
        var collectionView = CreateCollectionView();
        collectionView.Footer = "My Footer";

        // Access via public GetScrollViewer()
        var mainContainer = collectionView.GetScrollViewer()?.Content as StackPanel;

        Assert.NotNull(mainContainer);
        var footerView = mainContainer.Children.FirstOrDefault(c => c is TextBlock t && t.Text == "My Footer");
        Assert.NotNull(footerView);
    }

    [AvaloniaFact(DisplayName = "Single Selection Mode Updates SelectedItem Property")]
    public void SingleSelectionModeUpdatesSelectedItemProperty()
    {
        var collectionView = CreateCollectionView();
        var item = "Item 1";
        
        // Testing property setting logic directly since private interaction requires reflection
        collectionView.SelectionMode = MauiSelectionMode.Single;
        collectionView.SelectedItem = item;

        Assert.Equal(item, collectionView.SelectedItem);
    }

    [AvaloniaFact(DisplayName = "Multiple Selection Mode Updates SelectedItems Property")]
    public void MultipleSelectionModeUpdatesSelectedItemsProperty()
    {
        var collectionView = CreateCollectionView();
        var selectedItems = new ObservableCollection<object> { "Item 1", "Item 2" };
        
        collectionView.SelectionMode = MauiSelectionMode.Multiple;
        collectionView.SelectedItems = selectedItems;

        Assert.Equal(selectedItems, collectionView.SelectedItems);
        Assert.Contains("Item 1", collectionView.SelectedItems);
        Assert.Contains("Item 2", collectionView.SelectedItems);
    }

    [AvaloniaFact(DisplayName = "SelectionMode None Does Not Create Selection Containers")]
    public void SelectionModeNoneDoesNotCreateSelectionContainers()
    {
        var collectionView = CreateCollectionView();
        collectionView.SelectionMode = MauiSelectionMode.None;
        collectionView.ItemTemplate = new FuncDataTemplate<object?>((item, _) => new TextBlock { Text = item?.ToString() });
        collectionView.ItemsSource = new[] { "A", "B" };

        var window = new Window { Content = collectionView, Width = 300, Height = 200 };
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, CountSelectionContainers(collectionView));
    }

    [AvaloniaFact(DisplayName = "SelectionMode Single Creates Selection Containers")]
    public void SelectionModeSingleCreatesSelectionContainers()
    {
        var collectionView = CreateCollectionView();
        collectionView.SelectionMode = MauiSelectionMode.Single;
        collectionView.ItemTemplate = new FuncDataTemplate<object?>((item, _) => new TextBlock { Text = item?.ToString() });
        collectionView.ItemsSource = new[] { "A", "B" };

        var window = new Window { Content = collectionView, Width = 300, Height = 200 };
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(2, CountSelectionContainers(collectionView));
    }

    [AvaloniaFact(DisplayName = "SelectionMode Change Rebuilds Selection Containers")]
    public void SelectionModeChangeRebuildsSelectionContainers()
    {
        var collectionView = CreateCollectionView();
        collectionView.SelectionMode = MauiSelectionMode.Single;
        collectionView.ItemTemplate = new FuncDataTemplate<object?>((item, _) => new TextBlock { Text = item?.ToString() });
        collectionView.ItemsSource = new[] { "A", "B" };

        var window = new Window { Content = collectionView, Width = 300, Height = 200 };
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(2, CountSelectionContainers(collectionView));

        collectionView.SelectionMode = MauiSelectionMode.None;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, CountSelectionContainers(collectionView));
    }

    [AvaloniaFact(DisplayName = "Grouped Headers And Footers Are Not Wrapped For Selection")]
    public void GroupedHeadersAndFootersAreNotWrappedForSelection()
    {
        var groups = new ObservableCollection<ObservableCollection<string>>
        {
            new() { "A", "B" },
            new() { "C" }
        };

        var collectionView = CreateCollectionView();
        collectionView.SelectionMode = MauiSelectionMode.Single;
        collectionView.IsGrouped = true;
        collectionView.ItemTemplate = new FuncDataTemplate<object?>((item, _) => new TextBlock { Text = item?.ToString() });
        collectionView.GroupHeaderTemplate = new FuncDataTemplate<object?>((_, _) => new TextBlock { Text = "Header" });
        collectionView.GroupFooterTemplate = new FuncDataTemplate<object?>((_, _) => new TextBlock { Text = "Footer" });
        collectionView.ItemsSource = groups;

        var window = new Window { Content = collectionView, Width = 300, Height = 300 };
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(3, CountSelectionContainers(collectionView));
    }

    [AvaloniaFact(DisplayName = "Grouped Inner Collection Changes Update Items")]
    public void GroupedInnerCollectionChangesUpdateItems()
    {
        var firstGroup = new ObservableCollection<string> { "A" };
        var groups = new ObservableCollection<ObservableCollection<string>> { firstGroup };
        var collectionView = CreateCollectionView();
        collectionView.IsGrouped = true;
        collectionView.ItemsSource = groups;

        Assert.Equal(1, collectionView.GetItemsControl()?.ItemCount);

        firstGroup.Add("B");

        Assert.Equal(2, collectionView.GetItemsControl()?.ItemCount);
    }

    [AvaloniaFact(DisplayName = "Grouped Inner Collection Add Keeps Flattened ItemsSource Instance")]
    public void GroupedInnerCollectionAddKeepsFlattenedItemsSourceInstance()
    {
        var firstGroup = new ObservableCollection<string> { "A" };
        var groups = new ObservableCollection<ObservableCollection<string>> { firstGroup };
        var collectionView = CreateCollectionView();
        collectionView.IsGrouped = true;
        collectionView.GroupHeaderTemplate = new FuncDataTemplate<object?>((_, _) => new Label());
        collectionView.ItemsSource = groups;

        var flattenedItems = Assert.IsAssignableFrom<ObservableCollection<object>>(collectionView.GetItemsControl()?.ItemsSource);

        firstGroup.Add("B");

        var updatedFlattenedItems = Assert.IsAssignableFrom<ObservableCollection<object>>(collectionView.GetItemsControl()?.ItemsSource);
        Assert.Same(flattenedItems, updatedFlattenedItems);
        Assert.Equal(3, updatedFlattenedItems.Count);

        var addedItem = Assert.IsType<GroupItem>(updatedFlattenedItems[2]);
        Assert.Equal("B", addedItem.Data);
    }

    [AvaloniaFact(DisplayName = "Grouped Inner Collection Replace Updates Wrapper Without Replacing ItemsSource")]
    public void GroupedInnerCollectionReplaceUpdatesWrapperWithoutReplacingItemsSource()
    {
        var firstGroup = new ObservableCollection<string> { "A", "B" };
        var groups = new ObservableCollection<ObservableCollection<string>> { firstGroup };
        var collectionView = CreateCollectionView();
        collectionView.IsGrouped = true;
        collectionView.GroupHeaderTemplate = new FuncDataTemplate<object?>((_, _) => new Label());
        collectionView.ItemsSource = groups;

        var flattenedItems = Assert.IsAssignableFrom<ObservableCollection<object>>(collectionView.GetItemsControl()?.ItemsSource);

        firstGroup[1] = "C";

        var updatedFlattenedItems = Assert.IsAssignableFrom<ObservableCollection<object>>(collectionView.GetItemsControl()?.ItemsSource);
        Assert.Same(flattenedItems, updatedFlattenedItems);
        Assert.Equal(3, updatedFlattenedItems.Count);

        var replacedItem = Assert.IsType<GroupItem>(updatedFlattenedItems[2]);
        Assert.Equal("C", replacedItem.Data);
    }

    [AvaloniaFact(DisplayName = "Grouped Outer Collection Add Keeps Flattened ItemsSource Instance")]
    public void GroupedOuterCollectionAddKeepsFlattenedItemsSourceInstance()
    {
        var groups = new ObservableCollection<ObservableCollection<string>>
        {
            new() { "A" }
        };
        var collectionView = CreateCollectionView();
        collectionView.IsGrouped = true;
        collectionView.GroupHeaderTemplate = new FuncDataTemplate<object?>((_, _) => new Label());
        collectionView.ItemsSource = groups;

        var flattenedItems = Assert.IsAssignableFrom<ObservableCollection<object>>(collectionView.GetItemsControl()?.ItemsSource);

        groups.Add(new ObservableCollection<string> { "B", "C" });

        var updatedFlattenedItems = Assert.IsAssignableFrom<ObservableCollection<object>>(collectionView.GetItemsControl()?.ItemsSource);
        Assert.Same(flattenedItems, updatedFlattenedItems);
        Assert.Equal(5, updatedFlattenedItems.Count);
    }

    [AvaloniaFact(DisplayName = "Grouped Outer Collection Remove Keeps Flattened ItemsSource Instance")]
    public void GroupedOuterCollectionRemoveKeepsFlattenedItemsSourceInstance()
    {
        var groups = new ObservableCollection<ObservableCollection<string>>
        {
            new() { "A" },
            new() { "B", "C" }
        };
        var collectionView = CreateCollectionView();
        collectionView.IsGrouped = true;
        collectionView.GroupHeaderTemplate = new FuncDataTemplate<object?>((_, _) => new Label());
        collectionView.ItemsSource = groups;

        var flattenedItems = Assert.IsAssignableFrom<ObservableCollection<object>>(collectionView.GetItemsControl()?.ItemsSource);

        groups.RemoveAt(0);

        var updatedFlattenedItems = Assert.IsAssignableFrom<ObservableCollection<object>>(collectionView.GetItemsControl()?.ItemsSource);
        Assert.Same(flattenedItems, updatedFlattenedItems);
        Assert.Equal(3, updatedFlattenedItems.Count);
    }

    private static int CountSelectionContainers(MauiCollectionView collectionView)
    {
        return collectionView.GetVisualDescendants().Count(control => control.GetType().Name == "SelectionContainer");
    }
}
