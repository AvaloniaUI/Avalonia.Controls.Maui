using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Threading;
using Avalonia.Controls.Maui.Extensions;

namespace Avalonia.Controls.Maui.Handlers;

public class CollectionViewHandler : ViewHandler<CollectionView, MauiCollectionView>
{
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

    public static CommandMapper<CollectionView, CollectionViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {

        };

    public CollectionViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public CollectionViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public CollectionViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override MauiCollectionView CreatePlatformView()
    {
        return new MauiCollectionView();
    }

    protected override void ConnectHandler(MauiCollectionView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SelectionChanged += OnSelectionChanged;
        platformView.RemainingItemsThresholdReached += OnRemainingItemsThresholdReached;

        if (VirtualView is ItemsView itemsView)
        {
            itemsView.ScrollToRequested += OnScrollToRequested;
        }
    }

    protected override void DisconnectHandler(MauiCollectionView platformView)
    {
        platformView.SelectionChanged -= OnSelectionChanged;
        platformView.RemainingItemsThresholdReached -= OnRemainingItemsThresholdReached;

        if (VirtualView is ItemsView itemsView)
        {
            itemsView.ScrollToRequested -= OnScrollToRequested;
        }

        base.DisconnectHandler(platformView);
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        if (VirtualView is not SelectableItemsView selectableItemsView)
            return;

        var selectedItem = PlatformView.SelectedItem;

        if (selectableItemsView.SelectionMode == Microsoft.Maui.Controls.SelectionMode.Single)
        {
            if (!Equals(selectableItemsView.SelectedItem, selectedItem))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var parameter = selectableItemsView.SelectionChangedCommandParameter ?? selectedItem;
                    if (selectableItemsView.SelectionChangedCommand?.CanExecute(parameter) == true)
                    {
                        selectableItemsView.SelectionChangedCommand.Execute(parameter);
                    }
                });
            }
        }
    }

    private void OnRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        // Use MAUI's built-in method which fires both the event and command
        VirtualView?.SendRemainingItemsThresholdReached();
    }

    public override bool NeedsContainer => false;

    public static void MapItemsSource(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemsSource(itemsView);
    }

    public static void MapItemTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemTemplate(itemsView, handler);
    }

    public static void MapEmptyView(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateEmptyView(itemsView, handler);
    }

    public static void MapEmptyViewTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateEmptyViewTemplate(itemsView, handler);
    }

    public static void MapHorizontalScrollBarVisibility(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateHorizontalScrollBarVisibility(itemsView);
    }

    public static void MapVerticalScrollBarVisibility(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateVerticalScrollBarVisibility(itemsView);
    }

    public static void MapItemsLayout(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemsLayout(itemsView);
    }

    public static void MapIsGrouped(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateIsGrouped(itemsView);
    }

    public static void MapGroupHeaderTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateGroupHeaderTemplate(itemsView, handler);
    }

    public static void MapGroupFooterTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateGroupFooterTemplate(itemsView, handler);
    }

    public static void MapSelectedItem(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateSelectedItem(itemsView);
    }

    public static void MapSelectionMode(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateSelectionMode(itemsView);
    }

    public static void MapHeader(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateHeader(itemsView, handler);
    }

    public static void MapHeaderTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateHeaderTemplate(itemsView, handler);
    }

    public static void MapFooter(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateFooter(itemsView, handler);
    }

    public static void MapFooterTemplate(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateFooterTemplate(itemsView, handler);
    }

    public static void MapSelectedItems(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateSelectedItems(itemsView);
    }

    public static void MapItemsUpdatingScrollMode(CollectionViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemsUpdatingScrollMode(itemsView);
    }

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