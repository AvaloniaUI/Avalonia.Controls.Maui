using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using PointerEventArgs = Avalonia.Input.PointerEventArgs;

namespace Avalonia.Controls.Maui.SkiaSharp.Views.Controls;

/// <summary>
/// Avalonia control that provides a SkiaSharp raster drawing surface for <see cref="ISKCanvasView"/>.
/// </summary>
public class SKCanvasViewControl : Control
{
    private bool _enableTouchEvents;
    private bool _ignorePixelScaling;
    private long _touchId;

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
    /// Gets or sets the callback invoked during paint surface operations.
    /// </summary>
    public Action<SKPaintSurfaceEventArgs>? PaintSurfaceAction { get; set; }

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
        var operation = new SkiaCanvasDrawOperation(
            new Rect(bounds.Size),
            PaintSurfaceAction,
            _ignorePixelScaling,
            scaling);
        context.Custom(operation);
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
            e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_enableTouchEvents)
            return;

        var props = e.GetCurrentPoint(this).Properties;
        var inContact = props.IsLeftButtonPressed || props.IsMiddleButtonPressed || props.IsRightButtonPressed;
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

        var args = CreateTouchArgs(e, SKTouchAction.Released, false);
        if (TouchAction?.Invoke(args) == true || args.Handled)
            e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (!_enableTouchEvents)
            return;

        var args = CreateTouchArgs(e, SKTouchAction.Entered, false);
        if (TouchAction?.Invoke(args) == true || args.Handled)
            e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (!_enableTouchEvents)
            return;

        var args = CreateTouchArgs(e, SKTouchAction.Exited, false);
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

    private class SkiaCanvasDrawOperation : ICustomDrawOperation
    {
        private readonly Action<SKPaintSurfaceEventArgs> _paintAction;
        private readonly bool _ignorePixelScaling;
        private readonly double _scaling;

        public SkiaCanvasDrawOperation(
            Rect bounds,
            Action<SKPaintSurfaceEventArgs> paintAction,
            bool ignorePixelScaling,
            double scaling)
        {
            Bounds = bounds;
            _paintAction = paintAction;
            _ignorePixelScaling = ignorePixelScaling;
            _scaling = scaling;
        }

        public Rect Bounds { get; }

        public void Dispose()
        {
        }

        public bool Equals(ICustomDrawOperation? other) => other is SkiaCanvasDrawOperation;

        public bool HitTest(Point p) => Bounds.Contains(p);

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
                return;

            using var lease = leaseFeature.Lease();
            var canvas = lease?.SkCanvas;
            if (canvas == null)
                return;

            var pixelWidth = (int)(Bounds.Width * _scaling);
            var pixelHeight = (int)(Bounds.Height * _scaling);

            if (pixelWidth <= 0 || pixelHeight <= 0)
                return;

            var rawInfo = new SKImageInfo(pixelWidth, pixelHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

            // Create a dedicated surface sized to our control's bounds.
            // The lease surface is the entire window — passing it to the callback
            // would expose wrong dimensions and canvas transforms to the consumer.
            using var surface = SKSurface.Create(rawInfo);
            if (surface == null)
                return;

            var surfaceCanvas = surface.Canvas;

            SKImageInfo info;
            if (_ignorePixelScaling)
            {
                info = new SKImageInfo((int)Bounds.Width, (int)Bounds.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                surfaceCanvas.Scale((float)_scaling);
            }
            else
            {
                info = rawInfo;
            }

            var args = new SKPaintSurfaceEventArgs(surface, info, rawInfo);
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
