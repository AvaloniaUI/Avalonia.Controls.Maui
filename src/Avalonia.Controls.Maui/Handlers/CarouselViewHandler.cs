using System;
using System.Collections;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiCarouselView;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI CarouselView to Avalonia Carousel mapping
/// </summary>
internal partial class CarouselViewHandler : ViewHandler<Microsoft.Maui.Controls.CarouselView, PlatformView>
{
    public static IPropertyMapper<Microsoft.Maui.Controls.CarouselView, CarouselViewHandler> Mapper =
        new PropertyMapper<Microsoft.Maui.Controls.CarouselView, CarouselViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.ItemsView.ItemsSource)] = MapItemsSource,
            [nameof(Microsoft.Maui.Controls.ItemsView.ItemTemplate)] = MapItemTemplate,
            [nameof(Microsoft.Maui.Controls.CarouselView.CurrentItem)] = MapCurrentItem,
            [nameof(Microsoft.Maui.Controls.CarouselView.Position)] = MapPosition,
            [nameof(Microsoft.Maui.Controls.CarouselView.Loop)] = MapLoop,
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
            var newIndex = PlatformView.SelectedIndex;
            if (VirtualView.ItemsSource != null && newIndex >= 0)
            {
                var items = VirtualView.ItemsSource.Cast<object>().ToList();
                if (newIndex < items.Count)
                {
                    VirtualView.Position = newIndex;
                    VirtualView.SetValueFromRenderer(Microsoft.Maui.Controls.CarouselView.CurrentItemProperty, items[newIndex]);
                }
            }
        }
    }

    public static void MapItemsSource(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        if (handler.PlatformView == null || handler.VirtualView == null)
            return;

        handler.PlatformView.ItemsSource = carouselView.ItemsSource;
    }

    public static void MapItemTemplate(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        if (handler.PlatformView == null || handler.VirtualView == null)
            return;

        if (carouselView.ItemTemplate != null)
        {
            // Create an Avalonia DataTemplate from the MAUI DataTemplate
            var avaloniaTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (handler.MauiContext == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                // Create MAUI view from template
                var mauiView = carouselView.ItemTemplate.CreateContent() as Microsoft.Maui.Controls.View;
                if (mauiView == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                mauiView.BindingContext = item;

                // Convert to Avalonia control
                var platformControl = mauiView.ToPlatform(handler.MauiContext);
                return (platformControl as global::Avalonia.Controls.Control) ?? new TextBlock { Text = item?.ToString() ?? string.Empty };
            });

            handler.PlatformView.ItemTemplate = avaloniaTemplate;
        }
        else
        {
            // Default template
            handler.PlatformView.ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                new TextBlock { Text = item?.ToString() ?? string.Empty });
        }
    }

    public static void MapCurrentItem(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        if (handler.PlatformView == null || carouselView.ItemsSource == null)
            return;

        var currentItem = carouselView.CurrentItem;
        if (currentItem != null)
        {
            var items = carouselView.ItemsSource.Cast<object>().ToList();
            var index = items.IndexOf(currentItem);
            if (index >= 0)
            {
                handler.PlatformView.SelectedIndex = index;
            }
        }
    }

    public static void MapPosition(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        if (handler.PlatformView == null)
            return;

        handler.PlatformView.SelectedIndex = carouselView.Position;
    }

    public static void MapLoop(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        if (handler.PlatformView == null)
            return;

        handler.PlatformView.Loop = carouselView.Loop;
    }

    public override bool NeedsContainer => false;
}
