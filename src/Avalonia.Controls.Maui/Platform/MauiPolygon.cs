using global::Avalonia.Media;

namespace Avalonia.Controls.Maui.Platform;

internal sealed class MauiPolygon : global::Avalonia.Controls.Shapes.Polygon
{
    private Size _arrangedSize;

    static MauiPolygon()
    {
        AffectsGeometry<MauiPolygon>(StrokeProperty, StrokeThicknessProperty);
    }

    protected override Geometry CreateDefiningGeometry() =>
        ShapeAspect.Apply(base.CreateDefiningGeometry(), _arrangedSize, Stretch, ShapeAspect.GetStrokeInset(this));

    protected override Size MeasureOverride(Size availableSize) =>
        ShapeAspect.Measure(base.CreateDefiningGeometry(), availableSize, Stretch, ShapeAspect.GetStrokeInset(this));

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
