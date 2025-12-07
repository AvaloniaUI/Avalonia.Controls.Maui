using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Microsoft.Maui.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Maui;

public class CollectionView : TemplatedControl
{
    private ItemsControl? _itemsControl;
    private ScrollViewer? _scrollViewer;
    private Control? _emptyView;
    private Control? _headerView;
    private Control? _footerView;
    private Panel? _rootPanel;
    private StackPanel? _mainContainer;

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<CollectionView, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<CollectionView, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<object?> EmptyViewProperty =
        AvaloniaProperty.Register<CollectionView, object?>(nameof(EmptyView));

    public static readonly StyledProperty<IDataTemplate?> EmptyViewTemplateProperty =
        AvaloniaProperty.Register<CollectionView, IDataTemplate?>(nameof(EmptyViewTemplate));

    public static readonly StyledProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<CollectionView, ScrollBarVisibility>(
            nameof(HorizontalScrollBarVisibility),
            ScrollBarVisibility.Auto);

    public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<CollectionView, ScrollBarVisibility>(
            nameof(VerticalScrollBarVisibility),
            ScrollBarVisibility.Auto);

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<CollectionView, object?>(
            nameof(SelectedItem),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<global::Avalonia.Controls.SelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<CollectionView, global::Avalonia.Controls.SelectionMode>(
            nameof(SelectionMode),
            global::Avalonia.Controls.SelectionMode.Single);

    public static readonly StyledProperty<IItemsLayout?> ItemsLayoutProperty =
        AvaloniaProperty.Register<CollectionView, IItemsLayout?>(
            nameof(ItemsLayout),
            Microsoft.Maui.Controls.LinearItemsLayout.Vertical);

    public static readonly StyledProperty<bool> IsGroupedProperty =
        AvaloniaProperty.Register<CollectionView, bool>(nameof(IsGrouped), false);

    public static readonly StyledProperty<IDataTemplate?> GroupHeaderTemplateProperty =
        AvaloniaProperty.Register<CollectionView, IDataTemplate?>(nameof(GroupHeaderTemplate));

    public static readonly StyledProperty<IDataTemplate?> GroupFooterTemplateProperty =
        AvaloniaProperty.Register<CollectionView, IDataTemplate?>(nameof(GroupFooterTemplate));

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<CollectionView, object?>(nameof(Header));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<CollectionView, IDataTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<CollectionView, object?>(nameof(Footer));

    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<CollectionView, IDataTemplate?>(nameof(FooterTemplate));

    public static readonly StyledProperty<IList<object>?> SelectedItemsProperty =
        AvaloniaProperty.Register<CollectionView, IList<object>?>(nameof(SelectedItems));

    public static readonly StyledProperty<ItemsUpdatingScrollMode> ItemsUpdatingScrollModeProperty =
        AvaloniaProperty.Register<CollectionView, ItemsUpdatingScrollMode>(
            nameof(ItemsUpdatingScrollMode),
            ItemsUpdatingScrollMode.KeepItemsInView);

    public static readonly StyledProperty<int> RemainingItemsThresholdProperty =
        AvaloniaProperty.Register<CollectionView, int>(nameof(RemainingItemsThreshold), -1);

    public event EventHandler? SelectionChanged;
    public event EventHandler? RemainingItemsThresholdReached;

    static CollectionView()
    {
        ItemsSourceProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.OnItemsSourceChanged(e));
        EmptyViewProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.UpdateEmptyView());
        EmptyViewTemplateProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.UpdateEmptyView());
        ItemTemplateProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.OnItemTemplateChanged(e));
        HorizontalScrollBarVisibilityProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.OnScrollBarVisibilityChanged());
        VerticalScrollBarVisibilityProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.OnScrollBarVisibilityChanged());
        ItemsLayoutProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.OnItemsLayoutChanged(e));
        IsGroupedProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.OnGroupingChanged());
        GroupHeaderTemplateProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.OnGroupingChanged());
        GroupFooterTemplateProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.OnGroupingChanged());
        SelectedItemProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.OnSelectedItemChanged(e));
        HeaderProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.UpdateHeaderFooter());
        HeaderTemplateProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.UpdateHeaderFooter());
        FooterProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.UpdateHeaderFooter());
        FooterTemplateProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.UpdateHeaderFooter());
        RemainingItemsThresholdProperty.Changed.AddClassHandler<CollectionView>((cv, e) => cv.UpdateRemainingItemsThreshold());
    }

    public CollectionView()
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

        // Create a main container that holds header, items, and footer
        _mainContainer = new StackPanel { Orientation = Orientation.Vertical };
        _mainContainer.Children.Add(_itemsControl);

        _scrollViewer.Content = _mainContainer;
        _scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
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

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    public IList<object>? SelectedItems
    {
        get => GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    public ItemsUpdatingScrollMode ItemsUpdatingScrollMode
    {
        get => GetValue(ItemsUpdatingScrollModeProperty);
        set => SetValue(ItemsUpdatingScrollModeProperty, value);
    }

    public int RemainingItemsThreshold
    {
        get => GetValue(RemainingItemsThresholdProperty);
        set => SetValue(RemainingItemsThresholdProperty, value);
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

            var actualData = dataContext is GroupItem groupItem ? groupItem.Data : dataContext;

            if (SelectionMode == global::Avalonia.Controls.SelectionMode.Multiple)
            {
                var selectedItems = SelectedItems;
                if (selectedItems == null)
                {
                    selectedItems = new System.Collections.ObjectModel.ObservableCollection<object>();
                    SelectedItems = selectedItems;
                }

                if (actualData != null)
                {
                    if (selectedItems.Contains(actualData))
                    {
                        selectedItems.Remove(actualData);
                        if (SelectedItem == actualData)
                        {
                            SelectedItem = selectedItems.Count > 0 ? selectedItems[selectedItems.Count - 1] : null;
                        }
                    }
                    else
                    {
                        selectedItems.Add(actualData);
                        SelectedItem = actualData;
                    }
                }
                // Fire event for SelectedItems changes (SelectedItem change already fires via property handler)
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Single selection - just set SelectedItem, event fires via OnSelectedItemChanged
                SelectedItem = actualData;
            }

            e.Handled = true;
        };

        return border;
    }

    private void OnSelectedItemChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Only fire if value actually changed (prevents feedback loops)
        if (Equals(e.OldValue, e.NewValue))
            return;

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
            // For grid layouts, use our custom GridLayoutPanel that handles spacing properly
            // MAUI's Vertical grid = flows down in columns (like reading top-to-bottom, left-to-right)
            // MAUI's Horizontal grid = flows right in rows (like reading left-to-right, top-to-bottom)

            if (gridLayout.Orientation == Microsoft.Maui.Controls.ItemsLayoutOrientation.Vertical)
            {
                // Vertical orientation: Span = number of columns
                var gridPanel = new FuncTemplate<Panel?>(() => new GridLayoutPanel
                {
                    Columns = gridLayout.Span,
                    Orientation = global::Avalonia.Layout.Orientation.Vertical,
                    HorizontalSpacing = gridLayout.HorizontalItemSpacing,
                    VerticalSpacing = gridLayout.VerticalItemSpacing
                });
                _itemsControl.ItemsPanel = gridPanel;
            }
            else
            {
                // Horizontal orientation: Span = number of rows
                var gridPanel = new FuncTemplate<Panel?>(() => new GridLayoutPanel
                {
                    Rows = gridLayout.Span,
                    Orientation = global::Avalonia.Layout.Orientation.Horizontal,
                    HorizontalSpacing = gridLayout.HorizontalItemSpacing,
                    VerticalSpacing = gridLayout.VerticalItemSpacing
                });
                _itemsControl.ItemsPanel = gridPanel;
            }
        }
        else if (e.NewValue is Microsoft.Maui.Controls.LinearItemsLayout linearLayout)
        {
            // Use CollectionViewStackPanel for linear layouts - it constrains children
            // to the cross-axis dimension, matching MAUI's CollectionView behavior
            var stackPanel = new FuncTemplate<Panel?>(() => new CollectionViewStackPanel
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

    private void UpdateHeaderFooter()
    {
        if (_mainContainer == null)
            return;

        // Remove existing header if any
        if (_headerView != null && _mainContainer.Children.Contains(_headerView))
        {
            _mainContainer.Children.Remove(_headerView);
            _headerView = null;
        }

        // Remove existing footer if any
        if (_footerView != null && _mainContainer.Children.Contains(_footerView))
        {
            _mainContainer.Children.Remove(_footerView);
            _footerView = null;
        }

        // Create header view
        if (Header != null || HeaderTemplate != null)
        {
            if (HeaderTemplate != null)
            {
                _headerView = HeaderTemplate.Build(Header);
            }
            else if (Header is Control headerControl)
            {
                _headerView = headerControl;
            }
            else if (Header is string headerText)
            {
                _headerView = new TextBlock { Text = headerText };
            }

            if (_headerView != null)
            {
                // Insert at the beginning
                _mainContainer.Children.Insert(0, _headerView);
            }
        }

        // Create footer view
        if (Footer != null || FooterTemplate != null)
        {
            if (FooterTemplate != null)
            {
                _footerView = FooterTemplate.Build(Footer);
            }
            else if (Footer is Control footerControl)
            {
                _footerView = footerControl;
            }
            else if (Footer is string footerText)
            {
                _footerView = new TextBlock { Text = footerText };
            }

            if (_footerView != null)
            {
                // Add at the end
                _mainContainer.Children.Add(_footerView);
            }
        }
    }

    private void UpdateRemainingItemsThreshold()
    {
        // RemainingItemsThreshold is used with scroll events to trigger loading more items
        // The actual check happens in OnScrollViewerScrollChanged
    }

    private void OnScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_scrollViewer == null || RemainingItemsThreshold < 0)
            return;

        // Calculate how close we are to the bottom
        var verticalOffset = _scrollViewer.Offset.Y;
        var viewportHeight = _scrollViewer.Viewport.Height;
        var extentHeight = _scrollViewer.Extent.Height;

        if (extentHeight <= 0)
            return;

        // Calculate remaining items (approximate based on position)
        var remainingDistance = extentHeight - (verticalOffset + viewportHeight);

        // If we're close enough to the bottom (within threshold * average item height estimate)
        // For simplicity, we use a percentage-based approach
        var thresholdDistance = viewportHeight * (RemainingItemsThreshold + 1) / 10.0;

        if (remainingDistance <= thresholdDistance)
        {
            RemainingItemsThresholdReached?.Invoke(this, EventArgs.Empty);
        }
    }
}
