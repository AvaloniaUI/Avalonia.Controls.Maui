using Avalonia.Controls.Maui.SkiaSharp.Views.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Avalonia.Controls.Maui.SkiaSharp.Views.Handlers;

/// <summary>
/// Avalonia handler for <see cref="ISKGLView"/>.
/// </summary>
public class SKGLViewHandler : Avalonia.Controls.Maui.Handlers.ViewHandler<ISKGLView, SKGLViewControl>
{
    private SKSizeI _lastCanvasSize;

    /// <summary>
    /// The property mapper that maps <see cref="ISKGLView"/> properties to <see cref="SKGLViewHandler"/> methods.
    /// </summary>
    public static IPropertyMapper<ISKGLView, SKGLViewHandler> Mapper =
        new PropertyMapper<ISKGLView, SKGLViewHandler>(ViewMapper)
        {
            [nameof(ISKGLView.EnableTouchEvents)] = MapEnableTouchEvents,
            [nameof(ISKGLView.IgnorePixelScaling)] = MapIgnorePixelScaling,
            [nameof(ISKGLView.HasRenderLoop)] = MapHasRenderLoop,
        };

    /// <summary>
    /// The command mapper that maps <see cref="ISKGLView"/> commands to <see cref="SKGLViewHandler"/> methods.
    /// </summary>
    public static CommandMapper<ISKGLView, SKGLViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
            [nameof(ISKGLView.InvalidateSurface)] = OnInvalidateSurface,
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="SKGLViewHandler"/> class with default mappers.
    /// </summary>
    public SKGLViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SKGLViewHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The custom property mapper to use, or <see langword="null"/> to use the default mapper.</param>
    public SKGLViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SKGLViewHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The custom property mapper to use, or <see langword="null"/> to use the default mapper.</param>
    /// <param name="commandMapper">The custom command mapper to use, or <see langword="null"/> to use the default command mapper.</param>
    public SKGLViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <inheritdoc/>
    protected override SKGLViewControl CreatePlatformView() => new SKGLViewControl();

    /// <inheritdoc/>
    protected override void ConnectHandler(SKGLViewControl platformView)
    {
        platformView.PaintSurfaceAction = OnPaintSurface;
        platformView.GRContextChangedAction = OnGRContextChanged;
        platformView.TouchAction = OnTouch;
        base.ConnectHandler(platformView);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(SKGLViewControl platformView)
    {
        platformView.HasRenderLoop = false;
        platformView.PaintSurfaceAction = null;
        platformView.GRContextChangedAction = null;
        platformView.TouchAction = null;
        base.DisconnectHandler(platformView);
    }

    /// <summary>
    /// Maps the InvalidateSurface command to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the GL view.</param>
    /// <param name="view">The virtual view to map from.</param>
    /// <param name="args">The command argument.</param>
    public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args)
    {
        if (!handler.PlatformView.HasRenderLoop)
            handler.PlatformView.InvalidateCanvas();
    }

    /// <summary>
    /// Maps the IgnorePixelScaling property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the GL view.</param>
    /// <param name="view">The virtual view to map from.</param>
    public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
    {
        handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
        handler.PlatformView.InvalidateCanvas();
    }

    /// <summary>
    /// Maps the HasRenderLoop property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the GL view.</param>
    /// <param name="view">The virtual view to map from.</param>
    public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view)
    {
        handler.PlatformView.HasRenderLoop = view.HasRenderLoop;
    }

    /// <summary>
    /// Maps the EnableTouchEvents property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the GL view.</param>
    /// <param name="view">The virtual view to map from.</param>
    public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view)
    {
        handler.PlatformView.EnableTouchEvents = view.EnableTouchEvents;
    }

    private void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
    {
        var newCanvasSize = e.Info.Size;
        if (_lastCanvasSize != newCanvasSize)
        {
            _lastCanvasSize = newCanvasSize;
            VirtualView?.OnCanvasSizeChanged(newCanvasSize);
        }

        VirtualView?.OnPaintSurface(e);
    }

    private void OnGRContextChanged(GRContext? context)
    {
        VirtualView?.OnGRContextChanged(context);
    }

    private bool OnTouch(SKTouchEventArgs e)
    {
        VirtualView?.OnTouch(e);
        return e.Handled;
    }
}
