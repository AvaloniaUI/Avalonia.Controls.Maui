using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Maui.MauiBorder;

namespace Avalonia.Controls.Maui.Handlers;

public class BorderHandler : ViewHandler<IBorderView, PlatformView>
{
    public static IPropertyMapper<IBorderView, BorderHandler> Mapper =
        new PropertyMapper<IBorderView, BorderHandler>(ViewMapper)
        {
            [nameof(IBorderView.Content)] = MapContent,
            [nameof(IBorderView.Background)] = MapBackground,
            [nameof(IBorderStroke.Shape)] = MapStrokeShape,
            [nameof(IBorderStroke.Stroke)] = MapStroke,
            [nameof(IBorderStroke.StrokeThickness)] = MapStrokeThickness,
            [nameof(IBorderStroke.StrokeDashPattern)] = MapStrokeDashPattern,
            [nameof(IBorderStroke.StrokeDashOffset)] = MapStrokeDashOffset,
            [nameof(IBorderStroke.StrokeLineCap)] = MapStrokeLineCap,
            [nameof(IBorderStroke.StrokeLineJoin)] = MapStrokeLineJoin,
            [nameof(IBorderStroke.StrokeMiterLimit)] = MapStrokeMiterLimit,
            [nameof(IPadding.Padding)] = MapPadding,
        };

    public static CommandMapper<IBorderView, BorderHandler> CommandMapper =
        new(ViewCommandMapper);

    public BorderHandler() : base(Mapper, CommandMapper)
    {
    }

    public BorderHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public BorderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public static void MapContent(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateContent(border, handler.MauiContext);
    }

    public static void MapBackground(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateBackground(border);
    }

    public static void MapStroke(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStroke(border);
    }

    public static void MapStrokeThickness(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeThickness(border);
    }

    public static void MapStrokeShape(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeShape(border);
    }

    public static void MapStrokeDashPattern(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeDashPattern(border);
    }

    public static void MapStrokeDashOffset(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeDashOffset(border);
    }

    public static void MapStrokeLineCap(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeLineCap(border);
    }

    public static void MapStrokeLineJoin(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeLineJoin(border);
    }

    public static void MapStrokeMiterLimit(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeMiterLimit(border);
    }

    public static void MapPadding(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdatePadding(border);
    }
}