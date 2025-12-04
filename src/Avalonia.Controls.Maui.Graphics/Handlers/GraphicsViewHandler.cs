using Avalonia.Controls.Maui.Extensions;
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
        (handler.PlatformView as PlatformTouchGraphicsView)?.UpdateBackground(graphicsView);
    }

    public static void MapDrawable(IGraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.UpdateDrawable(graphicsView);
    }

    public static void MapFlowDirection(IGraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.UpdateFlowDirection(graphicsView);
        (handler.PlatformView as PlatformTouchGraphicsView)?.Invalidate();
    }

    public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.Invalidate();
    }
}
