using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformTouchGraphicsView>
{
    public static IPropertyMapper<IGraphicsView, GraphicsViewHandler> Mapper = new PropertyMapper<IGraphicsView, GraphicsViewHandler>(ViewMapper)
    {
        [nameof(IView.Background)] = MapBackground,
        [nameof(IGraphicsView.Drawable)] = MapDrawable,
        [nameof(IView.FlowDirection)] = MapFlowDirection
    };

    public static CommandMapper<IGraphicsView, GraphicsViewHandler> CommandMapper = new(ViewCommandMapper)
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

    public static void MapBackground(GraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.UpdateBackground(graphicsView);
    }

    public static void MapDrawable(GraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.UpdateDrawable(graphicsView);
    }

    public static void MapFlowDirection(GraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.UpdateFlowDirection(graphicsView);
        (handler.PlatformView as PlatformTouchGraphicsView)?.Invalidate();
    }

    public static void MapInvalidate(GraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.Invalidate();
    }
}
