#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiBoxView;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

public partial class BoxViewHandler : ViewHandler<IShapeView, PlatformView>
{
    public static IPropertyMapper<IShapeView, IViewHandler> Mapper = new PropertyMapper<IShapeView, IViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IShapeView.Fill)] = MapFill,
        [nameof(IShapeView.Stroke)] = MapStroke,
        [nameof(IShapeView.StrokeThickness)] = MapStrokeThickness,
    };

    public static CommandMapper<IShapeView, IViewHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public BoxViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public BoxViewHandler(IPropertyMapper mapper, CommandMapper commandMapper = null)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public static void MapFill(IViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Fill = shapeView.Fill?.ToAvaloniaBrush();
        }
    }

    public static void MapStroke(IViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Stroke = shapeView.Stroke?.ToAvaloniaBrush();
        }
    }

    public static void MapStrokeThickness(IViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.StrokeThickness = shapeView.StrokeThickness;
        }
    }

    public static void MapCornerRadius(IViewHandler handler, IShapeView shapeView)
    {
        if (handler.PlatformView is PlatformView platformView && shapeView is Microsoft.Maui.Controls.ICornerElement cornerElement)
        {
            var radius = cornerElement.CornerRadius;
            platformView.CornerRadius = new global::Avalonia.CornerRadius(
                radius.TopLeft,
                radius.TopRight,
                radius.BottomRight,
                radius.BottomLeft);
        }
    }
}