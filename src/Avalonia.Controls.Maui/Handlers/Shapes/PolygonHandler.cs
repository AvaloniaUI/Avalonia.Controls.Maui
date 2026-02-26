#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = global::Avalonia.Controls.Shapes.Polygon;
using Polygon = Microsoft.Maui.Controls.Shapes.Polygon;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

/// <summary>Avalonia handler for IPolygon shapes.</summary>
public partial class PolygonHandler : ShapeViewHandler<Polygon, PlatformView>
{
    /// <summary>Property mapper for <see cref="PolygonHandler"/>.</summary>
    public static new IPropertyMapper<Polygon, IShapeViewHandler> Mapper = new PropertyMapper<Polygon, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(IShapeView.Shape)] = MapShape,
        [nameof(Polygon.Points)] = MapPoints,
        [nameof(Polygon.FillRule)] = MapFillRule,
    };

    /// <summary>Initializes a new instance of <see cref="PolygonHandler"/>.</summary>
    public PolygonHandler() : base(Mapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="PolygonHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    public PolygonHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <summary>Maps the Shape property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shape">The shape view.</param>
    public static new void MapShape(IShapeViewHandler handler, IShapeView shape)
    {
        // Shape handled via Points property
    }

    /// <summary>Maps the Points property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="polygon">The polygon.</param>
    public static void MapPoints(IShapeViewHandler handler, Polygon polygon)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdatePoints(polygon);
        }
    }

    /// <summary>Maps the FillRule property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="polygon">The polygon.</param>
    public static void MapFillRule(IShapeViewHandler handler, Microsoft.Maui.Controls.Shapes.Polygon polygon)
    {
        // Avalonia Polygon doesn't have a FillRule property
        // FillRule is determined by the PathGeometry used internally
        // We cannot easily set it on the shape itself
    }
}
