using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformTouchGraphicsView>, IGraphicsViewHandler
{
    public static IPropertyMapper<IGraphicsView, IGraphicsViewHandler> Mapper = new PropertyMapper<IGraphicsView, IGraphicsViewHandler>(ViewMapper)
    {
        [nameof(IView.Background)] = MapBackground,
        [nameof(IGraphicsView.Drawable)] = MapDrawable,
        [nameof(IView.FlowDirection)] = MapFlowDirection
    };

    public static CommandMapper<IGraphicsView, IGraphicsViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(IGraphicsView.Invalidate)] = MapInvalidate
    };

    public GraphicsViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public GraphicsViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public GraphicsViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    IGraphicsView IGraphicsViewHandler.VirtualView => VirtualView;

    PlatformView IGraphicsViewHandler.PlatformView => PlatformView;

    protected override PlatformTouchGraphicsView CreatePlatformView()
    {
        return new PlatformTouchGraphicsView();
    }

    protected override void ConnectHandler(PlatformTouchGraphicsView platformView)
    {
        platformView.Connect(VirtualView);
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(PlatformTouchGraphicsView platformView)
    {
        platformView.Disconnect();
        base.DisconnectHandler(platformView);
    }

    public static void MapBackground(IGraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        ((GraphicsViewHandler)handler).UpdateBackground();
    }

    public static void MapDrawable(IGraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        ((GraphicsViewHandler)handler).UpdateDrawable();
    }

    public static void MapFlowDirection(IGraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        // Flow direction handling if needed
    }

    public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
    {
        ((GraphicsViewHandler)handler).PlatformView.Invalidate();
    }

    private void UpdateBackground()
    {
        if (VirtualView?.Background != null)
        {
            PlatformView.Background = VirtualView.Background.ToPlatform();
        }
    }

    private void UpdateDrawable()
    {
        PlatformView.UpdateDrawable(VirtualView);
    }
}
