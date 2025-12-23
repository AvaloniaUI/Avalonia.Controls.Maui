using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Collections.Specialized;

namespace Avalonia.Controls.Maui.Handlers;

public partial class TabbedPageHandler : ViewHandler<TabbedPage, TabControl>
{
    public static IPropertyMapper<TabbedPage, TabbedPageHandler> Mapper =
        new PropertyMapper<TabbedPage, TabbedPageHandler>(ViewMapper)
        {
            [nameof(TabbedPage.BarBackground)] = MapBarBackground,
            [nameof(TabbedPage.BarBackgroundColor)] = MapBarBackgroundColor,
            [nameof(TabbedPage.BarTextColor)] = MapBarTextColor,
            [nameof(TabbedPage.SelectedTabColor)] = MapSelectedTabColor,
            [nameof(TabbedPage.UnselectedTabColor)] = MapUnselectedTabColor,
            [nameof(TabbedPage.CurrentPage)] = MapCurrentPage,
            [nameof(TabbedPage.ItemsSource)] = MapItemsSource,
            [nameof(TabbedPage.ItemTemplate)] = MapItemTemplate,
            [nameof(MultiPage<Page>.SelectedItem)] = MapSelectedItem,
        };

    public static CommandMapper<TabbedPage, TabbedPageHandler> CommandMapper =
        new(ViewCommandMapper);
    
    public TabbedPageHandler() : base(Mapper, CommandMapper)
    {
    }

    public TabbedPageHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public TabbedPageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override TabControl CreatePlatformView()
    {
        if (VirtualView == null)
        {
            throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a TabControl");
        }

        var tabControl = new TabControl
        {
            TabStripPlacement = Dock.Top
        };

        // Add TabbedPage class for custom styling
        tabControl.Classes.Add("TabbedPage");

        tabControl.SelectionChanged += OnTabSelectionChanged;

        return tabControl;
    }

    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

        // Subscribe to children changes
        VirtualView.PagesChanged += OnPagesChanged;

        // Load initial pages - this is needed since Children is not a mapped property
        PlatformView.UpdateChildren(VirtualView, MauiContext);
    }
    
    protected override void DisconnectHandler(TabControl platformView)
    {
        VirtualView.PagesChanged -= OnPagesChanged;

        platformView.SelectionChanged -= OnTabSelectionChanged;

        base.DisconnectHandler(platformView);
    }

    private void OnPagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        PlatformView.UpdateChildren(VirtualView, MauiContext);
    }
    
    public static void MapBarBackground(TabbedPageHandler handler, TabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateBarBackground(tabbedPage);
    }
    
    public static void MapBarBackgroundColor(TabbedPageHandler handler, TabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateBarBackgroundColor(tabbedPage);
    }
    
    public static void MapBarTextColor(TabbedPageHandler handler, TabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateBarTextColor(tabbedPage);
    }

    public static void MapSelectedTabColor(TabbedPageHandler handler, TabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateSelectedTabColor(tabbedPage);
    }
    
    public static void MapUnselectedTabColor(TabbedPageHandler handler, TabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateUnselectedTabColor(tabbedPage);
    }

    public static void MapCurrentPage(TabbedPageHandler handler, TabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateCurrentPage(tabbedPage);
    }
    public static void MapItemsSource(TabbedPageHandler handler, TabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateChildren(tabbedPage, handler.MauiContext);
    }

    public static void MapItemTemplate(TabbedPageHandler handler, TabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateChildren(tabbedPage, handler.MauiContext);
    }

    public static void MapSelectedItem(TabbedPageHandler handler, TabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateSelectedItem(tabbedPage);
    }
    
    private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedIndex = PlatformView.SelectedIndex;
        if (selectedIndex >= 0 && selectedIndex < VirtualView.Children.Count)
        {
            var selectedPage = VirtualView.Children[selectedIndex];
            if (VirtualView.CurrentPage != selectedPage)
            {
                VirtualView.CurrentPage = selectedPage;
            }
        }
        
        // Re-apply tab colors when selection changes to update Selected/Unselected states
        PlatformView.UpdateTabColors(VirtualView);
    }
}