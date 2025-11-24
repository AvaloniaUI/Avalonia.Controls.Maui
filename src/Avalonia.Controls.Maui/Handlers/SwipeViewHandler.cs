using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Maui.Swipe;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for .NET MAUI SwipeView to Avalonia.Labs Swipe mapping
/// </summary>
public partial class SwipeViewHandler : ViewHandler<ISwipeView, PlatformView>, ISwipeViewHandler
{
    public static IPropertyMapper<ISwipeView, ISwipeViewHandler> Mapper =
        new PropertyMapper<ISwipeView, ISwipeViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(ISwipeView.Background)] = MapBackground,
            [nameof(ISwipeView.Content)] = MapContent,
            [nameof(ISwipeView.LeftItems)] = MapLeftItems,
            [nameof(ISwipeView.RightItems)] = MapRightItems,
            [nameof(ISwipeView.TopItems)] = MapTopItems,
            [nameof(ISwipeView.BottomItems)] = MapBottomItems,
            [nameof(ISwipeView.Threshold)] = MapThreshold,
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
    
    public static void MapBackground(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateBackground(swipeView);
    }

    public static void MapContent(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateContent(swipeView, handler.MauiContext);
    }

    public static void MapLeftItems(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateLeftItems(swipeView, handler.MauiContext);
    }

    public static void MapRightItems(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateRightItems(swipeView, handler.MauiContext);
    }

    public static void MapTopItems(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateTopItems(swipeView, handler.MauiContext);
    }

    public static void MapBottomItems(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateBottomItems(swipeView, handler.MauiContext);
    }

    public static void MapThreshold(ISwipeViewHandler handler, ISwipeView swipeView)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateThreshold(swipeView);
    }

    public static void MapRequestOpen(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
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

    public static void MapRequestClose(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
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

    ISwipeView ISwipeViewHandler.VirtualView => VirtualView;
    object ISwipeViewHandler.PlatformView => PlatformView;
}
