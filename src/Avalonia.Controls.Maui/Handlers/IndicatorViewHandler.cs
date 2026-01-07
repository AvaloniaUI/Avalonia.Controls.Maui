using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Maui.Controls.MauiIndicatorView;

namespace Avalonia.Controls.Maui.Handlers;

public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, PlatformView>, IIndicatorViewHandler
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
            [nameof(Microsoft.Maui.Controls.IndicatorView.ItemsSource)] = MapItemsSource,
            [nameof(Microsoft.Maui.Controls.IndicatorView.IndicatorTemplate)] = MapIndicatorTemplate,
        };
    
    public static CommandMapper<IIndicatorView, IIndicatorViewHandler> CommandMapper =
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
    
    IIndicatorView IIndicatorViewHandler.VirtualView => VirtualView;
    
    object IIndicatorViewHandler.PlatformView => PlatformView;

    protected override PlatformView CreatePlatformView() => new PlatformView();

    /// <summary>
    /// Connects the handler to the platform view and subscribes to property changes.
    /// </summary>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.PropertyChanged += OnPlatformPropertyChanged;
    }

    /// <summary>
    /// Disconnects the handler from the platform view and unsubscribes from property changes.
    /// </summary>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.PropertyChanged -= OnPlatformPropertyChanged;
        base.DisconnectHandler(platformView);
    }
    
    public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateCount(indicatorView);
    }

    public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdatePosition(indicatorView);
    }

    public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateHideSingle(indicatorView);
    }

    public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateMaximumVisible(indicatorView);
    }
    
    public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateIndicatorSize(indicatorView);
    }
    
    public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateIndicatorColor(indicatorView);
    }

    public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateSelectedIndicatorColor(indicatorView);
    }
    
    public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateIndicatorsShape(indicatorView);
    }
    
    public static void MapItemsSource(IIndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateItemsSource(indicatorView);
    }
    
    public static void MapIndicatorTemplate(IIndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        var mauiContext = (handler as IndicatorViewHandler)?.MauiContext;
        platformView.UpdateIndicatorTemplate(indicatorView, mauiContext);
    }
    
    private void OnPlatformPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == PlatformView.PositionProperty && VirtualView != null && PlatformView != null)
        {
            // Update the .NET MAUI virtual view when user taps an indicator
            VirtualView.Position = PlatformView.Position;
        }
    }
}
