using Microsoft.Maui.Handlers;
using System.Collections.Specialized;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Styling;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaTabbedPage = Avalonia.Controls.TabbedPage;
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

    /// <summary>Tabbed page used when multiple shell sections are displayed as tabs.</summary>
    internal AvaloniaTabbedPage? _tabbedPage;

    /// <summary>Maps shell sections to their wrapper content pages for tab display.</summary>
    internal Dictionary<ShellSection, Avalonia.Controls.ContentPage>? _sectionPageMap;

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
            // Create TabbedPage for multiple sections
            _tabbedPage = new AvaloniaTabbedPage
            {
                TabPlacement = Avalonia.Controls.TabPlacement.Bottom
            };

            _sectionPageMap = new Dictionary<ShellSection, Avalonia.Controls.ContentPage>();

            // Style to hide the tab strip when "hide-tabstrip" class is set.
            // TabbedPage template chain: TabbedPage -> TabControl#PART_TabControl -> Border#PART_TabStrip
            var hideTabStripStyle = new Styling.Style(x =>
                x.OfType<AvaloniaTabbedPage>().Class("hide-tabstrip").Template().OfType<TabControl>().Template().OfType<ItemsPresenter>());
            hideTabStripStyle.Setters.Add(
                new Styling.Setter(AvaloniaControl.IsVisibleProperty, false));
            _tabbedPage.Styles.Add(hideTabStripStyle);

            _tabbedPage.SelectionChanged += OnTabSelectionChanged;

            return _tabbedPage;
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

        if (_tabbedPage != null)
        {
            _tabbedPage.SelectionChanged -= OnTabSelectionChanged;
            _tabbedPage.Pages = new List<Avalonia.Controls.Page>();
            _sectionPageMap?.Clear();
        }

        // Clear content control to release any in-flight transition resources
        if (_contentControl != null)
        {
            _contentControl.PageTransition = null;
            _contentControl.Content = null;
        }

        // Disconnect the current section handler to release its event subscriptions and page references
        _currentSectionHandler?.VirtualView?.Handler?.DisconnectHandler();
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

    private void OnTabSelectionChanged(object? sender, Avalonia.Controls.PageSelectionChangedEventArgs e)
    {
        if (_isUpdatingTabs)
            return;

        if (_tabbedPage != null && VirtualView != null)
        {
            var selectedIndex = _tabbedPage.SelectedIndex;

            // Find the corresponding ShellSection from visible sections
            var sections = VirtualView is IShellItemController itemController2
                ? itemController2.GetItems()
                : VirtualView.Items;

            var visibleSections = sections.Where(s => s.IsVisible).ToList();

            if (selectedIndex >= 0 && selectedIndex < visibleSections.Count)
            {
                var section = visibleSections[selectedIndex];
                if (VirtualView is IShellItemController itemController)
                {
                    itemController.ProposeSection(section, true);
                }
            }

            this.UpdateTabAppearance(VirtualView);
        }
    }
}
