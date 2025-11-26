using Avalonia.Animation;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.Carousel;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping <see cref="CarouselView"/> properties to the Avalonia carousel control.
/// </summary>
public static class CarouselViewExtensions
{
    /// <summary>
    /// Applies the cross-platform items source to the platform carousel.
    /// </summary>
    /// <param name="platformView">Avalonia carousel control.</param>
    /// <param name="carouselView">.NET MAUI carousel view.</param>
    public static void UpdateItemsSource(this PlatformView platformView, CarouselView carouselView)
    {
        platformView.ItemsSource = carouselView.ItemsSource;
    }

    /// <summary>
    /// Applies the item template to the platform carousel, building Avalonia templates from MAUI templates when needed.
    /// </summary>
    /// <param name="platformView">Avalonia carousel control.</param>
    /// <param name="carouselView">.NET MAUI carousel view.</param>
    /// <param name="mauiContext">MAUI context for platform conversions.</param>
    public static void UpdateItemTemplate(this PlatformView platformView, CarouselView carouselView, IMauiContext? mauiContext)
    {
        if (carouselView.ItemTemplate != null)
        {
            platformView.ItemTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                if (mauiContext == null)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                if (carouselView.ItemTemplate.CreateContent() is not View mauiView)
                    return new TextBlock { Text = item?.ToString() ?? string.Empty };

                mauiView.BindingContext = item;
                
                var platformControl = mauiView.ToPlatform(mauiContext);
                return platformControl as Control ??
                       new TextBlock { Text = item?.ToString() ?? string.Empty };
            });
        }
        else
        {
            platformView.ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                new TextBlock { Text = item?.ToString() ?? string.Empty });
        }
    }

    /// <summary>
    /// Updates the platform selection to match the current item on the virtual view.
    /// </summary>
    /// <param name="platformView">Avalonia carousel control.</param>
    /// <param name="carouselView">.NET MAUI carousel view.</param>
    public static void UpdateCurrentItem(this PlatformView platformView, CarouselView carouselView)
    {
        if (carouselView.ItemsSource == null || carouselView.CurrentItem == null)
            return;

        var items = carouselView.ItemsSource.Cast<object>().ToList();
        var index = items.IndexOf(carouselView.CurrentItem);

        if (index >= 0)
        {
            platformView.SelectedIndex = index;
        }
    }

    /// <summary>
    /// Updates the platform selection index from the virtual view position.
    /// </summary>
    /// <param name="platformView">Avalonia carousel control.</param>
    /// <param name="carouselView">.NET MAUI carousel view.</param>
    public static void UpdatePosition(this PlatformView platformView, CarouselView carouselView)
    {
        platformView.SelectedIndex = carouselView.Position;
    }

    /// <summary>
    /// Applies loop configuration to the platform carousel.
    /// </summary>
    /// <param name="platformView">Avalonia carousel control.</param>
    /// <param name="carouselView">.NET MAUI carousel view.</param>
    public static void UpdateLoop(this PlatformView platformView, CarouselView carouselView)
    {
        platformView.IsLoopingEnabled = carouselView.Loop;
    }

    /// <summary>
    /// Updates the gesture (swipe) enabled state on the platform carousel.
    /// </summary>
    /// <param name="platformView">Avalonia carousel control.</param>
    /// <param name="carouselView">.NET MAUI carousel view.</param>
    public static void UpdateIsSwipeEnabled(this PlatformView platformView, CarouselView carouselView)
    {
        platformView.IsGestureEnabled = carouselView.IsSwipeEnabled;
    }

    /// <summary>
    /// Updates the orientation (layout) of the platform carousel based on the ItemsLayout.
    /// Sets the orientation on the underlying VirtualizingStackPanel, not on the Carousel itself.
    /// </summary>
    /// <param name="platformView">Avalonia carousel control.</param>
    /// <param name="carouselView">.NET MAUI carousel view.</param>
    public static void UpdateItemsLayout(this PlatformView platformView, CarouselView carouselView)
    {
        var itemsLayout = carouselView.ItemsLayout;
        var orientation = Orientation.Horizontal;

        // Resolve the MAUI orientation
        if (itemsLayout is LinearItemsLayout linearLayout)
        {
            orientation = linearLayout.Orientation == ItemsLayoutOrientation.Vertical
                ? Orientation.Vertical
                : Orientation.Horizontal;
        }

        // Apply Orientation to the Transition
        // The VirtualizingCarouselPanel does not use orientation; the sliding animation does.
        var axis = orientation == Orientation.Horizontal 
            ? PageSlide.SlideAxis.Horizontal 
            : PageSlide.SlideAxis.Vertical;

        if (platformView.PageTransition is PageSlide slide)
        {
            // Update existing slide transition
            slide.Orientation = axis;
        }
        else if (platformView.PageTransition == null)
        {
            // Create a default slide transition if none exists (mimicking default Carousel behavior)
            platformView.PageTransition = new PageSlide(TimeSpan.FromSeconds(0.25), axis);
        }
    }

    /// <summary>
    /// Synchronizes the virtual view when the platform selection changes.
    /// </summary>
    /// <param name="platformView">Avalonia carousel control.</param>
    /// <param name="virtualView">.NET MAUI carousel view.</param>
    public static void SyncSelectedIndex(this PlatformView platformView, CarouselView virtualView)
    {
        var newIndex = platformView.SelectedIndex;

        if (virtualView.ItemsSource != null && newIndex >= 0)
        {
            var items = virtualView.ItemsSource.Cast<object>().ToList();
            if (newIndex < items.Count)
            {
                virtualView.Position = newIndex;
                virtualView.SetValueFromRenderer(CarouselView.CurrentItemProperty, items[newIndex]);
            }
        }
    }
}