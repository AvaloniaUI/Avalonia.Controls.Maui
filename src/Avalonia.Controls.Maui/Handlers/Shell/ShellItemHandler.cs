using Microsoft.Maui.Handlers;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Layout;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaGrid = Avalonia.Controls.Grid;
using AvaloniaSelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;

namespace Avalonia.Controls.Maui.Handlers.Shell;

public partial class ShellItemHandler : ElementHandler<ShellItem, AvaloniaControl>
{
    public static IPropertyMapper<ShellItem, ShellItemHandler> Mapper =
        new PropertyMapper<ShellItem, ShellItemHandler>(ElementHandler.ElementMapper)
        {
            [nameof(ShellItem.CurrentItem)] = MapCurrentItem,
            [nameof(ShellItem.Items)] = MapItems,
            [nameof(ShellItem.Title)] = MapTitle,
        };

    public static CommandMapper<ShellItem, ShellItemHandler> CommandMapper =
        new CommandMapper<ShellItem, ShellItemHandler>(ElementHandler.ElementCommandMapper);

    TabControl? _tabControl;
    ContentControl? _contentControl;
    ShellSectionHandler? _currentSectionHandler;
    bool _showTabs;

    public ShellItemHandler() : base(Mapper, CommandMapper)
    {
    }

    public ShellItemHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override AvaloniaControl CreatePlatformElement()
    {
        // Determine tab visibility
        _showTabs = ShouldShowTabs();

        if (_showTabs)
        {
            // Create tab control for multiple sections
            _tabControl = new TabControl
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                TabStripPlacement = Dock.Top
            };
            _tabControl.SelectionChanged += OnTabSelectionChanged;
            return _tabControl;
        }
        else
        {
            // Single section displays content directly without a wrapper grid
            _contentControl = new ContentControl
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch
            };
            return _contentControl;
        }
    }

    protected override void ConnectHandler(AvaloniaControl platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is IShellItemController itemController)
        {
            itemController.ItemsCollectionChanged += OnItemsCollectionChanged;
        }

        UpdateTabs();
        UpdateCurrentItem();
    }

    protected override void DisconnectHandler(AvaloniaControl platformView)
    {
        if (VirtualView is IShellItemController itemController)
        {
            itemController.ItemsCollectionChanged -= OnItemsCollectionChanged;
        }

        if (_tabControl != null)
        {
            _tabControl.SelectionChanged -= OnTabSelectionChanged;
        }

        _currentSectionHandler = null;

        base.DisconnectHandler(platformView);
    }

    public static void MapCurrentItem(ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateCurrentItem();
    }

    public static void MapItems(ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabs();
    }

    public static void MapTitle(ShellItemHandler handler, ShellItem item)
    {
        // Title updates handled by parent shell
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateTabs();
    }

    private void OnTabSelectionChanged(object? sender, AvaloniaSelectionChangedEventArgs e)
    {
        if (_tabControl?.SelectedItem is TabItem tabItem &&
            tabItem.Tag is ShellSection section &&
            VirtualView != null)
        {
            if (VirtualView is IShellItemController itemController)
            {
                itemController.ProposeSection(section, true);
            }
        }
    }

    private bool ShouldShowTabs()
    {
        if (VirtualView is IShellItemController itemController)
        {
            return itemController.ShowTabs;
        }

        return VirtualView?.Items?.Count > 1;
    }

    private void UpdateTabs()
    {
        if (VirtualView == null)
            return;

        _showTabs = ShouldShowTabs();

        if (!_showTabs)
        {
            // Single section or no tabs display content directly
            UpdateCurrentItem();
            return;
        }

        if (_tabControl == null)
            return;

        _tabControl.Items.Clear();

        var sections = VirtualView is IShellItemController itemController
            ? itemController.GetItems()
            : VirtualView.Items;

        foreach (var section in sections)
        {
            if (!section.IsVisible)
                continue;

            var tabItem = new TabItem
            {
                Header = CreateTabHeader(section),
                Tag = section
            };

            _tabControl.Items.Add(tabItem);

            // Set selected if current
            if (section == VirtualView.CurrentItem)
            {
                _tabControl.SelectedItem = tabItem;
            }
        }
    }

    private object CreateTabHeader(ShellSection section)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        if (section.Icon != null)
        {
            // TODO: Implement proper icon rendering
            panel.Children.Add(new Avalonia.Controls.Border
            {
                Width = 16,
                Height = 16,
                Background = Avalonia.Media.Brushes.Gray
            });
        }

        if (!string.IsNullOrEmpty(section.Title))
        {
            panel.Children.Add(new TextBlock
            {
                Text = section.Title,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            });
        }

        return panel;
    }

    private void UpdateCurrentItem()
    {
        if (VirtualView?.CurrentItem == null || MauiContext == null)
            return;

        // Create handler for current section
        var handler = VirtualView.CurrentItem.ToHandler(MauiContext);
        _currentSectionHandler = handler as ShellSectionHandler;

        if (handler?.PlatformView is not AvaloniaControl control)
            return;

        if (_showTabs && _tabControl != null)
        {
            // Update tab content
            var selectedTab = _tabControl.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                selectedTab.Content = control;
            }
            else
            {
                // Find and select the correct tab
                foreach (var item in _tabControl.Items)
                {
                    if (item is TabItem tabItem && tabItem.Tag == VirtualView.CurrentItem)
                    {
                        _tabControl.SelectedItem = tabItem;
                        tabItem.Content = control;
                        break;
                    }
                }
            }
        }
        else if (_contentControl != null)
        {
            // Single section mode
            _contentControl.Content = control;
        }
    }

    /// <summary>
    /// Updates the visibility of the tab bar based on the Shell's TabBarIsVisible attached property.
    /// </summary>
    public void UpdateTabBarVisibility()
    {
        if (_tabControl == null || VirtualView == null)
            return;

        // Get the Shell from the parent hierarchy
        var shell = VirtualView.Parent as Microsoft.Maui.Controls.Shell;
        if (shell == null)
            return;

        // Check TabBarIsVisible for the current page
        var currentPage = shell.CurrentPage;
        var isVisible = currentPage != null
            ? Microsoft.Maui.Controls.Shell.GetTabBarIsVisible(currentPage)
            : true;

        // In Avalonia TabControl, we can hide the tab strip by setting TabStripPlacement
        // or by changing the template. For now, we'll use visibility on each tab item's header.
        // A full implementation would require a custom TabControl style.

        // For simplicity, we'll set the entire tab strip to visible/hidden
        // This requires accessing the tab strip part of the template
        _tabControl.IsVisible = isVisible || !_showTabs;
    }

    /// <summary>
    /// Updates the background color of the tab bar.
    /// </summary>
    public void UpdateTabBarBackgroundColor()
    {
        if (_tabControl == null || VirtualView == null)
            return;

        var shell = VirtualView.Parent as Microsoft.Maui.Controls.Shell;
        if (shell == null)
            return;

        // TabBarBackgroundColor is an attached property
        var color = Microsoft.Maui.Controls.Shell.GetTabBarBackgroundColor(shell);
        if (color != null)
        {
            _tabControl.Background = color.ToPlatform();
        }
        else
        {
            _tabControl.ClearValue(TabControl.BackgroundProperty);
        }
    }

    /// <summary>
    /// Updates the foreground color of the tab bar.
    /// </summary>
    public void UpdateTabBarForegroundColor()
    {
        if (_tabControl == null || VirtualView == null)
            return;

        var shell = VirtualView.Parent as Microsoft.Maui.Controls.Shell;
        if (shell == null)
            return;

        // TabBarForegroundColor is an attached property
        var color = Microsoft.Maui.Controls.Shell.GetTabBarForegroundColor(shell);
        if (color != null)
        {
            _tabControl.Foreground = color.ToPlatform();
        }
        else
        {
            _tabControl.ClearValue(TabControl.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the title color of the tab bar (active tab text color).
    /// </summary>
    public void UpdateTabBarTitleColor()
    {
        // TabBarTitleColor applies to the selected tab's text
        // This would require styling individual tab items
        // For now, this is a placeholder that could be enhanced with custom styling
    }

    /// <summary>
    /// Updates the disabled color of the tab bar.
    /// </summary>
    public void UpdateTabBarDisabledColor()
    {
        // TabBarDisabledColor applies to disabled tabs
        // This would require styling individual tab items when disabled
        // For now, this is a placeholder
    }

    /// <summary>
    /// Updates the unselected color of the tab bar (inactive tab text color).
    /// </summary>
    public void UpdateTabBarUnselectedColor()
    {
        // TabBarUnselectedColor applies to unselected tabs' text
        // This would require styling individual tab items
        // For now, this is a placeholder that could be enhanced with custom styling
    }
}
