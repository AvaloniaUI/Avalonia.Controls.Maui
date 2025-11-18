using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Specialized;

namespace Avalonia.Controls.Maui.Handlers;

public partial class TabbedViewHandler : ViewHandler<ITabbedView, TabControl>, ITabbedViewHandler
{
    public static IPropertyMapper<ITabbedView, ITabbedViewHandler> Mapper =
        new PropertyMapper<ITabbedView, ITabbedViewHandler>(ViewMapper)
        {
        };

    public static CommandMapper<ITabbedView, ITabbedViewHandler> CommandMapper =
        new(ViewCommandMapper);

    public TabbedViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public TabbedViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public TabbedViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    ITabbedView ITabbedViewHandler.VirtualView => VirtualView;

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

        tabControl.SelectionChanged += OnTabSelectionChanged;

        return tabControl;
    }

    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

        if (VirtualView is TabbedPage tabbedPage)
        {
            // Subscribe to children changes
            tabbedPage.PagesChanged += OnPagesChanged;

            // Load initial pages
            UpdateTabItems();

            // Set initial selection
            if (tabbedPage.CurrentPage != null)
            {
                var index = tabbedPage.Children.IndexOf(tabbedPage.CurrentPage);
                if (index >= 0)
                {
                    PlatformView.SelectedIndex = index;
                }
            }
        }
    }

    protected override void DisconnectHandler(TabControl platformView)
    {
        if (VirtualView is TabbedPage tabbedPage)
        {
            tabbedPage.PagesChanged -= OnPagesChanged;
        }

        platformView.SelectionChanged -= OnTabSelectionChanged;

        base.DisconnectHandler(platformView);
    }

    private void OnTabSelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (VirtualView is not TabbedPage tabbedPage)
            return;

        var selectedIndex = PlatformView.SelectedIndex;
        if (selectedIndex >= 0 && selectedIndex < tabbedPage.Children.Count)
        {
            var selectedPage = tabbedPage.Children[selectedIndex];
            if (tabbedPage.CurrentPage != selectedPage)
            {
                tabbedPage.CurrentPage = selectedPage;
            }
        }
    }

    private void OnPagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateTabItems();
    }

    private void UpdateTabItems()
    {
        if (VirtualView is not TabbedPage tabbedPage || MauiContext == null)
            return;

        PlatformView.Items.Clear();

        foreach (var page in tabbedPage.Children)
        {
            var tabItem = new TabItem
            {
                Header = page.Title ?? "Tab",
                Content = page.ToPlatform(MauiContext)
            };

            PlatformView.Items.Add(tabItem);
        }
    }
}
