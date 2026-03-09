#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = global::Avalonia.Controls.Shapes.Polyline;
using Polyline = Microsoft.Maui.Controls.Shapes.Polyline;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

/// <summary>Avalonia handler for IPolyline shapes.</summary>
public partial class PolylineHandler : ShapeViewHandler<Polyline, PlatformView>
{
    /// <summary>Property mapper for <see cref="PolylineHandler"/>.</summary>
    public static new IPropertyMapper<Polyline, IShapeViewHandler> Mapper = new PropertyMapper<Polyline, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(IShapeView.Shape)] = MapShape,
        [nameof(Polyline.Points)] = MapPoints,
        [nameof(Polyline.FillRule)] = MapFillRule,
    };

    /// <summary>Initializes a new instance of <see cref="PolylineHandler"/>.</summary>
    public PolylineHandler() : base(Mapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="PolylineHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    public PolylineHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
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
    /// <param name="polyline">The polyline.</param>
    public static void MapPoints(IShapeViewHandler handler, Polyline polyline)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdatePoints(polyline);
        }
    }

    /// <summary>Maps the FillRule property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="polyline">The polyline.</param>
    public static void MapFillRule(IShapeViewHandler handler, Microsoft.Maui.Controls.Shapes.Polyline polyline)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateFillRule(polyline);
        }
    }
}
