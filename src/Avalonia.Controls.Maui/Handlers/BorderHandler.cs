using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using PlatformView = Avalonia.Controls.Maui.Platform.BorderView;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI Border control using custom Avalonia BorderView.
/// </summary>
public class BorderHandler : ViewHandler<IBorderView, PlatformView>, IBorderHandler
{
    public static IPropertyMapper<IBorderView, IBorderHandler> Mapper =
        new PropertyMapper<IBorderView, IBorderHandler>(ViewMapper)
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

    public static CommandMapper<IBorderView, IBorderHandler> CommandMapper =
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

    IBorderView IBorderHandler.VirtualView => VirtualView;

    object IBorderHandler.PlatformView => PlatformView;

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public static void MapContent(IBorderHandler handler, IBorderView border)
    {
        if (handler is not BorderHandler borderHandler)
            return;

        borderHandler.UpdateContent();
    }

    public static void MapBackground(IBorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).Background = border.Background?.ToPlatform();
    }

    public static void MapStroke(IBorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var stroke = border.Stroke;
        ((PlatformView)handler.PlatformView).Stroke = stroke?.ToPlatform();
    }

    public static void MapStrokeThickness(IBorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).StrokeThickness = border.StrokeThickness;
    }

    public static void MapStrokeShape(IBorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).Shape = border.Shape;
    }

    public static void MapStrokeDashPattern(IBorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).StrokeDashPattern = border.StrokeDashPattern;
    }

    public static void MapStrokeDashOffset(IBorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).StrokeDashOffset = border.StrokeDashOffset;
    }

    public static void MapStrokeLineCap(IBorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).StrokeLineCap = border.StrokeLineCap;
    }

    public static void MapStrokeLineJoin(IBorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).StrokeLineJoin = border.StrokeLineJoin;
    }

    public static void MapStrokeMiterLimit(IBorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).StrokeMiterLimit = border.StrokeMiterLimit;
    }

    public static void MapPadding(IBorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var padding = border.Padding;
        ((PlatformView)handler.PlatformView).Padding = new global::Avalonia.Thickness(
            padding.Left,
            padding.Top,
            padding.Right,
            padding.Bottom);
    }

    void UpdateContent()
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        PlatformView.Child = null;

        if (VirtualView.PresentedContent is IView view)
        {
            PlatformView.Child = (global::Avalonia.Controls.Control)view.ToPlatform(MauiContext);
        }
    }
}