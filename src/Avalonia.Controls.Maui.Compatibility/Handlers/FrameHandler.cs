using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Handlers;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Border;

namespace Avalonia.Controls.Maui.Compatibility.Handlers;

/// <summary>
/// Avalonia handler for <see cref="Microsoft.Maui.Controls.Frame"/>.
/// </summary>
public class FrameHandler : ViewHandler<Microsoft.Maui.Controls.Frame, PlatformView>
{
    /// <summary>
    /// Property mapper for <see cref="FrameHandler"/>.
    /// </summary>
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

    /// <summary>
    /// Command mapper for <see cref="FrameHandler"/>.
    /// </summary>
    public static CommandMapper<Microsoft.Maui.Controls.Frame, FrameHandler> CommandMapper =
        new(ViewCommandMapper);

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameHandler"/> class.
    /// </summary>
    public FrameHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameHandler"/> class with a custom property mapper.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public FrameHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameHandler"/> class with custom mappers.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public FrameHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform view for this handler.
    /// </summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView
        {
            // Frame defaults to white background
            Background = Avalonia.Media.Brushes.White
        };
    }

    /// <summary>
    /// Maps the Content property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the Frame.</param>
    /// <param name="frame">The MAUI Frame virtual view.</param>
    public static void MapContent(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateContent(frame, handler.MauiContext);
    }

    /// <summary>
    /// Maps the BorderColor property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the Frame.</param>
    /// <param name="frame">The MAUI Frame virtual view.</param>
    public static void MapBorderColor(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateBorderColor(frame);
    }

    /// <summary>
    /// Maps the CornerRadius property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the Frame.</param>
    /// <param name="frame">The MAUI Frame virtual view.</param>
    public static void MapCornerRadius(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateCornerRadius(frame);
    }

    /// <summary>
    /// Maps the HasShadow property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the Frame.</param>
    /// <param name="frame">The MAUI Frame virtual view.</param>
    public static void MapHasShadow(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateHasShadow(frame);
    }

    /// <summary>
    /// Maps the Background property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the Frame.</param>
    /// <param name="frame">The MAUI Frame virtual view.</param>
    public static void MapBackground(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateBackground(frame);
    }

    /// <summary>
    /// Maps the Padding property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the Frame.</param>
    /// <param name="frame">The MAUI Frame virtual view.</param>
    public static void MapPadding(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdatePadding(frame);
    }

    /// <summary>
    /// Maps the IsClippedToBounds property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the Frame.</param>
    /// <param name="frame">The MAUI Frame virtual view.</param>
    public static void MapIsClippedToBounds(FrameHandler handler, Microsoft.Maui.Controls.Frame frame)
    {
        handler.PlatformView?.UpdateIsClippedToBounds(frame);
    }
}
