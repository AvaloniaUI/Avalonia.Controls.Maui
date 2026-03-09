using System.Collections;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Handlers.Shell;
using Avalonia.Controls.Maui.Services;
using Color = Microsoft.Maui.Graphics.Color;
using VerticalAlignment = Avalonia.Layout.VerticalAlignment;
using MauiPage = Microsoft.Maui.Controls.Page;
using AvaloniaTabbedPage = Avalonia.Controls.TabbedPage;
using AvaloniaContentPage = Avalonia.Controls.ContentPage;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for <see cref="ShellItemHandler"/>.
/// </summary>
public static class ShellItemExtensions
{
    /// <summary>
    /// Updates the tab items for the shell item.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabs(this ShellItemHandler handler, ShellItem item)
    {
        if (handler.VirtualView == null)
            return;

        handler._showTabs = handler.ShouldShowTabs(item);

        if (handler._tabbedPage == null)
        {
            handler.UpdateCurrentItem(item);
            return;
        }

        handler._isUpdatingTabs = true;
        try
        {
            // Clear content from old wrapper pages to detach section platform views
            // before TabbedPage replaces the Pages list during layout.
            if (handler._sectionPageMap != null)
            {
                foreach (var oldPage in handler._sectionPageMap.Values)
                {
                    oldPage.Content = null;
                }
                handler._sectionPageMap.Clear();
            }

            var sections = item is IShellItemController itemController
                ? itemController.GetItems()
                : item.Items;

            var pages = new List<Avalonia.Controls.Page>();
            int selectedIndex = -1;
            int index = 0;

            foreach (var section in sections)
            {
                if (!section.IsVisible)
                    continue;

                var wrapperPage = new AvaloniaContentPage
                {
                    Header = handler.CreateTabHeader(section)
                };

                handler._sectionPageMap![section] = wrapperPage;
                pages.Add(wrapperPage);

                if (section == item.CurrentItem)
                {
                    selectedIndex = index;
                }

                index++;
            }

            handler._tabbedPage.Pages = pages;

            if (selectedIndex >= 0)
            {
                handler._tabbedPage.SelectedIndex = selectedIndex;
            }
        }
        finally
        {
            handler._isUpdatingTabs = false;
        }

        // Ensure the current tab gets its content assigned
        handler.UpdateCurrentItem(item);

        // Apply appearance settings
        handler.UpdateTabAppearance(item);
    }

    /// <summary>
    /// Creates a tab header for a <see cref="ShellSection"/>.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="section">The <see cref="ShellSection"/>.</param>
    /// <returns>A panel containing the tab header icon and title.</returns>
    internal static object CreateTabHeader(this ShellItemHandler handler, ShellSection section)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        if (section.Icon != null)
        {
            var iconBorder = new Border
            {
                Width = 16,
                Height = 16,
                Background = Brushes.White
            };
            iconBorder.Classes.Add("shell-tab-header-icon");

            _ = handler.LoadIconForTintAsync(section.Icon, iconBorder);
            panel.Children.Add(iconBorder);
        }

        if (!string.IsNullOrEmpty(section.Title))
        {
            var textBlock = new TextBlock
            {
                Text = section.Title,
                VerticalAlignment = VerticalAlignment.Center
            };
            textBlock.Classes.Add("shell-tab-header-text");
            panel.Children.Add(textBlock);
        }

        return panel;
    }

    /// <summary>
    /// Loads an icon asynchronously for a tab header.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="iconSource">The icon source.</param>
    /// <param name="imageControl">The image control to display the icon.</param>
    internal static async Task LoadIconAsync(this ShellItemHandler handler, ImageSource iconSource, Avalonia.Controls.Image imageControl)
    {
        try
        {
            if (handler.MauiContext == null)
                return;

            var imageSourceServiceProvider = handler.MauiContext.Services.GetService<IImageSourceServiceProvider>();
            if (imageSourceServiceProvider == null)
                return;

            var service = imageSourceServiceProvider.GetImageSourceService(iconSource.GetType());
            if (service is IAvaloniaImageSourceService avaloniaService)
            {
                var result = await avaloniaService.GetImageAsync(iconSource);
                if (result?.Value != null)
                {
                    imageControl.Source = result.Value;
                }
            }
        }
        catch
        {
            // Icon loading failed silently
        }
    }

    /// <summary>
    /// Loads an icon asynchronously and applies it as an OpacityMask on a Border for tinting support.
    /// The Border's Background acts as the tint color; the icon's opaque pixels determine visibility.
    /// </summary>
    internal static async Task LoadIconForTintAsync(this ShellItemHandler handler, ImageSource iconSource, Border iconBorder)
    {
        try
        {
            if (handler.MauiContext == null)
                return;

            var imageSourceServiceProvider = handler.MauiContext.Services.GetService<IImageSourceServiceProvider>();
            if (imageSourceServiceProvider == null)
                return;

            var service = imageSourceServiceProvider.GetImageSourceService(iconSource.GetType());
            if (service is IAvaloniaImageSourceService avaloniaService)
            {
                var result = await avaloniaService.GetImageAsync(iconSource);
                if (result?.Value != null)
                {
                    iconBorder.OpacityMask = new Avalonia.Media.ImageBrush(result.Value)
                    {
                        Stretch = Avalonia.Media.Stretch.Uniform
                    };
                }
            }
        }
        catch
        {
            // Icon loading failed silently
        }
    }

    /// <summary>
    /// Updates the currently displayed shell section content.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateCurrentItem(this ShellItemHandler handler, ShellItem item)
    {
        if (item?.CurrentItem == null || handler.MauiContext == null)
            return;

        var platformHandler = item.CurrentItem.ToHandler(handler.MauiContext);
        handler._currentSectionHandler = platformHandler as ShellSectionHandler;

        if (platformHandler?.PlatformView is not Control control)
            return;

        if (handler._showTabs && handler._tabbedPage != null && handler._sectionPageMap != null)
        {
            if (handler._sectionPageMap.TryGetValue(item.CurrentItem, out var wrapperPage))
            {
                if (wrapperPage.Content != control)
                {
                    // The control may be parented deep inside a ContentPresenter chain
                    // (not directly to the wrapper page), so always detach if it has any parent.
                    if (control.Parent != null)
                    {
                        control.DetachFromVisualTree();
                    }
                    wrapperPage.Content = control;
                }

                // Find the index of this wrapper page in the Pages list
                var pageIndex = handler._tabbedPage.Pages?.IndexOf(wrapperPage) ?? -1;
                if (pageIndex >= 0 && handler._tabbedPage.SelectedIndex != pageIndex)
                    handler._tabbedPage.SelectedIndex = pageIndex;
            }
        }
        else if (handler._contentControl != null)
        {
            int currentIndex = item.Items.IndexOf(item.CurrentItem);
            handler._previousSectionIndex = currentIndex;

            // Clear old content without animation first to release any in-flight
            // transition's hidden presenter content (see ShellExtensions.UpdateCurrentItem).
            handler._contentControl.PageTransition = null;
            handler._contentControl.Content = null;

            handler._contentControl.PageTransition = new CrossFade(TimeSpan.FromMilliseconds(250));
            handler._contentControl.Content = control;
        }
    }

    /// <summary>
    /// Updates tab bar visibility based on the current page's TabBarIsVisible property.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabBarVisibility(this ShellItemHandler handler, ShellItem item)
    {
        if (handler._tabbedPage == null || item == null)
            return;

        var shell = item.Parent as Microsoft.Maui.Controls.Shell;
        if (shell == null)
            return;

        var currentPage = shell.CurrentPage;
        var isVisible = currentPage == null || Microsoft.Maui.Controls.Shell.GetTabBarIsVisible(currentPage);

        if (isVisible)
            handler._tabbedPage.Classes.Remove("hide-tabstrip");
        else
            handler._tabbedPage.Classes.Add("hide-tabstrip");
    }

    /// <summary>
    /// Updates tab bar appearance colors from shell properties.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabAppearance(this ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabBarBackgroundColor(item);
        handler.UpdateTabAppearanceInternal(item);
    }

    private const string TabStripBackgroundKey = "TabbedPageTabStripBackground";

    /// <summary>
    /// Updates the tab bar background color.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabBarBackgroundColor(this ShellItemHandler handler, ShellItem item)
    {
        if (handler._tabbedPage == null || item == null)
            return;

        var color = handler.GetResolvedProperty<Color?>(Microsoft.Maui.Controls.Shell.TabBarBackgroundColorProperty, item);
        if (color == null)
        {
            var shell = item.Parent as Microsoft.Maui.Controls.Shell;
            color = shell?.BackgroundColor;
        }

        if (color != null)
            handler._tabbedPage.Resources[TabStripBackgroundKey] = color.ToPlatform();
        else
            handler._tabbedPage.Resources.Remove(TabStripBackgroundKey);
    }

    /// <summary>
    /// Updates the tab bar foreground color by re-applying the full tab appearance.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabBarForegroundColor(this ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabAppearance(item);
    }

    /// <summary>
    /// Updates the tab bar title color by re-applying the full tab appearance.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabBarTitleColor(this ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabAppearance(item);
    }

    /// <summary>
    /// Updates the tab bar unselected item color by re-applying the full tab appearance.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabBarUnselectedColor(this ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabAppearance(item);
    }

    /// <summary>
    /// Updates the tab bar disabled item color by re-applying the full tab appearance.
    /// </summary>
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabBarDisabledColor(this ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabAppearance(item);
    }

    // TabbedPage Fluent theme resource keys we may override for tab appearance.
    // The TabbedPageTabItemTheme uses TabbedPage-prefixed keys (not generic TabItem keys).
    // The indicator binds to $parent[TabItem].Foreground, so setting ForegroundSelected
    // controls both the text and indicator color.
    private static readonly string[] ThemeResourceKeys =
    [
        "TabbedPageTabItemHeaderForegroundSelected",
        "TabbedPageTabItemHeaderForegroundUnselected",
        "TabbedPageTabItemHeaderForegroundDisabled"
    ];

    internal static void UpdateTabAppearanceInternal(this ShellItemHandler handler, ShellItem item)
    {
        if (handler._tabbedPage == null || item == null)
            return;

        var foregroundColor = handler.GetResolvedProperty<Color?>(Microsoft.Maui.Controls.Shell.TabBarForegroundColorProperty, item);
        var titleColor = handler.GetResolvedProperty<Color?>(Microsoft.Maui.Controls.Shell.TabBarTitleColorProperty, item);
        var unselectedColor = handler.GetResolvedProperty<Color?>(Microsoft.Maui.Controls.Shell.TabBarUnselectedColorProperty, item);
        var disabledColor = handler.GetResolvedProperty<Color?>(Microsoft.Maui.Controls.Shell.TabBarDisabledColorProperty, item);

        bool hasExplicitColors = foregroundColor != null || titleColor != null || unselectedColor != null || disabledColor != null;

        // In the TabbedPage Fluent theme, the selected indicator binds to TabItem.Foreground,
        // which comes from TabbedPageTabItemHeaderForegroundSelected. Setting this single key
        // controls both text and indicator color for the selected state.
        var selectedBrush = (titleColor ?? foregroundColor)?.ToPlatform();
        var selectedIconBrush = (foregroundColor ?? titleColor)?.ToPlatform();
        var unselectedBrush = unselectedColor?.ToPlatform();
        var disabledBrush = disabledColor?.ToPlatform();

        if (hasExplicitColors)
        {
            // Selected foreground controls both text and indicator (pipe) color
            if (selectedBrush != null)
            {
                handler._tabbedPage.Resources["TabbedPageTabItemHeaderForegroundSelected"] = selectedBrush;
            }

            // Unselected foreground
            if (unselectedBrush != null)
            {
                handler._tabbedPage.Resources["TabbedPageTabItemHeaderForegroundUnselected"] = unselectedBrush;
            }

            // Disabled foreground
            if (disabledBrush != null)
            {
                handler._tabbedPage.Resources["TabbedPageTabItemHeaderForegroundDisabled"] = disabledBrush;
            }
        }
        else
        {
            // No explicit MAUI colors — clear any previous overrides so Fluent theme defaults apply
            foreach (var key in ThemeResourceKeys)
            {
                handler._tabbedPage.Resources.Remove(key);
            }
        }

        // Apply icon tint colors to wrapper page headers.
        // Text foreground is handled by the Fluent theme through TextElement.Foreground
        // inheritance from PART_LayoutRoot, using the resource keys we set above.
        if (handler._sectionPageMap != null)
        {
            foreach (var kvp in handler._sectionPageMap)
            {
                var section = kvp.Key;
                var wrapperPage = kvp.Value;

                IBrush? iconBrush;
                if (section == item.CurrentItem)
                    iconBrush = selectedIconBrush;
                else
                    iconBrush = unselectedBrush;

                wrapperPage.ApplyIconColorsToPage(iconBrush);
            }
        }
    }

    /// <summary>
    /// Applies icon tint color and clears any direct text foreground overrides on the page header.
    /// Text color is handled by the Fluent theme through TextElement.Foreground inheritance.
    /// </summary>
    internal static void ApplyIconColorsToPage(this AvaloniaContentPage page, IBrush? iconBrush)
    {
        if (page.Header is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is TextBlock textBlock)
                {
                    // Clear direct foreground so Fluent theme TextElement.Foreground inheritance works
                    textBlock.ClearValue(TextBlock.ForegroundProperty);
                }
                else if (child is Border border && border.Classes.Contains("shell-tab-header-icon"))
                {
                    if (iconBrush != null) border.Background = iconBrush;
                    else border.ClearValue(Border.BackgroundProperty);
                }
            }
        }
        else if (page.Header is TextBlock tb)
        {
            tb.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    internal static MauiPage? GetCurrentPage(this ShellItemHandler handler, ShellItem item)
    {
        var shell = item.Parent as Microsoft.Maui.Controls.Shell;
        return shell?.CurrentPage;
    }

    internal static T? GetResolvedProperty<T>(this ShellItemHandler handler, BindableProperty property, ShellItem item)
    {
        var shell = item.Parent as Microsoft.Maui.Controls.Shell;
        if (shell == null) return default;

        var page = handler.GetCurrentPage(item);

        if (page != null && page.IsSet(property))
            return (T)page.GetValue(property);

        if (item.IsSet(property))
            return (T)item.GetValue(property);

        return (T)shell.GetValue(property);
    }
    
    internal static bool ShouldShowTabs(this ShellItemHandler handler, ShellItem item)
    {
        if (item is IShellItemController itemController)
        {
            return itemController.ShowTabs;
        }

        return item?.Items?.Count > 1;
    }
}
