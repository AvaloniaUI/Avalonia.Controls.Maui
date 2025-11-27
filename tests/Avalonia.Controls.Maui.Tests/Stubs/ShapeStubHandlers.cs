using Avalonia.Controls.Maui.Extensions;
using AvaloniaShapeHandler = Avalonia.Controls.Maui.Handlers.Shapes.ShapeViewHandler;
using ShapeHandlerInterface = global::Avalonia.Controls.Maui.Handlers.Shapes.IShapeViewHandler;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using AvaloniaRectangle = global::Avalonia.Controls.Shapes.Rectangle;
using AvaloniaEllipse = global::Avalonia.Controls.Shapes.Ellipse;
using AvaloniaLine = global::Avalonia.Controls.Shapes.Line;
using AvaloniaPolygon = global::Avalonia.Controls.Shapes.Polygon;
using AvaloniaPolyline = global::Avalonia.Controls.Shapes.Polyline;
using AvaloniaPath = global::Avalonia.Controls.Shapes.Path;
using AvaloniaRoundRectangle = Avalonia.Controls.Maui.Platform.MauiRoundRectangle;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public abstract class ShapeStubHandler<TStub, TPlatform> : ViewHandler<TStub, TPlatform>, ShapeHandlerInterface
    where TStub : class, IShapeView
    where TPlatform : global::Avalonia.Controls.Shapes.Shape
{
    protected ShapeStubHandler(IPropertyMapper mapper)
        : base(mapper)
    {
    }

    public static CommandMapper<IShapeView, ShapeHandlerInterface> StubCommandMapper { get; } = new(AvaloniaShapeHandler.CommandMapper);

    IShapeView ShapeHandlerInterface.VirtualView => VirtualView;

    global::Avalonia.Controls.Shapes.Shape ShapeHandlerInterface.PlatformView => PlatformView;
}

public class RectangleStubHandler : ShapeStubHandler<RectangleStub, AvaloniaRectangle>
{
    public static IPropertyMapper<RectangleStub, ShapeHandlerInterface> Mapper = new PropertyMapper<RectangleStub, ShapeHandlerInterface>(AvaloniaShapeHandler.Mapper)
    {
        [nameof(RectangleStub.RadiusX)] = MapRadiusX,
        [nameof(RectangleStub.RadiusY)] = MapRadiusY
    };

    public RectangleStubHandler()
        : base(Mapper)
    {
    }

    protected override AvaloniaRectangle CreatePlatformView() => new();

    public static void MapRadiusX(ShapeHandlerInterface handler, RectangleStub rectangle)
    {
        if (handler.PlatformView is AvaloniaRectangle platformView)
        {
            platformView.RadiusX = rectangle.RadiusX;
        }
    }

    public static void MapRadiusY(ShapeHandlerInterface handler, RectangleStub rectangle)
    {
        if (handler.PlatformView is AvaloniaRectangle platformView)
        {
            platformView.RadiusY = rectangle.RadiusY;
        }
    }
}

public class EllipseStubHandler : ShapeStubHandler<EllipseStub, AvaloniaEllipse>
{
    public static IPropertyMapper<EllipseStub, ShapeHandlerInterface> Mapper = new PropertyMapper<EllipseStub, ShapeHandlerInterface>(AvaloniaShapeHandler.Mapper);

    public EllipseStubHandler()
        : base(Mapper)
    {
    }

    protected override AvaloniaEllipse CreatePlatformView() => new();
}

public class LineStubHandler : ShapeStubHandler<LineStub, AvaloniaLine>
{
    public static IPropertyMapper<LineStub, ShapeHandlerInterface> Mapper = new PropertyMapper<LineStub, ShapeHandlerInterface>(AvaloniaShapeHandler.Mapper)
    {
        [nameof(LineStub.X1)] = MapX1,
        [nameof(LineStub.Y1)] = MapY1,
        [nameof(LineStub.X2)] = MapX2,
        [nameof(LineStub.Y2)] = MapY2
    };

    public LineStubHandler()
        : base(Mapper)
    {
    }

    protected override AvaloniaLine CreatePlatformView() => new();

    public static void MapX1(global::Avalonia.Controls.Maui.Handlers.Shapes.IShapeViewHandler handler, LineStub line)
    {
        if (handler.PlatformView is AvaloniaLine platformView)
        {
            platformView.StartPoint = new global::Avalonia.Point(line.X1, platformView.StartPoint.Y);
        }
    }

    public static void MapY1(global::Avalonia.Controls.Maui.Handlers.Shapes.IShapeViewHandler handler, LineStub line)
    {
        if (handler.PlatformView is AvaloniaLine platformView)
        {
            platformView.StartPoint = new global::Avalonia.Point(platformView.StartPoint.X, line.Y1);
        }
    }

    public static void MapX2(global::Avalonia.Controls.Maui.Handlers.Shapes.IShapeViewHandler handler, LineStub line)
    {
        if (handler.PlatformView is AvaloniaLine platformView)
        {
            platformView.EndPoint = new global::Avalonia.Point(line.X2, platformView.EndPoint.Y);
        }
    }

    public static void MapY2(global::Avalonia.Controls.Maui.Handlers.Shapes.IShapeViewHandler handler, LineStub line)
    {
        if (handler.PlatformView is AvaloniaLine platformView)
        {
            platformView.EndPoint = new global::Avalonia.Point(platformView.EndPoint.X, line.Y2);
        }
    }
}

public class PolygonStubHandler : ShapeStubHandler<PolygonStub, AvaloniaPolygon>
{
    public static IPropertyMapper<PolygonStub, ShapeHandlerInterface> Mapper = new PropertyMapper<PolygonStub, ShapeHandlerInterface>(AvaloniaShapeHandler.Mapper)
    {
        [nameof(PolygonStub.Points)] = MapPoints
    };

    public PolygonStubHandler()
        : base(Mapper)
    {
    }

    protected override AvaloniaPolygon CreatePlatformView() => new();

    public static void MapPoints(ShapeHandlerInterface handler, PolygonStub polygon)
    {
        if (handler.PlatformView is AvaloniaPolygon platformView)
        {
            platformView.Points.Clear();

            if (polygon.Points != null)
            {
                foreach (var point in polygon.Points)
                {
                    platformView.Points.Add(new global::Avalonia.Point(point.X, point.Y));
                }
            }
        }
    }
}

public class PolylineStubHandler : ShapeStubHandler<PolylineStub, AvaloniaPolyline>
{
    public static IPropertyMapper<PolylineStub, ShapeHandlerInterface> Mapper = new PropertyMapper<PolylineStub, ShapeHandlerInterface>(AvaloniaShapeHandler.Mapper)
    {
        [nameof(PolylineStub.Points)] = MapPoints
    };

    public PolylineStubHandler()
        : base(Mapper)
    {
    }

    protected override AvaloniaPolyline CreatePlatformView() => new();

    public static void MapPoints(ShapeHandlerInterface handler, PolylineStub polyline)
    {
        if (handler.PlatformView is AvaloniaPolyline platformView)
        {
            platformView.Points.Clear();

            if (polyline.Points != null)
            {
                foreach (var point in polyline.Points)
                {
                    platformView.Points.Add(new global::Avalonia.Point(point.X, point.Y));
                }
            }
        }
    }
}

public class PathStubHandler : ShapeStubHandler<PathStub, AvaloniaPath>
{
    public static IPropertyMapper<PathStub, ShapeHandlerInterface> Mapper = new PropertyMapper<PathStub, ShapeHandlerInterface>(AvaloniaShapeHandler.Mapper)
    {
        [nameof(PathStub.Data)] = MapData,
        [nameof(PathStub.RenderTransform)] = MapRenderTransform
    };

    public PathStubHandler()
        : base(Mapper)
    {
    }

    protected override AvaloniaPath CreatePlatformView() => new();

    protected override void ConnectHandler(AvaloniaPath platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView?.RenderTransform != null)
        {
            VirtualView.RenderTransform.PropertyChanged += OnRenderTransformPropertyChanged;
        }
    }

    protected override void DisconnectHandler(AvaloniaPath platformView)
    {
        if (VirtualView?.RenderTransform != null)
        {
            VirtualView.RenderTransform.PropertyChanged -= OnRenderTransformPropertyChanged;
        }

        base.DisconnectHandler(platformView);
    }

    private void OnRenderTransformPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        MapRenderTransform(this, VirtualView);
    }

    public static void MapData(ShapeHandlerInterface handler, PathStub path)
    {
        if (handler.PlatformView is AvaloniaPath platformView)
        {
            platformView.Data = path.Data?.ToAvaloniaGeometry();
        }
    }

    public static void MapRenderTransform(ShapeHandlerInterface handler, PathStub path)
    {
        if (handler.PlatformView is not AvaloniaPath platformView)
            return;

        if (path.RenderTransform == null)
        {
            platformView.RenderTransform = null;
            return;
        }

        var matrix = path.RenderTransform.Value;

        if (path.RenderTransform is RotateTransform rotate)
        {
            platformView.RenderTransform = new global::Avalonia.Media.RotateTransform
            {
                Angle = rotate.Angle
            };

            platformView.RenderTransformOrigin = new global::Avalonia.RelativePoint(
                rotate.CenterX,
                rotate.CenterY,
                global::Avalonia.RelativeUnit.Absolute);
        }
        else
        {
            platformView.RenderTransform = new global::Avalonia.Media.MatrixTransform(
                new global::Avalonia.Matrix(
                    matrix.M11,
                    matrix.M12,
                    matrix.M21,
                    matrix.M22,
                    matrix.OffsetX,
                    matrix.OffsetY));
        }
    }
}

public class RoundRectangleStubHandler : ShapeStubHandler<RoundRectangleStub, AvaloniaRoundRectangle>
{
    public static IPropertyMapper<RoundRectangleStub, ShapeHandlerInterface> Mapper = new PropertyMapper<RoundRectangleStub, ShapeHandlerInterface>(AvaloniaShapeHandler.Mapper)
    {
        [nameof(RoundRectangleStub.CornerRadius)] = MapCornerRadius
    };

    public RoundRectangleStubHandler()
        : base(Mapper)
    {
    }

    protected override AvaloniaRoundRectangle CreatePlatformView() => new();

    public static void MapCornerRadius(ShapeHandlerInterface handler, RoundRectangleStub roundRectangle)
    {
        if (handler.PlatformView is AvaloniaRoundRectangle platformView)
        {
            var radius = roundRectangle.CornerRadius;
            platformView.CornerRadius = new global::Avalonia.CornerRadius(
                radius.TopLeft,
                radius.TopRight,
                radius.BottomRight,
                radius.BottomLeft);
        }
    }
}
