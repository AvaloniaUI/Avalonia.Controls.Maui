using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class BorderStub : StubBase, IBorderView, IBorderStroke, IContentView, IPadding
{
    object? IContentView.Content => Content;

    public IView? Content { get; set; }

    public IView? PresentedContent => Content;

    public Microsoft.Maui.Thickness Padding { get; set; }

    public MauiGraphics.IShape? Shape { get; set; }

    public MauiGraphics.Paint? Stroke { get; set; }

    public double StrokeThickness { get; set; }

    public MauiGraphics.LineCap StrokeLineCap { get; set; }

    public MauiGraphics.LineJoin StrokeLineJoin { get; set; }

    public float[]? StrokeDashPattern { get; set; }

    public float StrokeDashOffset { get; set; }

    public float StrokeMiterLimit { get; set; }

    public MauiGraphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
    {
        return Measure(widthConstraint, heightConstraint);
    }

    public MauiGraphics.Size CrossPlatformArrange(MauiGraphics.Rect bounds)
    {
        return Arrange(bounds);
    }
}
