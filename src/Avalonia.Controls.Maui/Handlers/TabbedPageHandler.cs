using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Collections.Specialized;
using AvaloniaTabbedPage = Avalonia.Controls.TabbedPage;
using MauiTabbedPage = Microsoft.Maui.Controls.TabbedPage;
using MauiPage = Microsoft.Maui.Controls.Page;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="MauiTabbedPage"/>.</summary>
public partial class TabbedPageHandler : ViewHandler<MauiTabbedPage, AvaloniaTabbedPage>
{
    /// <summary>Property mapper for <see cref="TabbedPageHandler"/>.</summary>
    public static IPropertyMapper<MauiTabbedPage, TabbedPageHandler> Mapper =
        new PropertyMapper<MauiTabbedPage, TabbedPageHandler>(ViewMapper)
        {
            [nameof(MauiTabbedPage.BarBackground)] = MapBarBackground,
            [nameof(MauiTabbedPage.BarBackgroundColor)] = MapBarBackgroundColor,
            [nameof(MauiTabbedPage.BarTextColor)] = MapBarTextColor,
            [nameof(MauiTabbedPage.SelectedTabColor)] = MapSelectedTabColor,
            [nameof(MauiTabbedPage.UnselectedTabColor)] = MapUnselectedTabColor,
            [nameof(MauiTabbedPage.CurrentPage)] = MapCurrentPage,
            [nameof(MauiTabbedPage.ItemsSource)] = MapItemsSource,
            [nameof(MauiTabbedPage.ItemTemplate)] = MapItemTemplate,
            [nameof(MultiPage<MauiPage>.SelectedItem)] = MapSelectedItem,
        };

    /// <summary>Command mapper for <see cref="TabbedPageHandler"/>.</summary>
    public static CommandMapper<MauiTabbedPage, TabbedPageHandler> CommandMapper =
        new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="TabbedPageHandler"/>.</summary>
    public TabbedPageHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="TabbedPageHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public TabbedPageHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="TabbedPageHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public TabbedPageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override AvaloniaTabbedPage CreatePlatformView()
    {
        if (VirtualView == null)
        {
            throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a TabbedPage");
        }

        var tabbedPage = new AvaloniaTabbedPage();

        tabbedPage.SelectionChanged += OnTabSelectionChanged;

        return tabbedPage;
    }

    /// <inheritdoc/>
    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

        // Subscribe to children changes
        VirtualView.PagesChanged += OnPagesChanged;

        // Load initial pages
        PlatformView.UpdateChildren(VirtualView, MauiContext);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(AvaloniaTabbedPage platformView)
    {
        VirtualView.PagesChanged -= OnPagesChanged;

        platformView.SelectionChanged -= OnTabSelectionChanged;

        base.DisconnectHandler(platformView);
    }

    private void OnPagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        PlatformView.UpdateChildren(VirtualView, MauiContext);
    }

    /// <summary>Maps the BarBackground property to the platform view.</summary>
    /// <param name="handler">The handler for the tabbed page.</param>
    /// <param name="tabbedPage">The virtual tabbed page.</param>
    public static void MapBarBackground(TabbedPageHandler handler, MauiTabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateBarBackground(tabbedPage);
    }

    /// <summary>Maps the BarBackgroundColor property to the platform view.</summary>
    /// <param name="handler">The handler for the tabbed page.</param>
    /// <param name="tabbedPage">The virtual tabbed page.</param>
    public static void MapBarBackgroundColor(TabbedPageHandler handler, MauiTabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateBarBackgroundColor(tabbedPage);
    }

    /// <summary>Maps the BarTextColor property to the platform view.</summary>
    /// <param name="handler">The handler for the tabbed page.</param>
    /// <param name="tabbedPage">The virtual tabbed page.</param>
    public static void MapBarTextColor(TabbedPageHandler handler, MauiTabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateBarTextColor(tabbedPage);
    }

    /// <summary>Maps the SelectedTabColor property to the platform view.</summary>
    /// <param name="handler">The handler for the tabbed page.</param>
    /// <param name="tabbedPage">The virtual tabbed page.</param>
    public static void MapSelectedTabColor(TabbedPageHandler handler, MauiTabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateSelectedTabColor(tabbedPage);
    }

    /// <summary>Maps the UnselectedTabColor property to the platform view.</summary>
    /// <param name="handler">The handler for the tabbed page.</param>
    /// <param name="tabbedPage">The virtual tabbed page.</param>
    public static void MapUnselectedTabColor(TabbedPageHandler handler, MauiTabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateUnselectedTabColor(tabbedPage);
    }

    /// <summary>Maps the CurrentPage property to the platform view.</summary>
    /// <param name="handler">The handler for the tabbed page.</param>
    /// <param name="tabbedPage">The virtual tabbed page.</param>
    public static void MapCurrentPage(TabbedPageHandler handler, MauiTabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateCurrentPage(tabbedPage);
    }

    /// <summary>Maps the ItemsSource property to the platform view.</summary>
    /// <param name="handler">The handler for the tabbed page.</param>
    /// <param name="tabbedPage">The virtual tabbed page.</param>
    public static void MapItemsSource(TabbedPageHandler handler, MauiTabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateChildren(tabbedPage, handler.MauiContext);
    }

    /// <summary>Maps the ItemTemplate property to the platform view.</summary>
    /// <param name="handler">The handler for the tabbed page.</param>
    /// <param name="tabbedPage">The virtual tabbed page.</param>
    public static void MapItemTemplate(TabbedPageHandler handler, MauiTabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateChildren(tabbedPage, handler.MauiContext);
    }

    /// <summary>Maps the SelectedItem property to the platform view.</summary>
    /// <param name="handler">The handler for the tabbed page.</param>
    /// <param name="tabbedPage">The virtual tabbed page.</param>
    public static void MapSelectedItem(TabbedPageHandler handler, MauiTabbedPage tabbedPage)
    {
        handler.PlatformView?.UpdateSelectedItem(tabbedPage);
    }

    private void OnTabSelectionChanged(object? sender, Avalonia.Controls.PageSelectionChangedEventArgs e)
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

        // Re-apply tab colors when selection changes
        PlatformView.UpdateTabColors(VirtualView);
    }
}
