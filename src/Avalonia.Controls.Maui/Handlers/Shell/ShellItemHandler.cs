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

/// <summary>Avalonia handler for <see cref="ShellItem"/>.</summary>
public partial class ShellItemHandler : ElementHandler<ShellItem, AvaloniaControl>
{
    /// <summary>Property mapper for <see cref="ShellItemHandler"/>.</summary>
    public static IPropertyMapper<ShellItem, ShellItemHandler> Mapper =
        new PropertyMapper<ShellItem, ShellItemHandler>(ElementHandler.ElementMapper)
        {
            [nameof(ShellItem.CurrentItem)] = MapCurrentItem,
            [nameof(ShellItem.Items)] = MapItems,
            [nameof(ShellItem.Title)] = MapTitle,
        };

    /// <summary>Command mapper for <see cref="ShellItemHandler"/>.</summary>
    public static CommandMapper<ShellItem, ShellItemHandler> CommandMapper =
        new CommandMapper<ShellItem, ShellItemHandler>(ElementHandler.ElementCommandMapper);

    /// <summary>Tab control used when multiple shell sections are displayed as tabs.</summary>
    internal TabControl? _tabControl;

    /// <summary>Content control used when a single section is displayed without tabs.</summary>
    internal TransitioningContentControl? _contentControl;

    /// <summary>Handler for the currently active shell section.</summary>
    internal ShellSectionHandler? _currentSectionHandler;

    /// <summary>Index of the previously selected section for transition direction.</summary>
    internal int _previousSectionIndex = -1;

    /// <summary>Whether tabs should be shown for this shell item.</summary>
    internal bool _showTabs;

    /// <summary>Flag to prevent re-entrant tab update processing.</summary>
    internal bool _isUpdatingTabs;

    /// <summary>Initializes a new instance of <see cref="ShellItemHandler"/>.</summary>
    public ShellItemHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ShellItemHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    /// <param name="commandMapper">The command mapper to use.</param>
    public ShellItemHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
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
    
    /// <inheritdoc/>
    protected override void ConnectHandler(AvaloniaControl platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is IShellItemController itemController)
        {
            itemController.ItemsCollectionChanged += OnItemsCollectionChanged;
        }

        this.UpdateTabs(VirtualView);
    }

    /// <inheritdoc/>
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

        // Clear content control to release any in-flight transition resources
        if (_contentControl != null)
        {
            _contentControl.PageTransition = null;
            _contentControl.Content = null;
        }

        _currentSectionHandler = null;

        base.DisconnectHandler(platformView);
    }
    
    /// <summary>Maps the CurrentItem property to the platform view.</summary>
    /// <param name="handler">The shell item handler.</param>
    /// <param name="item">The MAUI ShellItem virtual view.</param>
    public static void MapCurrentItem(ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateCurrentItem(item);
    }

    /// <summary>Maps the Items property to the platform view.</summary>
    /// <param name="handler">The shell item handler.</param>
    /// <param name="item">The MAUI ShellItem virtual view.</param>
    public static void MapItems(ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabs(item);
    }

    /// <summary>Maps the Title property to the platform view.</summary>
    /// <param name="handler">The shell item handler.</param>
    /// <param name="item">The MAUI ShellItem virtual view.</param>
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
