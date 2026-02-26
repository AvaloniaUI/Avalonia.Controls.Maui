using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using PlatformView = Avalonia.Controls.Maui.Controls.MauiIndicatorView;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IIndicatorView"/>.</summary>
public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, PlatformView>
{
    /// <summary>Property mapper for <see cref="IndicatorViewHandler"/>.</summary>
    public static IPropertyMapper<IIndicatorView, IndicatorViewHandler> Mapper =
        new PropertyMapper<IIndicatorView, IndicatorViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(IIndicatorView.Count)] = MapCount,
            [nameof(IIndicatorView.Position)] = MapPosition,
            [nameof(IIndicatorView.HideSingle)] = MapHideSingle,
            [nameof(IIndicatorView.MaximumVisible)] = MapMaximumVisible,
            [nameof(IIndicatorView.IndicatorSize)] = MapIndicatorSize,
            [nameof(IIndicatorView.IndicatorColor)] = MapIndicatorColor,
            [nameof(IIndicatorView.SelectedIndicatorColor)] = MapSelectedIndicatorColor,
            [nameof(IIndicatorView.IndicatorsShape)] = MapIndicatorShape,
        };

    /// <summary>Command mapper for <see cref="IndicatorViewHandler"/>.</summary>
    public static CommandMapper<IIndicatorView, IndicatorViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
        };

    /// <summary>Initializes a new instance of <see cref="IndicatorViewHandler"/>.</summary>
    public IndicatorViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="IndicatorViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public IndicatorViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="IndicatorViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public IndicatorViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <summary>Maps the Count property to the platform view.</summary>
    /// <param name="handler">The handler for the indicator view.</param>
    /// <param name="indicator">The virtual indicator view.</param>
    public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.Count = indicator.Count;
    }

    /// <summary>Maps the Position property to the platform view.</summary>
    /// <param name="handler">The handler for the indicator view.</param>
    /// <param name="indicator">The virtual indicator view.</param>
    public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.Position = indicator.Position;
    }

    /// <summary>Maps the HideSingle property to the platform view.</summary>
    /// <param name="handler">The handler for the indicator view.</param>
    /// <param name="indicator">The virtual indicator view.</param>
    public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.HideSingle = indicator.HideSingle;
    }

    /// <summary>Maps the MaximumVisible property to the platform view.</summary>
    /// <param name="handler">The handler for the indicator view.</param>
    /// <param name="indicator">The virtual indicator view.</param>
    public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.MaximumVisible = indicator.MaximumVisible;
    }

    /// <summary>Maps the IndicatorSize property to the platform view.</summary>
    /// <param name="handler">The handler for the indicator view.</param>
    /// <param name="indicator">The virtual indicator view.</param>
    public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.IndicatorSize = indicator.IndicatorSize;
    }

    /// <summary>Maps the IndicatorColor property to the platform view.</summary>
    /// <param name="handler">The handler for the indicator view.</param>
    /// <param name="indicator">The virtual indicator view.</param>
    public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (indicator.IndicatorColor != null)
        {
            var color = indicator.IndicatorColor.ToColor();
            if (color != null)
            {
                var avaloniaColor = global::Avalonia.Media.Color.FromArgb(
                    (byte)(color.Alpha * 255),
                    (byte)(color.Red * 255),
                    (byte)(color.Green * 255),
                    (byte)(color.Blue * 255));

                platformView.IndicatorColor = new global::Avalonia.Media.SolidColorBrush(avaloniaColor);
            }
        }
    }

    /// <summary>Maps the SelectedIndicatorColor property to the platform view.</summary>
    /// <param name="handler">The handler for the indicator view.</param>
    /// <param name="indicator">The virtual indicator view.</param>
    public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (indicator.SelectedIndicatorColor != null)
        {
            var color = indicator.SelectedIndicatorColor.ToColor();
            if (color != null)
            {
                var avaloniaColor = global::Avalonia.Media.Color.FromArgb(
                    (byte)(color.Alpha * 255),
                    (byte)(color.Red * 255),
                    (byte)(color.Green * 255),
                    (byte)(color.Blue * 255));

                platformView.SelectedIndicatorColor = new global::Avalonia.Media.SolidColorBrush(avaloniaColor);
            }
        }
    }

    /// <summary>Maps the IndicatorsShape property to the platform view.</summary>
    /// <param name="handler">The handler for the indicator view.</param>
    /// <param name="indicator">The virtual indicator view.</param>
    public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        // Check if the shape is a circle (Ellipse) or square (Rectangle)
        platformView.IsCircleShape = indicator.IndicatorsShape is Microsoft.Maui.Graphics.IShape shape &&
                                      shape.GetType().Name.Contains("Ellipse");
    }
}
