using Microsoft.Maui;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.DrawerPage;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IFlyoutView"/>.</summary>
public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, PlatformView>
{
    private Style? _paneMinWidthStyle;

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
        return new PlatformView
        {
            ContentTemplate = null,
            DrawerTemplate = null
        };
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);

        platformView.Opened += OnFlyoutOpened;
        platformView.Closed += OnFlyoutClosed;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.Opened -= OnFlyoutOpened;
        platformView.Closed -= OnFlyoutClosed;

        base.DisconnectHandler(platformView);
    }

    private void OnFlyoutClosed(object? sender, RoutedEventArgs e)
    {
        // Ignore routed events bubbling up from nested DrawerPages (e.g. Shell inside FlyoutPage)
        if (e.Source != PlatformView)
            return;

        if (VirtualView != null)
        {
            VirtualView.IsPresented = false;
        }
    }

    private void OnFlyoutOpened(object? sender, RoutedEventArgs e)
    {
        // Ignore routed events bubbling up from nested DrawerPages (e.g. Shell inside FlyoutPage)
        if (e.Source != PlatformView)
            return;

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
            platformView.Drawer = flyoutPlatformView as Control;
        }
        else
        {
            platformView.Drawer = null;
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
            platformView.Content = detailPlatformView as Control;
        }
        else
        {
            platformView.Content = null;
        }
    }

    /// <summary>Maps the IsPresented property to the platform view.</summary>
    /// <param name="handler">The handler for the flyout view.</param>
    /// <param name="flyoutView">The virtual flyout view.</param>
    public static void MapIsPresented(FlyoutViewHandler handler, IFlyoutView flyoutView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.IsOpen = flyoutView.IsPresented;
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
                platformView.DrawerBehavior = DrawerBehavior.Disabled;
                platformView.IsOpen = false;
                break;
            case Microsoft.Maui.FlyoutBehavior.Flyout:
                platformView.DrawerBehavior = DrawerBehavior.Flyout;
                break;
            case Microsoft.Maui.FlyoutBehavior.Locked:
                platformView.DrawerBehavior = DrawerBehavior.Locked;
                platformView.IsOpen = true;
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

        double width;
        if (flyoutView.FlyoutWidth > 0)
        {
            width = flyoutView.FlyoutWidth;
        }
        else
        {
            // -1 means auto/match parent, use the default
            width = 320;
        }

        platformView.DrawerLength = width;

        // Prevent drawer content from reflowing during open/close animation.
        // The SplitView animates PART_PaneRoot's Width, which causes child content to
        // re-layout at the shrinking width. Setting MinWidth on the pane presenter ensures
        // content always lays out at full drawer width and is simply clipped.
        if (handler._paneMinWidthStyle != null)
            platformView.Styles.Remove(handler._paneMinWidthStyle);

        handler._paneMinWidthStyle = new Style(x =>
            x.OfType<DrawerPage>()
             .Template().OfType<SplitView>().Name("PART_SplitView")
             .Template().OfType<ContentPresenter>().Name("PART_PanePresenter"))
        {
            Setters = { new Setter(Layoutable.MinWidthProperty, width) }
        };
        platformView.Styles.Add(handler._paneMinWidthStyle);
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
