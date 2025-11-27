#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = global::Avalonia.Controls.Shapes.Rectangle;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

public partial class RectangleHandler : ShapeViewHandler<Rectangle, PlatformView>
{
    public static new IPropertyMapper<Rectangle, IShapeViewHandler> Mapper = new PropertyMapper<Rectangle, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(Rectangle.RadiusX)] = MapRadiusX,
        [nameof(Rectangle.RadiusY)] = MapRadiusY,
    };

    public RectangleHandler() : base(Mapper)
    {
    }

    public RectangleHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public static void MapRadiusX(IShapeViewHandler handler, Rectangle rectangle)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateRadiusX(rectangle);
        }
    }

    public static void MapRadiusY(IShapeViewHandler handler, Rectangle rectangle)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateRadiusY(rectangle);
        }
    }
}
