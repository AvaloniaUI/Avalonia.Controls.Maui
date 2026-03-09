using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace ControlGallery.Pages;

public partial class GraphicsViewPage : ContentPage
{
    public IDrawable BasicDrawable { get; }
    public IDrawable ShapesDrawable { get; }
    public IDrawable GradientsDrawable { get; }
    public IDrawable TextDrawable { get; }
    public IDrawable TransformationsDrawable { get; }
    public InteractiveDrawable InteractiveDrawable { get; }

    public GraphicsViewPage()
    {
        InitializeComponent();

        BasicDrawable = new BasicDrawableImpl(this);
        ShapesDrawable = new ShapesDrawableImpl(this);
        GradientsDrawable = new GradientsDrawableImpl(this);
        TextDrawable = new TextDrawableImpl(this);
        TransformationsDrawable = new TransformationsDrawableImpl(this);
        InteractiveDrawable = new InteractiveDrawable(this);

        BindingContext = this;
    }

    // Helper method to get theme-aware text color
    internal Color GetTextColor()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
    }

    // Helper method to get theme-aware label color
    internal Color GetLabelColor()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromRgb(200, 200, 200) : Color.FromRgb(80, 80, 80);
    }

    private void OnInvalidateClicked(object? sender, EventArgs e)
    {
        basicGraphicsView.Invalidate();
    }

    private void OnStartInteraction(object? sender, TouchEventArgs e)
    {
        var point = e.Touches.FirstOrDefault();
        InteractiveDrawable.StartDrawing(point);
        InteractionLabel.Text = $"Start at: ({point.X:F1}, {point.Y:F1})";
        interactiveGraphicsView.Invalidate();
    }

    private void OnDragInteraction(object? sender, TouchEventArgs e)
    {
        var point = e.Touches.FirstOrDefault();
        InteractiveDrawable.AddPoint(point);
        InteractionLabel.Text = $"Drawing at: ({point.X:F1}, {point.Y:F1})";
        interactiveGraphicsView.Invalidate();
    }

    private void OnEndInteraction(object? sender, TouchEventArgs e)
    {
        InteractiveDrawable.EndDrawing();
        InteractionLabel.Text = "Touch or drag to draw";
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        InteractiveDrawable.Clear();
        interactiveGraphicsView.Invalidate();
        InteractionLabel.Text = "Drawing cleared";
    }
}

/// <summary>
/// Basic drawable showing simple shapes and colors
/// </summary>
public class BasicDrawableImpl : IDrawable
{
    private readonly GraphicsViewPage _page;

    public BasicDrawableImpl(GraphicsViewPage page)
    {
        _page = page;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Draw a blue rectangle (vibrant color that works in both themes)
        canvas.FillColor = Color.FromRgb(100, 149, 237); // Cornflower Blue
        canvas.FillRectangle(10, 10, 100, 80);

        // Draw a red circle
        canvas.FillColor = Color.FromRgb(220, 20, 60); // Crimson
        canvas.FillEllipse(150, 10, 80, 80);

        // Draw a green rounded rectangle
        canvas.FillColor = Color.FromRgb(60, 179, 113); // Medium Sea Green
        canvas.FillRoundedRectangle(10, 120, 100, 80, 15);

        // Draw stroked shapes
        canvas.StrokeColor = Color.FromRgb(186, 85, 211); // Medium Orchid
        canvas.StrokeSize = 3;
        canvas.DrawRectangle(150, 120, 100, 80);

        canvas.StrokeColor = Color.FromRgb(255, 165, 0); // Orange
        canvas.StrokeSize = 4;
        canvas.DrawEllipse(10, 220, 80, 60);

        // Draw lines with theme-aware color
        canvas.StrokeColor = _page.GetTextColor();
        canvas.StrokeSize = 2;
        canvas.DrawLine(150, 220, 250, 280);
        canvas.DrawLine(250, 220, 150, 280);
    }
}

/// <summary>
/// Drawable showing various shapes and line styles
/// </summary>
public class ShapesDrawableImpl : IDrawable
{
    private readonly GraphicsViewPage _page;

    public ShapesDrawableImpl(GraphicsViewPage page)
    {
        _page = page;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Solid line with theme-aware color
        canvas.StrokeColor = _page.GetTextColor();
        canvas.StrokeSize = 2;
        canvas.DrawLine(10, 20, 280, 20);

        // Dashed line
        canvas.StrokeColor = Color.FromRgb(220, 20, 60); // Crimson
        canvas.StrokeSize = 2;
        canvas.StrokeDashPattern = new float[] { 5, 5 };
        canvas.DrawLine(10, 40, 280, 40);

        // Line with different caps
        canvas.StrokeDashPattern = null;
        canvas.StrokeColor = Color.FromRgb(100, 149, 237); // Cornflower Blue
        canvas.StrokeSize = 10;
        canvas.StrokeLineCap = LineCap.Butt;
        canvas.DrawLine(10, 70, 90, 70);

        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawLine(100, 70, 180, 70);

        canvas.StrokeLineCap = LineCap.Square;
        canvas.DrawLine(190, 70, 270, 70);

        // Polygons
        var path = new PathF();
        path.MoveTo(50, 100);
        path.LineTo(100, 150);
        path.LineTo(75, 200);
        path.LineTo(25, 200);
        path.LineTo(0, 150);
        path.Close();

        canvas.FillColor = Color.FromRgb(135, 206, 250); // Light Sky Blue
        canvas.FillPath(path);
        canvas.StrokeColor = Color.FromRgb(30, 144, 255); // Dodger Blue
        canvas.StrokeSize = 2;
        canvas.DrawPath(path);

        // Star shape
        var star = new PathF();
        star.MoveTo(200, 100);
        star.LineTo(215, 140);
        star.LineTo(255, 145);
        star.LineTo(225, 175);
        star.LineTo(235, 215);
        star.LineTo(200, 195);
        star.LineTo(165, 215);
        star.LineTo(175, 175);
        star.LineTo(145, 145);
        star.LineTo(185, 140);
        star.Close();

        canvas.FillColor = Color.FromRgb(255, 215, 0); // Gold
        canvas.FillPath(star);
    }
}

/// <summary>
/// Drawable showing gradients and visual effects
/// </summary>
public class GradientsDrawableImpl : IDrawable
{
    private readonly GraphicsViewPage _page;

    public GradientsDrawableImpl(GraphicsViewPage page)
    {
        _page = page;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Linear gradient
        var linearGradient = new LinearGradientPaint
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops = new PaintGradientStop[]
            {
                new PaintGradientStop(0, Colors.Blue),
                new PaintGradientStop(1, Colors.Purple)
            }
        };

        canvas.SetFillPaint(linearGradient, new RectF(10, 10, 130, 130));
        canvas.FillRectangle(10, 10, 130, 130);

        // Radial gradient
        var radialGradient = new RadialGradientPaint
        {
            Center = new Point(0.5, 0.5),
            Radius = 0.5f,
            GradientStops = new PaintGradientStop[]
            {
                new PaintGradientStop(0, Colors.Yellow),
                new PaintGradientStop(0.5f, Colors.Orange),
                new PaintGradientStop(1, Colors.Red)
            }
        };

        canvas.SetFillPaint(radialGradient, new RectF(160, 10, 130, 130));
        canvas.FillEllipse(160, 10, 130, 130);

        // Shadow effect
        canvas.SaveState();
        canvas.SetShadow(new SizeF(5, 5), 10, Colors.Gray);
        canvas.FillColor = Colors.LightGreen;
        canvas.FillRoundedRectangle(10, 160, 130, 80, 10);
        canvas.RestoreState();

        // Multiple color gradient
        var multiGradient = new LinearGradientPaint
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
            GradientStops = new PaintGradientStop[]
            {
                new PaintGradientStop(0, Colors.Red),
                new PaintGradientStop(0.25f, Colors.Yellow),
                new PaintGradientStop(0.5f, Colors.Green),
                new PaintGradientStop(0.75f, Colors.Blue),
                new PaintGradientStop(1, Colors.Purple)
            }
        };

        canvas.SetFillPaint(multiGradient, new RectF(160, 160, 130, 80));
        canvas.FillRectangle(160, 160, 130, 80);
    }
}

/// <summary>
/// Drawable showing text rendering capabilities
/// </summary>
public class TextDrawableImpl : IDrawable
{
    private readonly GraphicsViewPage _page;

    public TextDrawableImpl(GraphicsViewPage page)
    {
        _page = page;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Use theme-aware text color
        canvas.FontColor = _page.GetTextColor();
        canvas.FontSize = 18;

        // Left-aligned text
        canvas.DrawString("Left Aligned", 10, 20, HorizontalAlignment.Left);

        // Center-aligned text
        canvas.DrawString("Center Aligned", dirtyRect.Width / 2, 50, HorizontalAlignment.Center);

        // Right-aligned text
        canvas.DrawString("Right Aligned", dirtyRect.Width - 10, 80, HorizontalAlignment.Right);

        // Different font sizes
        canvas.FontSize = 12;
        canvas.FontColor = _page.GetLabelColor();
        canvas.DrawString("Small Text (12pt)", 10, 110, HorizontalAlignment.Left);

        canvas.FontSize = 24;
        canvas.FontColor = Color.FromRgb(100, 149, 237); // Cornflower Blue
        canvas.DrawString("Large Text (24pt)", 10, 140, HorizontalAlignment.Left);

        // Bold text (if supported by font)
        canvas.FontSize = 16;
        canvas.FontColor = Color.FromRgb(220, 20, 60); // Crimson
        canvas.Font = new Microsoft.Maui.Graphics.Font("Arial", 700); // Bold weight
        canvas.DrawString("Bold Text", 10, 175, HorizontalAlignment.Left);
    }
}

/// <summary>
/// Drawable showing transformations (scale, rotate, translate)
/// </summary>
public class TransformationsDrawableImpl : IDrawable
{
    private readonly GraphicsViewPage _page;

    public TransformationsDrawableImpl(GraphicsViewPage page)
    {
        _page = page;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Original rectangle
        canvas.FillColor = Color.FromRgb(100, 149, 237); // Cornflower Blue
        canvas.FillRectangle(10, 10, 60, 40);

        // Translated rectangle
        canvas.SaveState();
        canvas.Translate(100, 0);
        canvas.FillColor = Color.FromRgb(220, 20, 60); // Crimson
        canvas.FillRectangle(10, 10, 60, 40);
        canvas.RestoreState();

        // Scaled rectangle
        canvas.SaveState();
        canvas.Translate(10, 80);
        canvas.Scale(1.5f, 1.5f);
        canvas.FillColor = Color.FromRgb(60, 179, 113); // Medium Sea Green
        canvas.FillRectangle(0, 0, 60, 40);
        canvas.RestoreState();

        // Rotated rectangle (15 degrees)
        canvas.SaveState();
        canvas.Translate(150, 100);
        canvas.Rotate(15);
        canvas.FillColor = Color.FromRgb(255, 165, 0); // Orange
        canvas.FillRectangle(-30, -20, 60, 40);
        canvas.RestoreState();

        // Combined transformations
        canvas.SaveState();
        canvas.Translate(100, 200);
        canvas.Rotate(30);
        canvas.Scale(1.2f, 1.2f);
        canvas.FillColor = Color.FromRgb(186, 85, 211); // Medium Orchid
        canvas.FillRectangle(-30, -20, 60, 40);
        canvas.RestoreState();

        // Draw labels with theme-aware color
        canvas.FontColor = _page.GetLabelColor();
        canvas.FontSize = 10;
        canvas.DrawString("Original", 15, 60, HorizontalAlignment.Left);
        canvas.DrawString("Translated", 105, 60, HorizontalAlignment.Left);
        canvas.DrawString("Scaled", 15, 150, HorizontalAlignment.Left);
        canvas.DrawString("Rotated", 140, 150, HorizontalAlignment.Left);
        canvas.DrawString("Combined", 85, 245, HorizontalAlignment.Left);
    }
}

/// <summary>
/// Interactive drawable that allows drawing with touch
/// </summary>
public class InteractiveDrawable : IDrawable
{
    private readonly GraphicsViewPage _page;
    private List<List<PointF>> _strokes = new();
    private List<PointF>? _currentStroke;

    public InteractiveDrawable(GraphicsViewPage page)
    {
        _page = page;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Use theme-aware stroke color
        canvas.StrokeColor = _page.GetTextColor();
        canvas.StrokeSize = 3;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        // Draw all completed strokes
        foreach (var stroke in _strokes)
        {
            if (stroke.Count < 2)
                continue;

            var path = new PathF();
            path.MoveTo(stroke[0]);

            for (int i = 1; i < stroke.Count; i++)
            {
                path.LineTo(stroke[i]);
            }

            canvas.DrawPath(path);
        }

        // Draw current stroke
        if (_currentStroke != null && _currentStroke.Count >= 2)
        {
            var path = new PathF();
            path.MoveTo(_currentStroke[0]);

            for (int i = 1; i < _currentStroke.Count; i++)
            {
                path.LineTo(_currentStroke[i]);
            }

            canvas.DrawPath(path);
        }
    }

    public void StartDrawing(PointF point)
    {
        _currentStroke = new List<PointF> { point };
    }

    public void AddPoint(PointF point)
    {
        _currentStroke?.Add(point);
    }

    public void EndDrawing()
    {
        if (_currentStroke != null && _currentStroke.Count > 0)
        {
            _strokes.Add(_currentStroke);
            _currentStroke = null;
        }
    }

    public void Clear()
    {
        _strokes.Clear();
        _currentStroke = null;
    }
}
