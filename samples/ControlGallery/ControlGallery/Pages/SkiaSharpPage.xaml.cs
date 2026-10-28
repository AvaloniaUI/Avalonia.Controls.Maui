using System.Collections.Concurrent;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace ControlGallery.Pages;

public partial class SkiaSharpPage : ContentPage
{
    // Written on the UI thread and read by SKGLView, which paints on the render thread
    private readonly ConcurrentDictionary<object, SKTouchEventArgs> _lastTouches = new();
    private IDispatcherTimer? _animationTimer;
    private float _animationAngle;
    private int _paintCount;

    public SkiaSharpPage()
    {
        InitializeComponent();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopAnimation();
    }

    private void OnIgnorePixelScalingToggled(object? sender, ToggledEventArgs e)
    {
        ResolutionCanvas.IgnorePixelScaling = e.Value;
        ResolutionCanvas.InvalidateSurface();
    }

    private void OnResolutionPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        // One-unit lines every ten units, in the units the canvas is sized in
        using var line = new SKPaint { Color = SKColors.SteelBlue, StrokeWidth = 1, IsAntialias = false };
        for (var x = 0.5f; x < e.Info.Width; x += 10)
            canvas.DrawLine(x, 0, x, e.Info.Height, line);
        for (var y = 0.5f; y < e.Info.Height; y += 10)
            canvas.DrawLine(0, y, e.Info.Width, y, line);

        var view = (SKCanvasView)sender!;
        ResolutionLabel.Text = $"Canvas size: {e.Info.Width} x {e.Info.Height} " +
            $"({(view.IgnorePixelScaling ? "view units" : "pixels")}), " +
            $"view size: {view.Width:0} x {view.Height:0}";
    }

    private void OnViewTouch(object? sender, SKTouchEventArgs e)
    {
        if (sender is null)
            return;

        _lastTouches[sender] = e;
        e.Handled = true;

        var text = $"{e.ActionType} at ({e.Location.X:0}, {e.Location.Y:0}), " +
            $"{(e.InContact ? "in contact" : "not in contact")}, {e.DeviceType}";
        if (sender == TouchCanvas)
        {
            TouchCanvasLabel.Text = text;
            TouchCanvas.InvalidateSurface();
        }
        else
        {
            TouchGLViewLabel.Text = text;
            TouchGLView.InvalidateSurface();
        }
    }

    private void OnTouchCanvasPaintSurface(object? sender, SKPaintSurfaceEventArgs e) =>
        DrawTouch(e.Surface.Canvas, e.Info, sender);

    private void OnTouchGLViewPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e) =>
        DrawTouch(e.Surface.Canvas, e.Info, sender);

    private void DrawTouch(SKCanvas canvas, SKImageInfo info, object? sender)
    {
        canvas.Clear(new SKColor(0xF0, 0xF4, 0xF8));

        if (sender is null || !_lastTouches.TryGetValue(sender, out var touch) || touch.ActionType == SKTouchAction.Exited)
            return;

        // A crosshair at the last location, filled while the pointer is in contact
        using var paint = new SKPaint
        {
            Color = touch.InContact ? SKColors.OrangeRed : SKColors.SteelBlue,
            StrokeWidth = 2,
            IsAntialias = true,
        };
        canvas.DrawLine(touch.Location.X, 0, touch.Location.X, info.Height, paint);
        canvas.DrawLine(0, touch.Location.Y, info.Width, touch.Location.Y, paint);
        paint.Style = touch.InContact ? SKPaintStyle.Fill : SKPaintStyle.Stroke;
        canvas.DrawCircle(touch.Location, 12, paint);
    }

    private void OnPropertiesVisibleToggled(object? sender, ToggledEventArgs e) =>
        PropertiesCanvas.IsVisible = e.Value;

    private void OnPropertiesOpacityChanged(object? sender, ValueChangedEventArgs e)
    {
        PropertiesCanvas.Opacity = e.NewValue;
        PropertiesOpacityLabel.Text = $"Opacity: {e.NewValue:0.0}";
    }

    private void OnPropertiesAlignmentClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        PropertiesCanvas.HorizontalOptions = button.Text switch
        {
            "Start" => LayoutOptions.Start,
            "End" => LayoutOptions.End,
            "Fill" => LayoutOptions.Fill,
            _ => LayoutOptions.Center,
        };
        PropertiesCanvas.WidthRequest = button.Text == "Fill" ? -1 : 120;
    }

    private void OnPropertiesPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint { Color = SKColors.MediumSeaGreen, IsAntialias = true };
        canvas.DrawRoundRect(new SKRect(0, 0, e.Info.Width, e.Info.Height), 16, 16, paint);
    }

    private void OnAnimationClicked(object? sender, EventArgs e)
    {
        if (_animationTimer is { IsRunning: true })
        {
            StopAnimation();
            return;
        }

        _animationTimer ??= CreateAnimationTimer();
        _animationTimer.Start();
        AnimationButton.Text = "Stop Animation";
    }

    private IDispatcherTimer CreateAnimationTimer()
    {
        var timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(16);
        timer.Tick += (_, _) =>
        {
            _animationAngle = (_animationAngle + 4) % 360;
            AnimatedCanvas.InvalidateSurface();
        };
        return timer;
    }

    private void StopAnimation()
    {
        _animationTimer?.Stop();
        AnimationButton.Text = "Start Animation";
    }

    private void OnAnimatedPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        var size = Math.Min(e.Info.Width, e.Info.Height) * 0.8f;
        var bounds = SKRect.Create((e.Info.Width - size) / 2, (e.Info.Height - size) / 2, size, size);
        using var paint = new SKPaint
        {
            Color = SKColors.MediumPurple,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = size / 10,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true,
        };
        canvas.DrawArc(bounds, _animationAngle, 270, false, paint);

        _paintCount++;
        PaintCountLabel.Text = $"Painted {_paintCount} times";
    }
}
