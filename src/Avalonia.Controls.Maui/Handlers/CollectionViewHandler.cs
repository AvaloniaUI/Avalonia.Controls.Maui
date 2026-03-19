using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Threading;
using Avalonia.Controls.Maui.Extensions;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="CollectionView"/>.</summary>
public class CollectionViewHandler : ViewHandler<CollectionView, MauiCollectionView>
{
    /// <summary>Property mapper for <see cref="CollectionViewHandler"/>.</summary>
    public static IPropertyMapper<ItemsView, CollectionViewHandler> Mapper =
        new PropertyMapper<ItemsView, CollectionViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(ItemsView.ItemsSource)] = MapItemsSource,
            [nameof(ItemsView.ItemTemplate)] = MapItemTemplate,
            [nameof(ItemsView.EmptyView)] = MapEmptyView,
            [nameof(ItemsView.EmptyViewTemplate)] = MapEmptyViewTemplate,
            [nameof(ItemsView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
            [nameof(ItemsView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
            [nameof(StructuredItemsView.ItemsLayout)] = MapItemsLayout,
            [nameof(StructuredItemsView.ItemSizingStrategy)] = MapItemSizingStrategy,
            [nameof(StructuredItemsView.Header)] = MapHeader,
            [nameof(StructuredItemsView.HeaderTemplate)] = MapHeaderTemplate,
            [nameof(StructuredItemsView.Footer)] = MapFooter,
            [nameof(StructuredItemsView.FooterTemplate)] = MapFooterTemplate,
            [nameof(GroupableItemsView.IsGrouped)] = MapIsGrouped,
            [nameof(GroupableItemsView.GroupHeaderTemplate)] = MapGroupHeaderTemplate,
            [nameof(GroupableItemsView.GroupFooterTemplate)] = MapGroupFooterTemplate,
            [nameof(SelectableItemsView.SelectedItem)] = MapSelectedItem,
            [nameof(SelectableItemsView.SelectedItems)] = MapSelectedItems,
            [nameof(SelectableItemsView.SelectionMode)] = MapSelectionMode,
            [nameof(ItemsView.ItemsUpdatingScrollMode)] = MapItemsUpdatingScrollMode,
            [nameof(ItemsView.RemainingItemsThreshold)] = MapRemainingItemsThreshold,
        };

    /// <summary>Command mapper for <see cref="CollectionViewHandler"/>.</summary>
    public static CommandMapper<CollectionView, CollectionViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {

        };

    private bool _isUpdatingSelection;

    /// <summary>Initializes a new instance of <see cref="CollectionViewHandler"/>.</summary>
    public CollectionViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="CollectionViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public CollectionViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="CollectionViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public CollectionViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override MauiCollectionView CreatePlatformView()
    {
        return new MauiCollectionView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(MauiCollectionView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SelectionChanged += OnSelectionChanged;
        platformView.RemainingItemsThresholdReached += OnRemainingItemsThresholdReached;
        platformView.ScrollChanged += OnScrollChanged;

        if (VirtualView is ItemsView itemsView)
        {
            itemsView.ScrollToRequested += OnScrollToRequested;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(MauiCollectionView platformView)
    {
        platformView.SelectionChanged -= OnSelectionChanged;
        platformView.RemainingItemsThresholdReached -= OnRemainingItemsThresholdReached;
        platformView.ScrollChanged -= OnScrollChanged;

        if (VirtualView is ItemsView itemsView)
        {
            itemsView.ScrollToRequested -= OnScrollToRequested;
        }

        base.DisconnectHandler(platformView);
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (VirtualView is ItemsView itemsView && sender is MauiCollectionView platformView)
        {
            var offset = platformView.GetScrollViewer()?.Offset ?? default;
            var args = new ItemsViewScrolledEventArgs
            {
                HorizontalDelta = e.OffsetDelta.X,
                VerticalDelta = e.OffsetDelta.Y,
                HorizontalOffset = offset.X,
                VerticalOffset = offset.Y,
                FirstVisibleItemIndex = -1,
                CenterItemIndex = -1,
                LastVisibleItemIndex = -1
            };

            itemsView.SendScrolled(args);
        }
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (VirtualView == null || PlatformView == null || _isUpdatingSelection)
            return;

        if (VirtualView is not SelectableItemsView selectableItemsView)
            return;

        if (selectableItemsView.SelectionMode == Microsoft.Maui.Controls.SelectionMode.None)
            return;

        // Capture current state
        var selectedItem = PlatformView.SelectedItem;
        var selectedItems = PlatformView.SelectedItems?.Cast<object>().ToList(); // Create a copy

        Dispatcher.UIThread.Post(() =>
        {
            if (VirtualView == null || PlatformView == null || _isUpdatingSelection)
                return;

            _isUpdatingSelection = true;
            try
            {
                if (selectableItemsView.SelectionMode == Microsoft.Maui.Controls.SelectionMode.Single)
                {
                    if (!Equals(selectableItemsView.SelectedItem, selectedItem))
                    {
                        selectableItemsView.SelectedItem = selectedItem;
                    }

                    // Execute command for Single selection
                    var parameter = selectableItemsView.SelectionChangedCommandParameter ?? selectedItem;
                    if (selectableItemsView.SelectionChangedCommand?.CanExecute(parameter) == true)
                    {
                        selectableItemsView.SelectionChangedCommand.Execute(parameter);
                    }
                }
                else if (selectableItemsView.SelectionMode == Microsoft.Maui.Controls.SelectionMode.Multiple)
                {
                    var virtualSelectedItems = selectableItemsView.SelectedItems;
                    if (virtualSelectedItems != null)
                    {
                        SynchronizeSelectedItems(virtualSelectedItems, selectedItems ?? []);
                    }

                    // Execute command for Multiple selection (pass the list of items)
                    var parameter = selectableItemsView.SelectionChangedCommandParameter ?? virtualSelectedItems;
                    if (selectableItemsView.SelectionChangedCommand?.CanExecute(parameter) == true)
                    {
                        selectableItemsView.SelectionChangedCommand.Execute(parameter);
                    }
                }
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        });
    }

    internal static void SynchronizeSelectedItems(IList<object> target, IReadOnlyCollection<object> source)
    {
        var sourceSet = new HashSet<object>(source, ReferenceEqualityComparer.Instance);

        for (int i = target.Count - 1; i >= 0; i--)
        {
            if (!sourceSet.Contains(target[i]))
            {
                target.RemoveAt(i);
            }
        }

        var targetSet = new HashSet<object>(target, ReferenceEqualityComparer.Instance);

        foreach (var item in source)
        {
            if (!targetSet.Contains(item))
            {
                target.Add(item);
            }
        }
    }

    private void OnRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Use MAUI's built-in method which fires both the event and command
            VirtualView?.SendRemainingItemsThresholdReached();
        });
    }

    /// <inheritdoc/>
    public override bool NeedsContainer => false;

    /// <summary>Maps the ItemsSource property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapItemsSource(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemsSource(itemsView);
    }

    /// <summary>Maps the ItemTemplate property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapItemTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemTemplate(itemsView, handler);
    }

    /// <summary>Maps the EmptyView property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapEmptyView(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateEmptyView(itemsView, handler);
    }

    /// <summary>Maps the EmptyViewTemplate property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapEmptyViewTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateEmptyViewTemplate(itemsView, handler);
    }

    /// <summary>Maps the HorizontalScrollBarVisibility property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapHorizontalScrollBarVisibility(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateHorizontalScrollBarVisibility(itemsView);
    }

    /// <summary>Maps the VerticalScrollBarVisibility property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapVerticalScrollBarVisibility(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateVerticalScrollBarVisibility(itemsView);
    }

    /// <summary>Maps the ItemsLayout property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapItemsLayout(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemsLayout(itemsView);
    }

    /// <summary>Maps the ItemSizingStrategy property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapItemSizingStrategy(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemSizingStrategy(itemsView);
    }

    /// <summary>Maps the IsGrouped property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapIsGrouped(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateIsGrouped(itemsView);
    }

    /// <summary>Maps the GroupHeaderTemplate property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapGroupHeaderTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateGroupHeaderTemplate(itemsView, handler);
    }

    /// <summary>Maps the GroupFooterTemplate property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapGroupFooterTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateGroupFooterTemplate(itemsView, handler);
    }

    /// <summary>Maps the SelectedItem property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapSelectedItem(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateSelectedItem(itemsView);
    }

    /// <summary>Maps the SelectionMode property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapSelectionMode(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateSelectionMode(itemsView);

        if (itemsView is SelectableItemsView selectableItemsView &&
            selectableItemsView.SelectionMode != Microsoft.Maui.Controls.SelectionMode.None)
        {
            handler.PlatformView.UpdateSelectedItem(itemsView);
            handler.PlatformView.UpdateSelectedItems(itemsView);
        }
    }

    /// <summary>Maps the Header property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapHeader(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateHeader(itemsView, handler);
    }

    /// <summary>Maps the HeaderTemplate property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapHeaderTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateHeaderTemplate(itemsView, handler);
    }

    /// <summary>Maps the Footer property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapFooter(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateFooter(itemsView, handler);
    }

    /// <summary>Maps the FooterTemplate property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapFooterTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateFooterTemplate(itemsView, handler);
    }

    /// <summary>Maps the SelectedItems property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapSelectedItems(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateSelectedItems(itemsView);
    }

    /// <summary>Maps the ItemsUpdatingScrollMode property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapItemsUpdatingScrollMode(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemsUpdatingScrollMode(itemsView);
    }

    /// <summary>Maps the RemainingItemsThreshold property to the platform view.</summary>
    /// <param name="handler">The handler for the collection view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapRemainingItemsThreshold(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateRemainingItemsThreshold(itemsView);
    }

    private void OnScrollToRequested(object? sender, ScrollToRequestEventArgs request)
    {
        if (PlatformView == null)
            return;

        if (request.Mode == ScrollToMode.Position)
        {
            PlatformView.ScrollTo(request.Index, request.GroupIndex, request.ScrollToPosition, request.IsAnimated);
        }
        else
        {
            PlatformView.ScrollTo(request.Item, request.Group, request.ScrollToPosition, request.IsAnimated);
        }
    }
}
