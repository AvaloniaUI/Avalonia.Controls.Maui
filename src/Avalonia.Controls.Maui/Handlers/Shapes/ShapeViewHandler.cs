#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
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
        // TODO: Handle background separately from fill if needed
        if (handler.PlatformView is PlatformView platformView)
        {
            // In Avalonia, shapes don't have a separate background concept
            // The Fill property handles the interior painting
        }
    }

    public static void MapShape(IShapeViewHandler handler, IShapeView shapeView)
    {
        // Shape-specific handling done in derived handlers
    }

    public static void MapAspect(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Stretch = shapeView.Aspect switch
            {
                PathAspect.None => global::Avalonia.Media.Stretch.None,
                PathAspect.Center => global::Avalonia.Media.Stretch.None,
                PathAspect.Stretch => global::Avalonia.Media.Stretch.Fill,
                PathAspect.AspectFit => global::Avalonia.Media.Stretch.Uniform,
                PathAspect.AspectFill => global::Avalonia.Media.Stretch.UniformToFill,
                _ => global::Avalonia.Media.Stretch.None
            };
        }
    }

    public static void MapFill(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Fill = shapeView.Fill?.ToAvaloniaBrush();
        }
    }

    public static void MapStroke(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Stroke = shapeView.Stroke?.ToAvaloniaBrush();
        }
    }

    public static void MapStrokeThickness(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.StrokeThickness = shapeView.StrokeThickness;
        }
    }

    public static void MapStrokeDashPattern(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView && shapeView.StrokeDashPattern != null)
        {
            var dashArray = new global::Avalonia.Collections.AvaloniaList<double>();
            foreach (var value in shapeView.StrokeDashPattern)
            {
                dashArray.Add(value);
            }
            platformView.StrokeDashArray = dashArray;
        }
    }

    public static void MapStrokeDashOffset(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.StrokeDashOffset = shapeView.StrokeDashOffset;
        }
    }

    public static void MapStrokeLineCap(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.StrokeLineCap = shapeView.StrokeLineCap switch
            {
                LineCap.Butt => global::Avalonia.Media.PenLineCap.Flat,
                LineCap.Round => global::Avalonia.Media.PenLineCap.Round,
                LineCap.Square => global::Avalonia.Media.PenLineCap.Square,
                _ => global::Avalonia.Media.PenLineCap.Flat
            };
        }
    }

    public static void MapStrokeLineJoin(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.StrokeJoin = shapeView.StrokeLineJoin switch
            {
                LineJoin.Miter => global::Avalonia.Media.PenLineJoin.Miter,
                LineJoin.Round => global::Avalonia.Media.PenLineJoin.Round,
                LineJoin.Bevel => global::Avalonia.Media.PenLineJoin.Bevel,
                _ => global::Avalonia.Media.PenLineJoin.Miter
            };
        }
    }

    public static void MapStrokeMiterLimit(IShapeViewHandler handler, IShapeView shapeView)
    {
        // Avalonia Shape doesn't expose StrokeMiterLimit property
        // It uses the default miter limit from the PenLineJoin.Miter setting
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