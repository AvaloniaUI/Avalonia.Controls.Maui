using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using PointerEventArgs = Avalonia.Input.PointerEventArgs;

namespace Avalonia.Controls.Maui.SkiaSharp.Views.Controls;

/// <summary>
/// Avalonia control that provides a SkiaSharp GL drawing surface for <see cref="ISKGLView"/>.
/// </summary>
public class SKGLViewControl : Control
{
    private bool _enableTouchEvents;
    private bool _ignorePixelScaling;
    private bool _hasRenderLoop;
    private DispatcherTimer? _renderTimer;
    private GRContext? _lastGRContext;
    private long _touchId;

    /// <summary>
    /// Gets or sets whether to force software (CPU) rendering instead of GPU rendering.
    /// When true, the control always creates a raster surface even if a GPU context is available.
    /// This is useful for platforms like Browser/WASM where GPU rendering may not be reliable.
    /// </summary>
    public bool ForceSoftwareRendering { get; set; }

    /// <summary>
    /// Gets or sets whether pixel scaling should be ignored when reporting canvas size.
    /// </summary>
    public bool IgnorePixelScaling
    {
        get => _ignorePixelScaling;
        set
        {
            _ignorePixelScaling = value;
            InvalidateCanvas();
        }
    }

    /// <summary>
    /// Gets or sets whether touch events are enabled on this control.
    /// </summary>
    public bool EnableTouchEvents
    {
        get => _enableTouchEvents;
        set => _enableTouchEvents = value;
    }

    /// <summary>
    /// Gets or sets whether the control continuously renders in a loop.
    /// </summary>
    public bool HasRenderLoop
    {
        get => _hasRenderLoop;
        set
        {
            if (_hasRenderLoop == value)
                return;

            _hasRenderLoop = value;

            if (_hasRenderLoop)
            {
                _renderTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, OnRenderTimerTick);
                _renderTimer.Start();
            }
            else
            {
                _renderTimer?.Stop();
            }
        }
    }

    /// <summary>
    /// Gets the current GRContext from the last render pass.
    /// </summary>
    public GRContext? GRContext => _lastGRContext;

    /// <summary>
    /// Gets or sets the callback invoked during paint surface operations.
    /// </summary>
    public Action<SKPaintGLSurfaceEventArgs>? PaintSurfaceAction { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the GRContext changes.
    /// </summary>
    public Action<GRContext?>? GRContextChangedAction { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked for touch events. Return true if handled.
    /// </summary>
    public Func<SKTouchEventArgs, bool>? TouchAction { get; set; }

    /// <summary>
    /// Invalidates the canvas, triggering a re-render on the UI thread.
    /// </summary>
    public void InvalidateCanvas()
    {
        Threading.Dispatcher.UIThread.Post(() =>
        {
            base.InvalidateVisual();
        });
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (PaintSurfaceAction == null)
            return;

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
        var operation = new SkiaGLDrawOperation(
            new Rect(bounds.Size),
            PaintSurfaceAction,
            _ignorePixelScaling,
            ForceSoftwareRendering,
            scaling,
            this);
        context.Custom(operation);
    }

    /// <summary>
    /// Updates the cached GRContext and notifies if changed.
    /// </summary>
    internal void UpdateGRContext(GRContext? context)
    {
        if (_lastGRContext != context)
        {
            _lastGRContext = context;
            GRContextChangedAction?.Invoke(context);
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _renderTimer?.Stop();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_hasRenderLoop)
        {
            _renderTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, OnRenderTimerTick);
            _renderTimer.Start();
        }
    }

    private void OnRenderTimerTick(object? sender, EventArgs e)
    {
        base.InvalidateVisual();
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!_enableTouchEvents)
            return;

        _touchId++;
        var args = CreateTouchArgs(e, SKTouchAction.Pressed, true);
        if (TouchAction?.Invoke(args) == true || args.Handled)
        {
            e.Pointer.Capture(this);
            e.Handled = true;
            // Prevent ancestor gesture recognizers (e.g. SwipeGestureRecognizer on
            // NavigationPage) from stealing capture during fast drags.
            e.PreventGestureRecognition();
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_enableTouchEvents)
            return;

        var props = e.GetCurrentPoint(this).Properties;
        var inContact = props.IsLeftButtonPressed || props.IsMiddleButtonPressed || props.IsRightButtonPressed;
        if (!inContact)
            return;

        var args = CreateTouchArgs(e, SKTouchAction.Moved, inContact);
        if (TouchAction?.Invoke(args) == true || args.Handled)
            e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_enableTouchEvents)
            return;

        e.Pointer.Capture(null);
        var args = CreateTouchArgs(e, SKTouchAction.Released, false);
        if (TouchAction?.Invoke(args) == true || args.Handled)
            e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (!_enableTouchEvents)
            return;

        var point = e.GetPosition(this);
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
        var location = _ignorePixelScaling
            ? new SKPoint((float)point.X, (float)point.Y)
            : new SKPoint((float)(point.X * scaling), (float)(point.Y * scaling));

        var wheelDelta = (int)(e.Delta.Y * 120);
        var args = new SKTouchEventArgs(
            _touchId,
            SKTouchAction.WheelChanged,
            SKMouseButton.Unknown,
            SKTouchDeviceType.Mouse,
            location,
            false,
            wheelDelta);

        if (TouchAction?.Invoke(args) == true || args.Handled)
            e.Handled = true;
    }

    private SKTouchEventArgs CreateTouchArgs(PointerEventArgs e, SKTouchAction action, bool inContact)
    {
        var point = e.GetPosition(this);
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
        var location = _ignorePixelScaling
            ? new SKPoint((float)point.X, (float)point.Y)
            : new SKPoint((float)(point.X * scaling), (float)(point.Y * scaling));

        var pointerPoint = e.GetCurrentPoint(this);
        var deviceType = pointerPoint.Pointer.Type switch
        {
            PointerType.Touch => SKTouchDeviceType.Touch,
            PointerType.Pen => SKTouchDeviceType.Pen,
            _ => SKTouchDeviceType.Mouse,
        };

        var mouseButton = SKMouseButton.Unknown;
        if (e is PointerPressedEventArgs pressed)
        {
            mouseButton = pressed.GetCurrentPoint(this).Properties.PointerUpdateKind switch
            {
                PointerUpdateKind.LeftButtonPressed => SKMouseButton.Left,
                PointerUpdateKind.MiddleButtonPressed => SKMouseButton.Middle,
                PointerUpdateKind.RightButtonPressed => SKMouseButton.Right,
                _ => SKMouseButton.Left,
            };
        }
        else if (e is PointerReleasedEventArgs released)
        {
            mouseButton = released.GetCurrentPoint(this).Properties.PointerUpdateKind switch
            {
                PointerUpdateKind.LeftButtonReleased => SKMouseButton.Left,
                PointerUpdateKind.MiddleButtonReleased => SKMouseButton.Middle,
                PointerUpdateKind.RightButtonReleased => SKMouseButton.Right,
                _ => SKMouseButton.Left,
            };
        }
        else if (inContact)
        {
            var props = pointerPoint.Properties;
            if (props.IsLeftButtonPressed) mouseButton = SKMouseButton.Left;
            else if (props.IsMiddleButtonPressed) mouseButton = SKMouseButton.Middle;
            else if (props.IsRightButtonPressed) mouseButton = SKMouseButton.Right;
        }

        return new SKTouchEventArgs(
            _touchId,
            action,
            mouseButton,
            deviceType,
            location,
            inContact);
    }

    private class SkiaGLDrawOperation : ICustomDrawOperation
    {
        private readonly Action<SKPaintGLSurfaceEventArgs> _paintAction;
        private readonly bool _ignorePixelScaling;
        private readonly bool _forceSoftwareRendering;
        private readonly double _scaling;
        private readonly SKGLViewControl _owner;

        public SkiaGLDrawOperation(
            Rect bounds,
            Action<SKPaintGLSurfaceEventArgs> paintAction,
            bool ignorePixelScaling,
            bool forceSoftwareRendering,
            double scaling,
            SKGLViewControl owner)
        {
            Bounds = bounds;
            _paintAction = paintAction;
            _ignorePixelScaling = ignorePixelScaling;
            _forceSoftwareRendering = forceSoftwareRendering;
            _scaling = scaling;
            _owner = owner;
        }

        public Rect Bounds { get; }

        public void Dispose()
        {
        }

        public bool Equals(ICustomDrawOperation? other) => other is SkiaGLDrawOperation;

        public bool HitTest(Point p) => Bounds.Contains(p);

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
                return;

            using var lease = leaseFeature.Lease();
            var canvas = lease?.SkCanvas;
            var grContext = lease?.GrContext;
            if (canvas == null)
                return;

            _owner.UpdateGRContext(grContext);

            var pixelWidth = (int)(Bounds.Width * _scaling);
            var pixelHeight = (int)(Bounds.Height * _scaling);

            if (pixelWidth <= 0 || pixelHeight <= 0)
                return;

            var useGpu = grContext != null && !_forceSoftwareRendering;

            // Use Rgba8888 for GPU surfaces — BGRA is not supported by WebGL/OpenGL ES.
            // For CPU raster, use the platform's native color type for best performance.
            var colorType = useGpu ? SKColorType.Rgba8888 : SKImageInfo.PlatformColorType;
            var rawInfo = new SKImageInfo(pixelWidth, pixelHeight, colorType, SKAlphaType.Premul);

            // Create a dedicated surface sized to our control's bounds.
            // The lease surface is the entire window — passing it to the callback
            // would expose wrong dimensions and canvas transforms to the consumer.
            // Use a GPU-backed surface when a GRContext is available, falling back to CPU raster.
            // ForceSoftwareRendering bypasses GPU even when a context is available (useful for Browser/WASM).
            using var surface = useGpu
                ? SKSurface.Create(grContext!, false, rawInfo)
                : SKSurface.Create(rawInfo);
            if (surface == null)
                return;

            var surfaceCanvas = surface.Canvas;

            SKImageInfo info;
            if (_ignorePixelScaling)
            {
                info = new SKImageInfo((int)Bounds.Width, (int)Bounds.Height, colorType, SKAlphaType.Premul);
                surfaceCanvas.Scale((float)_scaling);
            }
            else
            {
                info = rawInfo;
            }

            var renderTarget = new GRBackendRenderTarget(pixelWidth, pixelHeight, 0, 8, new GRGlFramebufferInfo(0, 0x8058)); // GL_RGBA8
            var args = new SKPaintGLSurfaceEventArgs(surface, renderTarget, GRSurfaceOrigin.BottomLeft, info, rawInfo);
            _paintAction(args);

            // Blit the result onto the lease canvas at the control's position
            surface.Flush();
            using var image = surface.Snapshot();
            canvas.Save();
            canvas.Scale(1.0f / (float)_scaling);
            canvas.DrawImage(image, 0, 0);
            canvas.Restore();
        }
    }
}
