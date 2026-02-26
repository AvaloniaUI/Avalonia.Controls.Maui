using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Maui.MauiBorder;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IBorderView"/>.</summary>
public class BorderHandler : ViewHandler<IBorderView, PlatformView>
{
    /// <summary>Property mapper for <see cref="BorderHandler"/>.</summary>
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

    /// <summary>Command mapper for <see cref="BorderHandler"/>.</summary>
    public static CommandMapper<IBorderView, BorderHandler> CommandMapper =
        new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="BorderHandler"/>.</summary>
    public BorderHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="BorderHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public BorderHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="BorderHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public BorderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <summary>Maps the Content property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapContent(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateContent(border, handler.MauiContext);
    }

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapBackground(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateBackground(border);
    }

    /// <summary>Maps the Stroke property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapStroke(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStroke(border);
    }

    /// <summary>Maps the StrokeThickness property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapStrokeThickness(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeThickness(border);
    }

    /// <summary>Maps the StrokeShape property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapStrokeShape(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeShape(border);
    }

    /// <summary>Maps the StrokeDashPattern property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapStrokeDashPattern(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeDashPattern(border);
    }

    /// <summary>Maps the StrokeDashOffset property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapStrokeDashOffset(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeDashOffset(border);
    }

    /// <summary>Maps the StrokeLineCap property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapStrokeLineCap(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeLineCap(border);
    }

    /// <summary>Maps the StrokeLineJoin property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapStrokeLineJoin(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeLineJoin(border);
    }

    /// <summary>Maps the StrokeMiterLimit property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapStrokeMiterLimit(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateStrokeMiterLimit(border);
    }

    /// <summary>Maps the Padding property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="border">The virtual view.</param>
    public static void MapPadding(BorderHandler handler, IBorderView border)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdatePadding(border);
    }
}