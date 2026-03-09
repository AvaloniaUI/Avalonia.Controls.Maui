#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = global::Avalonia.Controls.Shapes.Line;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

/// <summary>Avalonia handler for ILine shapes.</summary>
public partial class LineHandler : ShapeViewHandler<Line, PlatformView>
{
    /// <summary>Property mapper for <see cref="LineHandler"/>.</summary>
    public static new IPropertyMapper<Line, IShapeViewHandler> Mapper = new PropertyMapper<Line, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(Line.X1)] = MapX1,
        [nameof(Line.Y1)] = MapY1,
        [nameof(Line.X2)] = MapX2,
        [nameof(Line.Y2)] = MapY2,
    };

    /// <summary>Initializes a new instance of <see cref="LineHandler"/>.</summary>
    public LineHandler() : base(Mapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="LineHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    public LineHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <summary>Maps the X1 property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="line">The line.</param>
    public static void MapX1(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateX1(line);
        }
    }

    /// <summary>Maps the Y1 property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="line">The line.</param>
    public static void MapY1(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateY1(line);
        }
    }

    /// <summary>Maps the X2 property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="line">The line.</param>
    public static void MapX2(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateX2(line);
        }
    }

    /// <summary>Maps the Y2 property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="line">The line.</param>
    public static void MapY2(IShapeViewHandler handler, Line line)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateY2(line);
        }
    }
}
