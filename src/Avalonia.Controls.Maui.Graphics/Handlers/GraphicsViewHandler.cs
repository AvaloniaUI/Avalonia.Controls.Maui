using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Avalonia handler for <see cref="IGraphicsView"/>.
/// </summary>
public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformTouchGraphicsView>
{
    /// <summary>
    /// The property mapper that maps <see cref="IGraphicsView"/> properties to <see cref="GraphicsViewHandler"/> methods.
    /// </summary>
    public static IPropertyMapper<IGraphicsView, GraphicsViewHandler> Mapper = new PropertyMapper<IGraphicsView, GraphicsViewHandler>(ViewMapper)
    {
        [nameof(IView.Background)] = MapBackground,
        [nameof(IGraphicsView.Drawable)] = MapDrawable,
        [nameof(IView.FlowDirection)] = MapFlowDirection
    };

    /// <summary>
    /// The command mapper that maps <see cref="IGraphicsView"/> commands to <see cref="GraphicsViewHandler"/> methods.
    /// </summary>
    public static CommandMapper<IGraphicsView, GraphicsViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(IGraphicsView.Invalidate)] = MapInvalidate
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsViewHandler"/> class with default mappers.
    /// </summary>
    public GraphicsViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsViewHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The custom property mapper to use, or <see langword="null"/> to use the default mapper.</param>
    public GraphicsViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsViewHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The custom property mapper to use, or <see langword="null"/> to use the default mapper.</param>
    /// <param name="commandMapper">The custom command mapper to use, or <see langword="null"/> to use the default command mapper.</param>
    public GraphicsViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <inheritdoc/>
    protected override PlatformTouchGraphicsView CreatePlatformView()
    {
        return new PlatformTouchGraphicsView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformTouchGraphicsView platformView)
    {
        platformView.Connect(VirtualView);
        base.ConnectHandler(platformView);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformTouchGraphicsView platformView)
    {
        platformView.Disconnect();
        base.DisconnectHandler(platformView);
    }

    /// <summary>
    /// Maps the Background property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the graphics view.</param>
    /// <param name="graphicsView">The virtual view to map from.</param>
    public static void MapBackground(GraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.UpdateBackground(graphicsView);
    }

    /// <summary>
    /// Maps the Drawable property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the graphics view.</param>
    /// <param name="graphicsView">The virtual view to map from.</param>
    public static void MapDrawable(GraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.UpdateDrawable(graphicsView);
    }

    /// <summary>
    /// Maps the FlowDirection property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the graphics view.</param>
    /// <param name="graphicsView">The virtual view to map from.</param>
    public static void MapFlowDirection(GraphicsViewHandler handler, IGraphicsView graphicsView)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.UpdateFlowDirection(graphicsView);
        (handler.PlatformView as PlatformTouchGraphicsView)?.Invalidate();
    }

    /// <summary>
    /// Maps the Invalidate command to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the graphics view.</param>
    /// <param name="graphicsView">The virtual view to map from.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapInvalidate(GraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
    {
        (handler.PlatformView as PlatformTouchGraphicsView)?.Invalidate();
    }
}
