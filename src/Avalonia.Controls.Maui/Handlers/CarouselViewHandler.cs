using Avalonia.Controls.Maui.Extensions;
using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaSelectionMode = Avalonia.Controls.SelectionMode;
using PlatformView = Avalonia.Controls.Carousel;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Avalonia handler for <see cref="CarouselView"/>.
/// </summary>
public class CarouselViewHandler : ViewHandler<CarouselView, PlatformView>
{
    private INotifyCollectionChanged? _itemsSourceCollectionChanged;
    private bool _updatingPlatformSelection;
    private bool _updatingVirtualSelection;

    /// <summary>
    /// Property mapper for <see cref="CarouselViewHandler"/>.
    /// </summary>
    public static IPropertyMapper<ItemsView, CarouselViewHandler> Mapper =
        new PropertyMapper<ItemsView, CarouselViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(ItemsView.ItemsSource)] = MapItemsSource,
            [nameof(ItemsView.ItemTemplate)] = MapItemTemplate,
            [nameof(ItemsView.EmptyView)] = MapEmptyView,
            [nameof(ItemsView.EmptyViewTemplate)] = MapEmptyViewTemplate,
            [nameof(CarouselView.CurrentItem)] = MapCurrentItem,
            [nameof(CarouselView.IsScrollAnimated)] = MapIsScrollAnimated,
            [nameof(CarouselView.IsSwipeEnabled)] = MapIsSwipeEnabled,
            [nameof(CarouselView.ItemsLayout)] = MapItemsLayout,
            [nameof(CarouselView.Loop)] = MapLoop,
            [nameof(CarouselView.PeekAreaInsets)] = MapPeekAreaInsets,
            [nameof(CarouselView.Position)] = MapPosition,
        };

    /// <summary>
    /// Command mapper for <see cref="CarouselViewHandler"/>.
    /// </summary>
    public static CommandMapper<CarouselView, CarouselViewHandler> CommandMapper =
        new(ViewCommandMapper);

    /// <summary>
    /// Initializes a new instance of <see cref="CarouselViewHandler"/>.
    /// </summary>
    public CarouselViewHandler()
        : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CarouselViewHandler"/>.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public CarouselViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CarouselViewHandler"/>.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public CarouselViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <inheritdoc/>
    public override bool NeedsContainer => false;

    /// <inheritdoc/>
    public override void PlatformArrange(Microsoft.Maui.Graphics.Rect frame)
    {
        base.PlatformArrange(frame);

        if (PlatformView is { } platformView && VirtualView is { } carouselView)
        {
            platformView.UpdatePeekAreaInsets(carouselView);
            platformView.UpdateSwipeGestureRecognizers();
        }
    }

    /// <summary>
    /// Creates the Avalonia platform view for this handler.
    /// </summary>
    /// <returns>The Avalonia carousel control.</returns>
    protected override PlatformView CreatePlatformView()
    {
        return new MauiCarousel();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);

        platformView.TemplateApplied += OnPlatformTemplateApplied;
        platformView.LayoutUpdated += OnPlatformLayoutUpdated;
        platformView.SelectionChanged += OnPlatformSelectionChanged;
        platformView.PropertyChanged += OnPlatformPropertyChanged;

        if (VirtualView is ItemsView itemsView)
        {
            itemsView.ScrollToRequested += OnScrollToRequested;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        UpdateItemsSourceCollectionChangedSubscription(null);

        platformView.TemplateApplied -= OnPlatformTemplateApplied;
        platformView.LayoutUpdated -= OnPlatformLayoutUpdated;
        platformView.SelectionChanged -= OnPlatformSelectionChanged;
        platformView.PropertyChanged -= OnPlatformPropertyChanged;

        if (VirtualView is ItemsView itemsView)
        {
            itemsView.ScrollToRequested -= OnScrollToRequested;
        }

        base.DisconnectHandler(platformView);
    }

    /// <summary>
    /// Maps the ItemsSource property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapItemsSource(CarouselViewHandler handler, ItemsView itemsView)
    {
        handler.UpdateItemsSourceCollectionChangedSubscription(itemsView.ItemsSource);

        handler.UpdatePlatformSelection(() =>
        {
            handler.PlatformView.UpdateItemsSource(itemsView, handler);

            if (itemsView is CarouselView carouselView && !handler.PlatformView.IsShowingEmptyView())
            {
                if (carouselView.ItemsSource == null)
                {
                    handler.PlatformView.UpdateCurrentItem((object?)null);
                }
                else if (CanSelectCurrentItem(carouselView))
                {
                    handler.PlatformView.UpdateCurrentItem(carouselView);
                }
                else
                {
                    handler.PlatformView.UpdatePosition(carouselView);
                }
            }
        }, syncVirtualSelection: true);
    }

    /// <summary>
    /// Maps the ItemTemplate property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapItemTemplate(CarouselViewHandler handler, ItemsView itemsView)
    {
        handler.PlatformView.UpdateItemTemplate(itemsView, handler);
    }

    /// <summary>
    /// Maps the EmptyView property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapEmptyView(CarouselViewHandler handler, ItemsView itemsView)
    {
        handler.UpdatePlatformSelection(
            () => handler.PlatformView.UpdateEmptyView(itemsView, handler),
            syncVirtualSelection: true);
    }

    /// <summary>
    /// Maps the EmptyViewTemplate property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="itemsView">The virtual items view.</param>
    public static void MapEmptyViewTemplate(CarouselViewHandler handler, ItemsView itemsView)
    {
        handler.UpdatePlatformSelection(
            () => handler.PlatformView.UpdateEmptyViewTemplate(itemsView, handler),
            syncVirtualSelection: true);
    }

    /// <summary>
    /// Maps the CurrentItem property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
    public static void MapCurrentItem(CarouselViewHandler handler, ItemsView carouselView)
    {
        if (handler._updatingVirtualSelection || carouselView is not CarouselView virtualCarousel)
            return;

        if (virtualCarousel.CurrentItem == null)
        {
            if (handler.PlatformView.IsShowingEmptyView())
                return;

            handler.UpdatePlatformSelection(
                () => handler.PlatformView.UpdateCurrentItem((object?)null),
                syncVirtualSelection: true);
            return;
        }

        handler.UpdatePlatformSelection(() => handler.PlatformView.UpdateCurrentItem(virtualCarousel), syncVirtualSelection: true);
    }

    /// <summary>
    /// Maps the IsScrollAnimated property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
    public static void MapIsScrollAnimated(CarouselViewHandler handler, ItemsView carouselView)
    {
        if (carouselView is CarouselView virtualCarousel)
        {
            handler.PlatformView.UpdatePageTransition(virtualCarousel);
        }
    }

    /// <summary>
    /// Maps the IsSwipeEnabled property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
    public static void MapIsSwipeEnabled(CarouselViewHandler handler, ItemsView carouselView)
    {
        if (carouselView is CarouselView virtualCarousel)
        {
            handler.PlatformView.UpdateIsSwipeEnabled(virtualCarousel);
        }
    }

    /// <summary>
    /// Maps the ItemsLayout property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
    public static void MapItemsLayout(CarouselViewHandler handler, ItemsView carouselView)
    {
        if (carouselView is CarouselView virtualCarousel)
        {
            handler.PlatformView.UpdatePageTransition(virtualCarousel);
            handler.PlatformView.UpdatePeekAreaInsets(virtualCarousel);
            handler.PlatformView.UpdateSwipeGestureRecognizers();
        }
    }

    /// <summary>
    /// Maps the Loop property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
    public static void MapLoop(CarouselViewHandler handler, ItemsView carouselView)
    {
        if (carouselView is CarouselView virtualCarousel)
        {
            handler.PlatformView.UpdateLoop(virtualCarousel);
            handler.PlatformView.UpdateSwipeGestureRecognizers();
        }
    }

    /// <summary>
    /// Maps the PeekAreaInsets property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
    public static void MapPeekAreaInsets(CarouselViewHandler handler, ItemsView carouselView)
    {
        if (carouselView is CarouselView virtualCarousel)
        {
            handler.PlatformView.UpdatePeekAreaInsets(virtualCarousel);
            handler.PlatformView.UpdateSwipeGestureRecognizers();
        }
    }

    /// <summary>
    /// Maps the Position property to the platform view.
    /// </summary>
    /// <param name="handler">The handler for the carousel view.</param>
    /// <param name="carouselView">The virtual carousel view.</param>
    public static void MapPosition(CarouselViewHandler handler, ItemsView carouselView)
    {
        if (handler._updatingVirtualSelection || carouselView is not CarouselView virtualCarousel)
            return;

        handler.UpdatePlatformSelection(() => handler.PlatformView.UpdatePosition(virtualCarousel), syncVirtualSelection: true);
    }

    private void OnPlatformTemplateApplied(object? sender, Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
    {
        PlatformView?.UpdateSwipeGestureRecognizers();
    }

    private void OnPlatformLayoutUpdated(object? sender, EventArgs e)
    {
        PlatformView?.UpdateSwipeGestureRecognizers();
    }

    private void OnPlatformSelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (_updatingPlatformSelection)
            return;

        SyncVirtualSelectionFromPlatform();
    }

    private void OnPlatformPropertyChanged(object? sender, global::Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Avalonia.Controls.Carousel.IsSwipingProperty && VirtualView is { } carouselView)
        {
            carouselView.SetIsDragging(e.GetNewValue<bool>());
        }
    }

    private void OnScrollToRequested(object? sender, ScrollToRequestEventArgs request)
    {
        if (PlatformView == null)
            return;

        if (request.Mode == ScrollToMode.Position)
        {
            if (PlatformView.IsShowingEmptyView())
                return;

            UpdatePlatformSelection(() => PlatformView.UpdatePosition(request.Index), syncVirtualSelection: true);
        }
        else if (request.Item != null)
        {
            if (PlatformView.IsShowingEmptyView())
                return;

            UpdatePlatformSelection(() => PlatformView.UpdateCurrentItem(request.Item), syncVirtualSelection: true);
        }
    }

    private void UpdatePlatformSelection(Action updateSelection, bool syncVirtualSelection = false)
    {
        _updatingPlatformSelection = true;

        try
        {
            updateSelection();
        }
        finally
        {
            _updatingPlatformSelection = false;
        }

        if (syncVirtualSelection)
        {
            SyncVirtualSelectionFromPlatform();
        }
    }

    private void SyncVirtualSelectionFromPlatform()
    {
        if (_updatingVirtualSelection || VirtualView == null || PlatformView == null)
            return;

        _updatingVirtualSelection = true;

        try
        {
            var selectedIndex = PlatformView.SelectedIndex;
            var selectedItem = PlatformView.SelectedItem;

            if (PlatformView.IsShowingEmptyView())
            {
                if (VirtualView.CurrentItem != null)
                {
                    VirtualView.CurrentItem = null;
                }

                return;
            }

            if (selectedIndex >= 0 && VirtualView.Position != selectedIndex)
            {
                VirtualView.Position = selectedIndex;
            }

            if (!Equals(VirtualView.CurrentItem, selectedItem))
            {
                VirtualView.CurrentItem = selectedItem;
            }
        }
        finally
        {
            _updatingVirtualSelection = false;
        }
    }

    private void UpdateItemsSourceCollectionChangedSubscription(IEnumerable? itemsSource)
    {
        if (_itemsSourceCollectionChanged != null)
        {
            _itemsSourceCollectionChanged.CollectionChanged -= OnItemsSourceCollectionChanged;
        }

        _itemsSourceCollectionChanged = itemsSource as INotifyCollectionChanged;

        if (_itemsSourceCollectionChanged != null)
        {
            _itemsSourceCollectionChanged.CollectionChanged += OnItemsSourceCollectionChanged;
        }
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        global::Avalonia.Threading.Dispatcher.UIThread.Post(RefreshItemsSourceAfterCollectionChanged);
    }

    private void RefreshItemsSourceAfterCollectionChanged()
    {
        if (((IElementHandler)this).PlatformView is not PlatformView platformView ||
            ((IElementHandler)this).VirtualView is not CarouselView virtualView)
        {
            return;
        }

        UpdatePlatformSelection(() =>
        {
            platformView.UpdateItemsSource(virtualView, this);

            if (platformView.IsShowingEmptyView())
                return;

            if (CanSelectCurrentItem(virtualView))
            {
                platformView.UpdateCurrentItem(virtualView);
            }
            else
            {
                platformView.UpdatePosition(virtualView);
            }
        }, syncVirtualSelection: true);
    }

    private static bool CanSelectCurrentItem(CarouselView carouselView)
    {
        var currentItem = carouselView.CurrentItem;
        if (currentItem == null || carouselView.ItemsSource == null)
            return false;

        foreach (var item in carouselView.ItemsSource)
        {
            if (Equals(item, currentItem))
            {
                return true;
            }
        }

        return false;
    }

    private sealed class MauiCarousel : PlatformView
    {
        protected override Type StyleKeyOverride => typeof(PlatformView);

        internal MauiCarousel()
        {
            SelectionMode = AvaloniaSelectionMode.Single;
        }
    }
}
