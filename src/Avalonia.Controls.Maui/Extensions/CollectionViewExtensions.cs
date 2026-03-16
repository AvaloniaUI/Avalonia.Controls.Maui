using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Maui.Platform;
using Avalonia.VisualTree;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System;
using System.Collections;
using Avalonia.Controls.Maui.Handlers;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping MAUI CollectionView properties to Avalonia CollectionView.
/// </summary>
public static class CollectionViewExtensions
{
    /// <summary>
    /// Updates the items source of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateItemsSource(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        platformView.ItemsSource = itemsView.ItemsSource;
    }

    /// <summary>
    /// Updates the item template of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The collection view handler for context.</param>
    public static void UpdateItemTemplate(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView, CollectionViewHandler handler)
    {
        if (itemsView.ItemTemplate != null)
        {
            var avaloniaTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                var mauiView = itemsView.ItemTemplate.CreateContent() as Microsoft.Maui.Controls.View;
                if (mauiView == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                mauiView.BindingContext = item;

                // Establish MAUI parent chain so RelativeSource AncestorType bindings
                // can walk up to find the ViewModel (e.g. for TapGestureRecognizer commands).
                // Native MAUI CollectionView platforms do this via AddLogicalChild in their cell containers.
                if (itemsView is Microsoft.Maui.Controls.Element parentElement)
                {
                    parentElement.AddLogicalChild(mauiView);
                }

                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);

                // Store the MAUI view in the Tag so we can access it for Visual State updates
                if (platformControl != null)
                {
                    platformControl.Tag = mauiView;
                    AttachLogicalChildCleanup(platformControl);
                }

                return platformControl ?? new TextBlock { Text = item?.ToString() ?? string.Empty };
            });

            platformView.ItemTemplate = avaloniaTemplate;
        }
        else
        {
            platformView.ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                new TextBlock { Text = item?.ToString() ?? string.Empty });
        }
    }

    /// <summary>
    /// Updates the empty view of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The collection view handler for context.</param>
    public static void UpdateEmptyView(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView, CollectionViewHandler handler)
    {
        if (itemsView.EmptyView is Microsoft.Maui.Controls.View emptyView)
        {
            _ = handler.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null");
            var platformControl = (Control)emptyView.ToPlatform(handler.MauiContext);
            platformView.EmptyView = platformControl;
        }
        else if (itemsView.EmptyView is string emptyText)
        {
            platformView.EmptyView = emptyText;
        }
        else
        {
            platformView.EmptyView = itemsView.EmptyView;
        }
    }

    /// <summary>
    /// Updates the empty view template of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The collection view handler for context.</param>
    public static void UpdateEmptyViewTemplate(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView, CollectionViewHandler handler)
    {
        if (itemsView.EmptyViewTemplate != null)
        {
            var avaloniaTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null)
                    return new TextBlock { Text = "No items" };

                var mauiView = itemsView.EmptyViewTemplate.CreateContent() as Microsoft.Maui.Controls.View;
                if (mauiView == null)
                    return new TextBlock { Text = "No items" };

                mauiView.BindingContext = item;
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                if (platformControl != null)
                {
                    AttachLogicalChildCleanup(platformControl);
                }
                return platformControl ?? new TextBlock { Text = "No items" };
            });

            platformView.EmptyViewTemplate = avaloniaTemplate;
        }
    }

    /// <summary>
    /// Updates the horizontal scroll bar visibility of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateHorizontalScrollBarVisibility(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        platformView.HorizontalScrollBarVisibility = itemsView.HorizontalScrollBarVisibility switch
        {
            Microsoft.Maui.ScrollBarVisibility.Always => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
            Microsoft.Maui.ScrollBarVisibility.Never => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden,
            Microsoft.Maui.ScrollBarVisibility.Default => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            _ => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    /// <summary>
    /// Updates the vertical scroll bar visibility of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateVerticalScrollBarVisibility(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        platformView.VerticalScrollBarVisibility = itemsView.VerticalScrollBarVisibility switch
        {
            Microsoft.Maui.ScrollBarVisibility.Always => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
            Microsoft.Maui.ScrollBarVisibility.Never => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden,
            Microsoft.Maui.ScrollBarVisibility.Default => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            _ => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    /// <summary>
    /// Updates the items layout of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateItemsLayout(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (itemsView is Microsoft.Maui.Controls.StructuredItemsView structuredItemsView)
        {
            platformView.ItemsLayout = structuredItemsView.ItemsLayout;
        }
    }

    /// <summary>
    /// Updates the item sizing strategy of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateItemSizingStrategy(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (itemsView is Microsoft.Maui.Controls.StructuredItemsView structuredItemsView)
        {
            platformView.ItemSizingStrategy = structuredItemsView.ItemSizingStrategy;
        }
    }

    /// <summary>
    /// Updates the is grouped property of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateIsGrouped(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (itemsView is Microsoft.Maui.Controls.GroupableItemsView groupableItemsView)
        {
            platformView.IsGrouped = groupableItemsView.IsGrouped;
        }
    }

    /// <summary>
    /// Updates the group header template of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The collection view handler for context.</param>
    public static void UpdateGroupHeaderTemplate(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView, CollectionViewHandler handler)
    {
        if (itemsView is Microsoft.Maui.Controls.GroupableItemsView groupableItemsView &&
            groupableItemsView.GroupHeaderTemplate != null)
        {
            var avaloniaTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null)
                    return new TextBlock { Text = "Group Header" };

                var mauiView = groupableItemsView.GroupHeaderTemplate.CreateContent() as Microsoft.Maui.Controls.View;
                if (mauiView == null)
                    return new TextBlock { Text = "Group Header" };

                mauiView.BindingContext = item;
                if (itemsView is Microsoft.Maui.Controls.Element parentElement)
                {
                    parentElement.AddLogicalChild(mauiView);
                }
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                if (platformControl != null)
                {
                    platformControl.Tag = mauiView;
                    AttachLogicalChildCleanup(platformControl);
                }
                return platformControl ?? new TextBlock { Text = "Group Header" };
            });

            platformView.GroupHeaderTemplate = avaloniaTemplate;
        }
    }

    /// <summary>
    /// Updates the group footer template of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The collection view handler for context.</param>
    public static void UpdateGroupFooterTemplate(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView, CollectionViewHandler handler)
    {
        if (itemsView is Microsoft.Maui.Controls.GroupableItemsView groupableItemsView &&
            groupableItemsView.GroupFooterTemplate != null)
        {
            var avaloniaTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null)
                    return new TextBlock { Text = "Group Footer" };

                var mauiView = groupableItemsView.GroupFooterTemplate.CreateContent() as Microsoft.Maui.Controls.View;
                if (mauiView == null)
                    return new TextBlock { Text = "Group Footer" };

                mauiView.BindingContext = item;
                if (itemsView is Microsoft.Maui.Controls.Element parentElement)
                {
                    parentElement.AddLogicalChild(mauiView);
                }
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                if (platformControl != null)
                {
                    platformControl.Tag = mauiView;
                    AttachLogicalChildCleanup(platformControl);
                }
                return platformControl ?? new TextBlock { Text = "Group Footer" };
            });

            platformView.GroupFooterTemplate = avaloniaTemplate;
        }
    }

    /// <summary>
    /// Updates the selected item of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateSelectedItem(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (itemsView is Microsoft.Maui.Controls.SelectableItemsView selectableItemsView)
        {
            if (!Equals(platformView.SelectedItem, selectableItemsView.SelectedItem))
            {
                platformView.SelectedItem = selectableItemsView.SelectedItem;
            }
        }
    }

    /// <summary>
    /// Updates the selection mode of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateSelectionMode(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (itemsView is Microsoft.Maui.Controls.SelectableItemsView selectableItemsView)
        {
            platformView.SelectionMode = selectableItemsView.SelectionMode;

            if (selectableItemsView.SelectionMode == Microsoft.Maui.Controls.SelectionMode.None)
            {
                platformView.SelectedItem = null;
            }
        }
    }

    /// <summary>
    /// Updates the header content of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The collection view handler for context.</param>
    public static void UpdateHeader(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView, CollectionViewHandler handler)
    {
        if (itemsView is Microsoft.Maui.Controls.StructuredItemsView structuredItemsView)
        {
            if (structuredItemsView.Header is Microsoft.Maui.Controls.View headerView)
            {
                _ = handler.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null");
                headerView.BindingContext = structuredItemsView.BindingContext;
                if (itemsView is Microsoft.Maui.Controls.Element parentElement)
                {
                    parentElement.AddLogicalChild(headerView);
                }
                var platformControl = (Control)headerView.ToPlatform(handler.MauiContext);
                AttachLogicalChildCleanup(platformControl);
                platformView.Header = platformControl;
            }
            else if (structuredItemsView.Header is string headerText)
            {
                platformView.Header = headerText;
            }
            else
            {
                platformView.Header = structuredItemsView.Header;
            }
        }
    }

    /// <summary>
    /// Updates the header template of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The collection view handler for context.</param>
    public static void UpdateHeaderTemplate(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView, CollectionViewHandler handler)
    {
        if (itemsView is Microsoft.Maui.Controls.StructuredItemsView structuredItemsView &&
            structuredItemsView.HeaderTemplate != null)
        {
            var avaloniaTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null)
                    return new TextBlock { Text = "Header" };

                var mauiView = structuredItemsView.HeaderTemplate.CreateContent() as Microsoft.Maui.Controls.View;
                if (mauiView == null)
                    return new TextBlock { Text = "Header" };

                mauiView.BindingContext = item;
                if (itemsView is Microsoft.Maui.Controls.Element parentElement)
                {
                    parentElement.AddLogicalChild(mauiView);
                }
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                if (platformControl != null)
                {
                    platformControl.Tag = mauiView;
                    AttachLogicalChildCleanup(platformControl);
                }
                return platformControl ?? new TextBlock { Text = "Header" };
            });

            platformView.HeaderTemplate = avaloniaTemplate;
        }
    }

    /// <summary>
    /// Updates the footer content of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The collection view handler for context.</param>
    public static void UpdateFooter(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView, CollectionViewHandler handler)
    {
        if (itemsView is Microsoft.Maui.Controls.StructuredItemsView structuredItemsView)
        {
            if (structuredItemsView.Footer is Microsoft.Maui.Controls.View footerView)
            {
                _ = handler.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null");
                footerView.BindingContext = structuredItemsView.BindingContext;
                if (itemsView is Microsoft.Maui.Controls.Element parentElement)
                {
                    parentElement.AddLogicalChild(footerView);
                }
                var platformControl = (Control)footerView.ToPlatform(handler.MauiContext);
                AttachLogicalChildCleanup(platformControl);
                platformView.Footer = platformControl;
            }
            else if (structuredItemsView.Footer is string footerText)
            {
                platformView.Footer = footerText;
            }
            else
            {
                platformView.Footer = structuredItemsView.Footer;
            }
        }
    }

    /// <summary>
    /// Updates the footer template of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The collection view handler for context.</param>
    public static void UpdateFooterTemplate(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView, CollectionViewHandler handler)
    {
        if (itemsView is Microsoft.Maui.Controls.StructuredItemsView structuredItemsView &&
            structuredItemsView.FooterTemplate != null)
        {
            var avaloniaTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null)
                    return new TextBlock { Text = "Footer" };

                var mauiView = structuredItemsView.FooterTemplate.CreateContent() as Microsoft.Maui.Controls.View;
                if (mauiView == null)
                    return new TextBlock { Text = "Footer" };

                mauiView.BindingContext = item;
                if (itemsView is Microsoft.Maui.Controls.Element parentElement)
                {
                    parentElement.AddLogicalChild(mauiView);
                }
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                if (platformControl != null)
                {
                    platformControl.Tag = mauiView;
                    AttachLogicalChildCleanup(platformControl);
                }
                return platformControl ?? new TextBlock { Text = "Footer" };
            });

            platformView.FooterTemplate = avaloniaTemplate;
        }
    }

    /// <summary>
    /// Updates the selected items of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateSelectedItems(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (itemsView is Microsoft.Maui.Controls.SelectableItemsView selectableItemsView)
        {
            platformView.SelectedItems = selectableItemsView.SelectedItems;
        }
    }

    /// <summary>
    /// Updates the items updating scroll mode of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateItemsUpdatingScrollMode(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        platformView.ItemsUpdatingScrollMode = itemsView.ItemsUpdatingScrollMode;
    }

    /// <summary>
    /// Updates the remaining items threshold of the collection view.
    /// </summary>
    /// <param name="platformView">The platform collection view.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    public static void UpdateRemainingItemsThreshold(this MauiCollectionView platformView, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        platformView.RemainingItemsThreshold = itemsView.RemainingItemsThreshold;
    }

    private static void AttachLogicalChildCleanup(Control platformControl)
    {
        platformControl.DetachedFromVisualTree -= OnPlatformControlDetachedFromVisualTree;
        platformControl.DetachedFromVisualTree += OnPlatformControlDetachedFromVisualTree;
    }

    private static void OnPlatformControlDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
        {
            MauiCollectionView.CleanupLogicalChild(control);
        }
    }
}
