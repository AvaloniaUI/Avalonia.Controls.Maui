using Avalonia.Controls.Maui.SkiaSharp.Views.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Avalonia.Controls.Maui.SkiaSharp.Views.Handlers;

/// <summary>
/// Avalonia handler for <see cref="ISKCanvasView"/>.
/// </summary>
public class SKCanvasViewHandler : Avalonia.Controls.Maui.Handlers.ViewHandler<ISKCanvasView, SKCanvasViewControl>
{
    private SKSizeI _lastCanvasSize;

    /// <summary>
    /// The property mapper that maps <see cref="ISKCanvasView"/> properties to <see cref="SKCanvasViewHandler"/> methods.
    /// </summary>
    public static IPropertyMapper<ISKCanvasView, SKCanvasViewHandler> Mapper =
        new PropertyMapper<ISKCanvasView, SKCanvasViewHandler>(ViewMapper)
        {
            [nameof(ISKCanvasView.EnableTouchEvents)] = MapEnableTouchEvents,
            [nameof(ISKCanvasView.IgnorePixelScaling)] = MapIgnorePixelScaling,
        };

    /// <summary>
    /// The command mapper that maps <see cref="ISKCanvasView"/> commands to <see cref="SKCanvasViewHandler"/> methods.
    /// </summary>
    public static CommandMapper<ISKCanvasView, SKCanvasViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
            [nameof(ISKCanvasView.InvalidateSurface)] = OnInvalidateSurface,
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="SKCanvasViewHandler"/> class with default mappers.
    /// </summary>
    public SKCanvasViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SKCanvasViewHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The custom property mapper to use, or <see langword="null"/> to use the default mapper.</param>
    public SKCanvasViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SKCanvasViewHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The custom property mapper to use, or <see langword="null"/> to use the default mapper.</param>
    /// <param name="commandMapper">The custom command mapper to use, or <see langword="null"/> to use the default command mapper.</param>
    public SKCanvasViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <inheritdoc/>
    protected override SKCanvasViewControl CreatePlatformView() => new SKCanvasViewControl();

    /// <inheritdoc/>
    protected override void ConnectHandler(SKCanvasViewControl platformView)
    {
        platformView.PaintSurfaceAction = OnPaintSurface;
        platformView.TouchAction = OnTouch;
        base.ConnectHandler(platformView);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(SKCanvasViewControl platformView)
    {
        platformView.PaintSurfaceAction = null;
        platformView.TouchAction = null;
        base.DisconnectHandler(platformView);
    }

    /// <summary>
    /// Maps the InvalidateSurface command to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the canvas view.</param>
    /// <param name="canvasView">The virtual view to map from.</param>
    /// <param name="args">The command argument.</param>
    public static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView canvasView, object? args)
    {
        handler.PlatformView?.InvalidateCanvas();
    }

    /// <summary>
    /// Maps the IgnorePixelScaling property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the canvas view.</param>
    /// <param name="canvasView">The virtual view to map from.</param>
    public static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView canvasView)
    {
        handler.PlatformView.IgnorePixelScaling = canvasView.IgnorePixelScaling;
    }

    /// <summary>
    /// Maps the EnableTouchEvents property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the canvas view.</param>
    /// <param name="canvasView">The virtual view to map from.</param>
    public static void MapEnableTouchEvents(SKCanvasViewHandler handler, ISKCanvasView canvasView)
    {
        handler.PlatformView.EnableTouchEvents = canvasView.EnableTouchEvents;
    }

    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var newCanvasSize = e.Info.Size;
        if (_lastCanvasSize != newCanvasSize)
        {
            _lastCanvasSize = newCanvasSize;
            VirtualView?.OnCanvasSizeChanged(newCanvasSize);
        }

        VirtualView?.OnPaintSurface(e);
    }

    private bool OnTouch(SKTouchEventArgs e)
    {
        VirtualView?.OnTouch(e);
        return e.Handled;
    }
}
