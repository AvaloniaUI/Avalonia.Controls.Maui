using System;
using System.Collections;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiCarouselView;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="Microsoft.Maui.Controls.CarouselView"/>.</summary>
public partial class CarouselViewHandler : ViewHandler<Microsoft.Maui.Controls.CarouselView, PlatformView>
{
    /// <summary>Property mapper for <see cref="CarouselViewHandler"/>.</summary>
    public static IPropertyMapper<Microsoft.Maui.Controls.CarouselView, CarouselViewHandler> Mapper =
        new PropertyMapper<Microsoft.Maui.Controls.CarouselView, CarouselViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.ItemsView.ItemsSource)] = MapItemsSource,
            [nameof(Microsoft.Maui.Controls.ItemsView.ItemTemplate)] = MapItemTemplate,
            [nameof(Microsoft.Maui.Controls.CarouselView.CurrentItem)] = MapCurrentItem,
            [nameof(Microsoft.Maui.Controls.CarouselView.Position)] = MapPosition,
            [nameof(Microsoft.Maui.Controls.CarouselView.Loop)] = MapLoop,
        };

    /// <summary>Command mapper for <see cref="CarouselViewHandler"/>.</summary>
    public static CommandMapper<Microsoft.Maui.Controls.CarouselView, CarouselViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
        };

    /// <summary>Initializes a new instance of <see cref="CarouselViewHandler"/>.</summary>
    public CarouselViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="CarouselViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public CarouselViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="CarouselViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public CarouselViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.PropertyChanged += OnPlatformPropertyChanged;
    }

    /// <inheritdoc/>
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

    /// <summary>Maps the ItemsSource property to the platform view.</summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
    public static void MapItemsSource(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        if (handler.PlatformView == null || handler.VirtualView == null)
            return;

        handler.PlatformView.ItemsSource = carouselView.ItemsSource;
    }

    /// <summary>Maps the ItemTemplate property to the platform view.</summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
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

    /// <summary>Maps the CurrentItem property to the platform view.</summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
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

    /// <summary>Maps the Position property to the platform view.</summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
    public static void MapPosition(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        if (handler.PlatformView == null)
            return;

        handler.PlatformView.SelectedIndex = carouselView.Position;
    }

    /// <summary>Maps the Loop property to the platform view.</summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
    public static void MapLoop(CarouselViewHandler handler, Microsoft.Maui.Controls.CarouselView carouselView)
    {
        if (handler.PlatformView == null)
            return;

        handler.PlatformView.Loop = carouselView.Loop;
    }

    /// <inheritdoc/>
    public override bool NeedsContainer => false;
}
