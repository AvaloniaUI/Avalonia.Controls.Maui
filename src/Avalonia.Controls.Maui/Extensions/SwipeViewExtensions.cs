using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.Swipe;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Extension methods for mapping ISwipeView to the Avalonia Swipe control.
/// </summary>
public static class SwipeViewExtensions
{
    /// <summary>
    /// Updates the main content of the Swipe control.
    /// </summary>
    /// <param name="platformView">The native Avalonia Swipe control.</param>
    /// <param name="swipeView">The virtual .NET MAUI SwipeView.</param>
    /// <param name="context">The .NET MAUI context used to inflate the content.</param>
    public static void UpdateContent(this PlatformView platformView, ISwipeView swipeView, IMauiContext? context)
    {
        if (context == null) return;

        if (swipeView.PresentedContent != null)
        {
            platformView.Content = swipeView.PresentedContent.ToPlatform(context) as Control;
        }
        else
        {
            platformView.Content = null;
        }
    }

    /// <summary>
    /// Updates items shown when swiping from left to right.
    /// </summary>
    /// <param name="platformView">The native Avalonia Swipe control.</param>
    /// <param name="swipeView">The virtual .NET MAUI SwipeView.</param>
    /// <param name="context">The .NET MAUI context.</param>
    public static void UpdateLeftItems(this PlatformView platformView, ISwipeView swipeView, IMauiContext? context)
    {
        platformView.Left = CreateItemsTemplate(platformView, swipeView.LeftItems, context, stretchToWidth: false);

        if (swipeView.LeftItems != null)
        {
            platformView.LeftMode = swipeView.LeftItems.Mode == Microsoft.Maui.SwipeMode.Execute
                ? SwipeMode.Execute
                : SwipeMode.Reveal;
        }
    }

    /// <summary>
    /// Updates items shown when swiping from right to left.
    /// </summary>
    /// <param name="platformView">The native Avalonia Swipe control.</param>
    /// <param name="swipeView">The virtual .NET MAUI SwipeView.</param>
    /// <param name="context">The .NET MAUI context.</param>
    public static void UpdateRightItems(this PlatformView platformView, ISwipeView swipeView, IMauiContext? context)
    {
        platformView.Right = CreateItemsTemplate(platformView, swipeView.RightItems, context, stretchToWidth: false);

        if (swipeView.RightItems != null)
        {
            platformView.RightMode = swipeView.RightItems.Mode == Microsoft.Maui.SwipeMode.Execute
                ? SwipeMode.Execute
                : SwipeMode.Reveal;
        }
    }

    /// <summary>
    /// Updates items shown when swiping down.
    /// </summary>
    /// <param name="platformView">The native Avalonia Swipe control.</param>
    /// <param name="swipeView">The virtual .NET MAUI SwipeView.</param>
    /// <param name="context">The .NET MAUI context.</param>
    public static void UpdateTopItems(this PlatformView platformView, ISwipeView swipeView, IMauiContext? context)
    {
        platformView.Top = CreateItemsTemplate(platformView, swipeView.TopItems, context, stretchToWidth: true);

        if (swipeView.TopItems != null)
        {
            platformView.TopMode = swipeView.TopItems.Mode == Microsoft.Maui.SwipeMode.Execute
                ? SwipeMode.Execute
                : SwipeMode.Reveal;
        }
    }

    /// <summary>
    /// Updates items shown when swiping up.
    /// </summary>
    /// <param name="platformView">The native Avalonia Swipe control.</param>
    /// <param name="swipeView">The virtual .NET MAUI SwipeView.</param>
    /// <param name="context">The .NET MAUI context.</param>
    public static void UpdateBottomItems(this PlatformView platformView, ISwipeView swipeView, IMauiContext? context)
    {
        platformView.Bottom = CreateItemsTemplate(platformView, swipeView.BottomItems, context, stretchToWidth: true);

        if (swipeView.BottomItems != null)
        {
            platformView.BottomMode = swipeView.BottomItems.Mode == Microsoft.Maui.SwipeMode.Execute
                ? SwipeMode.Execute
                : SwipeMode.Reveal;
        }
    }

    /// <summary>
    /// Updates the swipe threshold.
    /// </summary>
    /// <param name="platformView">The native Avalonia Swipe control.</param>
    /// <param name="swipeView">The virtual .NET MAUI SwipeView.</param>
    public static void UpdateThreshold(this PlatformView platformView, ISwipeView swipeView)
    {
        platformView.Threshold = swipeView.Threshold;
    }

    /// <summary>
    /// Converts Avalonia SwipeDirection to .NET MAUI SwipeDirection.
    /// </summary>
    public static Microsoft.Maui.SwipeDirection ToMauiSwipeDirection(this SwipeDirection direction)
    {
        return direction switch
        {
            SwipeDirection.Left => Microsoft.Maui.SwipeDirection.Left,
            SwipeDirection.Right => Microsoft.Maui.SwipeDirection.Right,
            SwipeDirection.Up => Microsoft.Maui.SwipeDirection.Up,
            SwipeDirection.Down => Microsoft.Maui.SwipeDirection.Down,
            _ => Microsoft.Maui.SwipeDirection.Right
        };
    }

    private static IDataTemplate? CreateItemsTemplate(PlatformView owner, ISwipeItems? swipeItems, IMauiContext? context, bool stretchToWidth)
    {
        if (swipeItems == null || swipeItems.Count == 0 || context == null)
            return null;

        return new FuncDataTemplate<object>((_, _) =>
        {
            Panel panel;

            if (stretchToWidth)
            {
                var grid = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                for (int i = 0; i < swipeItems.Count; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                }

                panel = grid;
            }
            else
            {
                panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 0,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
            }

            int index = 0;
            foreach (var item in swipeItems)
            {
                Control? avaloniaControl = null;

                if (item is ISwipeItemView swipeItemView)
                {
                    if (swipeItemView.Handler == null)
                    {
                        swipeItemView.ToHandler(context);
                    }

                    if (swipeItemView.Handler?.PlatformView is Control swipeItemViewControl)
                    {
                        avaloniaControl = swipeItemViewControl;
                    }
                }
                else if (item is ISwipeItemMenuItem menuItem)
                {
                    if (menuItem.Handler == null)
                    {
                        menuItem.ToHandler(context);
                    }

                    if (menuItem.Handler?.PlatformView is Control menuItemControl)
                    {
                        avaloniaControl = menuItemControl;
                    }
                }

                if (avaloniaControl != null)
                {
                    avaloniaControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                    avaloniaControl.VerticalAlignment = VerticalAlignment.Stretch;

                    // Tag used for auto-close behavior
                    avaloniaControl.Tag = (swipeItems.SwipeBehaviorOnInvoked, owner);

                    // Ensure the control is not already parented before adding it to a new panel
                    // This can happen when the SwipeView is recycled or re-materialized
                    if (avaloniaControl.Parent is Panel oldPanel)
                    {
                        oldPanel.Children.Remove(avaloniaControl);
                    }

                    if (panel is Grid grid)
                    {
                        Grid.SetColumn(avaloniaControl, index);
                        grid.Children.Add(avaloniaControl);
                    }
                    else
                    {
                        panel.Children.Add(avaloniaControl);
                    }

                    index++;
                }
            }

            return panel;
        });
    }
}
