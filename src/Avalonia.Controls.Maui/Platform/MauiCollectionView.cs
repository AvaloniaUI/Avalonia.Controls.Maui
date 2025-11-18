using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Microsoft.Maui.Controls;
using System.Collections;
using System.Collections.Specialized;

namespace Avalonia.Controls.Maui.Platform;

public class MauiCollectionView : TemplatedControl
{
    private ItemsControl? _itemsControl;
    private ScrollViewer? _scrollViewer;
    private Control? _emptyView;
    private Panel? _rootPanel;

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MauiCollectionView, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<object?> EmptyViewProperty =
        AvaloniaProperty.Register<MauiCollectionView, object?>(nameof(EmptyView));

    public static readonly StyledProperty<IDataTemplate?> EmptyViewTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(EmptyViewTemplate));

    public static readonly StyledProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<MauiCollectionView, ScrollBarVisibility>(
            nameof(HorizontalScrollBarVisibility),
            ScrollBarVisibility.Auto);

    public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<MauiCollectionView, ScrollBarVisibility>(
            nameof(VerticalScrollBarVisibility),
            ScrollBarVisibility.Auto);

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<MauiCollectionView, object?>(
            nameof(SelectedItem),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<global::Avalonia.Controls.SelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<MauiCollectionView, global::Avalonia.Controls.SelectionMode>(
            nameof(SelectionMode),
            global::Avalonia.Controls.SelectionMode.Single);

    public static readonly StyledProperty<IItemsLayout?> ItemsLayoutProperty =
        AvaloniaProperty.Register<MauiCollectionView, IItemsLayout?>(
            nameof(ItemsLayout),
            Microsoft.Maui.Controls.LinearItemsLayout.Vertical);

    public static readonly StyledProperty<bool> IsGroupedProperty =
        AvaloniaProperty.Register<MauiCollectionView, bool>(nameof(IsGrouped), false);

    public static readonly StyledProperty<IDataTemplate?> GroupHeaderTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(GroupHeaderTemplate));

    public static readonly StyledProperty<IDataTemplate?> GroupFooterTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(GroupFooterTemplate));

    public event EventHandler? SelectionChanged;

    static MauiCollectionView()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnItemsSourceChanged(e));
        EmptyViewProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.UpdateEmptyView());
        EmptyViewTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.UpdateEmptyView());
        ItemTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnItemTemplateChanged(e));
        HorizontalScrollBarVisibilityProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnScrollBarVisibilityChanged());
        VerticalScrollBarVisibilityProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnScrollBarVisibilityChanged());
        ItemsLayoutProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnItemsLayoutChanged(e));
        IsGroupedProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnGroupingChanged());
        GroupHeaderTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnGroupingChanged());
        GroupFooterTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnGroupingChanged());
        SelectedItemProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnSelectedItemChanged(e));
    }

    public MauiCollectionView()
    {
        // Create default template inline if not set from AXAML
        InitializeDefaultTemplate();
    }

    private void InitializeDefaultTemplate()
    {
        _rootPanel = new Panel();
        _scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = HorizontalScrollBarVisibility,
            VerticalScrollBarVisibility = VerticalScrollBarVisibility
        };
        _itemsControl = new ItemsControl();

        _scrollViewer.Content = _itemsControl;
        _rootPanel.Children.Add(_scrollViewer);

        // For TemplatedControl, add to both visual and logical children
        VisualChildren.Add(_rootPanel);
        LogicalChildren.Add(_rootPanel);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public object? EmptyView
    {
        get => GetValue(EmptyViewProperty);
        set => SetValue(EmptyViewProperty, value);
    }

    public IDataTemplate? EmptyViewTemplate
    {
        get => GetValue(EmptyViewTemplateProperty);
        set => SetValue(EmptyViewTemplateProperty, value);
    }

    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public global::Avalonia.Controls.SelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public IItemsLayout? ItemsLayout
    {
        get => GetValue(ItemsLayoutProperty);
        set => SetValue(ItemsLayoutProperty, value);
    }

    public bool IsGrouped
    {
        get => GetValue(IsGroupedProperty);
        set => SetValue(IsGroupedProperty, value);
    }

    public IDataTemplate? GroupHeaderTemplate
    {
        get => GetValue(GroupHeaderTemplateProperty);
        set => SetValue(GroupHeaderTemplateProperty, value);
    }

    public IDataTemplate? GroupFooterTemplate
    {
        get => GetValue(GroupFooterTemplateProperty);
        set => SetValue(GroupFooterTemplateProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Try to get parts from template if available
        var templateScrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        var templateItemsControl = e.NameScope.Find<ItemsControl>("PART_ItemsControl");
        var templateRootPanel = e.NameScope.Find<Panel>("PART_RootPanel");

        // Use template parts if found, otherwise keep the default ones
        if (templateRootPanel != null)
            _rootPanel = templateRootPanel;
        if (templateScrollViewer != null)
            _scrollViewer = templateScrollViewer;
        if (templateItemsControl != null)
            _itemsControl = templateItemsControl;

        UpdateEmptyView();
    }

    private void OnItemTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UpdateItemTemplate();
    }

    private void UpdateItemTemplate()
    {
        if (_itemsControl == null)
            return;

        if (IsGrouped && (GroupHeaderTemplate != null || GroupFooterTemplate != null))
        {
            // Create a template selector that chooses between group header/footer and item templates
            var template = new FuncDataTemplate<object>((obj, _) =>
            {
                Control? content = null;

                if (obj is GroupItem groupItem)
                {
                    if (groupItem.IsHeader && GroupHeaderTemplate != null)
                    {
                        content = GroupHeaderTemplate.Build(groupItem.Data);
                    }
                    else if (groupItem.IsFooter && GroupFooterTemplate != null)
                    {
                        content = GroupFooterTemplate.Build(groupItem.Data);
                    }
                    else if (!groupItem.IsHeader && !groupItem.IsFooter && ItemTemplate != null)
                    {
                        content = ItemTemplate.Build(groupItem.Data);
                    }
                }
                else if (ItemTemplate != null)
                {
                    content = ItemTemplate.Build(obj);
                }

                if (content == null)
                {
                    content = new TextBlock { Text = obj?.ToString() ?? string.Empty };
                }

                // Wrap content in a container that handles selection
                return WrapItemForSelection(content, obj);
            });

            _itemsControl.ItemTemplate = template;
        }
        else if (ItemTemplate != null)
        {
            var template = new FuncDataTemplate<object>((obj, _) =>
            {
                var content = ItemTemplate.Build(obj);
                if (content != null)
                {
                    return WrapItemForSelection(content, obj);
                }

                return null;
            });
            _itemsControl.ItemTemplate = template;
        }
    }

    private Control WrapItemForSelection(Control content, object? dataContext)
    {
        // Wrap the content in a button-like container to handle clicks
        var border = new global::Avalonia.Controls.Border
        {
            Child = content,
            Background = global::Avalonia.Media.Brushes.Transparent,
            Cursor = new global::Avalonia.Input.Cursor(global::Avalonia.Input.StandardCursorType.Hand)
        };

        border.PointerPressed += (sender, e) =>
        {
            if (SelectionMode != global::Avalonia.Controls.SelectionMode.Single &&
                SelectionMode != global::Avalonia.Controls.SelectionMode.Multiple)
                return;

            // Update selected item
            var actualData = dataContext is GroupItem groupItem ? groupItem.Data : dataContext;

            if (SelectedItem != actualData)
            {
                SelectedItem = actualData;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }

            e.Handled = true;
        };

        return border;
    }

    private void OnSelectedItemChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Update visual state of items if needed
        // For now, we'll just raise the event
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnScrollBarVisibilityChanged()
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.HorizontalScrollBarVisibility = HorizontalScrollBarVisibility;
            _scrollViewer.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
        }
    }

    private void OnItemsLayoutChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_itemsControl == null)
            return;

        // Update the ItemsControl's ItemsPanel based on the layout
        if (e.NewValue is Microsoft.Maui.Controls.GridItemsLayout gridLayout)
        {
            // For grid layouts, we need to use UniformGrid
            // MAUI's Vertical grid = flows down in columns (like reading top-to-bottom, left-to-right)
            // MAUI's Horizontal grid = flows right in rows (like reading left-to-right, top-to-bottom)

            if (gridLayout.Orientation == Microsoft.Maui.Controls.ItemsLayoutOrientation.Vertical)
            {
                // Vertical orientation: Span = number of columns
                var uniformGrid = new FuncTemplate<Panel?>(() => new UniformGrid
                {
                    Columns = gridLayout.Span
                });
                _itemsControl.ItemsPanel = uniformGrid;
            }
            else
            {
                // Horizontal orientation: Span = number of rows
                var uniformGrid = new FuncTemplate<Panel?>(() => new UniformGrid
                {
                    Rows = gridLayout.Span
                });
                _itemsControl.ItemsPanel = uniformGrid;
            }
        }
        else if (e.NewValue is Microsoft.Maui.Controls.LinearItemsLayout linearLayout)
        {
            // Use StackPanel for linear layouts
            var stackPanel = new FuncTemplate<Panel?>(() => new StackPanel
            {
                Orientation = linearLayout.Orientation == Microsoft.Maui.Controls.ItemsLayoutOrientation.Vertical
                    ? Orientation.Vertical
                    : Orientation.Horizontal,
                Spacing = linearLayout.ItemSpacing
            });
            _itemsControl.ItemsPanel = stackPanel;
        }
    }

    private void OnItemsSourceChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Unsubscribe from old collection
        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= OnCollectionChanged;
        }

        // Subscribe to new collection
        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += OnCollectionChanged;
        }

        UpdateItemsSource();
        UpdateEmptyView();
    }

    private void OnGroupingChanged()
    {
        UpdateItemTemplate();
        UpdateItemsSource();
    }

    private void UpdateItemsSource()
    {
        if (_itemsControl == null)
            return;

        if (IsGrouped && ItemsSource is IEnumerable enumerable)
        {
            // Flatten grouped data with headers and footers
            var flattenedItems = new System.Collections.ObjectModel.ObservableCollection<object>();

            foreach (var group in enumerable)
            {
                // Add group header
                if (GroupHeaderTemplate != null)
                {
                    flattenedItems.Add(new GroupItem { Data = group, IsHeader = true });
                }

                // Add group items
                if (group is IEnumerable groupItems)
                {
                    foreach (var item in groupItems)
                    {
                        flattenedItems.Add(new GroupItem { Data = item, IsHeader = false, IsFooter = false });
                    }
                }

                // Add group footer
                if (GroupFooterTemplate != null)
                {
                    flattenedItems.Add(new GroupItem { Data = group, IsFooter = true });
                }
            }

            _itemsControl.ItemsSource = flattenedItems;
        }
        else if (ItemsSource is IEnumerable nonGroupedEnumerable)
        {
            _itemsControl.ItemsSource = nonGroupedEnumerable;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateEmptyView();
    }

    private void UpdateEmptyView()
    {
        if (_rootPanel == null)
            return;

        // Remove existing empty view if any
        if (_emptyView != null && _rootPanel.Children.Contains(_emptyView))
        {
            _rootPanel.Children.Remove(_emptyView);
            _emptyView = null;
        }

        // Check if collection is empty
        bool isEmpty = ItemsSource == null || !ItemsSource.GetEnumerator().MoveNext();

        if (isEmpty && (EmptyView != null || EmptyViewTemplate != null))
        {
            if (EmptyViewTemplate != null)
            {
                _emptyView = EmptyViewTemplate.Build(EmptyView);
            }
            else if (EmptyView is Control control)
            {
                _emptyView = control;
            }
            else if (EmptyView is string text)
            {
                _emptyView = new TextBlock { Text = text };
            }

            if (_emptyView != null)
            {
                _rootPanel.Children.Add(_emptyView);
            }
        }

        // Show/hide the scroll viewer based on whether we have items
        if (_scrollViewer != null)
        {
            _scrollViewer.IsVisible = !isEmpty;
        }
    }

    public ItemsControl? GetItemsControl() => _itemsControl;

    public ScrollViewer? GetScrollViewer() => _scrollViewer;
}

/// <summary>
/// Helper class to wrap items in a grouped collection view
/// </summary>
internal class GroupItem
{
    public object? Data { get; set; }
    public bool IsHeader { get; set; }
    public bool IsFooter { get; set; }
}
