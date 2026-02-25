using Microsoft.Maui.Handlers;
using System.Collections.Specialized;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Styling;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaSelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;
using Avalonia.Controls.Maui.Extensions;

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

    internal TabControl? _tabControl;
    internal TransitioningContentControl? _contentControl;
    internal ShellSectionHandler? _currentSectionHandler;
    internal int _previousSectionIndex = -1;
    internal bool _showTabs;
    internal bool _isUpdatingTabs;
    
    public ShellItemHandler() : base(Mapper, CommandMapper)
    {
    }
    
    public ShellItemHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override AvaloniaControl CreatePlatformElement()
    {
        // Determine if we should show tabs
        _showTabs = this.ShouldShowTabs(VirtualView);

        if (_showTabs)
        {
            // Create tab control for multiple sections
            _tabControl = new TabControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                TabStripPlacement = Dock.Top,
                Padding = new Thickness(0)
            };

            // Style to hide the tab strip (ItemsPresenter) when "hide-tabstrip" class is set
            var hideTabStripStyle = new Styling.Style(x =>
                x.OfType<TabControl>().Class("hide-tabstrip").Template().OfType<ItemsPresenter>());
            hideTabStripStyle.Setters.Add(
                new Styling.Setter(AvaloniaControl.IsVisibleProperty, false));
            _tabControl.Styles.Add(hideTabStripStyle);

            _tabControl.SelectionChanged += OnTabSelectionChanged;

            var selectedStyle = new Styling.Style(x => x.OfType<TabItem>().Class(":selected"));
            selectedStyle.Setters.Add(new Styling.Setter(TabItem.BackgroundProperty, new Markup.Xaml.MarkupExtensions.DynamicResourceExtension("ShellTabSelectedBackground")));
            selectedStyle.Setters.Add(new Styling.Setter(TabItem.ForegroundProperty, new Markup.Xaml.MarkupExtensions.DynamicResourceExtension("ShellTabSelectedForeground")));
            _tabControl.Styles.Add(selectedStyle);

            var hoverStyle = new Styling.Style(x => x.OfType<TabItem>().Class(":pointerover"));
            hoverStyle.Setters.Add(new Styling.Setter(TabItem.BackgroundProperty, new Markup.Xaml.MarkupExtensions.DynamicResourceExtension("ShellTabHoverBackground")));
            _tabControl.Styles.Add(hoverStyle);

            return _tabControl;
        }
        else
        {
            // Single section, show content directly without wrapper grid
            _contentControl = new TransitioningContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
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

        this.UpdateTabs(VirtualView);
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
        handler.UpdateCurrentItem(item);
    }
    
    public static void MapItems(ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabs(item);
    }

    public static void MapTitle(ShellItemHandler handler, ShellItem item)
    {
        // Title updates handled by parent shell
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (VirtualView != null)
            this.UpdateTabs(VirtualView);
    }

    private void OnTabSelectionChanged(object? sender, AvaloniaSelectionChangedEventArgs e)
    {
        if (_isUpdatingTabs)
            return;

        if (e.AddedItems.Count > 0)
        {
            var selectedItem = e.AddedItems[0];
            ShellSection? section = null;

            if (selectedItem is TabItem tabItem && VirtualView != null && _tabControl != null)
            {
                // Find the section associated with this TabItem (Store it in Tag or just match by index)
                var index = _tabControl.Items.IndexOf(tabItem);
                if (index >= 0 && index < VirtualView.Items.Count)
                {
                    section = VirtualView.Items[index];
                }
            }
            else if (selectedItem is ShellSection s)
            {
                section = s;
            }

            if (section != null && VirtualView is IShellItemController itemController)
            {
                itemController.ProposeSection(section, true);
            }

            if (VirtualView != null)
                this.UpdateTabAppearance(VirtualView);
        }
    }
}
