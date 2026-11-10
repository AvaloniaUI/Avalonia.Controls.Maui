using global::Avalonia.Media;

namespace Avalonia.Controls.Maui.Platform;

internal sealed class MauiLine : global::Avalonia.Controls.Shapes.Line
{
    private Size _arrangedSize;

    static MauiLine()
    {
        AffectsGeometry<MauiLine>(StrokeProperty, StrokeThicknessProperty);
    }

    protected override Geometry CreateDefiningGeometry() =>
        ShapeAspect.Apply(base.CreateDefiningGeometry(), _arrangedSize, Stretch, ShapeAspect.GetStrokeInset(this));

    protected override Size MeasureOverride(Size availableSize) =>
        ShapeAspect.Measure(base.CreateDefiningGeometry(), availableSize, Stretch);

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_arrangedSize != finalSize)
        {
            _arrangedSize = finalSize;
            InvalidateGeometry();
        }

        return DefiningGeometry is null ? default : finalSize;
    }
}
