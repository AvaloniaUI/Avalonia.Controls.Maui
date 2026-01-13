using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Maui.Controls;
using System.Collections;
using System.Collections.Specialized;

namespace Avalonia.Controls.Maui;

public class MauiCollectionView : TemplatedControl
{
    private ItemsControl? _itemsControl;
    private ScrollViewer? _scrollViewer;
    private Control? _emptyView;
    private Control? _headerView;
    private Control? _footerView;
    private Panel? _rootPanel;
    private StackPanel? _mainContainer;

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
            defaultBindingMode: Data.BindingMode.TwoWay);

    public static readonly StyledProperty<SelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<MauiCollectionView, SelectionMode>(
            nameof(SelectionMode),
            SelectionMode.Single);

    public static readonly StyledProperty<IItemsLayout?> ItemsLayoutProperty =
        AvaloniaProperty.Register<MauiCollectionView, IItemsLayout?>(
            nameof(ItemsLayout),
            LinearItemsLayout.Vertical);

    public static readonly StyledProperty<bool> IsGroupedProperty =
        AvaloniaProperty.Register<MauiCollectionView, bool>(nameof(IsGrouped), false);

    public static readonly StyledProperty<IDataTemplate?> GroupHeaderTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(GroupHeaderTemplate));

    public static readonly StyledProperty<IDataTemplate?> GroupFooterTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(GroupFooterTemplate));

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MauiCollectionView, object?>(nameof(Header));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<MauiCollectionView, object?>(nameof(Footer));

    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(FooterTemplate));

    public static readonly StyledProperty<IList<object>?> SelectedItemsProperty =
        AvaloniaProperty.Register<MauiCollectionView, IList<object>?>(nameof(SelectedItems));

    public static readonly StyledProperty<ItemsUpdatingScrollMode> ItemsUpdatingScrollModeProperty =
        AvaloniaProperty.Register<MauiCollectionView, ItemsUpdatingScrollMode>(
            nameof(ItemsUpdatingScrollMode),
            ItemsUpdatingScrollMode.KeepItemsInView);

    public static readonly StyledProperty<int> RemainingItemsThresholdProperty =
        AvaloniaProperty.Register<MauiCollectionView, int>(nameof(RemainingItemsThreshold), -1);

    public event EventHandler? SelectionChanged;
    public event EventHandler? RemainingItemsThresholdReached;

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
        HeaderProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.UpdateHeaderFooter());
        HeaderTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.UpdateHeaderFooter());
        FooterProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.UpdateHeaderFooter());
        FooterTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.UpdateHeaderFooter());
        RemainingItemsThresholdProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.UpdateRemainingItemsThreshold());
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

    /// <summary>
    /// Gets or sets the item template.
    /// </summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the empty view.
    /// </summary>
    public object? EmptyView
    {
        get => GetValue(EmptyViewProperty);
        set => SetValue(EmptyViewProperty, value);
    }

    /// <summary>
    /// Gets or sets the empty view template.
    /// </summary>
    public IDataTemplate? EmptyViewTemplate
    {
        get => GetValue(EmptyViewTemplateProperty);
        set => SetValue(EmptyViewTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal scroll bar visibility.
    /// </summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical scroll bar visibility.
    /// </summary>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the selection mode.
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the items layout.
    /// </summary>
    public IItemsLayout? ItemsLayout
    {
        get => GetValue(ItemsLayoutProperty);
        set => SetValue(ItemsLayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the data is grouped.
    /// </summary>
    public bool IsGrouped
    {
        get => GetValue(IsGroupedProperty);
        set => SetValue(IsGroupedProperty, value);
    }

    /// <summary>
    /// Gets or sets the group header template.
    /// </summary>
    public IDataTemplate? GroupHeaderTemplate
    {
        get => GetValue(GroupHeaderTemplateProperty);
        set => SetValue(GroupHeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the group footer template.
    /// </summary>
    public IDataTemplate? GroupFooterTemplate
    {
        get => GetValue(GroupFooterTemplateProperty);
        set => SetValue(GroupFooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the header content.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the header template.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the footer content.
    /// </summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the footer template.
    /// </summary>
    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected items list.
    /// </summary>
    public IList<object>? SelectedItems
    {
        get => GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the items updating scroll mode.
    /// </summary>
    public ItemsUpdatingScrollMode ItemsUpdatingScrollMode
    {
        get => GetValue(ItemsUpdatingScrollModeProperty);
        set => SetValue(ItemsUpdatingScrollModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the threshold of remaining items to trigger the event.
    /// </summary>
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

        // Apply scrollbar visibility settings to the template's scroll viewer
        OnScrollBarVisibilityChanged();

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
        var border = new Border
        {
            Child = content,
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        border.PointerPressed += (sender, e) =>
        {
            if (SelectionMode != SelectionMode.Single &&
                SelectionMode != SelectionMode.Multiple)
                return;

            var actualData = dataContext is GroupItem groupItem ? groupItem.Data : dataContext;

            if (SelectionMode == SelectionMode.Multiple)
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
                // Fire event for SelectedItems changes
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Single selection, just set SelectedItem, event fires via OnSelectedItemChanged
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
        if (e.NewValue is GridItemsLayout gridLayout)
        {
            // For grid layouts, use our custom GridLayoutPanel that handles spacing properly
            // Configure ScrollViewer to disable scrolling in the cross-axis direction
            // This ensures the width/height constraint flows through properly to children

            if (gridLayout.Orientation == ItemsLayoutOrientation.Vertical)
            {
                // Vertical orientation: Span = number of columns
                // Scrolls vertically, width is constrained to viewport
                var gridPanel = new FuncTemplate<Panel?>(() => new GridLayoutPanel
                {
                    Columns = gridLayout.Span,
                    Orientation = Orientation.Vertical,
                    HorizontalSpacing = gridLayout.HorizontalItemSpacing,
                    VerticalSpacing = gridLayout.VerticalItemSpacing
                });
                _itemsControl.ItemsPanel = gridPanel;

                // Disable horizontal scrolling so width constraint is passed to children
                // Use user's setting for vertical scrollbar, falling back to Auto if not set
                if (_scrollViewer != null)
                {
                    _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    _scrollViewer.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
                }
            }
            else
            {
                // Horizontal orientation: Span = number of rows
                // Scrolls horizontally, height is constrained to viewport
                var gridPanel = new FuncTemplate<Panel?>(() => new GridLayoutPanel
                {
                    Rows = gridLayout.Span,
                    Orientation = Orientation.Horizontal,
                    HorizontalSpacing = gridLayout.HorizontalItemSpacing,
                    VerticalSpacing = gridLayout.VerticalItemSpacing
                });
                _itemsControl.ItemsPanel = gridPanel;

                // Disable vertical scrolling so height constraint is passed to children
                // Use user's setting for horizontal scrollbar, falling back to Auto if not set
                if (_scrollViewer != null)
                {
                    _scrollViewer.HorizontalScrollBarVisibility = HorizontalScrollBarVisibility;
                    _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
            }
        }
        else if (e.NewValue is LinearItemsLayout linearLayout)
        {
            // Use CollectionViewStackPanel for linear layouts - it constrains children
            // to the cross-axis dimension, matching MAUI's CollectionView behavior
            var stackPanel = new FuncTemplate<Panel?>(() => new MauiCollectionViewStackPanel
            {
                Orientation = linearLayout.Orientation == ItemsLayoutOrientation.Vertical
                    ? Orientation.Vertical
                    : Orientation.Horizontal,
                Spacing = linearLayout.ItemSpacing
            });
            _itemsControl.ItemsPanel = stackPanel;

            // Configure ScrollViewer based on orientation
            // Use user's setting for the scroll-axis scrollbar, falling back to Auto if not set
            if (_scrollViewer != null)
            {
                if (linearLayout.Orientation == ItemsLayoutOrientation.Vertical)
                {
                    _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    _scrollViewer.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
                }
                else
                {
                    _scrollViewer.HorizontalScrollBarVisibility = HorizontalScrollBarVisibility;
                    _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
            }
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

        // Calculate threshold distance
        var thresholdDistance = viewportHeight * (RemainingItemsThreshold + 1) / 10.0;

        if (remainingDistance <= thresholdDistance)
        {
            RemainingItemsThresholdReached?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ScrollTo(object item, object group, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
    {
        if (_itemsControl == null || _scrollViewer == null)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            var container = _itemsControl.ContainerFromItem(item);
            if (container != null)
            {
                ScrollToContainer(container, position, animate);
            }
        }, DispatcherPriority.Background);
    }

    public void ScrollTo(int index, int groupIndex, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
    {
        if (_itemsControl == null || _scrollViewer == null)
            return;

        Threading.Dispatcher.UIThread.Post(() =>
        {
            if (index >= 0 && _itemsControl.ItemCount > index)
            {
                Control? container = _itemsControl.ContainerFromIndex(index);
                
                if (container == null && _itemsControl.ItemsPanelRoot is Panel panel && panel.Children.Count > index)
                {
                    container = panel.Children[index] as Control;
                }

                if (container != null)
                {
                    ScrollToContainer(container, position, animate);
                }
            }
        }, Threading.DispatcherPriority.Background);
    }

    private void ScrollToContainer(Control container, ScrollToPosition position, bool animate)
    {
        if (_scrollViewer == null) return;
        var content = _scrollViewer.Content as Visual;
        if (content == null) return;

        var transform = container.TransformToVisual(content);
        if (transform == null) return;

        // Container top-left relative to the content
        var containerPos = transform.Value.Transform(new Point(0, 0));
        var containerRect = new Rect(containerPos, container.Bounds.Size);

        // Current offset
        var currentOffset = _scrollViewer.Offset;
        var viewport = _scrollViewer.Viewport;
        var extent = _scrollViewer.Extent;

        Vector targetOffset = currentOffset;

        if (extent.Height > viewport.Height)
        {
            switch (position)
            {
                case ScrollToPosition.MakeVisible:
                    container.BringIntoView();
                    return;
                case ScrollToPosition.Start:
                    targetOffset = new Vector(targetOffset.X, containerRect.Top);
                    break;
                case ScrollToPosition.Center:
                    targetOffset = new Vector(targetOffset.X, containerRect.Center.Y - (viewport.Height / 2));
                    break;
                case ScrollToPosition.End:
                    targetOffset = new Vector(targetOffset.X, containerRect.Bottom - viewport.Height);
                    break;
            }
        }
        
        // Horizontal Logic
        if (extent.Width > viewport.Width)
        {
             switch (position)
            {
                case ScrollToPosition.MakeVisible:
                    container.BringIntoView();
                    return;
                case ScrollToPosition.Start:
                    targetOffset = new Vector(containerRect.Left, targetOffset.Y);
                    break;
                case ScrollToPosition.Center:
                    targetOffset = new Vector(containerRect.Center.X - (viewport.Width / 2), targetOffset.Y);
                    break;
                case ScrollToPosition.End:
                    targetOffset = new Vector(containerRect.Right - viewport.Width, targetOffset.Y);
                    break;
            }
        }

        // Clamp to valid range
        var maxX = Math.Max(0, extent.Width - viewport.Width);
        var maxY = Math.Max(0, extent.Height - viewport.Height);
        
        var finalX = Math.Max(0, Math.Min(targetOffset.X, maxX));
        var finalY = Math.Max(0, Math.Min(targetOffset.Y, maxY));
        
        _scrollViewer.Offset = new Vector(finalX, finalY);
    }
}
