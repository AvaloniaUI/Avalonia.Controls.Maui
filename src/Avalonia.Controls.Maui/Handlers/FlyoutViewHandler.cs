using System;
using Microsoft.Maui;
using Avalonia.Controls;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.Controls.Shell.FlyoutContainer;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IFlyoutView"/>.</summary>
public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, PlatformView>
{
    // Properties are set with priority because other mappers depend on them.
    private static readonly IPropertyMapper<IFlyoutView, FlyoutViewHandler> FlyoutLayoutMapper = new PropertyMapper<IFlyoutView, FlyoutViewHandler>()
    {
        [nameof(IFlyoutView.Flyout)] = MapFlyout,
        [nameof(IFlyoutView.Detail)] = MapDetail,
    };

    /// <summary>Property mapper for <see cref="FlyoutViewHandler"/>.</summary>
    public static IPropertyMapper<IFlyoutView, FlyoutViewHandler> Mapper =
        new PropertyMapper<IFlyoutView, FlyoutViewHandler>(ViewHandler.ViewMapper, FlyoutLayoutMapper)
        {
            [nameof(IFlyoutView.IsPresented)] = MapIsPresented,
            [nameof(IFlyoutView.FlyoutBehavior)] = MapFlyoutBehavior,
            [nameof(IFlyoutView.FlyoutWidth)] = MapFlyoutWidth,
            [nameof(IFlyoutView.IsGestureEnabled)] = MapIsGestureEnabled,
        };

    /// <summary>Command mapper for <see cref="FlyoutViewHandler"/>.</summary>
    public static CommandMapper<IFlyoutView, FlyoutViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
        };

    /// <summary>Initializes a new instance of <see cref="FlyoutViewHandler"/>.</summary>
    public FlyoutViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="FlyoutViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public FlyoutViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="FlyoutViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public FlyoutViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);

        platformView.FlyoutOpened += OnFlyoutOpened;
        platformView.FlyoutClosed += OnFlyoutClosed;
    }

    /// <inheritdoc/>
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

    /// <summary>Maps the Flyout property to the platform view.</summary>
    /// <param name="handler">The handler for the flyout view.</param>
    /// <param name="flyoutView">The virtual flyout view.</param>
    public static void MapFlyout(FlyoutViewHandler handler, IFlyoutView flyoutView)
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

    /// <summary>Maps the Detail property to the platform view.</summary>
    /// <param name="handler">The handler for the flyout view.</param>
    /// <param name="flyoutView">The virtual flyout view.</param>
    public static void MapDetail(FlyoutViewHandler handler, IFlyoutView flyoutView)
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

    /// <summary>Maps the IsPresented property to the platform view.</summary>
    /// <param name="handler">The handler for the flyout view.</param>
    /// <param name="flyoutView">The virtual flyout view.</param>
    public static void MapIsPresented(FlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.IsFlyoutOpen = flyoutView.IsPresented;
    }

    /// <summary>Maps the FlyoutBehavior property to the platform view.</summary>
    /// <param name="handler">The handler for the flyout view.</param>
    /// <param name="flyoutView">The virtual flyout view.</param>
    public static void MapFlyoutBehavior(FlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        switch (flyoutView.FlyoutBehavior)
        {
            case Microsoft.Maui.FlyoutBehavior.Disabled:
                platformView.FlyoutBehavior = Controls.Shell.FlyoutBehavior.Disabled;
                platformView.IsFlyoutOpen = false;
                break;
            case Microsoft.Maui.FlyoutBehavior.Flyout:
                platformView.FlyoutBehavior = Controls.Shell.FlyoutBehavior.Popover;
                break;
            case Microsoft.Maui.FlyoutBehavior.Locked:
                platformView.FlyoutBehavior = Controls.Shell.FlyoutBehavior.Locked;
                platformView.IsFlyoutOpen = true;
                break;
        }
    }

    /// <summary>Maps the FlyoutWidth property to the platform view.</summary>
    /// <param name="handler">The handler for the flyout view.</param>
    /// <param name="flyoutView">The virtual flyout view.</param>
    public static void MapFlyoutWidth(FlyoutViewHandler handler, IFlyoutView flyoutView)
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
            platformView.FlyoutWidth = PlatformView.DefaultFlyoutWidth;
        }
    }

    /// <summary>Maps the IsGestureEnabled property to the platform view.</summary>
    /// <param name="handler">The handler for the flyout view.</param>
    /// <param name="flyoutView">The virtual flyout view.</param>
    public static void MapIsGestureEnabled(FlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.IsGestureEnabled = flyoutView.IsGestureEnabled;
    }

    /// <summary>Maps the Toolbar property to the platform view.</summary>
    /// <param name="handler">The handler for the flyout view.</param>
    /// <param name="flyoutView">The virtual flyout view.</param>
    public static void MapToolbar(FlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        // Toolbar is handled by NavigationPage's StackNavigationManager
    }
}
