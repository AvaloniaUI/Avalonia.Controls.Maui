#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
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
            platformView.StartPoint = new global::Avalonia.Point(line.X1, platformView.StartPoint.Y);
        }
    }

    public static void MapY1(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.StartPoint = new global::Avalonia.Point(platformView.StartPoint.X, line.Y1);
        }
    }

    public static void MapX2(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.EndPoint = new global::Avalonia.Point(line.X2, platformView.EndPoint.Y);
        }
    }

    public static void MapY2(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.EndPoint = new global::Avalonia.Point(platformView.EndPoint.X, line.Y2);
        }
    }
}