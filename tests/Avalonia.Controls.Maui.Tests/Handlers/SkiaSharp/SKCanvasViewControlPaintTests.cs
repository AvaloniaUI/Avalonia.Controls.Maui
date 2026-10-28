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

    [AvaloniaFact(DisplayName = "Invalidating the canvas from another thread paints it on the UI thread")]
    public void InvalidatingFromAnotherThreadPaintsOnUIThread()
    {
        // The first paint invalidates the canvas from another thread while it is painting
        var paints = new List<bool>();
        Exception? threadException = null;
        SKCanvasViewControl? canvas = null;
        var control = canvas = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = _ =>
            {
                paints.Add(Dispatcher.UIThread.CheckAccess());
                if (paints.Count == 1)
                {
                    // A thread of its own; waiting on a task could run it on this thread
                    var thread = new Thread(() =>
                    {
                        try
                        {
                            canvas!.InvalidateCanvas();
                        }
                        catch (Exception ex)
                        {
                            threadException = ex;
                        }
                    });
                    thread.Start();
                    thread.Join();
                }
            },
        };

        Host(control, window =>
        {
            Dispatcher.UIThread.RunJobs();

            Assert.Null(threadException);
            Assert.Equal(2, paints.Count);
            Assert.All(paints, Assert.True);
        });
    }

    [AvaloniaFact(DisplayName = "Input is handled before a paint requested from another thread")]
    public void InputIsHandledBeforePaintFromAnotherThread()
    {
        // A thread that keeps invalidating the canvas, such as a game loop, must not hold up input
        var events = new List<string>();
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = _ => events.Add("paint"),
        };

        Host(control, window =>
        {
            events.Clear();

            var thread = new Thread(control.InvalidateCanvas);
            thread.Start();
            thread.Join();
            Dispatcher.UIThread.Post(() => events.Add("input"), DispatcherPriority.Input);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(new[] { "input", "paint" }, events);
        });
    }

    [AvaloniaFact(DisplayName = "A canvas in a hidden parent paints once the parent is shown")]
    public void CanvasInHiddenParentPaintsWhenShown()
    {
        var paintCount = 0;
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = _ => paintCount++,
        };
        var parent = new Border { Child = control };

        var window = new Window { Width = 200, Height = 200, Content = new Panel { Children = { parent } } };
        window.Show();
        try
        {
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            parent.IsVisible = false;
            window.UpdateLayout();
            paintCount = 0;

            control.InvalidateCanvas();
            Dispatcher.UIThread.RunJobs();
            control.InvalidateCanvas();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, paintCount);

            parent.IsVisible = true;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, paintCount);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact(DisplayName = "Removing the canvas during its paint lets the paint finish and paints it again when added back")]
    public void RemovingDuringPaintLetsPaintFinish()
    {
        // The first paint removes the canvas from its window and then keeps drawing
        var paintCount = 0;
        Window? window = null;
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = e =>
            {
                if (++paintCount == 1)
                    window!.Content = null;

                using var paint = new SKPaint { Color = SKColors.Red };
                e.Surface.Canvas.DrawRect(0, 0, e.Info.Width, e.Info.Height, paint);
            },
        };
        control.HorizontalAlignment = Layout.HorizontalAlignment.Left;
        control.VerticalAlignment = Layout.VerticalAlignment.Top;

        window = new Window { Width = 200, Height = 200, Background = Avalonia.Media.Brushes.White, Content = control };
        window.Show();
        try
        {
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, paintCount);
            Assert.Null(window.Content);

            window.Content = control;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, paintCount);

            using var frame = window.CaptureRenderedFrame()!;
            using var locked = frame.Lock();
            Assert.Equal(Avalonia.Media.Colors.Red, GetPixel(locked, 50, 25));
        }
        finally
        {
            window.Close();
        }
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

    [AvaloniaFact(DisplayName = "The canvas is drawn at its size when a resize and a scaling change keep its pixel size")]
    public void CanvasIsDrawnAtItsSizeWhenPixelSizeIsUnchanged()
    {
        var control = new SKCanvasViewControl
        {
            Width = 100,
            Height = 50,
            PaintSurfaceAction = e => e.Surface.Canvas.Clear(SKColors.Red),
        };

        Host(control, window =>
        {
            // 100 x 50 at a scaling of 1 and 50 x 25 at a scaling of 2 are both 100 x 50 pixels.
            // The resize is made with the scaling change so that one paint sees both.
            window.ScalingChanged += (_, _) =>
            {
                control.Width = 50;
                control.Height = 25;
            };
            window.SetRenderScaling(2);
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            using var frame = window.CaptureRenderedFrame()!;
            using var locked = frame.Lock();
            Assert.Equal(Avalonia.Media.Colors.Red, GetPixel(locked, 90, 40));
            Assert.NotEqual(Avalonia.Media.Colors.Red, GetPixel(locked, 110, 10));
            Assert.NotEqual(Avalonia.Media.Colors.Red, GetPixel(locked, 10, 60));
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
