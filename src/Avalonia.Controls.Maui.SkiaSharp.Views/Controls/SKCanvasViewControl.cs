using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
    private long _nextTouchId;
    private readonly Dictionary<IPointer, long> _pointerTouchIds = new();
    private WriteableBitmap? _bitmap;
    private TopLevel? _topLevel;
    private bool _paintPending;
    private bool _painting;

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
    /// Invalidates the canvas. The canvas is painted on the UI thread, outside the render pass,
    /// and requests made before the paint runs are merged into one paint. A request made while
    /// the canvas paints is painted on the next frame.
    /// </summary>
    public void InvalidateCanvas()
    {
        if (_paintPending)
            return;

        _paintPending = true;
        if (_painting && _topLevel != null)
            _topLevel.RequestAnimationFrame(_ => PaintCanvas());
        else
            Dispatcher.Post(PaintCanvas);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _topLevel = TopLevel.GetTopLevel(this);
        if (_topLevel != null)
            _topLevel.ScalingChanged += OnScalingChanged;

        InvalidateCanvas();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_topLevel != null)
        {
            _topLevel.ScalingChanged -= OnScalingChanged;
            _topLevel = null;
        }

        _bitmap?.Dispose();
        _bitmap = null;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty)
        {
            var (oldBounds, newBounds) = change.GetOldAndNewValue<Rect>();
            if (oldBounds.Size != newBounds.Size)
                InvalidateCanvas();
        }
        else if (change.Property == IsVisibleProperty && IsVisible)
        {
            InvalidateCanvas();
        }
    }

    private void OnScalingChanged(object? sender, EventArgs e) => InvalidateCanvas();

    private void PaintCanvas()
    {
        _paintPending = false;

        // A paint queued before the canvas left the window; attaching requests a new one
        if (PaintSurfaceAction == null || !IsVisible || _topLevel == null)
            return;

        var scaling = _topLevel.RenderScaling;
        var viewSize = new PixelSize((int)Bounds.Width, (int)Bounds.Height);
        var pixelSize = new PixelSize((int)(Bounds.Width * scaling), (int)(Bounds.Height * scaling));
        if (pixelSize.Width <= 0 || pixelSize.Height <= 0)
            return;

        var dpi = new Vector(96 * scaling, 96 * scaling);
        if (_bitmap == null || _bitmap.PixelSize != pixelSize || _bitmap.Dpi != dpi)
        {
            _bitmap?.Dispose();
            _bitmap = new WriteableBitmap(
                pixelSize,
                dpi,
                SKImageInfo.PlatformColorType == SKColorType.Rgba8888 ? PixelFormat.Rgba8888 : PixelFormat.Bgra8888,
                AlphaFormat.Premul);
        }

        using (var framebuffer = _bitmap.Lock())
        {
            var colorType = framebuffer.Format == PixelFormat.Rgba8888 ? SKColorType.Rgba8888 : SKColorType.Bgra8888;
            var rawInfo = new SKImageInfo(pixelSize.Width, pixelSize.Height, colorType, SKAlphaType.Premul);

            using var surface = SKSurface.Create(rawInfo, framebuffer.Address, framebuffer.RowBytes);
            if (surface == null)
                return;

            SKImageInfo info;
            if (_ignorePixelScaling)
            {
                info = rawInfo.WithSize(viewSize.Width, viewSize.Height);
                surface.Canvas.Scale((float)scaling);
            }
            else
            {
                info = rawInfo;
            }

            _painting = true;
            try
            {
                PaintSurfaceAction(new SKPaintSurfaceEventArgs(surface, info, rawInfo));
            }
            finally
            {
                _painting = false;
            }
            surface.Flush();
        }

        InvalidateVisual();
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(
            double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (PaintSurfaceAction == null)
            return;

        // Keeps the whole control hit-testable, also before the first paint
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));

        // The source rect is in pixels: Avalonia's Skia backend reads it that way, so passing
        // the bitmap's DPI-scaled Size would crop a high-DPI bitmap
        if (_bitmap != null)
        {
            var pixelSize = _bitmap.PixelSize;
            context.DrawImage(_bitmap, new Rect(0, 0, pixelSize.Width, pixelSize.Height), new Rect(_bitmap.Size));
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!_enableTouchEvents)
            return;

        var touchId = ++_nextTouchId;
        _pointerTouchIds[e.Pointer] = touchId;
        var args = CreateTouchArgs(e, SKTouchAction.Pressed, true, touchId);
        if (TouchAction?.Invoke(args) == true || args.Handled)
        {
            e.Handled = true;
            e.Pointer.Capture(this);
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
        {
            RaiseHover(e, SKTouchAction.Moved);
            return;
        }

        if (!_pointerTouchIds.TryGetValue(e.Pointer, out var touchId))
            return;

        var args = CreateTouchArgs(e, SKTouchAction.Moved, inContact, touchId);
        if (TouchAction?.Invoke(args) == true || args.Handled)
            e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (!_enableTouchEvents)
            return;

        RaiseHover(e, SKTouchAction.Entered);
    }

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (!_enableTouchEvents)
            return;

        RaiseHover(e, SKTouchAction.Exited);
    }

    private void RaiseHover(PointerEventArgs e, SKTouchAction action)
    {
        var props = e.GetCurrentPoint(this).Properties;
        var inContact = props.IsLeftButtonPressed || props.IsMiddleButtonPressed || props.IsRightButtonPressed;

        var args = CreateTouchArgs(e, action, inContact, e.Pointer.Id);
        if (TouchAction?.Invoke(args) == true || args.Handled)
            e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_enableTouchEvents)
            return;

        if (!_pointerTouchIds.Remove(e.Pointer, out var touchId))
            return;

        var args = CreateTouchArgs(e, SKTouchAction.Released, false, touchId);
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

        _pointerTouchIds.TryGetValue(e.Pointer, out var touchId);
        var wheelDelta = (int)(e.Delta.Y * 120);
        var args = new SKTouchEventArgs(
            touchId,
            SKTouchAction.WheelChanged,
            SKMouseButton.Unknown,
            SKTouchDeviceType.Mouse,
            location,
            false,
            wheelDelta);

        if (TouchAction?.Invoke(args) == true || args.Handled)
            e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (!_enableTouchEvents)
            return;

        _pointerTouchIds.Remove(e.Pointer);
    }

    private SKTouchEventArgs CreateTouchArgs(PointerEventArgs e, SKTouchAction action, bool inContact, long touchId)
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
            touchId,
            action,
            mouseButton,
            deviceType,
            location,
            inContact);
    }
}
