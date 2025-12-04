using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class GraphicsViewStub : StubBase, IGraphicsView
{
    private IDrawable? _drawable;
    private Paint? _background;

    public IDrawable Drawable
    {
        get => _drawable!;
        set => SetProperty(ref _drawable, value);
    }

    public new Paint? Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
    }

    public void Invalidate()
    {
        Handler?.Invoke(nameof(IGraphicsView.Invalidate));
    }

    public void StartInteraction(PointF[] touchPoints)
    {
        // Stub implementation for testing
    }

    public void DragInteraction(PointF[] touchPoints)
    {
        // Stub implementation for testing
    }

    public void EndInteraction(PointF[] touchPoints, bool isInsideBounds)
    {
        // Stub implementation for testing
    }

    public void CancelInteraction()
    {
        // Stub implementation for testing
    }

    public void StartHoverInteraction(PointF[] hoverPoints)
    {
        // Stub implementation for testing
    }

    public void MoveHoverInteraction(PointF[] hoverPoints)
    {
        // Stub implementation for testing
    }

    public void EndHoverInteraction()
    {
        // Stub implementation for testing
    }
}

/// <summary>
/// Simple drawable for testing purposes.
/// </summary>
public class TestDrawable : IDrawable
{
    public Color? FillColor { get; set; } = Colors.Blue;
    public RectF DrawRect { get; set; } = new RectF(10, 10, 100, 100);
    public bool WasDrawn { get; private set; }
    public int DrawCount { get; private set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        WasDrawn = true;
        DrawCount++;

        if (FillColor != null)
        {
            canvas.FillColor = FillColor;
            canvas.FillRectangle(DrawRect);
        }
    }

    public void Reset()
    {
        WasDrawn = false;
        DrawCount = 0;
    }
}
