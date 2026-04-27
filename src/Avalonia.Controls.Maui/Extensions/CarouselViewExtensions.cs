using Avalonia.Animation;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.VisualTree;
using System.Collections;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AvaloniaCarousel = Avalonia.Controls.Carousel;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaSwipeGestureRecognizer = Avalonia.Input.GestureRecognizers.SwipeGestureRecognizer;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using MauiCollectionViewControl = Avalonia.Controls.Maui.MauiCollectionView;
using MauiDataTemplate = Microsoft.Maui.Controls.DataTemplate;
using MauiItemsView = Microsoft.Maui.Controls.ItemsView;
using MauiView = Microsoft.Maui.Controls.View;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Provides CarouselView mapping extensions for the Avalonia Carousel control.
/// </summary>
public static class CarouselViewExtensions
{
    private static readonly TimeSpan DefaultTransitionDuration = TimeSpan.FromMilliseconds(250);
    private static readonly EmptyCarouselItem EmptyItem = new();
    private static readonly object[] EmptyItemsSource = [EmptyItem];

    /// <summary>
    /// Updates the items source of the carousel.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The carousel view handler for context.</param>
    public static void UpdateItemsSource(this AvaloniaCarousel platformView, MauiItemsView itemsView, CarouselViewHandler handler)
    {
        platformView.UpdateCarouselContent(itemsView, handler);
    }

    /// <summary>
    /// Updates the item template of the carousel.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The carousel view handler for context.</param>
    public static void UpdateItemTemplate(this AvaloniaCarousel platformView, MauiItemsView itemsView, CarouselViewHandler handler)
    {
        platformView.UpdateCarouselContent(itemsView, handler);
    }

    /// <summary>
    /// Updates the empty view of the carousel.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The carousel view handler for context.</param>
    public static void UpdateEmptyView(this AvaloniaCarousel platformView, MauiItemsView itemsView, CarouselViewHandler handler)
    {
        platformView.UpdateCarouselContent(itemsView, handler);
    }

    /// <summary>
    /// Updates the empty view template of the carousel.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="itemsView">The cross-platform items view.</param>
    /// <param name="handler">The carousel view handler for context.</param>
    public static void UpdateEmptyViewTemplate(this AvaloniaCarousel platformView, MauiItemsView itemsView, CarouselViewHandler handler)
    {
        platformView.UpdateCarouselContent(itemsView, handler);
    }

    /// <summary>
    /// Gets a value indicating whether the carousel is showing the MAUI empty view.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <returns><c>true</c> when the carousel is presenting the empty view; otherwise, <c>false</c>.</returns>
    public static bool IsShowingEmptyView(this AvaloniaCarousel platformView)
    {
        return ReferenceEquals(platformView.ItemsSource, EmptyItemsSource);
    }

    /// <summary>
    /// Updates the selected item of the carousel.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="carouselView">The cross-platform carousel view.</param>
    public static void UpdateCurrentItem(this AvaloniaCarousel platformView, CarouselView carouselView)
    {
        if (carouselView.CurrentItem != null)
        {
            platformView.UpdateCurrentItem(carouselView.CurrentItem);
        }
    }

    /// <summary>
    /// Updates the selected item of the carousel.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="currentItem">The selected item.</param>
    public static void UpdateCurrentItem(this AvaloniaCarousel platformView, object? currentItem)
    {
        if (currentItem == null)
        {
            if (platformView.SelectedIndex != -1)
            {
                platformView.SelectedIndex = -1;
            }

            if (platformView.SelectedItem != null)
            {
                platformView.SelectedItem = null;
            }

            return;
        }

        if (!Equals(platformView.SelectedItem, currentItem))
        {
            platformView.SelectedItem = currentItem;
        }
    }

    /// <summary>
    /// Updates whether carousel selection wraps at the first and last items.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="carouselView">The cross-platform carousel view.</param>
    public static void UpdateLoop(this AvaloniaCarousel platformView, CarouselView carouselView)
    {
        platformView.WrapSelection = carouselView.Loop;
    }

    /// <summary>
    /// Updates whether swipe gestures are enabled for carousel navigation.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="carouselView">The cross-platform carousel view.</param>
    public static void UpdateIsSwipeEnabled(this AvaloniaCarousel platformView, CarouselView carouselView)
    {
        platformView.IsSwipeEnabled = carouselView.IsSwipeEnabled;
        platformView.UpdateSwipeGestureRecognizers();
    }

    /// <summary>
    /// Updates the Avalonia swipe recognizers created by the carousel panel.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    public static void UpdateSwipeGestureRecognizers(this AvaloniaCarousel platformView)
    {
        if (platformView.ItemsPanelRoot == null)
            return;

        foreach (var recognizer in platformView.ItemsPanelRoot.GestureRecognizers)
        {
            if (recognizer is AvaloniaSwipeGestureRecognizer swipeGestureRecognizer &&
                swipeGestureRecognizer.IsMouseEnabled != platformView.IsSwipeEnabled)
            {
                swipeGestureRecognizer.IsMouseEnabled = platformView.IsSwipeEnabled;
            }
        }
    }

    /// <summary>
    /// Updates the Avalonia page transition used by the carousel.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="carouselView">The cross-platform carousel view.</param>
    public static void UpdatePageTransition(this AvaloniaCarousel platformView, CarouselView carouselView)
    {
        platformView.PageTransition = new PageSlide(
            carouselView.IsScrollAnimated ? DefaultTransitionDuration : TimeSpan.Zero,
            carouselView.GetSlideAxis());
    }

    /// <summary>
    /// Updates the carousel viewport fraction from the MAUI peek area insets.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="carouselView">The cross-platform carousel view.</param>
    public static void UpdatePeekAreaInsets(this AvaloniaCarousel platformView, CarouselView carouselView)
    {
        var primaryLength = carouselView.GetPrimaryLength(platformView);
        var primaryInsets = carouselView.GetPrimaryPeekInsets();

        if (primaryLength <= 0 || primaryInsets <= 0 || primaryInsets >= primaryLength)
        {
            platformView.ViewportFraction = 1;
            return;
        }

        platformView.ViewportFraction = Math.Clamp((primaryLength - primaryInsets) / primaryLength, 0.01, 1);
    }

    /// <summary>
    /// Updates the selected position of the carousel.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="carouselView">The cross-platform carousel view.</param>
    public static void UpdatePosition(this AvaloniaCarousel platformView, CarouselView carouselView)
    {
        platformView.UpdatePosition(carouselView.Position);
    }

    /// <summary>
    /// Updates the selected position of the carousel.
    /// </summary>
    /// <param name="platformView">The platform carousel control.</param>
    /// <param name="position">The zero-based selected item index.</param>
    public static void UpdatePosition(this AvaloniaCarousel platformView, int position)
    {
        if (position < 0)
            return;

        if (platformView.ItemCount > 0 && position >= platformView.ItemCount)
            return;

        if (platformView.SelectedIndex != position)
        {
            platformView.SelectedIndex = position;
        }
    }

    private static void UpdateCarouselContent(this AvaloniaCarousel platformView, MauiItemsView itemsView, CarouselViewHandler handler)
    {
        if (itemsView.ShouldShowEmptyView())
        {
            platformView.SetCarouselItemsSource(EmptyItemsSource);
            platformView.ItemTemplate = CreateEmptyViewTemplate(itemsView, handler);

            if (platformView.SelectedIndex != 0)
            {
                platformView.SelectedIndex = 0;
            }

            return;
        }

        platformView.SetCarouselItemsSource(itemsView.ItemsSource);
        platformView.ItemTemplate = CreateItemTemplate(itemsView, handler);
    }

    private static void SetCarouselItemsSource(this AvaloniaCarousel platformView, IEnumerable? itemsSource)
    {
        if (ReferenceEquals(platformView.ItemsSource, itemsSource))
            return;

        if (platformView.SelectedIndex != -1)
        {
            platformView.SelectedIndex = -1;
        }

        platformView.ItemsSource = itemsSource;
    }

    private static IDataTemplate CreateItemTemplate(MauiItemsView itemsView, CarouselViewHandler handler)
    {
        if (itemsView.ItemTemplate == null)
        {
            return new FuncDataTemplate<object>((item, _) => CreateTextItem(item));
        }

        return new FuncDataTemplate<object>((item, _) =>
        {
            if (handler.MauiContext == null)
                return CreateTextItem(item);

            var mauiView = CreateMauiTemplateView(itemsView.ItemTemplate, item);
            if (mauiView == null)
                return CreateTextItem(item);

            AddLogicalChild(itemsView, mauiView);

            var platformControl = (AvaloniaControl?)mauiView.ToPlatform(handler.MauiContext);
            if (platformControl == null)
                return CreateTextItem(item);

            platformControl.Tag = mauiView;
            AttachLogicalChildCleanup(platformControl);
            return platformControl;
        });
    }

    private static IDataTemplate CreateEmptyViewTemplate(MauiItemsView itemsView, CarouselViewHandler handler)
    {
        return new FuncDataTemplate<object>((_, _) => CreateEmptyView(itemsView, handler));
    }

    private static AvaloniaControl CreateEmptyView(MauiItemsView itemsView, CarouselViewHandler handler)
    {
        if (itemsView.EmptyViewTemplate != null)
        {
            if (handler.MauiContext == null)
                return CreateTextItem(itemsView.EmptyView);

            var mauiView = CreateMauiTemplateView(itemsView.EmptyViewTemplate, itemsView.EmptyView);
            if (mauiView == null)
                return CreateTextItem(itemsView.EmptyView);

            AddLogicalChild(itemsView, mauiView);

            var platformControl = (AvaloniaControl?)mauiView.ToPlatform(handler.MauiContext);
            if (platformControl == null)
                return CreateTextItem(itemsView.EmptyView);

            platformControl.Tag = mauiView;
            AttachLogicalChildCleanup(platformControl);
            return platformControl;
        }

        if (itemsView.EmptyView is MauiView emptyView)
        {
            if (handler.MauiContext == null)
                return CreateTextItem(emptyView.ToString());

            AddLogicalChild(itemsView, emptyView);

            var platformControl = (AvaloniaControl?)emptyView.ToPlatform(handler.MauiContext);
            if (platformControl == null)
                return CreateTextItem(emptyView.ToString());

            platformControl.Tag = emptyView;
            AttachLogicalChildCleanup(platformControl);
            return platformControl;
        }

        return CreateTextItem(itemsView.EmptyView);
    }

    private static MauiView? CreateMauiTemplateView(MauiDataTemplate itemTemplate, object? item)
    {
        var content = itemTemplate.CreateContent();
        var mauiView = content switch
        {
            MauiView view => view,
            ViewCell viewCell => viewCell.View,
            _ => null
        };

        if (mauiView != null)
        {
            mauiView.BindingContext = item;
        }

        return mauiView;
    }

    private static void AddLogicalChild(MauiItemsView itemsView, MauiView mauiView)
    {
        if (itemsView is Element parentElement && mauiView.Parent != parentElement)
        {
            parentElement.AddLogicalChild(mauiView);
        }
    }

    private static bool ShouldShowEmptyView(this MauiItemsView itemsView)
    {
        return (itemsView.EmptyView != null || itemsView.EmptyViewTemplate != null) &&
            IsItemsSourceEmpty(itemsView.ItemsSource);
    }

    private static bool IsItemsSourceEmpty(IEnumerable? itemsSource)
    {
        if (itemsSource == null)
            return true;

        if (itemsSource is ICollection collection)
            return collection.Count == 0;

        if (itemsSource is IReadOnlyCollection<object> readOnlyCollection)
            return readOnlyCollection.Count == 0;

        var enumerator = itemsSource.GetEnumerator();

        try
        {
            return !enumerator.MoveNext();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    private static AvaloniaTextBlock CreateTextItem(object? item)
    {
        return new AvaloniaTextBlock
        {
            Text = item?.ToString() ?? string.Empty,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private static PageSlide.SlideAxis GetSlideAxis(this CarouselView carouselView)
    {
        return carouselView.ItemsLayout?.Orientation == ItemsLayoutOrientation.Vertical
            ? PageSlide.SlideAxis.Vertical
            : PageSlide.SlideAxis.Horizontal;
    }

    private static double GetPrimaryLength(this CarouselView carouselView, AvaloniaCarousel platformView)
    {
        if (carouselView.GetSlideAxis() == PageSlide.SlideAxis.Vertical)
        {
            if (platformView.Bounds.Height > 0)
                return platformView.Bounds.Height;

            return carouselView.HeightRequest > 0 ? carouselView.HeightRequest : 0;
        }

        if (platformView.Bounds.Width > 0)
            return platformView.Bounds.Width;

        return carouselView.WidthRequest > 0 ? carouselView.WidthRequest : 0;
    }

    private static double GetPrimaryPeekInsets(this CarouselView carouselView)
    {
        var insets = carouselView.PeekAreaInsets;
        return carouselView.GetSlideAxis() == PageSlide.SlideAxis.Vertical
            ? insets.Top + insets.Bottom
            : insets.Left + insets.Right;
    }

    private static void AttachLogicalChildCleanup(AvaloniaControl platformControl)
    {
        platformControl.DetachedFromVisualTree -= OnPlatformControlDetachedFromVisualTree;
        platformControl.DetachedFromVisualTree += OnPlatformControlDetachedFromVisualTree;
    }

    private static void OnPlatformControlDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is AvaloniaControl control)
        {
            MauiCollectionViewControl.CleanupLogicalChild(control);
        }
    }

    private sealed class EmptyCarouselItem
    {
    }
}
