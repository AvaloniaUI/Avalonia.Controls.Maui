#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = global::Avalonia.Controls.Shapes.Rectangle;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

/// <summary>Avalonia handler for IRectangle shapes.</summary>
public partial class RectangleHandler : ShapeViewHandler<Rectangle, PlatformView>
{
    /// <summary>Property mapper for <see cref="RectangleHandler"/>.</summary>
    public static new IPropertyMapper<Rectangle, IShapeViewHandler> Mapper = new PropertyMapper<Rectangle, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(Rectangle.RadiusX)] = MapRadiusX,
        [nameof(Rectangle.RadiusY)] = MapRadiusY,
    };

    /// <summary>Initializes a new instance of <see cref="RectangleHandler"/>.</summary>
    public RectangleHandler() : base(Mapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="RectangleHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    public RectangleHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <summary>Maps the RadiusX property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="rectangle">The rectangle.</param>
    public static void MapRadiusX(IShapeViewHandler handler, Rectangle rectangle)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateRadiusX(rectangle);
        }
    }

    /// <summary>Maps the RadiusY property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="rectangle">The rectangle.</param>
    public static void MapRadiusY(IShapeViewHandler handler, Rectangle rectangle)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateRadiusY(rectangle);
        }
    }
}
