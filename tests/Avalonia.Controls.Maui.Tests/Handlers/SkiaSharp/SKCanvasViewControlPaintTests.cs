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

    [AvaloniaFact(DisplayName = "Invalidating the canvas while it paints paints it once per frame")]
    public void InvalidatingWhilePaintingPaintsOnNextFrame()
    {
        // Invalidates from every paint, as an animation does, up to a limit so a loop ends
        var paintCount = 0;
        SKCanvasViewControl? canvas = null;
        var control = canvas = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = _ =>
            {
                if (++paintCount < 100)
                    canvas!.InvalidateCanvas();
            },
        };

        Host(control, window =>
        {
            // The first paint asked for another, which waits for a frame
            Assert.Equal(1, paintCount);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, paintCount);

            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, paintCount);

            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(3, paintCount);
        });
    }

    [AvaloniaFact(DisplayName = "A paint requested before the canvas is removed from the window does not run")]
    public void PaintRequestedBeforeRemovalDoesNotRun()
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
            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, paintCount);
        });
    }

    [AvaloniaTheory(DisplayName = "Painted pixels appear where they were painted in the rendered frame")]
    [InlineData(1.0, false)]
    [InlineData(2.0, false)]
    [InlineData(1.0, true)]
    [InlineData(2.0, true)]
    public void PaintedPixelsAppearWhereTheyWerePainted(double scaling, bool ignorePixelScaling)
    {
        // Paints the left half of the canvas red, in the units the canvas reports
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            IgnorePixelScaling = ignorePixelScaling,
            PaintSurfaceAction = e =>
            {
                e.Surface.Canvas.Clear(SKColors.Transparent);
                using var paint = new SKPaint { Color = SKColors.Red };
                e.Surface.Canvas.DrawRect(0, 0, e.Info.Width / 2f, e.Info.Height, paint);
            },
        };

        Host(control, window =>
        {
            // The frame is in device pixels: the control covers 100 x 50 view units from the origin
            using var frame = window.CaptureRenderedFrame()!;
            using var locked = frame.Lock();
            int Px(double viewUnits) => (int)(viewUnits * scaling);

            Assert.Equal(Avalonia.Media.Colors.Red, GetPixel(locked, Px(5), Px(5)));
            Assert.Equal(Avalonia.Media.Colors.Red, GetPixel(locked, Px(45), Px(45)));
            Assert.NotEqual(Avalonia.Media.Colors.Red, GetPixel(locked, Px(55), Px(5)));
            Assert.NotEqual(Avalonia.Media.Colors.Red, GetPixel(locked, Px(95), Px(45)));
            Assert.NotEqual(Avalonia.Media.Colors.Red, GetPixel(locked, Px(150), Px(150)));
        }, scaling);
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

    [AvaloniaFact(DisplayName = "Canvas is sized in pixels at a render scaling of 2")]
    public void CanvasIsSizedInPixelsAtScaling2()
    {
        var paints = new List<(SKImageInfo Info, SKImageInfo RawInfo)>();
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = e => paints.Add((e.Info, e.RawInfo)),
        };

        Host(control, window =>
        {
            var (info, rawInfo) = paints[^1];
            Assert.Equal(new SKSizeI(200, 100), info.Size);
            Assert.Equal(new SKSizeI(200, 100), rawInfo.Size);
        }, scaling: 2);
    }

    [AvaloniaFact(DisplayName = "IgnorePixelScaling sizes the canvas in view units at a render scaling of 2")]
    public void IgnorePixelScalingSizesCanvasInViewUnitsAtScaling2()
    {
        var paints = new List<(SKImageInfo Info, SKImageInfo RawInfo, float Scale)>();
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            IgnorePixelScaling = true,
            PaintSurfaceAction = e => paints.Add((e.Info, e.RawInfo, e.Surface.Canvas.TotalMatrix.ScaleX)),
        };

        Host(control, window =>
        {
            var (info, rawInfo, scale) = paints[^1];
            Assert.Equal(new SKSizeI(100, 50), info.Size);
            Assert.Equal(new SKSizeI(200, 100), rawInfo.Size);
            Assert.Equal(2f, scale);
        }, scaling: 2);
    }

    [AvaloniaFact(DisplayName = "Changing the render scaling paints the canvas again at the new pixel size")]
    public void ChangingRenderScalingPaintsAgain()
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
            Assert.Equal(new SKSizeI(100, 50), sizes[^1]);

            window.SetRenderScaling(1.5);
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(new SKSizeI(150, 75), sizes[^1]);
        });
    }

    static void Host(SKCanvasViewControl control, Action<Window> test, double scaling = 1)
    {
        control.HorizontalAlignment = Layout.HorizontalAlignment.Left;
        control.VerticalAlignment = Layout.VerticalAlignment.Top;

        var window = new Window { Width = 200, Height = 200, Content = control };
        window.Show();
        try
        {
            if (scaling != 1)
                window.SetRenderScaling(scaling);
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
