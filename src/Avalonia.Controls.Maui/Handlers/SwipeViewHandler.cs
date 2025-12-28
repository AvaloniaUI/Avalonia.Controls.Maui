using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = Avalonia.Controls.Maui.Swipe;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for .NET MAUI SwipeView to Avalonia.Labs Swipe mapping
/// </summary>
public partial class SwipeViewHandler : ViewHandler<ISwipeView, PlatformView>
{
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

    public static CommandMapper<ISwipeView, SwipeViewHandler> CommandMapper =
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
        platformView.ExecuteRequested += OnExecuteRequested;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.SwipeStarted -= OnSwipeStarted;
        platformView.SwipeChanging -= OnSwipeChanging;
        platformView.SwipeEnded -= OnSwipeEnded;
        platformView.ExecuteRequested -= OnExecuteRequested;
        base.DisconnectHandler(platformView);
    }
    
    public static void MapBackground(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateBackground(swipeView);
    }

    public static void MapContent(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateContent(swipeView, handler.MauiContext);
    }

    public static void MapLeftItems(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateLeftItems(swipeView, handler.MauiContext);
    }

    public static void MapRightItems(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateRightItems(swipeView, handler.MauiContext);
    }

    public static void MapTopItems(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateTopItems(swipeView, handler.MauiContext);
    }

    public static void MapBottomItems(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateBottomItems(swipeView, handler.MauiContext);
    }

    public static void MapThreshold(SwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateThreshold(swipeView);
    }

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
