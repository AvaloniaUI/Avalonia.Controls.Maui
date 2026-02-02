#nullable disable
using Microsoft.Maui;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using PlatformView = global::Avalonia.Controls.Shapes.Shape;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

public interface IShapeViewHandler : IViewHandler
{
    new IShapeView VirtualView { get; }
    new PlatformView PlatformView { get; }
}

public abstract partial class ShapeViewHandler : ViewHandler<IShapeView, PlatformView>, IShapeViewHandler
{
    public static IPropertyMapper<IShapeView, IShapeViewHandler> Mapper = new PropertyMapper<IShapeView, IShapeViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IShapeView.Background)] = MapBackground,
        [nameof(IShapeView.Shape)] = MapShape,
        [nameof(IShapeView.Aspect)] = MapAspect,
        [nameof(IShapeView.Fill)] = MapFill,
        [nameof(IShapeView.Stroke)] = MapStroke,
        [nameof(IShapeView.StrokeThickness)] = MapStrokeThickness,
        [nameof(IShapeView.StrokeDashPattern)] = MapStrokeDashPattern,
        [nameof(IShapeView.StrokeDashOffset)] = MapStrokeDashOffset,
        [nameof(IShapeView.StrokeLineCap)] = MapStrokeLineCap,
        [nameof(IShapeView.StrokeLineJoin)] = MapStrokeLineJoin,
        [nameof(IShapeView.StrokeMiterLimit)] = MapStrokeMiterLimit
    };

    public static CommandMapper<IShapeView, IShapeViewHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    protected ShapeViewHandler(IPropertyMapper mapper, CommandMapper commandMapper = null)
        : base(mapper, commandMapper ?? CommandMapper)
    {
    }

    IShapeView IShapeViewHandler.VirtualView => VirtualView;

    PlatformView IShapeViewHandler.PlatformView => PlatformView;

    public static void MapBackground(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateBackground(shapeView);
        }
    }

    public static void MapShape(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateShape(shapeView);
        }
    }

    public static void MapAspect(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateAspect(shapeView);
        }
    }

    public static void MapFill(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateFill(shapeView);
        }
    }

    public static void MapStroke(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStroke(shapeView);
        }
    }

    public static void MapStrokeThickness(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeThickness(shapeView);
        }
    }

    public static void MapStrokeDashPattern(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeDashPattern(shapeView);
        }
    }

    public static void MapStrokeDashOffset(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeDashOffset(shapeView);
        }
    }

    public static void MapStrokeLineCap(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeLineCap(shapeView);
        }
    }

    public static void MapStrokeLineJoin(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeLineJoin(shapeView);
        }
    }

    [Avalonia.Controls.Maui.Platform.NotImplemented("Avalonia Shape doesn't expose StrokeMiterLimit property.")]
    public static void MapStrokeMiterLimit(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeMiterLimit(shapeView);
        }
    }
}

public abstract class ShapeViewHandler<TVirtualView, TPlatformView> : ShapeViewHandler
    where TVirtualView : class, IShapeView
    where TPlatformView : PlatformView
{
    protected ShapeViewHandler(IPropertyMapper mapper, CommandMapper commandMapper = null)
        : base(mapper, commandMapper)
    {
    }

    public new TVirtualView VirtualView => (TVirtualView)base.VirtualView;

    public new TPlatformView PlatformView => (TPlatformView)base.PlatformView;

    protected virtual void ConnectHandler(TPlatformView platformView)
    {
    }

    protected virtual void DisconnectHandler(TPlatformView platformView)
    {
    }

    public sealed override void OnConnectHandler(object platformView)
    {
        base.OnConnectHandler(platformView);
        ConnectHandler((TPlatformView)platformView);
    }

    public sealed override void OnDisconnectHandler(object platformView)
    {
        DisconnectHandler((TPlatformView)platformView);
        base.OnDisconnectHandler(platformView);
    }
}
