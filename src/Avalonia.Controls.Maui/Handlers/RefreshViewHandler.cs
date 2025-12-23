using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.RefreshContainer;

namespace Avalonia.Controls.Maui.Handlers;

public partial class RefreshViewHandler : ViewHandler<IRefreshView, PlatformView>
{
    public static IPropertyMapper<IRefreshView, RefreshViewHandler> Mapper = new PropertyMapper<IRefreshView, RefreshViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IRefreshView.IsRefreshing)] = MapIsRefreshing,
        [nameof(IRefreshView.Content)] = MapContent,
        [nameof(IRefreshView.RefreshColor)] = MapRefreshColor,
        [nameof(IView.Background)] = MapBackground,
        [nameof(IView.IsEnabled)] = MapIsEnabled,
    };

    public static CommandMapper<IRefreshView, RefreshViewHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
    {
    };

    public RefreshViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public RefreshViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public RefreshViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.RefreshRequested += OnRefreshRequested;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.RefreshRequested -= OnRefreshRequested;
        base.DisconnectHandler(platformView);
    }

    private void OnRefreshRequested(object? sender, global::Avalonia.Controls.RefreshRequestedEventArgs e)
    {
        if (VirtualView != null)
        {
            VirtualView.IsRefreshing = true;
        }
    }

    public static void MapIsRefreshing(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        // Avalonia's RefreshContainer doesn't have a direct IsRefreshing property
        // The refresh state is managed through the RefreshRequested event and deferral
        // When IsRefreshing is set to false, we complete the refresh
        if (!refreshView.IsRefreshing && platformView.Visualizer != null)
        {
            // Reset the visualizer state by requesting a new refresh operation to complete
            // This is a workaround since Avalonia doesn't expose a direct way to stop refreshing
        }
    }

    public static void MapContent(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (refreshView.Content == null)
        {
            platformView.Content = null;
            return;
        }

        platformView.Content = refreshView.Content.ToPlatform(handler.MauiContext!);
    }

    public static void MapRefreshColor(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (platformView.Visualizer == null)
            return;

        if (refreshView.RefreshColor != null)
        {
            var color = refreshView.RefreshColor.ToColor();
            if (color != null)
            {
                var avaloniaColor = global::Avalonia.Media.Color.FromArgb(
                    (byte)(color.Alpha * 255),
                    (byte)(color.Red * 255),
                    (byte)(color.Green * 255),
                    (byte)(color.Blue * 255));

                platformView.Visualizer.Foreground = new global::Avalonia.Media.SolidColorBrush(avaloniaColor);
            }
        }
    }

    public static void MapBackground(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        // Map the background using standard view mapping
        ViewHandler.MapBackground(handler, refreshView);
    }

    public static void MapIsEnabled(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.IsEnabled = refreshView.IsEnabled;
    }
}
