using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections;
using Avalonia.Controls.Maui.Extensions;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI CollectionView to Avalonia CollectionView mapping
/// </summary>
public class CollectionViewHandler : ViewHandler<Microsoft.Maui.Controls.CollectionView, CollectionView>
{
    public static IPropertyMapper<Microsoft.Maui.Controls.ItemsView, CollectionViewHandler> Mapper =
        new PropertyMapper<Microsoft.Maui.Controls.ItemsView, CollectionViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.ItemsView.ItemsSource)] = MapItemsSource,
            [nameof(Microsoft.Maui.Controls.ItemsView.ItemTemplate)] = MapItemTemplate,
            [nameof(Microsoft.Maui.Controls.ItemsView.EmptyView)] = MapEmptyView,
            [nameof(Microsoft.Maui.Controls.ItemsView.EmptyViewTemplate)] = MapEmptyViewTemplate,
            [nameof(Microsoft.Maui.Controls.ItemsView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
            [nameof(Microsoft.Maui.Controls.ItemsView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
            [nameof(Microsoft.Maui.Controls.StructuredItemsView.ItemsLayout)] = MapItemsLayout,
            [nameof(Microsoft.Maui.Controls.StructuredItemsView.Header)] = MapHeader,
            [nameof(Microsoft.Maui.Controls.StructuredItemsView.HeaderTemplate)] = MapHeaderTemplate,
            [nameof(Microsoft.Maui.Controls.StructuredItemsView.Footer)] = MapFooter,
            [nameof(Microsoft.Maui.Controls.StructuredItemsView.FooterTemplate)] = MapFooterTemplate,
            [nameof(Microsoft.Maui.Controls.GroupableItemsView.IsGrouped)] = MapIsGrouped,
            [nameof(Microsoft.Maui.Controls.GroupableItemsView.GroupHeaderTemplate)] = MapGroupHeaderTemplate,
            [nameof(Microsoft.Maui.Controls.GroupableItemsView.GroupFooterTemplate)] = MapGroupFooterTemplate,
            [nameof(Microsoft.Maui.Controls.SelectableItemsView.SelectedItem)] = MapSelectedItem,
            [nameof(Microsoft.Maui.Controls.SelectableItemsView.SelectedItems)] = MapSelectedItems,
            [nameof(Microsoft.Maui.Controls.SelectableItemsView.SelectionMode)] = MapSelectionMode,
            [nameof(Microsoft.Maui.Controls.ItemsView.ItemsUpdatingScrollMode)] = MapItemsUpdatingScrollMode,
            [nameof(Microsoft.Maui.Controls.ItemsView.RemainingItemsThreshold)] = MapRemainingItemsThreshold,
        };

    public static CommandMapper<Microsoft.Maui.Controls.CollectionView, CollectionViewHandler> CommandMapper =
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

    protected override CollectionView CreatePlatformView()
    {
        return new CollectionView();
    }

    protected override void ConnectHandler(CollectionView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SelectionChanged += OnSelectionChanged;
        platformView.RemainingItemsThresholdReached += OnRemainingItemsThresholdReached;
    }

    protected override void DisconnectHandler(CollectionView platformView)
    {
        platformView.SelectionChanged -= OnSelectionChanged;
        platformView.RemainingItemsThresholdReached -= OnRemainingItemsThresholdReached;
        base.DisconnectHandler(platformView);
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        if (VirtualView is not Microsoft.Maui.Controls.SelectableItemsView selectableItemsView)
            return;

        var selectedItem = PlatformView.SelectedItem;

        if (selectableItemsView.SelectionMode == Microsoft.Maui.Controls.SelectionMode.Single)
        {
                if (!Equals(selectableItemsView.SelectedItem, selectedItem))
                {
                    selectableItemsView.SelectedItem = selectedItem;
                }
        }
    }

    private void OnRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        // Use MAUI's built-in method which fires both the event and command
        VirtualView?.SendRemainingItemsThresholdReached();
    }

    public override bool NeedsContainer => false;

    public static void MapItemsSource(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemsSource(itemsView);
    }

    public static void MapItemTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemTemplate(itemsView, handler);
    }

    public static void MapEmptyView(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateEmptyView(itemsView, handler);
    }

    public static void MapEmptyViewTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateEmptyViewTemplate(itemsView, handler);
    }

    public static void MapHorizontalScrollBarVisibility(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateHorizontalScrollBarVisibility(itemsView);
    }

    public static void MapVerticalScrollBarVisibility(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateVerticalScrollBarVisibility(itemsView);
    }

    public static void MapItemsLayout(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemsLayout(itemsView);
    }

    public static void MapIsGrouped(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateIsGrouped(itemsView);
    }

    public static void MapGroupHeaderTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateGroupHeaderTemplate(itemsView, handler);
    }

    public static void MapGroupFooterTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateGroupFooterTemplate(itemsView, handler);
    }

    public static void MapSelectedItem(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateSelectedItem(itemsView);
    }

    public static void MapSelectionMode(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateSelectionMode(itemsView);
    }

    public static void MapHeader(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateHeader(itemsView, handler);
    }

    public static void MapHeaderTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateHeaderTemplate(itemsView, handler);
    }

    public static void MapFooter(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateFooter(itemsView, handler);
    }

    public static void MapFooterTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateFooterTemplate(itemsView, handler);
    }

    public static void MapSelectedItems(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateSelectedItems(itemsView);
    }

    public static void MapItemsUpdatingScrollMode(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemsUpdatingScrollMode(itemsView);
    }

    public static void MapRemainingItemsThreshold(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        handler.PlatformView.UpdateRemainingItemsThreshold(itemsView);
    }
}