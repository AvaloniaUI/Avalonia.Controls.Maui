using Avalonia.Controls.Maui.SkiaSharp.Views.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Avalonia.Controls.Maui.Tests.Handlers.SkiaSharp;

public class SKCanvasViewControlPaintTests
{
    [AvaloniaFact(DisplayName = "Invalidating the canvas paints on the UI thread without a render pass")]
    public void InvalidatingPaintsOnUIThreadWithoutRenderPass()
    {
        var paints = new List<(bool OnUIThread, SKImageInfo Info)>();
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = e => paints.Add((Dispatcher.UIThread.CheckAccess(), e.Info)),
        };

        Host(control, window =>
        {
            paints.Clear();

            control.InvalidateCanvas();
            Dispatcher.UIThread.RunJobs();

            var paint = Assert.Single(paints);
            Assert.True(paint.OnUIThread);
            Assert.Equal(100, paint.Info.Width);
            Assert.Equal(50, paint.Info.Height);
        });
    }

    [AvaloniaFact(DisplayName = "Invalidations before a paint are merged into one paint")]
    public void InvalidationsBeforePaintAreMerged()
    {
        var paintCount = 0;
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = _ => paintCount++,
        };

        Host(control, window =>
        {
            paintCount = 0;

            control.InvalidateCanvas();
            control.InvalidateCanvas();
            control.InvalidateCanvas();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, paintCount);
        });
    }

    [AvaloniaFact(DisplayName = "Painted pixels appear in the rendered frame")]
    public void PaintedPixelsAppearInRenderedFrame()
    {
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = e => e.Surface.Canvas.Clear(SKColors.Red),
        };

        Host(control, window =>
        {
            using var frame = window.CaptureRenderedFrame()!;
            using var locked = frame.Lock();
            var pixel = GetPixel(locked, 10, 10);
            var outside = GetPixel(locked, 150, 150);

            Assert.Equal(Avalonia.Media.Colors.Red, pixel);
            Assert.NotEqual(Avalonia.Media.Colors.Red, outside);
        });
    }

    [AvaloniaFact(DisplayName = "Resizing the canvas paints it again at the new size")]
    public void ResizingPaintsAgainAtNewSize()
    {
        var sizes = new List<SKSizeI>();
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = e => sizes.Add(e.Info.Size),
        };

        Host(control, window =>
        {
            sizes.Clear();

            control.Width = 120;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(new SKSizeI(120, 50), sizes[^1]);
        });
    }

    static void Host(SKCanvasViewControl control, Action<Window> test)
    {
        control.HorizontalAlignment = Layout.HorizontalAlignment.Left;
        control.VerticalAlignment = Layout.VerticalAlignment.Top;

        var window = new Window { Width = 200, Height = 200, Content = control };
        window.Show();
        try
        {
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            test(window);
        }
        finally
        {
            window.Close();
        }
    }

    static Avalonia.Media.Color GetPixel(Avalonia.Platform.ILockedFramebuffer framebuffer, int x, int y)
    {
        var offset = y * framebuffer.RowBytes + x * 4;
        byte At(int i) => System.Runtime.InteropServices.Marshal.ReadByte(framebuffer.Address, offset + i);
        return framebuffer.Format == Avalonia.Platform.PixelFormat.Rgba8888
            ? Avalonia.Media.Color.FromArgb(At(3), At(0), At(1), At(2))
            : Avalonia.Media.Color.FromArgb(At(3), At(2), At(1), At(0));
    }
}
