using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Handlers;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Extension methods for mapping MAUI ListView properties to Avalonia MauiListView.
/// </summary>
public static class ListViewExtensions
{
    /// <summary>
    /// Updates the items source of the list view.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    public static void UpdateItemsSource(this MauiListView platformView, ListView listView)
    {
        platformView.ListBox.ItemsSource = listView.ItemsSource;
        platformView.OnItemsSourceChanged();
    }

    /// <summary>
    /// Updates the item template of the list view.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    /// <param name="handler">The list view handler for context.</param>
    public static void UpdateItemTemplate(this MauiListView platformView, ListView listView, ListViewHandler handler)
    {
        platformView.ListBox.ItemTemplate = new FuncDataTemplate<object>((item, _) =>
        {
            if (handler.MauiContext == null)
                return new TextBlock { Text = item?.ToString() ?? string.Empty };

            // Handle Group Headers
            if (item is MauiListView.GroupHeader groupHeader)
            {
                if (platformView.GroupHeaderTemplate != null)
                {
                    return platformView.GroupHeaderTemplate.Build(groupHeader.Data);
                }
                
                return new TextBlock 
                { 
                    Text = groupHeader.Data?.ToString() ?? string.Empty,
                    FontWeight = Media.FontWeight.Bold,
                    Padding = new Thickness(10, 5)
                };
            }

            // Handle regular items
            DataTemplate? actualTemplate = listView.ItemTemplate;
            if (actualTemplate is DataTemplateSelector selector)
            {
                actualTemplate = selector.SelectTemplate(item, listView);
            }

            Control? content = null;

            if (actualTemplate != null)
            {
                var templateContent = actualTemplate.CreateContent();
                
                if (templateContent is ViewCell viewCell)
                {
                    if (viewCell.View != null)
                    {
                        viewCell.BindingContext = item;
                        viewCell.View.BindingContext = item;
                        viewCell.View.Parent = viewCell;
                        viewCell.Parent = listView;
                        content = (Control)viewCell.View.ToPlatform(handler.MauiContext);

                        if (viewCell.HasContextActions)
                        {
                            var contextMenu = new ContextMenu();
                            foreach (var action in viewCell.ContextActions)
                            {
                                var menuItem = new MenuItem
                                {
                                    Header = action.Text,
                                };

                                var capturedAction = action;
                                menuItem.Click += (s, e) =>
                                {
                                    if (capturedAction.Command?.CanExecute(capturedAction.CommandParameter) == true)
                                    {
                                        capturedAction.Command.Execute(capturedAction.CommandParameter);
                                    }
                                };
                                
                                contextMenu.Items.Add(menuItem);
                            }
                            content.ContextMenu = contextMenu;
                        }
                    }
                }

                if (content == null && templateContent is Cell cell)
                {
                    cell.BindingContext = item;
                    cell.Parent = listView;
                    var cellHandler = cell.ToHandler(handler.MauiContext);
                    if (cellHandler?.PlatformView is Control cellPlatformView)
                    {
                         content = cellPlatformView;
                    }
                }

                if (content == null && templateContent is View mauiView)
                {
                    mauiView.BindingContext = item;
                    mauiView.Parent = listView;
                    content = (Control)mauiView.ToPlatform(handler.MauiContext);
                }
            }

            if (content == null)
            {
                content = new TextBlock 
                { 
                    Text = item?.ToString() ?? string.Empty,
                    Padding = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            if (platformView.SeparatorVisibility)
            {
                var grid = new Grid
                {
                    RowDefinitions = new RowDefinitions("*,Auto")
                };
                grid.Children.Add(content);
                
                var separator = new Shapes.Rectangle
                {
                    Height = 1,
                    Fill = platformView.SeparatorColor,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                Grid.SetRow(separator, 1);
                grid.Children.Add(separator);
                return grid;
            }

            return content;
        });
    }

    /// <summary>
    /// Updates the selected item of the list view.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    public static void UpdateSelectedItem(this MauiListView platformView, ListView listView)
    {
        if (!Equals(platformView.ListBox.SelectedItem, listView.SelectedItem))
        {
            platformView.ListBox.SelectedItem = listView.SelectedItem;
        }
    }

    /// <summary>
    /// Updates the selection mode of the list view.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    public static void UpdateSelectionMode(this MauiListView platformView, ListView listView)
    {
        platformView.ListBox.SelectionMode = listView.SelectionMode switch
        {
            Microsoft.Maui.Controls.ListViewSelectionMode.None => global::Avalonia.Controls.SelectionMode.Single,
            Microsoft.Maui.Controls.ListViewSelectionMode.Single => global::Avalonia.Controls.SelectionMode.Single,
            _ => global::Avalonia.Controls.SelectionMode.Single
        };

        if (listView.SelectionMode == ListViewSelectionMode.None)
        {
            platformView.ListBox.SelectedItems?.Clear();
            platformView.ListBox.SelectedItem = null;
        }
    }

    /// <summary>
    /// Updates the header content.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    /// <param name="handler">The list view handler for context.</param>
    public static void UpdateHeader(this MauiListView platformView, ListView listView, ListViewHandler handler)
    {
        if (listView.Header is View headerView)
        {
            _ = handler.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null");
            platformView.Header = headerView.ToPlatform(handler.MauiContext);
        }
        else
        {
            platformView.Header = listView.Header;
        }
    }

    /// <summary>
    /// Updates the header template.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    /// <param name="handler">The list view handler for context.</param>
    public static void UpdateHeaderTemplate(this MauiListView platformView, ListView listView, ListViewHandler handler)
    {
        if (listView.HeaderTemplate != null)
        {
            platformView.HeaderTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null) return new TextBlock { Text = "Header" };
                var mauiView = listView.HeaderTemplate.CreateContent() as View;
                
                if (mauiView == null)
                    return new TextBlock { Text = "Header" };
                
                mauiView.BindingContext = item;
                return (Control)mauiView.ToPlatform(handler.MauiContext);
            });
        }
        else
        {
            platformView.HeaderTemplate = null;
        }
        
        platformView.Header = listView.Header ?? string.Empty;
    }

    /// <summary>
    /// Updates the footer content.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    /// <param name="handler">The list view handler for context.</param>
    public static void UpdateFooter(this MauiListView platformView, ListView listView, ListViewHandler handler)
    {
        if (listView.Footer is View footerView)
        {
            _ = handler.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null");
            platformView.Footer = footerView.ToPlatform(handler.MauiContext);
        }
        else
        {
            platformView.Footer = listView.Footer;
        }
    }

    /// <summary>
    /// Updates the footer template.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    /// <param name="handler">The list view handler for context.</param>
    public static void UpdateFooterTemplate(this MauiListView platformView, ListView listView, ListViewHandler handler)
    {
        if (listView.FooterTemplate != null)
        {
            platformView.FooterTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null) return new TextBlock { Text = "Footer" };
                var mauiView = listView.FooterTemplate.CreateContent() as View;
             
                if (mauiView == null) 
                    return new TextBlock { Text = "Footer" };
                
                mauiView.BindingContext = item;
                return (Control)mauiView.ToPlatform(handler.MauiContext);
            });
        }
        else
        {
            platformView.FooterTemplate = null;
        }
        
        platformView.Footer = listView.Footer ?? string.Empty;
    }

    /// <summary>
    /// Updates separators.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    /// <param name="handler">The list view handler for context.</param>
    public static void UpdateSeparators(this MauiListView platformView, ListView listView, ListViewHandler handler)
    {
        platformView.SeparatorVisibility = listView.SeparatorVisibility == SeparatorVisibility.Default;
        if (listView.SeparatorColor != null)
        {
            platformView.SeparatorColor = listView.SeparatorColor.ToPlatform();
        }
        
        // Re-apply item template to reflect separator changes
        platformView.UpdateItemTemplate(listView, handler);
    }

    /// <summary>
    /// Updates row height.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    public static void UpdateRowHeight(this MauiListView platformView, ListView listView)
    {
        // Placeholder for future optimizations
    }

    /// <summary>
    /// Updates the grouping enabled state.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    public static void UpdateIsGroupingEnabled(this MauiListView platformView, ListView listView)
    {
        platformView.IsGroupingEnabled = listView.IsGroupingEnabled;
    }

    /// <summary>
    /// Updates the group header template.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    /// <param name="handler">The list view handler for context.</param>
    public static void UpdateGroupHeaderTemplate(this MauiListView platformView, ListView listView, ListViewHandler handler)
    {
        if (listView.GroupHeaderTemplate != null)
        {
            platformView.GroupHeaderTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null) 
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };
                
                DataTemplate? actualTemplate = listView.GroupHeaderTemplate;
                if (actualTemplate is DataTemplateSelector selector)
                {
                    actualTemplate = selector.SelectTemplate(item, listView);
                }

                if (actualTemplate != null)
                {
                     var templateContent = actualTemplate.CreateContent();
                     if (templateContent is ViewCell viewCell)
                     {
                         if (viewCell.View != null)
                         {
                             viewCell.View.BindingContext = item;
                             viewCell.View.Parent = viewCell;
                             viewCell.Parent = listView;
                             return (Control)viewCell.View.ToPlatform(handler.MauiContext);
                         }
                     }

                     if (templateContent is Cell cell)
                     {
                         cell.BindingContext = item;
                         cell.Parent = listView;
                         var cellHandler = cell.ToHandler(handler.MauiContext);
                         return cellHandler?.PlatformView as Control ?? new TextBlock { Text = "Cell Error" };
                     }
                     if (templateContent is View mauiView)
                     {
                         mauiView.BindingContext = item;
                         mauiView.Parent = listView;
                         return (Control)mauiView.ToPlatform(handler.MauiContext);
                     }
                }

                return new TextBlock { Text = item?.ToString() ?? string.Empty };
            });
        }
        else
        {
            platformView.GroupHeaderTemplate = null;
        }
    }

    /// <summary>
    /// Updates the horizontal scroll bar visibility.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    public static void UpdateHorizontalScrollBarVisibility(this MauiListView platformView, Microsoft.Maui.Controls.ListView listView)
    {
        platformView.HorizontalScrollBarVisibility = listView.HorizontalScrollBarVisibility switch
        {
            Microsoft.Maui.ScrollBarVisibility.Always => Primitives.ScrollBarVisibility.Visible,
            Microsoft.Maui.ScrollBarVisibility.Never => Primitives.ScrollBarVisibility.Hidden,
            _ => Primitives.ScrollBarVisibility.Disabled
        };
    }

    /// <summary>
    /// Updates the vertical scroll bar visibility.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    public static void UpdateVerticalScrollBarVisibility(this MauiListView platformView, ListView listView)
    {
        platformView.VerticalScrollBarVisibility = listView.VerticalScrollBarVisibility switch
        {
            Microsoft.Maui.ScrollBarVisibility.Always => Primitives.ScrollBarVisibility.Visible,
            Microsoft.Maui.ScrollBarVisibility.Never => Primitives.ScrollBarVisibility.Hidden,
            _ => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    /// <summary>
    /// Updates pull to refresh enabled state.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    public static void UpdatePullToRefreshEnabled(this MauiListView platformView, ListView listView)
    {
        platformView.IsPullToRefreshEnabled = listView.IsPullToRefreshEnabled;
    }

    /// <summary>
    /// Updates refreshing state.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    public static void UpdateIsRefreshing(this MauiListView platformView, ListView listView)
    {
        platformView.IsRefreshing = listView.IsRefreshing;
    }

    /// <summary>
    /// Updates refresh control color.
    /// </summary>
    /// <param name="platformView">The platform list view control.</param>
    /// <param name="listView">The cross-platform list view.</param>
    public static void UpdateRefreshControlColor(this MauiListView platformView, ListView listView)
    {
        if (listView.RefreshControlColor != null)
        {
            platformView.RefreshControlColor = listView.RefreshControlColor.ToPlatform();
        }
    }
}