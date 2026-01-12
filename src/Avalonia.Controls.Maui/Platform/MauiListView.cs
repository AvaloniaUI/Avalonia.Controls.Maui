using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Microsoft.Maui;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Data;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Avalonia platform control for MAUI ListView.
/// Handles the rendering and interaction of a ListView within the Avalonia visual tree.
/// </summary>
public class MauiListView : MauiView
{
    private readonly ListBox _listBox;
    private readonly ScrollViewer _scrollViewer;
    private readonly RefreshContainer _refreshContainer;
    private readonly StackPanel _mainContainer;
    private Control? _headerView;
    private Control? _footerView;

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiListView"/> class.
    /// </summary>
    public MauiListView()
    {
        _listBox = new ListBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel())
        };

        // Custom template for ListBox to remove its internal ScrollViewer
        _listBox.Template = new FuncControlTemplate<ListBox>((parent, scope) =>
        {
            return new ItemsPresenter
            {
                Name = "PART_ItemsPresenter",
                [~ItemsPresenter.ItemsPanelProperty] = parent[~ItemsControl.ItemsPanelProperty]
            }.RegisterInNameScope(scope);
        });

        _mainContainer = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        _mainContainer.Children.Add(_listBox);

        _scrollViewer = new ScrollViewer
        {
            Content = _mainContainer,
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        _refreshContainer = new RefreshContainer
        {
            Content = _scrollViewer
        };

        _scrollViewer.ScrollChanged += OnScrollChanged;
        _listBox.PointerPressed += OnListBoxPointerPressed;
        _listBox.ContainerPrepared += OnContainerPrepared;
        _listBox.ContainerClearing += OnContainerClearing;
        _refreshContainer.RefreshRequested += OnRefreshRequested;

        Children.Add(_refreshContainer);
    }

    /// <summary>
    /// Gets the internal ListBox control.
    /// </summary>
    public ListBox ListBox => _listBox;

    /// <summary>
    /// Gets or sets the MAUI context used for creating platform views.
    /// </summary>
    public IMauiContext? MauiContext { get; set; }

    /// <summary>
    /// Defines the <see cref="IsGroupingEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsGroupingEnabledProperty =
        AvaloniaProperty.Register<MauiListView, bool>(nameof(IsGroupingEnabled), false);

    /// <summary>
    /// Gets or sets a value indicating whether grouping is enabled.
    /// </summary>
    public bool IsGroupingEnabled
    {
        get => GetValue(IsGroupingEnabledProperty);
        set => SetValue(IsGroupingEnabledProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="GroupHeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> GroupHeaderTemplateProperty =
        AvaloniaProperty.Register<MauiListView, IDataTemplate?>(nameof(GroupHeaderTemplate));

    /// <summary>
    /// Gets or sets the data template for group headers.
    /// </summary>
    public IDataTemplate? GroupHeaderTemplate
    {
        get => GetValue(GroupHeaderTemplateProperty);
        set => SetValue(GroupHeaderTemplateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Header"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MauiListView, object?>(nameof(Header));

    /// <summary>
    /// Gets or sets the header content.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<MauiListView, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Gets or sets the data template for the header.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="FooterProperty"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<MauiListView, object?>(nameof(Footer));

    /// <summary>
    /// Defines the <see cref="HorizontalScrollBarVisibility"/> property.
    /// </summary>
    public static readonly StyledProperty<global::Avalonia.Controls.Primitives.ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<MauiListView, global::Avalonia.Controls.Primitives.ScrollBarVisibility>(nameof(HorizontalScrollBarVisibility), global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled);

    /// <summary>
    /// Defines the <see cref="VerticalScrollBarVisibility"/> property.
    /// </summary>
    public static readonly StyledProperty<global::Avalonia.Controls.Primitives.ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<MauiListView, global::Avalonia.Controls.Primitives.ScrollBarVisibility>(nameof(VerticalScrollBarVisibility), global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto);

    /// <summary>
    /// Gets or sets the horizontal scroll bar visibility.
    /// </summary>
    public global::Avalonia.Controls.Primitives.ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical scroll bar visibility.
    /// </summary>
    public global::Avalonia.Controls.Primitives.ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    static MauiListView()
    {
        HorizontalScrollBarVisibilityProperty.Changed.AddClassHandler<MauiListView>((x, e) => x.UpdateHorizontalScrollBarVisibility());
        VerticalScrollBarVisibilityProperty.Changed.AddClassHandler<MauiListView>((x, e) => x.UpdateVerticalScrollBarVisibility());
        HeaderProperty.Changed.AddClassHandler<MauiListView>((x, e) => x.UpdateHeader());
        HeaderTemplateProperty.Changed.AddClassHandler<MauiListView>((x, e) => x.UpdateHeader());
        FooterProperty.Changed.AddClassHandler<MauiListView>((x, e) => x.UpdateFooter());
        FooterTemplateProperty.Changed.AddClassHandler<MauiListView>((x, e) => x.UpdateFooter());
        IsGroupingEnabledProperty.Changed.AddClassHandler<MauiListView>((x, e) => x.OnItemsSourceChanged());
        IsPullToRefreshEnabledProperty.Changed.AddClassHandler<MauiListView>((x, e) => x.UpdatePullToRefresh());
        IsRefreshingProperty.Changed.AddClassHandler<MauiListView>((x, e) => x.UpdateIsRefreshing());
        RefreshControlColorProperty.Changed.AddClassHandler<MauiListView>((x, e) => x.UpdateRefreshControlColor());
    }

    private void UpdateHorizontalScrollBarVisibility()
    {
        _scrollViewer.HorizontalScrollBarVisibility = HorizontalScrollBarVisibility;
    }

    private void UpdateVerticalScrollBarVisibility()
    {
        _scrollViewer.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
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
    /// Defines the <see cref="FooterTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<MauiListView, IDataTemplate?>(nameof(FooterTemplate));

    /// <summary>
    /// Gets or sets the data template for the footer.
    /// </summary>
    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="SeparatorVisibility"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> SeparatorVisibilityProperty =
        AvaloniaProperty.Register<MauiListView, bool>(nameof(SeparatorVisibility), true);

    /// <summary>
    /// Gets or sets a value indicating whether separators are visible.
    /// </summary>
    public bool SeparatorVisibility
    {
        get => GetValue(SeparatorVisibilityProperty);
        set => SetValue(SeparatorVisibilityProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="SeparatorColor"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SeparatorColorProperty =
        AvaloniaProperty.Register<MauiListView, IBrush?>(nameof(SeparatorColor), Brushes.LightGray);

    /// <summary>
    /// Gets or sets the color of separators.
    /// </summary>
    public IBrush? SeparatorColor
    {
        get => GetValue(SeparatorColorProperty);
        set => SetValue(SeparatorColorProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsPullToRefreshEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPullToRefreshEnabledProperty =
        AvaloniaProperty.Register<MauiListView, bool>(nameof(IsPullToRefreshEnabled), false);

    /// <summary>
    /// Gets or sets a value indicating whether pull-to-refresh is enabled.
    /// </summary>
    public bool IsPullToRefreshEnabled
    {
        get => GetValue(IsPullToRefreshEnabledProperty);
        set => SetValue(IsPullToRefreshEnabledProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsRefreshing"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRefreshingProperty =
        AvaloniaProperty.Register<MauiListView, bool>(nameof(IsRefreshing), false);

    /// <summary>
    /// Gets or sets a value indicating whether the list is currently refreshing.
    /// </summary>
    public bool IsRefreshing
    {
        get => GetValue(IsRefreshingProperty);
        set => SetValue(IsRefreshingProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="RefreshControlColor"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> RefreshControlColorProperty =
        AvaloniaProperty.Register<MauiListView, IBrush?>(nameof(RefreshControlColor));

    /// <summary>
    /// Gets or sets the color of the refresh control.
    /// </summary>
    public IBrush? RefreshControlColor
    {
        get => GetValue(RefreshControlColorProperty);
        set => SetValue(RefreshControlColorProperty, value);
    }

    public event EventHandler? RefreshRequested;

    public event EventHandler<Microsoft.Maui.Controls.ScrolledEventArgs>? Scrolled;
    public event EventHandler<object?>? ItemTapped;
    public event EventHandler<object?>? ItemAppearing;
    public event EventHandler<object?>? ItemDisappearing;


    private void OnListBoxPointerPressed(object? sender, global::Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.ClickCount == 1 && e.Source is Visual visual)
        {
             var listBoxItem = visual.FindAncestorOfType<ListBoxItem>();
             if (listBoxItem != null)
             {
                 ItemTapped?.Invoke(this, listBoxItem.DataContext);
             }
        }
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        ItemAppearing?.Invoke(this, e.Container.DataContext);
    }

    private void OnContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        ItemDisappearing?.Invoke(this, e.Container.DataContext);
    }

    private void OnRefreshRequested(object? sender, global::Avalonia.Controls.RefreshRequestedEventArgs e)
    {
        var deferral = e.GetDeferral();
        IsRefreshing = true;
        RefreshRequested?.Invoke(this, EventArgs.Empty);
        
        // We need to keep a reference to the deferral to complete it when IsRefreshing becomes false
        _currentRefreshDeferral = deferral;
    }

    private global::Avalonia.Controls.RefreshCompletionDeferral? _currentRefreshDeferral;

    private void UpdatePullToRefresh()
    {
        // Avalonia's RefreshContainer doesn't have an IsEnabled per se that affects the gesture easily 
        // without affecting content interaction, but we can try to disable it.
        _refreshContainer.IsEnabled = IsPullToRefreshEnabled;
    }

    private void UpdateIsRefreshing()
    {
        if (!IsRefreshing && _currentRefreshDeferral != null)
        {
            _currentRefreshDeferral.Complete();
            _currentRefreshDeferral = null;
        }
    }

    private void UpdateRefreshControlColor()
    {
        if (_refreshContainer.Visualizer != null)
        {
            _refreshContainer.Visualizer.Foreground = RefreshControlColor;
        }
    }

    public void OnItemsSourceChanged()
    {
        if (IsGroupingEnabled && _listBox.ItemsSource is IEnumerable groups && !(groups is ObservableCollection<object>))
        {
            var flattened = new ObservableCollection<object>();
            foreach (var group in groups)
            {
                flattened.Add(new GroupHeader { Data = group });
                if (group is IEnumerable items)
                {
                    foreach (var item in items)
                    {
                        flattened.Add(item);
                    }
                }
            }
            _listBox.ItemsSource = flattened;
        }
    }

    public class GroupHeader
    {
        public object? Data { get; init; }
    }

    private void UpdateHeader()
    {
        if (_headerView != null)
        {
            _mainContainer.Children.Remove(_headerView);
            _headerView = null;
        }

        if (Header == null && HeaderTemplate == null)
            return;

        _headerView = CreateView(Header, HeaderTemplate);
        if (_headerView != null)
        {
            _mainContainer.Children.Insert(0, _headerView);
        }
    }

    private void UpdateFooter()
    {
        if (_footerView != null)
        {
            _mainContainer.Children.Remove(_footerView);
            _footerView = null;
        }

        if (Footer == null && FooterTemplate == null)
            return;

        _footerView = CreateView(Footer, FooterTemplate);
        if (_footerView != null)
        {
            _mainContainer.Children.Add(_footerView);
        }
    }

    private Control? CreateView(object? item, IDataTemplate? template)
    {
        if (template != null) return template.Build(item);
        if (item is Control control) return control;
        if (item != null) return new TextBlock { Text = item.ToString() };
        return null;
    }

    public void ScrollTo(object item, Microsoft.Maui.Controls.ScrollToPosition position, bool animated)
    {
        var container = _listBox.ContainerFromItem(item) as Control;
        if (container == null)
        {
            _listBox.ScrollIntoView(item);
            // After ScrollIntoView, we still might not have the container immediately 
            // but for simple cases like \"MakeVisible\", this already helps.
            // For Start/Center/End, we\'ll try to get it again.
            container = _listBox.ContainerFromItem(item) as Control;
        }

        if (container == null)
            return;

        var bounds = container.Bounds;
        var point = container.TranslatePoint(new Point(0, 0), _mainContainer);
        if (point == null) return;

        var itemTop = point.Value.Y;
        var itemHeight = bounds.Height;
        var itemBottom = itemTop + itemHeight;
        
        var viewportHeight = _scrollViewer.Viewport.Height;
        var currentOffset = _scrollViewer.Offset.Y;

        double finalOffset = position switch
        {
            Microsoft.Maui.Controls.ScrollToPosition.MakeVisible => 
                itemTop < currentOffset ? itemTop : 
                (itemBottom > currentOffset + viewportHeight ? itemBottom - viewportHeight : currentOffset),
            Microsoft.Maui.Controls.ScrollToPosition.Start => itemTop,
            Microsoft.Maui.Controls.ScrollToPosition.Center => itemTop + (itemHeight / 2) - (viewportHeight / 2),
            Microsoft.Maui.Controls.ScrollToPosition.End => itemBottom - viewportHeight,
            _ => itemTop
        };

        _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, finalOffset);
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        // Convert Avalonia ScrollChangedEventArgs to MAUI ScrolledEventArgs
        // Avalonia ScrollViewer Offset is a Vector.
        // We need current Scroll X and Y.
        if (sender is ScrollViewer sv)
        {
             Scrolled?.Invoke(this, new Microsoft.Maui.Controls.ScrolledEventArgs(sv.Offset.X, sv.Offset.Y));
        }
    }
}
