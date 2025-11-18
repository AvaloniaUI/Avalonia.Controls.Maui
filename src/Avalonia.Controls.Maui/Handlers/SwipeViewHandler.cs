using System;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Avalonia.Controls;
using PlatformView = Avalonia.Controls.Maui.Controls.MauiSwipeView;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI SwipeView to Avalonia MauiSwipeView mapping
/// </summary>
public partial class SwipeViewHandler : ViewHandler<ISwipeView, PlatformView>, ISwipeViewHandler
{
    public static IPropertyMapper<ISwipeView, ISwipeViewHandler> Mapper =
        new PropertyMapper<ISwipeView, ISwipeViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(ISwipeView.Content)] = MapContent,
            [nameof(ISwipeView.LeftItems)] = MapLeftItems,
            [nameof(ISwipeView.RightItems)] = MapRightItems,
            [nameof(ISwipeView.TopItems)] = MapTopItems,
            [nameof(ISwipeView.BottomItems)] = MapBottomItems,
            [nameof(ISwipeView.Threshold)] = MapThreshold,
            [nameof(ISwipeView.SwipeTransitionMode)] = MapSwipeTransitionMode,
        };

    public static CommandMapper<ISwipeView, ISwipeViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
            [nameof(ISwipeView.RequestOpen)] = MapRequestOpen,
            [nameof(ISwipeView.RequestClose)] = MapRequestClose,
        };

    public SwipeViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public SwipeViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public SwipeViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SwipeStarted += OnSwipeStarted;
        platformView.SwipeChanging += OnSwipeChanging;
        platformView.SwipeEnded += OnSwipeEnded;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.SwipeStarted -= OnSwipeStarted;
        platformView.SwipeChanging -= OnSwipeChanging;
        platformView.SwipeEnded -= OnSwipeEnded;
        base.DisconnectHandler(platformView);
    }

    private void OnSwipeStarted(object? sender, Controls.SwipeEventArgs e)
    {
        if (VirtualView == null)
            return;

        // Default to Right direction for now - would need to track actual direction
        VirtualView.SwipeStarted(new SwipeViewSwipeStarted(SwipeDirection.Right));
    }

    private void OnSwipeChanging(object? sender, Controls.SwipeEventArgs e)
    {
        if (VirtualView == null)
            return;

        // Default to Right direction and 0 offset for now
        VirtualView.SwipeChanging(new SwipeViewSwipeChanging(SwipeDirection.Right, 0));
    }

    private void OnSwipeEnded(object? sender, Controls.SwipeEventArgs e)
    {
        if (VirtualView == null)
            return;

        VirtualView.SwipeEnded(new SwipeViewSwipeEnded(SwipeDirection.Right, PlatformView.IsOpen));
    }

    public static void MapContent(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (handler.MauiContext == null)
            return;

        if (swipeView.PresentedContent != null)
        {
            var content = swipeView.PresentedContent.ToPlatform(handler.MauiContext);
            platformView.Content = content;
        }
        else
        {
            platformView.Content = null;
        }
    }

    public static void MapLeftItems(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (handler.MauiContext == null)
            return;

        UpdateSwipeItems(handler, swipeView.LeftItems, "PART_LeftItems");
    }

    public static void MapRightItems(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (handler.MauiContext == null)
            return;

        UpdateSwipeItems(handler, swipeView.RightItems, "PART_RightItems");
    }

    public static void MapTopItems(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (handler.MauiContext == null)
            return;

        UpdateSwipeItems(handler, swipeView.TopItems, "PART_TopItems");
    }

    public static void MapBottomItems(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (handler.MauiContext == null)
            return;

        UpdateSwipeItems(handler, swipeView.BottomItems, "PART_BottomItems");
    }

    private static void UpdateSwipeItems(ISwipeViewHandler handler, ISwipeItems? swipeItems, string panelName)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        // Find the panel by name
        var panel = platformView.FindControl<Panel>(panelName);
        if (panel == null)
            return;

        panel.Children.Clear();

        if (swipeItems == null || swipeItems.Count == 0)
            return;

        foreach (var item in swipeItems)
        {
            Control? itemControl = null;

            if (item is ISwipeItemView swipeItemView && handler.MauiContext != null)
            {
                // SwipeItemView with custom content
                if (swipeItemView.PresentedContent != null)
                {
                    itemControl = swipeItemView.PresentedContent.ToPlatform(handler.MauiContext) as Control;
                }
            }
            else if (item is ISwipeItemMenuItem menuItem && handler.MauiContext != null)
            {
                // SwipeItemMenuItem - the handler will create the platform control
                // We need to ensure it has a handler first
                if (menuItem.Handler?.PlatformView is Control platformControl)
                {
                    itemControl = platformControl;
                }
            }

            if (itemControl != null)
            {
                // Wire up the invoked event
                if (itemControl is Button button)
                {
                    button.Click += (s, e) => item.OnInvoked();
                }

                panel.Children.Add(itemControl);
            }
        }
    }

    public static void MapThreshold(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.Threshold = swipeView.Threshold;
    }

    public static void MapSwipeTransitionMode(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        // Avalonia implementation doesn't have different transition modes
        // This would need custom animation implementation
    }

    public static void MapRequestOpen(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (args is not SwipeViewOpenRequest request)
            return;

        var direction = request.OpenSwipeItem switch
        {
            OpenSwipeItem.LeftItems => Controls.SwipeDirection.Right,
            OpenSwipeItem.RightItems => Controls.SwipeDirection.Left,
            OpenSwipeItem.TopItems => Controls.SwipeDirection.Down,
            OpenSwipeItem.BottomItems => Controls.SwipeDirection.Up,
            _ => Controls.SwipeDirection.Right
        };

        platformView.RequestOpen(direction);
    }

    public static void MapRequestClose(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.RequestClose();
    }

    ISwipeView ISwipeViewHandler.VirtualView => VirtualView;

    object ISwipeViewHandler.PlatformView => PlatformView;
}
