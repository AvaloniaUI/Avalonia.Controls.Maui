using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Handlers;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Border;

namespace Avalonia.Controls.Maui.Compatibility.Handlers;

public class FrameHandler : ViewHandler<Microsoft.Maui.Controls.Frame, PlatformView>
{
    public static IPropertyMapper<Microsoft.Maui.Controls.Frame, FrameHandler> Mapper =
        new PropertyMapper<Microsoft.Maui.Controls.Frame, FrameHandler>(ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.Frame.Content)] = MapContent,
            [nameof(Microsoft.Maui.Controls.Frame.BorderColor)] = MapBorderColor,
            [nameof(Microsoft.Maui.Controls.Frame.CornerRadius)] = MapCornerRadius,
            [nameof(Microsoft.Maui.Controls.Frame.HasShadow)] = MapHasShadow,
            [nameof(Microsoft.Maui.Controls.Frame.Background)] = MapBackground,
            [nameof(Microsoft.Maui.Controls.Frame.BackgroundColor)] = MapBackground,
            [nameof(Microsoft.Maui.Controls.Frame.Padding)] = MapPadding,
            [nameof(Microsoft.Maui.Controls.Frame.IsClippedToBounds)] = MapIsClippedToBounds,
        };

    public static CommandMapper<Microsoft.Maui.Controls.Frame, FrameHandler> CommandMapper =
        new(ViewCommandMapper);

    public FrameHandler() : base(Mapper, CommandMapper)
    {
    }

    public FrameHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public FrameHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView
        {
            // Frame defaults to white background
            Background = Avalonia.Media.Brushes.White
        };
    }

    public static void MapContent(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateContent(frame, handler.MauiContext);
    }

    public static void MapBorderColor(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateBorderColor(frame);
    }

    public static void MapCornerRadius(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateCornerRadius(frame);
    }

    public static void MapHasShadow(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateHasShadow(frame);
    }

    public static void MapBackground(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateBackground(frame);
    }

    public static void MapPadding(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdatePadding(frame);
    }

    public static void MapIsClippedToBounds(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateIsClippedToBounds(frame);
    }
}
