#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = global::Avalonia.Controls.Shapes.Polyline;
using Polyline = Microsoft.Maui.Controls.Shapes.Polyline;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

public partial class PolylineHandler : ShapeViewHandler<Polyline, PlatformView>
{
    public static new IPropertyMapper<Polyline, IShapeViewHandler> Mapper = new PropertyMapper<Polyline, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(IShapeView.Shape)] = MapShape,
        [nameof(Polyline.Points)] = MapPoints,
        [nameof(Polyline.FillRule)] = MapFillRule,
    };

    public PolylineHandler() : base(Mapper)
    {
    }

    public PolylineHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
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

    public static void MapPoints(IShapeViewHandler handler, Polyline polyline)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdatePoints(polyline);
        }
    }

    public static void MapFillRule(IShapeViewHandler handler, Microsoft.Maui.Controls.Shapes.Polyline polyline)
    {
        // Avalonia Polyline doesn't have a FillRule property
        // FillRule is determined by the PathGeometry used internally
        // We cannot easily set it on the shape itself
    }
}
