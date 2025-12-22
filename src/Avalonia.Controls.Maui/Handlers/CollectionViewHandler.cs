using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections;

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
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        handler.PlatformView.ItemsSource = itemsView.ItemsSource;
    }

    public static void MapItemTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (itemsView.ItemTemplate != null)
        {
            // Create an Avalonia DataTemplate from the MAUI DataTemplate
            var avaloniaTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                // Resolve the actual template to use
                DataTemplate? actualTemplate = itemsView.ItemTemplate;
                if (itemsView.ItemTemplate is DataTemplateSelector selector)
                {
                    actualTemplate = selector.SelectTemplate(item, handler.VirtualView);
                }

                if (actualTemplate == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                // Create MAUI view from template
                var mauiView = actualTemplate.CreateContent() as Microsoft.Maui.Controls.View;
                if (mauiView == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                // Set the binding context
                mauiView.BindingContext = item;

                // Convert to platform control
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                return platformControl ?? new TextBlock { Text = item?.ToString() ?? string.Empty };
            });

            handler.PlatformView.ItemTemplate = avaloniaTemplate;
        }
        else
        {
            // Default template - just show ToString()
            handler.PlatformView.ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                new TextBlock { Text = item?.ToString() ?? string.Empty });
        }
    }

    public static void MapEmptyView(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (itemsView.EmptyView is Microsoft.Maui.Controls.View emptyView)
        {
            _ = handler.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null");
            var platformControl = (Control)emptyView.ToPlatform(handler.MauiContext);
            handler.PlatformView.EmptyView = platformControl;
        }
        else if (itemsView.EmptyView is string emptyText)
        {
            handler.PlatformView.EmptyView = emptyText;
        }
        else
        {
            handler.PlatformView.EmptyView = itemsView.EmptyView;
        }
    }

    public static void MapEmptyViewTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

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
                return platformControl ?? new TextBlock { Text = "No items" };
            });

            handler.PlatformView.EmptyViewTemplate = avaloniaTemplate;
        }
    }

    public static void MapHorizontalScrollBarVisibility(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        handler.PlatformView.HorizontalScrollBarVisibility = itemsView.HorizontalScrollBarVisibility switch
        {
            Microsoft.Maui.ScrollBarVisibility.Always => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
            Microsoft.Maui.ScrollBarVisibility.Never => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden,
            Microsoft.Maui.ScrollBarVisibility.Default => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            _ => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    public static void MapVerticalScrollBarVisibility(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        handler.PlatformView.VerticalScrollBarVisibility = itemsView.VerticalScrollBarVisibility switch
        {
            Microsoft.Maui.ScrollBarVisibility.Always => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
            Microsoft.Maui.ScrollBarVisibility.Never => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden,
            Microsoft.Maui.ScrollBarVisibility.Default => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            _ => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    public static void MapItemsLayout(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (itemsView is Microsoft.Maui.Controls.StructuredItemsView structuredItemsView)
        {
            handler.PlatformView.ItemsLayout = structuredItemsView.ItemsLayout;
        }
    }

    public static void MapIsGrouped(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (itemsView is Microsoft.Maui.Controls.GroupableItemsView groupableItemsView)
        {
            handler.PlatformView.IsGrouped = groupableItemsView.IsGrouped;
        }
    }

    public static void MapGroupHeaderTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

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
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                return platformControl ?? new TextBlock { Text = "Group Header" };
            });

            handler.PlatformView.GroupHeaderTemplate = avaloniaTemplate;
        }
    }

    public static void MapGroupFooterTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

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
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                return platformControl ?? new TextBlock { Text = "Group Footer" };
            });

            handler.PlatformView.GroupFooterTemplate = avaloniaTemplate;
        }
    }

    public static void MapSelectedItem(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (itemsView is Microsoft.Maui.Controls.SelectableItemsView selectableItemsView)
        {
            // Only update if different to prevent feedback loops
            if (!Equals(handler.PlatformView.SelectedItem, selectableItemsView.SelectedItem))
            {
                handler.PlatformView.SelectedItem = selectableItemsView.SelectedItem;
            }
        }
    }

    public static void MapSelectionMode(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (itemsView is Microsoft.Maui.Controls.SelectableItemsView selectableItemsView)
        {
            handler.PlatformView.SelectionMode = selectableItemsView.SelectionMode switch
            {
                Microsoft.Maui.Controls.SelectionMode.None => global::Avalonia.Controls.SelectionMode.Single,
                Microsoft.Maui.Controls.SelectionMode.Single => global::Avalonia.Controls.SelectionMode.Single,
                Microsoft.Maui.Controls.SelectionMode.Multiple => global::Avalonia.Controls.SelectionMode.Multiple,
                _ => global::Avalonia.Controls.SelectionMode.Single
            };
        }
    }

    public static void MapHeader(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (itemsView is Microsoft.Maui.Controls.StructuredItemsView structuredItemsView)
        {
            if (structuredItemsView.Header is Microsoft.Maui.Controls.View headerView)
            {
                _ = handler.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null");
                var platformControl = (Control)headerView.ToPlatform(handler.MauiContext);
                handler.PlatformView.Header = platformControl;
            }
            else if (structuredItemsView.Header is string headerText)
            {
                handler.PlatformView.Header = headerText;
            }
            else
            {
                handler.PlatformView.Header = structuredItemsView.Header;
            }
        }
    }

    public static void MapHeaderTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

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
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                return platformControl ?? new TextBlock { Text = "Header" };
            });

            handler.PlatformView.HeaderTemplate = avaloniaTemplate;
        }
    }

    public static void MapFooter(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (itemsView is Microsoft.Maui.Controls.StructuredItemsView structuredItemsView)
        {
            if (structuredItemsView.Footer is Microsoft.Maui.Controls.View footerView)
            {
                _ = handler.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null");
                var platformControl = (Control)footerView.ToPlatform(handler.MauiContext);
                handler.PlatformView.Footer = platformControl;
            }
            else if (structuredItemsView.Footer is string footerText)
            {
                handler.PlatformView.Footer = footerText;
            }
            else
            {
                handler.PlatformView.Footer = structuredItemsView.Footer;
            }
        }
    }

    public static void MapFooterTemplate(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

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
                var platformControl = (Control)mauiView.ToPlatform(handler.MauiContext);
                return platformControl ?? new TextBlock { Text = "Footer" };
            });

            handler.PlatformView.FooterTemplate = avaloniaTemplate;
        }
    }

    public static void MapSelectedItems(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (itemsView is Microsoft.Maui.Controls.SelectableItemsView selectableItemsView)
        {
            handler.PlatformView.SelectedItems = selectableItemsView.SelectedItems;
        }
    }

    public static void MapItemsUpdatingScrollMode(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        handler.PlatformView.ItemsUpdatingScrollMode = itemsView.ItemsUpdatingScrollMode;
    }

    public static void MapRemainingItemsThreshold(CollectionViewHandler handler, Microsoft.Maui.Controls.ItemsView itemsView)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        handler.PlatformView.RemainingItemsThreshold = itemsView.RemainingItemsThreshold;
    }
}