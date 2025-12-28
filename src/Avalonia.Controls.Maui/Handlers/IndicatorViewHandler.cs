using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Maui.Controls.MauiIndicatorView;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI IndicatorView to Avalonia MauiIndicatorView mapping
/// </summary>
public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, PlatformView>
{
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

    public static CommandMapper<IIndicatorView, IndicatorViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
        };

    public IndicatorViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public IndicatorViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public IndicatorViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.Count = indicator.Count;
    }

    public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.Position = indicator.Position;
    }

    public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.HideSingle = indicator.HideSingle;
    }

    public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.MaximumVisible = indicator.MaximumVisible;
    }

    public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.IndicatorSize = indicator.IndicatorSize;
    }

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

    public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        // Check if the shape is a circle (Ellipse) or square (Rectangle)
        platformView.IsCircleShape = indicator.IndicatorsShape is Microsoft.Maui.Graphics.IShape shape &&
                                      shape.GetType().Name.Contains("Ellipse");
    }
}
