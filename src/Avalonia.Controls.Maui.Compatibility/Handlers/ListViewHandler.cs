using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Reflection;

namespace Avalonia.Controls.Maui.Compatibility.Handlers;

/// <summary>
/// Avalonia handler for <see cref="Microsoft.Maui.Controls.ListView"/>.
/// </summary>
public class ListViewHandler : ViewHandler<Microsoft.Maui.Controls.ListView, MauiListView>
{
    private static MethodInfo? _sendItemTappedMethod;
    private static MethodInfo? _sendCellAppearingMethod;
    private static MethodInfo? _sendCellDisappearingMethod;

    /// <summary>
    /// Property mapper for <see cref="ListViewHandler"/>.
    /// </summary>
    public static IPropertyMapper<Microsoft.Maui.Controls.ListView, ListViewHandler> Mapper =
        new PropertyMapper<Microsoft.Maui.Controls.ListView, ListViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.ListView.ItemsSource)] = MapItemsSource,
            [nameof(Microsoft.Maui.Controls.ListView.ItemTemplate)] = MapItemTemplate,
            [nameof(Microsoft.Maui.Controls.ListView.SelectedItem)] = MapSelectedItem,
            [nameof(Microsoft.Maui.Controls.ListView.SelectionMode)] = MapSelectionMode,
            [nameof(Microsoft.Maui.Controls.ListView.Header)] = MapHeader,
            [nameof(Microsoft.Maui.Controls.ListView.HeaderTemplate)] = MapHeaderTemplate,
            [nameof(Microsoft.Maui.Controls.ListView.Footer)] = MapFooter,
            [nameof(Microsoft.Maui.Controls.ListView.FooterTemplate)] = MapFooterTemplate,
            [nameof(Microsoft.Maui.Controls.ListView.SeparatorVisibility)] = MapSeparators,
            [nameof(Microsoft.Maui.Controls.ListView.SeparatorColor)] = MapSeparators,
            [nameof(Microsoft.Maui.Controls.ListView.RowHeight)] = MapRowHeight,
            [nameof(Microsoft.Maui.Controls.ListView.HasUnevenRows)] = MapRowHeight,
            [nameof(Microsoft.Maui.Controls.ListView.IsGroupingEnabled)] = MapIsGroupingEnabled,
            [nameof(Microsoft.Maui.Controls.ListView.GroupHeaderTemplate)] = MapGroupHeaderTemplate,
            [nameof(Microsoft.Maui.Controls.ListView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
            [nameof(Microsoft.Maui.Controls.ListView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
            [nameof(Microsoft.Maui.Controls.ListView.IsPullToRefreshEnabled)] = MapIsPullToRefreshEnabled,
            [nameof(Microsoft.Maui.Controls.ListView.IsRefreshing)] = MapIsRefreshing,
            [nameof(Microsoft.Maui.Controls.ListView.RefreshControlColor)] = MapRefreshControlColor,
        };

    /// <summary>
    /// Command mapper for <see cref="ListViewHandler"/>.
    /// </summary>
    public static CommandMapper<Microsoft.Maui.Controls.ListView, ListViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="ListViewHandler"/> class.
    /// </summary>
    public ListViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListViewHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public ListViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListViewHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public ListViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform view for this handler.
    /// </summary>
    protected override MauiListView CreatePlatformView()
    {
        return new MauiListView(VirtualView.CachingStrategy)
        {
            MauiContext = MauiContext
        };
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(MauiListView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ListBox.SelectionChanged += OnSelectionChanged;
        platformView.Scrolled += OnScrolled;
        platformView.ItemTapped += OnItemTapped;
        platformView.ItemAppearing += OnItemAppearing;
        platformView.ItemDisappearing += OnItemDisappearing;
        platformView.RefreshRequested += OnRefreshRequested;
        
        if (VirtualView != null)
        {
            VirtualView.ScrollToRequested += OnScrollToRequested;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(MauiListView platformView)
    {
        platformView.ListBox.SelectionChanged -= OnSelectionChanged;
        platformView.Scrolled -= OnScrolled;
        platformView.ItemTapped -= OnItemTapped;
        platformView.ItemAppearing -= OnItemAppearing;
        platformView.ItemDisappearing -= OnItemDisappearing;
        platformView.RefreshRequested -= OnRefreshRequested;

        if (VirtualView != null)
        {
            VirtualView.ScrollToRequested -= OnScrollToRequested;
        }

        base.DisconnectHandler(platformView);
    }

    private void OnScrollToRequested(object? sender, ScrollToRequestedEventArgs e)
    {
        PlatformView?.ScrollTo(e.Item, e.Position, e.ShouldAnimate);
    }

    private void OnScrolled(object? sender, ScrolledEventArgs e)
    {
        VirtualView?.SendScrolled(e);
    }

    private void OnItemTapped(object? sender, object? item)
    {
        if (item is MauiListView.GroupHeader)
            return;

        if (VirtualView is Microsoft.Maui.Controls.ListView listView)
        {
            _sendItemTappedMethod ??= typeof(Microsoft.Maui.Controls.ListView).GetMethod("SendItemTapped", BindingFlags.Instance | BindingFlags.NonPublic);
            _sendItemTappedMethod?.Invoke(listView, new object?[] { item, null });
        }
    }

    private void OnItemAppearing(object? sender, object? item)
    {
        if (item is MauiListView.GroupHeader)
            return;

        if (VirtualView is Microsoft.Maui.Controls.ListView listView)
        {
            _sendCellAppearingMethod ??= typeof(Microsoft.Maui.Controls.ListView).GetMethod("SendCellAppearing", BindingFlags.Instance | BindingFlags.NonPublic);
            _sendCellAppearingMethod?.Invoke(listView, new object?[] { null, item });
        }
    }

    private void OnItemDisappearing(object? sender, object? item)
    {
        if (item is MauiListView.GroupHeader)
            return;

        if (VirtualView is Microsoft.Maui.Controls.ListView listView)
        {
            _sendCellDisappearingMethod ??= typeof(Microsoft.Maui.Controls.ListView).GetMethod("SendCellDisappearing", BindingFlags.Instance | BindingFlags.NonPublic);
            _sendCellDisappearingMethod?.Invoke(listView, new object?[] { null, item });
        }
    }

    private void OnRefreshRequested(object? sender, EventArgs e)
    {
        if (VirtualView is Microsoft.Maui.Controls.ListView listView && listView.IsPullToRefreshEnabled)
        {
            if (listView.RefreshCommand?.CanExecute(null) == true)
            {
                listView.RefreshCommand.Execute(null);
            }
        }
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        if (VirtualView.SelectionMode == Microsoft.Maui.Controls.ListViewSelectionMode.None)
        {
            if (PlatformView.ListBox.SelectedItem != null)
            {
               PlatformView.ListBox.SelectedItem = null;
            }
            return;
        }

        var selectedItem = PlatformView.ListBox.SelectedItem;

        if (VirtualView.SelectedItem != selectedItem)
        {
            VirtualView.SelectedItem = selectedItem;
        }
    }

    /// <inheritdoc/>
    public override bool NeedsContainer => false;

    /// <summary>
    /// Maps the ItemsSource property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapItemsSource(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateItemsSource(listView);
    }

    /// <summary>
    /// Maps the ItemTemplate property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapItemTemplate(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateItemTemplate(listView, handler);
    }

    /// <summary>
    /// Maps the SelectedItem property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapSelectedItem(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateSelectedItem(listView);
    }

    /// <summary>
    /// Maps the SelectionMode property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapSelectionMode(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateSelectionMode(listView);
    }

    /// <summary>
    /// Maps the Header property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapHeader(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateHeader(listView, handler);
    }

    /// <summary>
    /// Maps the HeaderTemplate property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapHeaderTemplate(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateHeaderTemplate(listView, handler);
    }

    /// <summary>
    /// Maps the Footer property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapFooter(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateFooter(listView, handler);
    }

    /// <summary>
    /// Maps the FooterTemplate property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapFooterTemplate(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateFooterTemplate(listView, handler);
    }

    /// <summary>
    /// Maps the SeparatorVisibility and SeparatorColor properties to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapSeparators(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateSeparators(listView, handler);
    }

    /// <summary>
    /// Maps the RowHeight and HasUnevenRows properties to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapRowHeight(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateRowHeight(listView);
    }

    /// <summary>
    /// Maps the IsGroupingEnabled property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapIsGroupingEnabled(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateIsGroupingEnabled(listView);
    }

    /// <summary>
    /// Maps the GroupHeaderTemplate property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapGroupHeaderTemplate(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateGroupHeaderTemplate(listView, handler);
    }

    /// <summary>
    /// Maps the HorizontalScrollBarVisibility property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapHorizontalScrollBarVisibility(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateHorizontalScrollBarVisibility(listView);
    }

    /// <summary>
    /// Maps the VerticalScrollBarVisibility property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapVerticalScrollBarVisibility(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateVerticalScrollBarVisibility(listView);
    }

    /// <summary>
    /// Maps the IsPullToRefreshEnabled property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapIsPullToRefreshEnabled(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdatePullToRefreshEnabled(listView);
    }

    /// <summary>
    /// Maps the IsRefreshing property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapIsRefreshing(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateIsRefreshing(listView);
    }

    /// <summary>
    /// Maps the RefreshControlColor property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the ListView.</param>
    /// <param name="listView">The MAUI ListView virtual view.</param>
    public static void MapRefreshControlColor(ListViewHandler handler, Microsoft.Maui.Controls.ListView listView)
    {
        handler.PlatformView.UpdateRefreshControlColor(listView);
    }
}