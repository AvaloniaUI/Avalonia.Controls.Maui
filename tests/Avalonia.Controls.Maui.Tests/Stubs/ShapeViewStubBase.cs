using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class ShapeViewStubBase : StubBase, IShapeView
{
    public IShape? Shape { get; set; }

    public PathAspect Aspect { get; set; } = PathAspect.Stretch;

    public new Paint? Background
    {
        get => (Paint?)base.Background;
        set => base.Background = value;
    }

    public Paint? Fill { get; set; }

    public Paint? Stroke { get; set; }

    public double StrokeThickness { get; set; }

    public float[]? StrokeDashPattern { get; set; }

    public float StrokeDashOffset { get; set; }

    public LineCap StrokeLineCap { get; set; }

    public LineJoin StrokeLineJoin { get; set; }

    public float StrokeMiterLimit { get; set; }
}
