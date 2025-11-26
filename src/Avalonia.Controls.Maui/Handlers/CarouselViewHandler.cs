using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Maui.Carousel;

namespace Avalonia.Controls.Maui.Handlers;

public partial class CarouselViewHandler : ViewHandler<Microsoft.Maui.Controls.CarouselView, PlatformView>
{
    public static IPropertyMapper<Microsoft.Maui.Controls.CarouselView, CarouselViewHandler> Mapper =
        new PropertyMapper<Microsoft.Maui.Controls.CarouselView, CarouselViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.ItemsView.ItemsSource)] = MapItemsSource,
            [nameof(Microsoft.Maui.Controls.ItemsView.ItemTemplate)] = MapItemTemplate,
            [nameof(Microsoft.Maui.Controls.CarouselView.ItemsLayout)] = MapItemsLayout,
            [nameof(Microsoft.Maui.Controls.CarouselView.CurrentItem)] = MapCurrentItem,
            [nameof(Microsoft.Maui.Controls.CarouselView.Position)] = MapPosition,
            [nameof(Microsoft.Maui.Controls.CarouselView.Loop)] = MapLoop,
            [nameof(Microsoft.Maui.Controls.CarouselView.IsSwipeEnabled)] = MapIsSwipeEnabled,
            [nameof(Microsoft.Maui.Controls.ItemsView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
            [nameof(Microsoft.Maui.Controls.ItemsView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
            [nameof(Microsoft.Maui.Controls.ItemsView.EmptyView)] = MapEmptyView,
            [nameof(Microsoft.Maui.Controls.ItemsView.EmptyViewTemplate)] = MapEmptyViewTemplate,
        };

    public static CommandMapper<Microsoft.Maui.Controls.CarouselView, CarouselViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
        };

    public CarouselViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public CarouselViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public CarouselViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
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
        platformView.PropertyChanged += OnPlatformPropertyChanged;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.PropertyChanged -= OnPlatformPropertyChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnPlatformPropertyChanged(object? sender, global::Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == PlatformView.SelectedIndexProperty && VirtualView != null && PlatformView != null)
        {
            PlatformView.SyncSelectedIndex(VirtualView);
        }
    }

    public static void MapItemsSource(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdateItemsSource(carouselView);
    }

    public static void MapItemTemplate(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdateItemTemplate(carouselView, handler.MauiContext);
    }

    public static void MapItemsLayout(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdateItemsLayout(carouselView);
    }

    public static void MapCurrentItem(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdateCurrentItem(carouselView);
    }

    public static void MapPosition(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdatePosition(carouselView);
    }

    public static void MapLoop(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdateLoop(carouselView);
    }

    public static void MapIsSwipeEnabled(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdateIsSwipeEnabled(carouselView);
    }

    public static void MapHorizontalScrollBarVisibility(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdateHorizontalScrollBarVisibility(carouselView);
    }

    public static void MapVerticalScrollBarVisibility(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdateVerticalScrollBarVisibility(carouselView);
    }

    public static void MapEmptyView(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdateEmptyView(carouselView);
    }

    public static void MapEmptyViewTemplate(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        handler.PlatformView?.UpdateEmptyViewTemplate(carouselView, handler.MauiContext);
    }

    public override bool NeedsContainer => false;
}
