#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = global::Avalonia.Controls.Shapes.Line;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

public partial class LineHandler : ShapeViewHandler<Line, PlatformView>
{
    public static new IPropertyMapper<Line, IShapeViewHandler> Mapper = new PropertyMapper<Line, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(Line.X1)] = MapX1,
        [nameof(Line.Y1)] = MapY1,
        [nameof(Line.X2)] = MapX2,
        [nameof(Line.Y2)] = MapY2,
    };

    public LineHandler() : base(Mapper)
    {
    }

    public LineHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public static void MapX1(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateX1(line);
        }
    }

    public static void MapY1(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateY1(line);
        }
    }

    public static void MapX2(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateX2(line);
        }
    }

    public static void MapY2(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateY2(line);
        }
    }
}
