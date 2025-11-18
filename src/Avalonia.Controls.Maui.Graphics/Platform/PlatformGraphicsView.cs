using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;
using System;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Platform graphics view that integrates Microsoft.Maui.Graphics IDrawable with Avalonia rendering
/// </summary>
public class PlatformGraphicsView : Control
{
    private IGraphicsView? _graphicsView;
    private IDrawable? _drawable;

    public void UpdateDrawable(IGraphicsView graphicsView)
    {
        _graphicsView = graphicsView;
        _drawable = graphicsView?.Drawable;
        InvalidateVisual();
    }

    public new void InvalidateVisual()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            base.InvalidateVisual();
        });
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_drawable == null || _graphicsView == null)
            return;

        var bounds = Bounds;
        var dirtyRect = new RectF(0, 0, (float)bounds.Width, (float)bounds.Height);

        var operation = new MauiDrawOperation(new Rect(bounds.Size), _drawable, dirtyRect);
        context.Custom(operation);
    }

    private class MauiDrawOperation : ICustomDrawOperation
    {
        private readonly IDrawable _drawable;
        private readonly RectF _dirtyRect;

        public MauiDrawOperation(Rect bounds, IDrawable drawable, RectF dirtyRect)
        {
            Bounds = bounds;
            _drawable = drawable;
            _dirtyRect = dirtyRect;
        }

        public Rect Bounds { get; }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is MauiDrawOperation;
        }

        public bool HitTest(global::Avalonia.Point p)
        {
            return Bounds.Contains(p);
        }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
                return;

            using var lease = leaseFeature.Lease();
            var canvas = lease?.SkCanvas;
            if (canvas == null)
                return;

            // Create a Microsoft.Maui.Graphics canvas from the Skia canvas
            var mauiCanvas = new SkiaCanvas
            {
                Canvas = canvas
            };

            // Save the canvas state
            canvas.Save();

            try
            {
                // Draw using the MAUI drawable
                _drawable.Draw(mauiCanvas, _dirtyRect);
            }
            finally
            {
                // Restore the canvas state
                canvas.Restore();
            }
        }
    }
}
