using Microsoft.Maui;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = Avalonia.Controls.Maui.Swipe;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="ISwipeView"/>.</summary>
public partial class SwipeViewHandler : ViewHandler<ISwipeView, PlatformView>
{
    /// <summary>Property mapper for <see cref="SwipeViewHandler"/>.</summary>
    public static IPropertyMapper<ISwipeView, SwipeViewHandler> Mapper =
        new PropertyMapper<ISwipeView, SwipeViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(ISwipeView.Background)] = MapBackground,
            [nameof(ISwipeView.Content)] = MapContent,
            [nameof(ISwipeView.LeftItems)] = MapLeftItems,
            [nameof(ISwipeView.RightItems)] = MapRightItems,
            [nameof(ISwipeView.TopItems)] = MapTopItems,
            [nameof(ISwipeView.BottomItems)] = MapBottomItems,
            [nameof(ISwipeView.Threshold)] = MapThreshold,
        };

    /// <summary>Command mapper for <see cref="SwipeViewHandler"/>.</summary>
    public static CommandMapper<ISwipeView, SwipeViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
            [nameof(ISwipeView.RequestOpen)] = MapRequestOpen,
            [nameof(ISwipeView.RequestClose)] = MapRequestClose,
        };

    /// <summary>Initializes a new instance of <see cref="SwipeViewHandler"/>.</summary>
    public SwipeViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SwipeViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public SwipeViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SwipeViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public SwipeViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
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
        platformView.SwipeStarted += OnSwipeStarted;
        platformView.SwipeChanging += OnSwipeChanging;
        platformView.SwipeEnded += OnSwipeEnded;
        platformView.ExecuteRequested += OnExecuteRequested;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.SwipeStarted -= OnSwipeStarted;
        platformView.SwipeChanging -= OnSwipeChanging;
        platformView.SwipeEnded -= OnSwipeEnded;
        platformView.ExecuteRequested -= OnExecuteRequested;
        base.DisconnectHandler(platformView);
    }

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The handler for the swipe view.</param>
    /// <param name="swipeView">The virtual swipe view.</param>
    public static void MapBackground(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateBackground(swipeView);
    }

    /// <summary>Maps the Content property to the platform view.</summary>
    /// <param name="handler">The handler for the swipe view.</param>
    /// <param name="swipeView">The virtual swipe view.</param>
    public static void MapContent(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateContent(swipeView, handler.MauiContext);
    }

    /// <summary>Maps the LeftItems property to the platform view.</summary>
    /// <param name="handler">The handler for the swipe view.</param>
    /// <param name="swipeView">The virtual swipe view.</param>
    public static void MapLeftItems(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateLeftItems(swipeView, handler.MauiContext);
    }

    /// <summary>Maps the RightItems property to the platform view.</summary>
    /// <param name="handler">The handler for the swipe view.</param>
    /// <param name="swipeView">The virtual swipe view.</param>
    public static void MapRightItems(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateRightItems(swipeView, handler.MauiContext);
    }

    /// <summary>Maps the TopItems property to the platform view.</summary>
    /// <param name="handler">The handler for the swipe view.</param>
    /// <param name="swipeView">The virtual swipe view.</param>
    public static void MapTopItems(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateTopItems(swipeView, handler.MauiContext);
    }

    /// <summary>Maps the BottomItems property to the platform view.</summary>
    /// <param name="handler">The handler for the swipe view.</param>
    /// <param name="swipeView">The virtual swipe view.</param>
    public static void MapBottomItems(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateBottomItems(swipeView, handler.MauiContext);
    }

    /// <summary>Maps the Threshold property to the platform view.</summary>
    /// <param name="handler">The handler for the swipe view.</param>
    /// <param name="swipeView">The virtual swipe view.</param>
    public static void MapThreshold(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateThreshold(swipeView);
    }

    /// <summary>Maps the RequestOpen command to the platform view.</summary>
    /// <param name="handler">The handler for the swipe view.</param>
    /// <param name="swipeView">The virtual swipe view.</param>
    /// <param name="args">The open request arguments.</param>
    public static void MapRequestOpen(SwipeViewHandler handler, ISwipeView swipeView, object? args)
    {
        if (handler.PlatformView is PlatformView platformView && args is SwipeViewOpenRequest request)
        {
            var target = request.OpenSwipeItem switch
            {
                Microsoft.Maui.OpenSwipeItem.LeftItems => SwipeState.LeftVisible,
                Microsoft.Maui.OpenSwipeItem.RightItems => SwipeState.RightVisible,
                Microsoft.Maui.OpenSwipeItem.TopItems => SwipeState.TopVisible,
                Microsoft.Maui.OpenSwipeItem.BottomItems => SwipeState.BottomVisible,
                _ => SwipeState.Hidden
            };

            platformView.SetSwipeState(target, request.Animated);
        }
    }

    /// <summary>Maps the RequestClose command to the platform view.</summary>
    /// <param name="handler">The handler for the swipe view.</param>
    /// <param name="swipeView">The virtual swipe view.</param>
    /// <param name="args">The close request arguments.</param>
    public static void MapRequestClose(SwipeViewHandler handler, ISwipeView swipeView, object? args)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.SetSwipeState(SwipeState.Hidden, animated: true);
    }

    private void OnSwipeStarted(object? sender, SwipeStartedEventArgs e)
    {
        if (VirtualView == null) return;

        var direction = e.SwipeDirection.ToMauiSwipeDirection();
        VirtualView.SwipeStarted(new SwipeViewSwipeStarted(direction));
    }

    private void OnSwipeChanging(object? sender, SwipeChangingEventArgs e)
    {
        if (VirtualView == null) return;

        var direction = e.SwipeDirection.ToMauiSwipeDirection();
        VirtualView.SwipeChanging(new SwipeViewSwipeChanging(direction, e.Offset));
    }

    private void OnSwipeEnded(object? sender, SwipeEndedEventArgs e)
    {
        if (VirtualView == null) return;

        var direction = e.SwipeDirection.ToMauiSwipeDirection();
        VirtualView.SwipeEnded(new SwipeViewSwipeEnded(direction, e.IsOpen));
    }

    private void OnExecuteRequested(object? sender, SwipeDirection direction)
    {
        if (VirtualView == null)
            return;

        var items = GetItemsForDirection(direction.ToMauiSwipeDirection());

        if (items is { Count: > 0 } && items[0] is ISwipeItem swipeItem)
        {
            swipeItem.OnInvoked();
        }
    }

    private ISwipeItems? GetItemsForDirection(Microsoft.Maui.SwipeDirection direction)
    {
        return direction switch
        {
            Microsoft.Maui.SwipeDirection.Left => VirtualView?.RightItems,
            Microsoft.Maui.SwipeDirection.Right => VirtualView?.LeftItems,
            Microsoft.Maui.SwipeDirection.Up => VirtualView?.BottomItems,
            Microsoft.Maui.SwipeDirection.Down => VirtualView?.TopItems,
            _ => null
        };
    }
}
