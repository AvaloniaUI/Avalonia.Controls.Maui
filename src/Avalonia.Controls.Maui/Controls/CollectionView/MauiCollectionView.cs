using Avalonia.Controls;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Maui.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MauiItemSizingStrategy = Microsoft.Maui.Controls.ItemSizingStrategy;
using MauiSelectionMode = Microsoft.Maui.Controls.SelectionMode;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Avalonia templated control that implements MAUI's CollectionView with support for selection, grouping, and virtualized scrolling.
/// </summary>
public class MauiCollectionView : TemplatedControl
{
    private ItemsControl? _itemsControl;
    private ScrollViewer? _scrollViewer;
    private Control? _emptyView;
    private Control? _headerView;
    private Control? _footerView;
    private Panel? _rootPanel;
    private StackPanel? _mainContainer;
    private ObservableCollection<object>? _flattenedGroupedItems;
    private readonly HashSet<INotifyCollectionChanged> _subscribedGroupCollections = new();
    private object? _renderedEmptyViewSource;
    private IDataTemplate? _renderedEmptyViewTemplate;
    private bool _isEmptyViewVisible;
    private bool _hasReachedRemainingItemsThreshold;

    /// <summary>Defines the <see cref="ItemsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MauiCollectionView, IEnumerable?>(nameof(ItemsSource));

    /// <summary>Defines the <see cref="ItemTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>Defines the <see cref="EmptyView"/> property.</summary>
    public static readonly StyledProperty<object?> EmptyViewProperty =
        AvaloniaProperty.Register<MauiCollectionView, object?>(nameof(EmptyView));

    /// <summary>Defines the <see cref="EmptyViewTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyViewTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(EmptyViewTemplate));

    /// <summary>Defines the <see cref="HorizontalScrollBarVisibility"/> property.</summary>
    public static readonly StyledProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<MauiCollectionView, ScrollBarVisibility>(
            nameof(HorizontalScrollBarVisibility),
            ScrollBarVisibility.Auto);

    /// <summary>Defines the <see cref="VerticalScrollBarVisibility"/> property.</summary>
    public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<MauiCollectionView, ScrollBarVisibility>(
            nameof(VerticalScrollBarVisibility),
            ScrollBarVisibility.Auto);

    /// <summary>Defines the <see cref="SelectedItem"/> property.</summary>
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<MauiCollectionView, object?>(
            nameof(SelectedItem),
            defaultBindingMode: Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="SelectionMode"/> property.</summary>
    public static readonly StyledProperty<MauiSelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<MauiCollectionView, MauiSelectionMode>(
            nameof(SelectionMode),
            MauiSelectionMode.None);

    /// <summary>Defines the <see cref="ItemsLayout"/> property.</summary>
    public static readonly StyledProperty<IItemsLayout?> ItemsLayoutProperty =
        AvaloniaProperty.Register<MauiCollectionView, IItemsLayout?>(
            nameof(ItemsLayout),
            LinearItemsLayout.Vertical);

    /// <summary>Defines the <see cref="ItemSizingStrategy"/> property.</summary>
    public static readonly StyledProperty<MauiItemSizingStrategy> ItemSizingStrategyProperty =
        AvaloniaProperty.Register<MauiCollectionView, MauiItemSizingStrategy>(
            nameof(ItemSizingStrategy),
            MauiItemSizingStrategy.MeasureAllItems);

    /// <summary>Defines the <see cref="IsGrouped"/> property.</summary>
    public static readonly StyledProperty<bool> IsGroupedProperty =
        AvaloniaProperty.Register<MauiCollectionView, bool>(nameof(IsGrouped), false);

    /// <summary>Defines the <see cref="GroupHeaderTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> GroupHeaderTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(GroupHeaderTemplate));

    /// <summary>Defines the <see cref="GroupFooterTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> GroupFooterTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(GroupFooterTemplate));

    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MauiCollectionView, object?>(nameof(Header));

    /// <summary>Defines the <see cref="HeaderTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>Defines the <see cref="Footer"/> property.</summary>
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<MauiCollectionView, object?>(nameof(Footer));

    /// <summary>Defines the <see cref="FooterTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<MauiCollectionView, IDataTemplate?>(nameof(FooterTemplate));

    /// <summary>Defines the <see cref="SelectedItems"/> property.</summary>
    public static readonly StyledProperty<IList<object>?> SelectedItemsProperty =
        AvaloniaProperty.Register<MauiCollectionView, IList<object>?>(nameof(SelectedItems));

    /// <summary>Defines the <see cref="ItemsUpdatingScrollMode"/> property.</summary>
    public static readonly StyledProperty<ItemsUpdatingScrollMode> ItemsUpdatingScrollModeProperty =
        AvaloniaProperty.Register<MauiCollectionView, ItemsUpdatingScrollMode>(
            nameof(ItemsUpdatingScrollMode),
            ItemsUpdatingScrollMode.KeepItemsInView);

    /// <summary>Defines the <see cref="RemainingItemsThreshold"/> property.</summary>
    public static readonly StyledProperty<int> RemainingItemsThresholdProperty =
        AvaloniaProperty.Register<MauiCollectionView, int>(nameof(RemainingItemsThreshold), -1);

    /// <summary>Occurs when the current selection changes.</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>Occurs when the user has scrolled close enough to the end of the items that the remaining items threshold has been reached.</summary>
    public event EventHandler? RemainingItemsThresholdReached;

    /// <summary>Occurs when the scroll position changes within the underlying <see cref="ScrollViewer"/>.</summary>
    public event EventHandler<ScrollChangedEventArgs>? ScrollChanged;

    static MauiCollectionView()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnItemsSourceChanged(e));
        EmptyViewProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.UpdateEmptyView());
        EmptyViewTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.UpdateEmptyView());
        ItemTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.UpdateItemTemplate());
        HorizontalScrollBarVisibilityProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.OnScrollBarVisibilityChanged());
        VerticalScrollBarVisibilityProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.OnScrollBarVisibilityChanged());
        ItemsLayoutProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnItemsLayoutChanged(e));
        ItemSizingStrategyProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.OnItemSizingStrategyChanged());
        IsGroupedProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.OnGroupingChanged());
        GroupHeaderTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.OnGroupingChanged());
        GroupFooterTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.OnGroupingChanged());
        SelectedItemProperty.Changed.AddClassHandler<MauiCollectionView>((cv, e) => cv.OnSelectedItemChanged(e));
        SelectionModeProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.OnSelectionModeChanged());
        HeaderProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.UpdateHeaderFooter());
        HeaderTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.UpdateHeaderFooter());
        FooterProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.UpdateHeaderFooter());
        FooterTemplateProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.UpdateHeaderFooter());
        RemainingItemsThresholdProperty.Changed.AddClassHandler<MauiCollectionView>((cv, _) => cv.ResetRemainingItemsThresholdGate());
    }

    /// <summary>
    /// Initializes a new instance of <see cref="MauiCollectionView"/>.
    /// </summary>
    public MauiCollectionView()
    {
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

        _mainContainer = new StackPanel { Orientation = Orientation.Vertical };
        _mainContainer.Children.Add(_itemsControl);

        _scrollViewer.Content = _mainContainer;
        _scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
        _rootPanel.Children.Add(_scrollViewer);

        VisualChildren.Add(_rootPanel);
        LogicalChildren.Add(_rootPanel);
    }

    /// <summary>
    /// Gets or sets the data source for the collection view.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to render each item in the collection.
    /// </summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the content displayed when the items source is empty.
    /// </summary>
    public object? EmptyView
    {
        get => GetValue(EmptyViewProperty);
        set => SetValue(EmptyViewProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to render the empty view.
    /// </summary>
    public IDataTemplate? EmptyViewTemplate
    {
        get => GetValue(EmptyViewTemplateProperty);
        set => SetValue(EmptyViewTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal scroll bar visibility for the collection view.
    /// </summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical scroll bar visibility for the collection view.
    /// </summary>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected item.
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the selection behavior for the collection view.
    /// </summary>
    public MauiSelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the layout specification that determines how items are arranged in the collection.
    /// </summary>
    public IItemsLayout? ItemsLayout
    {
        get => GetValue(ItemsLayoutProperty);
        set => SetValue(ItemsLayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets the strategy used to measure and size items in the collection.
    /// </summary>
    public MauiItemSizingStrategy ItemSizingStrategy
    {
        get => GetValue(ItemSizingStrategyProperty);
        set => SetValue(ItemSizingStrategyProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether items in the collection are grouped.
    /// </summary>
    public bool IsGrouped
    {
        get => GetValue(IsGroupedProperty);
        set => SetValue(IsGroupedProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to render the header for each group.
    /// </summary>
    public IDataTemplate? GroupHeaderTemplate
    {
        get => GetValue(GroupHeaderTemplateProperty);
        set => SetValue(GroupHeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to render the footer for each group.
    /// </summary>
    public IDataTemplate? GroupFooterTemplate
    {
        get => GetValue(GroupFooterTemplateProperty);
        set => SetValue(GroupFooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the content displayed at the top of the collection view.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to render the header content.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the content displayed at the bottom of the collection view.
    /// </summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to render the footer content.
    /// </summary>
    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the list of currently selected items when using multiple selection mode.
    /// </summary>
    public IList<object>? SelectedItems
    {
        get => GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the scroll behavior when items are updated in the collection.
    /// </summary>
    public ItemsUpdatingScrollMode ItemsUpdatingScrollMode
    {
        get => GetValue(ItemsUpdatingScrollModeProperty);
        set => SetValue(ItemsUpdatingScrollModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of items remaining before the <see cref="RemainingItemsThresholdReached"/> event fires.
    /// </summary>
    public int RemainingItemsThreshold
    {
        get => GetValue(RemainingItemsThresholdProperty);
        set => SetValue(RemainingItemsThresholdProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ThemeVariantScope.ActualThemeVariantProperty)
        {
            OnThemeChanged();
        }
    }

    private void OnThemeChanged()
    {
        if (_itemsControl == null)
            return;

        // When the theme changes, MAUI-created controls have explicit color values
        // (not DynamicResource bindings), so we need to force recreation of all items
        // by re-applying the template and refreshing the items source.
        UpdateItemTemplate();
        UpdateItemsSource(forceRebuild: true);
        UpdateHeaderFooter();
        UpdateEmptyView();
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var templateScrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        var templateItemsControl = e.NameScope.Find<ItemsControl>("PART_ItemsControl");
        var templateRootPanel = e.NameScope.Find<Panel>("PART_RootPanel");

        if (templateRootPanel != null)
            _rootPanel = templateRootPanel;
        if (templateScrollViewer != null)
            _scrollViewer = templateScrollViewer;
        if (templateItemsControl != null)
            _itemsControl = templateItemsControl;

        OnScrollBarVisibilityChanged();
        UpdateEmptyView();
    }

    private void UpdateItemTemplate()
    {
        if (_itemsControl == null)
            return;

        if (IsGrouped && (GroupHeaderTemplate != null || GroupFooterTemplate != null))
        {
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

                    // The ItemsControl sets DataContext to the GroupItem wrapper,
                    // but templates expect the actual data object.
                    if (content != null)
                    {
                        content.DataContext = groupItem.Data;
                    }
                }
                else if (ItemTemplate != null)
                {
                    content = ItemTemplate.Build(obj);
                }

                content ??= new TextBlock { Text = obj?.ToString() ?? string.Empty };

                return WrapItemIfSelectable(content, obj);
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
                    return WrapItemIfSelectable(content, obj);
                }

                return null;
            });
            _itemsControl.ItemTemplate = template;
        }
    }

    private Control WrapItemIfSelectable(Control content, object? item)
    {
        if (!ShouldWrapItemForSelection(item))
        {
            return content;
        }

        return new SelectionContainer(this)
        {
            Child = content,
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
    }

    private bool ShouldWrapItemForSelection(object? item)
    {
        if (SelectionMode == MauiSelectionMode.None)
        {
            return false;
        }

        if (item is GroupItem groupItem)
        {
            return !groupItem.IsHeader && !groupItem.IsFooter;
        }

        return true;
    }

    private void HandleSelection(object? dataContext)
    {
        if (SelectionMode != MauiSelectionMode.Single &&
            SelectionMode != MauiSelectionMode.Multiple)
            return;

        var actualData = dataContext is GroupItem groupItem ? groupItem.Data : dataContext;

        if (SelectionMode == MauiSelectionMode.Multiple)
        {
            var selectedItems = SelectedItems;
            if (selectedItems == null)
            {
                selectedItems = new ObservableCollection<object>();
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
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            UpdateSelectionVisuals();
        }
        else
        {
            SelectedItem = actualData;
        }
    }

    private void OnSelectedItemChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (Equals(e.OldValue, e.NewValue))
            return;

        if (SelectionMode == MauiSelectionMode.None)
        {
            if (e.NewValue != null)
            {
                SetCurrentValue(SelectedItemProperty, null);
            }

            UpdateSelectionVisuals();
            return;
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
        UpdateSelectionVisuals();
    }

    private void OnSelectionModeChanged()
    {
        if (SelectionMode == MauiSelectionMode.None)
        {
            if (SelectedItem != null)
            {
                SetCurrentValue(SelectedItemProperty, null);
            }

            SelectedItems?.Clear();
        }

        UpdateItemTemplate();
        UpdateSelectionVisuals();
    }

    private class SelectionContainer : Border
    {
        private readonly MauiCollectionView _owner;

        public SelectionContainer(MauiCollectionView owner)
        {
            _owner = owner;
            // Use handledEventsToo so selection works even when child gesture recognizers
            // (e.g. TapGestureRecognizer) have already handled the event during bubble routing.
            AddHandler(PointerPressedEvent, OnContainerPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
        }

        private void OnContainerPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _owner.HandleSelection(DataContext);
            e.Handled = true;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (Child is Control content)
            {
                _owner.RegisterItemContainer(this, content, DataContext);
                var isSelected = _owner.IsItemSelected(DataContext);
                _owner.UpdateVisualState(content, isSelected);
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _owner.UnregisterItemContainer(this);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (Child is Control content)
            {
                if (this.Parent != null)
                {
                    _owner.UnregisterItemContainer(this);
                    _owner.RegisterItemContainer(this, content, DataContext);
                }

                var isSelected = _owner.IsItemSelected(DataContext);
                _owner.UpdateVisualState(content, isSelected);
            }
        }
    }

    private class ItemContainerInfo
    {
        public WeakReference<Border> Border { get; }
        public WeakReference<Control> Content { get; }
        public WeakReference<object?> DataContext { get; }

        public ItemContainerInfo(Border border, Control content, object? dataContext)
        {
            Border = new WeakReference<Border>(border);
            Content = new WeakReference<Control>(content);
            DataContext = new WeakReference<object?>(dataContext);
        }
    }

    private readonly List<ItemContainerInfo> _itemContainers = new();

    private void RegisterItemContainer(Border border, Control content, object? dataContext)
    {
        _itemContainers.Add(new ItemContainerInfo(border, content, dataContext));
    }

    private void UnregisterItemContainer(Border border)
    {
        for (int i = 0; i < _itemContainers.Count; i++)
        {
            if (_itemContainers[i].Border.TryGetTarget(out var target) && target == border)
            {
                _itemContainers.RemoveAt(i);
                break;
            }
        }
    }

    private void UpdateSelectionVisuals()
    {
        for (int i = _itemContainers.Count - 1; i >= 0; i--)
        {
            var info = _itemContainers[i];
            if (info.Border.TryGetTarget(out var border) &&
                info.Content.TryGetTarget(out var content) &&
                info.DataContext.TryGetTarget(out var dataContext))
            {
                if (border.IsVisible && border.Parent != null)
                {
                    bool isSelected = IsItemSelected(dataContext);
                    UpdateVisualState(content, isSelected);
                }
            }
            else
            {
                _itemContainers.RemoveAt(i);
            }
        }
    }

    private bool IsItemSelected(object? item)
    {
        if (item == null) return false;

        if (item is GroupItem groupItem)
            item = groupItem.Data;

        if (SelectionMode == MauiSelectionMode.Single)
        {
            return Equals(SelectedItem, item);
        }
        else if (SelectionMode == MauiSelectionMode.Multiple)
        {
            return SelectedItems != null && SelectedItems.Contains(item!);
        }

        return false;
    }

    private void UpdateVisualState(Control content, bool isSelected)
    {
        if (content.Tag is VisualElement mauiView)
        {
            VisualStateManager.GoToState(mauiView, isSelected ? "Selected" : "Normal");

            IBrush? brush = null;

            if (mauiView.Background is Microsoft.Maui.Controls.SolidColorBrush solidBrush)
            {
                brush = solidBrush.Color.ToAvaloniaBrush();
            }
            else if (mauiView.BackgroundColor != null)
            {
                brush = mauiView.BackgroundColor.ToAvaloniaBrush();
            }

            if (brush != null)
            {
                if (content is Panel panel)
                {
                    panel.Background = brush;
                }
                else if (content is Border border)
                {
                    border.Background = brush;
                }
                else if (content is TemplatedControl templatedControl)
                {
                    templatedControl.Background = brush;
                }
            }
        }
        else
        {
            ((IPseudoClasses)content.Classes).Set(":selected", isSelected);
        }
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

        if (e.NewValue is GridItemsLayout gridLayout)
        {
            if (gridLayout.Orientation == ItemsLayoutOrientation.Vertical)
            {
                var gridPanel = new FuncTemplate<Panel?>(() => new GridLayoutPanel
                {
                    Columns = gridLayout.Span,
                    Orientation = Orientation.Vertical,
                    HorizontalSpacing = gridLayout.HorizontalItemSpacing,
                    VerticalSpacing = gridLayout.VerticalItemSpacing
                });
                _itemsControl.ItemsPanel = gridPanel;

                if (_scrollViewer != null)
                {
                    _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    _scrollViewer.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
                }
            }
            else
            {
                var gridPanel = new FuncTemplate<Panel?>(() => new GridLayoutPanel
                {
                    Rows = gridLayout.Span,
                    Orientation = Orientation.Horizontal,
                    HorizontalSpacing = gridLayout.HorizontalItemSpacing,
                    VerticalSpacing = gridLayout.VerticalItemSpacing
                });
                _itemsControl.ItemsPanel = gridPanel;

                if (_scrollViewer != null)
                {
                    _scrollViewer.HorizontalScrollBarVisibility = HorizontalScrollBarVisibility;
                    _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
            }
        }
        else if (e.NewValue is LinearItemsLayout linearLayout)
        {
            var stackPanel = new FuncTemplate<Panel?>(() => new MauiCollectionViewStackPanel
            {
                Orientation = linearLayout.Orientation == ItemsLayoutOrientation.Vertical
                    ? Orientation.Vertical
                    : Orientation.Horizontal,
                Spacing = linearLayout.ItemSpacing
            });
            _itemsControl.ItemsPanel = stackPanel;

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

    private void OnItemSizingStrategyChanged()
    {
        // ItemSizingStrategy is currently consumed as an opt-in scroll fast path.
        // Layout still uses the same measurement behavior unless a more complete
        // virtualization/recycling implementation is introduced.
    }

    private void OnItemsSourceChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= OnCollectionChanged;
        }

        UnsubscribeFromGroupCollections();

        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += OnCollectionChanged;
        }

        SubscribeToGroupCollections();

        UpdateItemsSource(forceRebuild: true);
        UpdateEmptyView();
        ResetRemainingItemsThresholdGate();
    }

    private void OnGroupingChanged()
    {
        SubscribeToGroupCollections();
        UpdateItemTemplate();
        UpdateItemsSource(forceRebuild: true);
        UpdateEmptyView();
        ResetRemainingItemsThresholdGate();
    }

    private void UpdateItemsSource(bool forceRebuild = false)
    {
        if (_itemsControl == null)
            return;

        if (ItemsSource == null)
        {
            _flattenedGroupedItems = null;
            _itemsControl.ItemsSource = null;
            return;
        }

        if (IsGrouped && ItemsSource is IEnumerable enumerable)
        {
            _flattenedGroupedItems ??= new ObservableCollection<object>();

            if (forceRebuild || !ReferenceEquals(_itemsControl.ItemsSource, _flattenedGroupedItems))
            {
                RebuildFlattenedGroupedItems(enumerable, _flattenedGroupedItems);
            }

            _itemsControl.ItemsSource = _flattenedGroupedItems;
        }
        else if (ItemsSource is IEnumerable nonGroupedEnumerable)
        {
            _flattenedGroupedItems = null;
            _itemsControl.ItemsSource = nonGroupedEnumerable;
        }
        else
        {
            _flattenedGroupedItems = null;
            _itemsControl.ItemsSource = null;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (ReferenceEquals(sender, ItemsSource))
        {
            SubscribeToGroupCollections();
        }

        if (IsGrouped)
        {
            if (!TryHandleGroupedCollectionChanged(sender, e))
            {
                UpdateItemsSource(forceRebuild: true);
            }
        }

        UpdateEmptyView();
        ResetRemainingItemsThresholdGate();
    }

    private bool TryHandleGroupedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_itemsControl == null || _flattenedGroupedItems == null || ItemsSource is not IEnumerable enumerable)
            return false;

        if (!ReferenceEquals(_itemsControl.ItemsSource, _flattenedGroupedItems))
        {
            UpdateItemsSource(forceRebuild: true);
            return true;
        }

        if (ReferenceEquals(sender, ItemsSource))
        {
            return TryHandleOuterGroupedCollectionChanged(e, enumerable);
        }

        return TryHandleInnerGroupedCollectionChanged(sender, e);
    }

    private bool TryHandleOuterGroupedCollectionChanged(NotifyCollectionChangedEventArgs e, IEnumerable groups)
    {
        if (_flattenedGroupedItems == null)
            return false;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e.NewItems != null && e.NewStartingIndex >= 0:
                int insertionIndex = GetFlatStartIndexForGroupIndex(e.NewStartingIndex);
                InsertGroups(insertionIndex, e.NewItems);
                return true;

            case NotifyCollectionChangedAction.Remove when e.OldItems != null && e.OldStartingIndex >= 0:
                int removalIndex = GetFlatStartIndexForGroupIndex(e.OldStartingIndex);
                RemoveGroups(removalIndex, e.OldItems);
                return true;

            case NotifyCollectionChangedAction.Replace
                when e.OldItems != null && e.NewItems != null && e.NewStartingIndex >= 0:
                int replacementIndex = GetFlatStartIndexForGroupIndex(e.NewStartingIndex);
                RemoveGroups(replacementIndex, e.OldItems);
                InsertGroups(replacementIndex, e.NewItems);
                return true;

            case NotifyCollectionChangedAction.Reset:
                RebuildFlattenedGroupedItems(groups, _flattenedGroupedItems);
                return true;

            default:
                return false;
        }
    }

    private bool TryHandleInnerGroupedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_flattenedGroupedItems == null || sender == null)
            return false;

        int groupIndex = GetGroupIndex(sender);
        if (groupIndex < 0)
            return false;

        int groupStartIndex = GetFlatStartIndexForGroupIndex(groupIndex);
        int itemStartIndex = groupStartIndex + (GroupHeaderTemplate != null ? 1 : 0);

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e.NewItems != null && e.NewStartingIndex >= 0:
                InsertItems(itemStartIndex + e.NewStartingIndex, e.NewItems);
                return true;

            case NotifyCollectionChangedAction.Remove when e.OldItems != null && e.OldStartingIndex >= 0:
                RemoveRange(itemStartIndex + e.OldStartingIndex, e.OldItems.Count);
                return true;

            case NotifyCollectionChangedAction.Replace
                when e.NewItems != null && e.OldItems != null && e.NewStartingIndex >= 0 && e.OldItems.Count == e.NewItems.Count:
                ReplaceItems(itemStartIndex + e.NewStartingIndex, e.NewItems);
                return true;

            case NotifyCollectionChangedAction.Reset:
                return false;

            default:
                return false;
        }
    }

    private void RebuildFlattenedGroupedItems(IEnumerable groups, ObservableCollection<object> target)
    {
        target.Clear();

        foreach (var group in groups)
        {
            AppendGroup(target, group);
        }
    }

    private void AppendGroup(ICollection<object> target, object? group)
    {
        if (GroupHeaderTemplate != null)
        {
            target.Add(new GroupItem { Data = group, IsHeader = true });
        }

        if (group is IEnumerable groupItems)
        {
            foreach (var item in groupItems)
            {
                target.Add(CreateGroupItem(item));
            }
        }

        if (GroupFooterTemplate != null)
        {
            target.Add(new GroupItem { Data = group, IsFooter = true });
        }
    }

    private GroupItem CreateGroupItem(object? item)
    {
        return new GroupItem
        {
            Data = item,
            IsHeader = false,
            IsFooter = false
        };
    }

    private void InsertGroups(int insertionIndex, IList groups)
    {
        if (_flattenedGroupedItems == null)
            return;

        foreach (var group in groups)
        {
            foreach (var flattenedItem in EnumerateFlattenedGroup(group))
            {
                _flattenedGroupedItems.Insert(insertionIndex++, flattenedItem);
            }
        }
    }

    private void RemoveGroups(int removalIndex, IList groups)
    {
        int itemsToRemove = 0;

        foreach (var group in groups)
        {
            itemsToRemove += GetFlatLengthForGroup(group);
        }

        RemoveRange(removalIndex, itemsToRemove);
    }

    private void InsertItems(int insertionIndex, IList items)
    {
        if (_flattenedGroupedItems == null)
            return;

        foreach (var item in items)
        {
            _flattenedGroupedItems.Insert(insertionIndex++, CreateGroupItem(item));
        }
    }

    private void ReplaceItems(int replacementIndex, IList items)
    {
        if (_flattenedGroupedItems == null)
            return;

        for (int i = 0; i < items.Count; i++)
        {
            _flattenedGroupedItems[replacementIndex + i] = CreateGroupItem(items[i]);
        }
    }

    private void RemoveRange(int index, int count)
    {
        if (_flattenedGroupedItems == null || count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            if (index >= 0 && index < _flattenedGroupedItems.Count)
            {
                _flattenedGroupedItems.RemoveAt(index);
            }
        }
    }

    private IEnumerable<object> EnumerateFlattenedGroup(object? group)
    {
        if (GroupHeaderTemplate != null)
        {
            yield return new GroupItem { Data = group, IsHeader = true };
        }

        if (group is IEnumerable groupItems)
        {
            foreach (var item in groupItems)
            {
                yield return CreateGroupItem(item);
            }
        }

        if (GroupFooterTemplate != null)
        {
            yield return new GroupItem { Data = group, IsFooter = true };
        }
    }

    private int GetFlatStartIndexForGroupIndex(int groupIndex)
    {
        if (groupIndex <= 0 || ItemsSource is not IEnumerable groups)
            return 0;

        int flatIndex = 0;
        int currentGroupIndex = 0;

        foreach (var group in groups)
        {
            if (currentGroupIndex >= groupIndex)
                break;

            flatIndex += GetFlatLengthForGroup(group);
            currentGroupIndex++;
        }

        return flatIndex;
    }

    private int GetGroupIndex(object groupToFind)
    {
        if (ItemsSource is not IEnumerable groups)
            return -1;

        int index = 0;
        foreach (var group in groups)
        {
            if (ReferenceEquals(group, groupToFind))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    private int GetFlatLengthForGroup(object? group)
    {
        int count = 0;

        if (GroupHeaderTemplate != null)
        {
            count++;
        }

        if (group is IEnumerable groupItems)
        {
            foreach (var _ in groupItems)
            {
                count++;
            }
        }

        if (GroupFooterTemplate != null)
        {
            count++;
        }

        return count;
    }

    private void UpdateEmptyView()
    {
        if (_rootPanel == null)
            return;

        bool isEmpty = IsItemsSourceEmpty();

        if (_itemsControl != null)
        {
            _itemsControl.IsVisible = !isEmpty;
        }

        if (!isEmpty)
        {
            RemoveEmptyView();
            return;
        }

        if (CanReuseEmptyView())
        {
            return;
        }

        RemoveEmptyView();

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
            _isEmptyViewVisible = true;
            _renderedEmptyViewSource = EmptyView;
            _renderedEmptyViewTemplate = EmptyViewTemplate;
        }
    }

    /// <summary>
    /// Gets the underlying <see cref="ItemsControl"/> used to display items.
    /// </summary>
    /// <returns>The <see cref="ItemsControl"/> instance, or <c>null</c> if the control has not been initialized.</returns>
    public ItemsControl? GetItemsControl() => _itemsControl;

    /// <summary>
    /// Gets the underlying <see cref="ScrollViewer"/> used for scrolling the collection.
    /// </summary>
    /// <returns>The <see cref="ScrollViewer"/> instance, or <c>null</c> if the control has not been initialized.</returns>
    public ScrollViewer? GetScrollViewer() => _scrollViewer;

    private void UpdateHeaderFooter()
    {
        if (_mainContainer == null)
            return;

        if (_headerView != null && _mainContainer.Children.Contains(_headerView))
        {
            _mainContainer.Children.Remove(_headerView);
            _headerView = null;
        }

        if (_footerView != null && _mainContainer.Children.Contains(_footerView))
        {
            _mainContainer.Children.Remove(_footerView);
            _footerView = null;
        }

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
                _mainContainer.Children.Insert(0, _headerView);
            }
        }

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
                _mainContainer.Children.Add(_footerView);
            }
        }
    }

    private void OnScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_scrollViewer == null)
            return;

        ScrollChanged?.Invoke(this, e);

        if (RemainingItemsThreshold < 0)
        {
            _hasReachedRemainingItemsThreshold = false;
            return;
        }

        var verticalOffset = _scrollViewer.Offset.Y;
        var viewportHeight = _scrollViewer.Viewport.Height;
        var extentHeight = _scrollViewer.Extent.Height;

        if (extentHeight <= 0)
        {
            _hasReachedRemainingItemsThreshold = false;
            return;
        }

        var remainingDistance = extentHeight - (verticalOffset + viewportHeight);
        var thresholdDistance = viewportHeight * (RemainingItemsThreshold + 1) / 10.0;
        var isWithinThreshold = remainingDistance <= thresholdDistance;

        if (isWithinThreshold && !_hasReachedRemainingItemsThreshold)
        {
            _hasReachedRemainingItemsThreshold = true;
            RemainingItemsThresholdReached?.Invoke(this, EventArgs.Empty);
        }
        else if (!isWithinThreshold)
        {
            _hasReachedRemainingItemsThreshold = false;
        }
    }

    private void SubscribeToGroupCollections()
    {
        UnsubscribeFromGroupCollections();

        if (!IsGrouped || ItemsSource is not IEnumerable groups)
            return;

        foreach (var group in groups)
        {
            if (group is INotifyCollectionChanged groupCollection)
            {
                groupCollection.CollectionChanged += OnCollectionChanged;
                _subscribedGroupCollections.Add(groupCollection);
            }
        }
    }

    private void UnsubscribeFromGroupCollections()
    {
        foreach (var groupCollection in _subscribedGroupCollections)
        {
            groupCollection.CollectionChanged -= OnCollectionChanged;
        }

        _subscribedGroupCollections.Clear();
    }

    private bool IsItemsSourceEmpty()
    {
        if (ItemsSource == null)
            return true;

        if (ItemsSource is ICollection collection)
            return collection.Count == 0;

        if (ItemsSource is IReadOnlyCollection<object> readOnlyCollection)
            return readOnlyCollection.Count == 0;

        var enumerator = ItemsSource.GetEnumerator();

        try
        {
            return !enumerator.MoveNext();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    private bool CanReuseEmptyView()
    {
        return _isEmptyViewVisible &&
            _emptyView != null &&
            Equals(_renderedEmptyViewSource, EmptyView) &&
            ReferenceEquals(_renderedEmptyViewTemplate, EmptyViewTemplate);
    }

    private void RemoveEmptyView()
    {
        if (_rootPanel == null)
            return;

        if (_emptyView != null && _rootPanel.Children.Contains(_emptyView))
        {
            _rootPanel.Children.Remove(_emptyView);
        }

        _emptyView = null;
        _isEmptyViewVisible = false;
        _renderedEmptyViewSource = null;
        _renderedEmptyViewTemplate = null;
    }

    private void ResetRemainingItemsThresholdGate()
    {
        _hasReachedRemainingItemsThreshold = false;
    }

    /// <summary>
    /// Scrolls the collection view to bring the specified item into view.
    /// </summary>
    /// <param name="item">The item to scroll to.</param>
    /// <param name="group">The group that contains the item.</param>
    /// <param name="position">The desired scroll position for the item.</param>
    /// <param name="animate">Whether to animate the scroll operation.</param>
    public void ScrollTo(object item, object group, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
    {
        if (_itemsControl == null || _scrollViewer == null)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            if (!IsGrouped &&
                group == null &&
                TryGetItemIndex(item, out var index) &&
                TryScrollToUniformLinearIndex(index, position))
            {
                return;
            }

            var container = _itemsControl.ContainerFromItem(item);
            if (container != null)
            {
                ScrollToContainer(container, position, animate);
            }
        }, DispatcherPriority.Background);
    }

    /// <summary>
    /// Scrolls the collection view to bring the item at the specified index into view.
    /// </summary>
    /// <param name="index">The zero-based index of the item to scroll to.</param>
    /// <param name="groupIndex">The zero-based index of the group that contains the item.</param>
    /// <param name="position">The desired scroll position for the item.</param>
    /// <param name="animate">Whether to animate the scroll operation.</param>
    public void ScrollTo(int index, int groupIndex, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
    {
        if (_itemsControl == null || _scrollViewer == null)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            if (!IsGrouped &&
                groupIndex < 0 &&
                TryScrollToUniformLinearIndex(index, position))
            {
                return;
            }

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
        }, DispatcherPriority.Background);
    }

    private bool TryGetItemIndex(object item, out int index)
    {
        if (ItemsSource is IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (Equals(list[i], item))
                {
                    index = i;
                    return true;
                }
            }
        }
        else if (ItemsSource is IEnumerable enumerable)
        {
            index = 0;
            foreach (var candidate in enumerable)
            {
                if (Equals(candidate, item))
                {
                    return true;
                }

                index++;
            }
        }

        index = -1;
        return false;
    }

    private bool TryScrollToUniformLinearIndex(int index, ScrollToPosition position)
    {
        if (_scrollViewer == null ||
            _itemsControl == null ||
            ItemSizingStrategy != MauiItemSizingStrategy.MeasureFirstItem ||
            ItemsLayout is not LinearItemsLayout linearLayout ||
            index < 0 ||
            index >= _itemsControl.ItemCount)
        {
            return false;
        }

        if (_itemsControl.ItemsPanelRoot is not Panel panel || panel.Children.Count == 0)
        {
            return false;
        }

        if (panel.Children[0] is not Control firstContainer)
        {
            return false;
        }

        var isVertical = linearLayout.Orientation == ItemsLayoutOrientation.Vertical;
        var itemSize = ResolveMeasuredExtent(
            isVertical ? firstContainer.Bounds.Height : firstContainer.Bounds.Width,
            isVertical ? firstContainer.DesiredSize.Height : firstContainer.DesiredSize.Width);
        var itemsControlOffset = GetItemsControlOffsetInScrollContent(isVertical);

        if (!(itemSize > 0) || itemsControlOffset < 0)
        {
            return false;
        }

        var spacing = Math.Max(0, linearLayout.ItemSpacing);
        var itemStart = itemsControlOffset + (index * (itemSize + spacing));
        var itemEnd = itemStart + itemSize;
        var viewport = isVertical ? _scrollViewer.Viewport.Height : _scrollViewer.Viewport.Width;
        var extent = isVertical ? _scrollViewer.Extent.Height : _scrollViewer.Extent.Width;
        var currentOffset = isVertical ? _scrollViewer.Offset.Y : _scrollViewer.Offset.X;

        if (!(viewport > 0) || !(extent > 0))
        {
            return false;
        }

        var targetOffset = currentOffset;
        switch (position)
        {
            case ScrollToPosition.MakeVisible:
                if (itemStart < currentOffset)
                {
                    targetOffset = itemStart;
                }
                else if (itemEnd > currentOffset + viewport)
                {
                    targetOffset = itemEnd - viewport;
                }
                else
                {
                    return true;
                }
                break;
            case ScrollToPosition.Start:
                targetOffset = itemStart;
                break;
            case ScrollToPosition.Center:
                targetOffset = itemStart + (itemSize / 2) - (viewport / 2);
                break;
            case ScrollToPosition.End:
                targetOffset = itemEnd - viewport;
                break;
        }

        var maxOffset = Math.Max(0, extent - viewport);
        targetOffset = Math.Max(0, Math.Min(targetOffset, maxOffset));

        _scrollViewer.Offset = isVertical
            ? new Vector(_scrollViewer.Offset.X, targetOffset)
            : new Vector(targetOffset, _scrollViewer.Offset.Y);

        return true;
    }

    private static double ResolveMeasuredExtent(double boundsExtent, double desiredExtent)
    {
        if (boundsExtent > 0 && !double.IsNaN(boundsExtent) && !double.IsInfinity(boundsExtent))
        {
            return boundsExtent;
        }

        if (desiredExtent > 0 && !double.IsNaN(desiredExtent) && !double.IsInfinity(desiredExtent))
        {
            return desiredExtent;
        }

        return 0;
    }

    private double GetItemsControlOffsetInScrollContent(bool isVertical)
    {
        if (_itemsControl == null || _scrollViewer?.Content is not Visual content)
        {
            return -1;
        }

        var transform = _itemsControl.TransformToVisual(content);
        if (transform == null)
        {
            return -1;
        }

        var origin = transform.Value.Transform(new Point(0, 0));
        return isVertical ? origin.Y : origin.X;
    }

    internal static void CleanupLogicalChild(Control content)
    {
        if (content.Tag is Microsoft.Maui.Controls.Element mauiElement &&
            mauiElement.Parent is Microsoft.Maui.Controls.Element parentElement)
        {
            parentElement.RemoveLogicalChild(mauiElement);
        }
    }

    private void ScrollToContainer(Control container, ScrollToPosition position, bool animate)
    {
        if (_scrollViewer == null) return;
        if (_scrollViewer.Content is not Visual content) return;

        var transform = container.TransformToVisual(content);
        if (transform == null) return;

        var containerPos = transform.Value.Transform(new Point(0, 0));
        var containerRect = new Rect(containerPos, container.Bounds.Size);

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

        var maxX = Math.Max(0, extent.Width - viewport.Width);
        var maxY = Math.Max(0, extent.Height - viewport.Height);

        var finalX = Math.Max(0, Math.Min(targetOffset.X, maxX));
        var finalY = Math.Max(0, Math.Min(targetOffset.Y, maxY));

        _scrollViewer.Offset = new Vector(finalX, finalY);
    }
}
