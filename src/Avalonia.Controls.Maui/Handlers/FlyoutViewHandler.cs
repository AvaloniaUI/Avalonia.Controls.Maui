using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.Platform.FlyoutContainer;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI FlyoutView to Avalonia SplitView mapping
/// </summary>
public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, PlatformView>, IFlyoutViewHandler
{
    // Like IViewHandler.ContainerView, those properties should be set with priority because other mappers depend on them.
    private static readonly IPropertyMapper<IFlyoutView, IFlyoutViewHandler> FlyoutLayoutMapper = new PropertyMapper<IFlyoutView, IFlyoutViewHandler>()
    {
        [nameof(IFlyoutView.Flyout)] = MapFlyout,
        [nameof(IFlyoutView.Detail)] = MapDetail,
    };

    public static IPropertyMapper<IFlyoutView, IFlyoutViewHandler> Mapper =
        new PropertyMapper<IFlyoutView, IFlyoutViewHandler>(ViewHandler.ViewMapper, FlyoutLayoutMapper)
        {
            [nameof(IFlyoutView.IsPresented)] = MapIsPresented,
            [nameof(IFlyoutView.FlyoutBehavior)] = MapFlyoutBehavior,
            [nameof(IFlyoutView.FlyoutWidth)] = MapFlyoutWidth,
            [nameof(IFlyoutView.IsGestureEnabled)] = MapIsGestureEnabled,
        };

    public static CommandMapper<IFlyoutView, IFlyoutViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
        };

    public FlyoutViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public FlyoutViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public FlyoutViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
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
        
        platformView.FlyoutOpened += OnFlyoutOpened;
        platformView.FlyoutClosed += OnFlyoutClosed;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.FlyoutOpened -= OnFlyoutOpened;
        platformView.FlyoutClosed -= OnFlyoutClosed;
        
        base.DisconnectHandler(platformView);
    }

    private void OnFlyoutClosed(object? sender, EventArgs e)
    {
        if (VirtualView != null)
        {
            VirtualView.IsPresented = false;
        }
    }

    private void OnFlyoutOpened(object? sender, EventArgs e)
    {
        if (VirtualView != null)
        {
            VirtualView.IsPresented = true;
        }
    }

    public static void MapFlyout(IFlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (handler.MauiContext == null)
            return;

        if (flyoutView.Flyout != null)
        {
            var flyoutPlatformView = flyoutView.Flyout.ToPlatform(handler.MauiContext);
            platformView.SetFlyoutContent(flyoutPlatformView as Control);
        }
        else
        {
            platformView.SetFlyoutContent(null);
        }
    }

    public static void MapDetail(IFlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (handler.MauiContext == null)
            return;

        if (flyoutView.Detail != null)
        {
            var detailPlatformView = flyoutView.Detail.ToPlatform(handler.MauiContext);
            platformView.SetDetailContent(detailPlatformView as Control);
        }
        else
        {
            platformView.SetDetailContent(null);
        }
    }

    public static void MapIsPresented(IFlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.IsFlyoutOpen = flyoutView.IsPresented;
    }

    public static void MapFlyoutBehavior(IFlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.FlyoutBehavior = (Platform.FlyoutBehavior)(int)flyoutView.FlyoutBehavior;
    }

    public static void MapFlyoutWidth(IFlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (flyoutView.FlyoutWidth > 0)
        {
            platformView.FlyoutWidth = flyoutView.FlyoutWidth;
        }
        else if (flyoutView.FlyoutWidth == -1)
        {
            // -1 means auto/match parent, use a reasonable default
            platformView.FlyoutWidth = 320;
        }
    }

    public static void MapIsGestureEnabled(IFlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.IsGestureEnabled = flyoutView.IsGestureEnabled;
    }

    public static void MapToolbar(IFlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        // Toolbar support would require wrapping the detail content in a DockPanel
        // This is not currently implemented with the custom FlyoutContainer
        // TODO: Add toolbar support by wrapping detail content when toolbar is present
    }

    IFlyoutView IFlyoutViewHandler.VirtualView => VirtualView;

    object IFlyoutViewHandler.PlatformView => PlatformView;
}
