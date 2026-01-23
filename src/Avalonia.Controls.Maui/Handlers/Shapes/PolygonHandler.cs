#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = global::Avalonia.Controls.Shapes.Polygon;
using Polygon = Microsoft.Maui.Controls.Shapes.Polygon;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

public partial class PolygonHandler : ShapeViewHandler<Polygon, PlatformView>
{
    public static new IPropertyMapper<Polygon, IShapeViewHandler> Mapper = new PropertyMapper<Polygon, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(IShapeView.Shape)] = MapShape,
        [nameof(Polygon.Points)] = MapPoints,
        [nameof(Polygon.FillRule)] = MapFillRule,
    };

    public PolygonHandler() : base(Mapper)
    {
    }

    public PolygonHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public static new void MapShape(IShapeViewHandler handler, IShapeView shape)
    {
        // Shape handled via Points property
    }

    public static void MapPoints(IShapeViewHandler handler, Polygon polygon)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdatePoints(polygon);
        }
    }

    public static void MapFillRule(IShapeViewHandler handler, Microsoft.Maui.Controls.Shapes.Polygon polygon)
    {
        // Avalonia Polygon doesn't have a FillRule property
        // FillRule is determined by the PathGeometry used internally
        // We cannot easily set it on the shape itself
    }
}
