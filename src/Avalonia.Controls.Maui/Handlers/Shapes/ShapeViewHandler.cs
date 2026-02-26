#nullable disable
using Microsoft.Maui;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using PlatformView = global::Avalonia.Controls.Shapes.Shape;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

/// <summary>Avalonia handler for IShapeView shapes.</summary>
public interface IShapeViewHandler : IViewHandler
{
    /// <summary>Gets the virtual view for this handler.</summary>
    new IShapeView VirtualView { get; }

    /// <summary>Gets the platform view for this handler.</summary>
    new PlatformView PlatformView { get; }
}

/// <summary>Avalonia handler for IShapeView shapes.</summary>
public abstract partial class ShapeViewHandler : ViewHandler<IShapeView, PlatformView>, IShapeViewHandler
{
    /// <summary>Property mapper for <see cref="ShapeViewHandler"/>.</summary>
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

    /// <summary>Command mapper for <see cref="ShapeViewHandler"/>.</summary>
    public static CommandMapper<IShapeView, IShapeViewHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="ShapeViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    /// <param name="commandMapper">The command mapper to use.</param>
    protected ShapeViewHandler(IPropertyMapper mapper, CommandMapper commandMapper = null)
        : base(mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <inheritdoc/>
    IShapeView IShapeViewHandler.VirtualView => VirtualView;

    /// <inheritdoc/>
    PlatformView IShapeViewHandler.PlatformView => PlatformView;

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    public static void MapBackground(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateBackground(shapeView);
        }
    }

    /// <summary>Maps the Shape property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    public static void MapShape(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateShape(shapeView);
        }
    }

    /// <summary>Maps the Aspect property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    public static void MapAspect(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateAspect(shapeView);
        }
    }

    /// <summary>Maps the Fill property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    public static void MapFill(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateFill(shapeView);
        }
    }

    /// <summary>Maps the Stroke property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    public static void MapStroke(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStroke(shapeView);
        }
    }

    /// <summary>Maps the StrokeThickness property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    public static void MapStrokeThickness(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeThickness(shapeView);
        }
    }

    /// <summary>Maps the StrokeDashPattern property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    public static void MapStrokeDashPattern(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeDashPattern(shapeView);
        }
    }

    /// <summary>Maps the StrokeDashOffset property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    public static void MapStrokeDashOffset(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeDashOffset(shapeView);
        }
    }

    /// <summary>Maps the StrokeLineCap property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    public static void MapStrokeLineCap(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeLineCap(shapeView);
        }
    }

    /// <summary>Maps the StrokeLineJoin property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    public static void MapStrokeLineJoin(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeLineJoin(shapeView);
        }
    }

    /// <summary>Maps the StrokeMiterLimit property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shapeView">The shape view.</param>
    [Avalonia.Controls.Maui.Platform.NotImplemented("Avalonia Shape doesn't expose StrokeMiterLimit property.")]
    public static void MapStrokeMiterLimit(IShapeViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateStrokeMiterLimit(shapeView);
        }
    }
}

/// <summary>Avalonia handler for IShapeView shapes with strongly-typed virtual view and platform view.</summary>
/// <typeparam name="TVirtualView">The type of the virtual view.</typeparam>
/// <typeparam name="TPlatformView">The type of the platform view.</typeparam>
public abstract class ShapeViewHandler<TVirtualView, TPlatformView> : ShapeViewHandler
    where TVirtualView : class, IShapeView
    where TPlatformView : PlatformView
{
    /// <summary>Initializes a new instance of <see cref="ShapeViewHandler{TVirtualView, TPlatformView}"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    /// <param name="commandMapper">The command mapper to use.</param>
    protected ShapeViewHandler(IPropertyMapper mapper, CommandMapper commandMapper = null)
        : base(mapper, commandMapper)
    {
    }

    /// <summary>Gets the strongly-typed virtual view.</summary>
    public new TVirtualView VirtualView => (TVirtualView)base.VirtualView;

    /// <summary>Gets the strongly-typed platform view.</summary>
    public new TPlatformView PlatformView => (TPlatformView)base.PlatformView;

    /// <summary>Called when the handler is connected to a platform view.</summary>
    /// <param name="platformView">The platform view being connected.</param>
    protected virtual void ConnectHandler(TPlatformView platformView)
    {
    }

    /// <summary>Called when the handler is disconnected from a platform view.</summary>
    /// <param name="platformView">The platform view being disconnected.</param>
    protected virtual void DisconnectHandler(TPlatformView platformView)
    {
    }

    /// <inheritdoc/>
    public sealed override void OnConnectHandler(object platformView)
    {
        base.OnConnectHandler(platformView);
        ConnectHandler((TPlatformView)platformView);
    }

    /// <inheritdoc/>
    public sealed override void OnDisconnectHandler(object platformView)
    {
        DisconnectHandler((TPlatformView)platformView);
        base.OnDisconnectHandler(platformView);
    }
}
