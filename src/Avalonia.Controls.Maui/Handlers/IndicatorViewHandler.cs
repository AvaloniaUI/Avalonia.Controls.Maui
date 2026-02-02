using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using MIndicatorView = Avalonia.Controls.Maui.Controls.MauiIndicatorView;

namespace Avalonia.Controls.Maui.Handlers;

public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, MIndicatorView>
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

    protected override MIndicatorView CreatePlatformView() => new MIndicatorView();
    
    protected override void ConnectHandler(MIndicatorView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.PropertyChanged += OnPlatformPropertyChanged;
    }

    protected override void DisconnectHandler(MIndicatorView platformView)
    {
        platformView.PropertyChanged -= OnPlatformPropertyChanged;
        base.DisconnectHandler(platformView);
    }
    
    public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        handler.PlatformView?.UpdateCount(indicatorView);
    }

    public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        handler.PlatformView?.UpdatePosition(indicatorView);
    }

    public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        handler.PlatformView?.UpdateHideSingle(indicatorView);
    }

    public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        handler.PlatformView?.UpdateMaximumVisible(indicatorView);
    }
    
    public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        handler.PlatformView?.UpdateIndicatorSize(indicatorView);
    }
    
    public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        handler.PlatformView?.UpdateIndicatorColor(indicatorView);
    }

    public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        handler.PlatformView?.UpdateSelectedIndicatorColor(indicatorView);
    }
    
    public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        handler.PlatformView?.UpdateIndicatorsShape(indicatorView);
    }
    
    public static void MapItemsSource(IndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        handler.PlatformView?.UpdateItemsSource(indicatorView);
    }
    
    public static void MapIndicatorTemplate(IndicatorViewHandler handler, IIndicatorView indicatorView)
    {
        var mauiContext = (handler as IElementHandler)?.MauiContext;
        handler.PlatformView?.UpdateIndicatorTemplate(indicatorView, mauiContext);
    }
    
    private void OnPlatformPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MIndicatorView.PositionProperty && VirtualView != null && PlatformView != null)
        {
            // Update the .NET MAUI virtual view when user taps an indicator
            VirtualView.Position = PlatformView.Position;
        }
    }
}