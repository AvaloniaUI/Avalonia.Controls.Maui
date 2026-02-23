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

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for <see cref="ShellItemHandler"/>.
/// </summary>
public static class ShellItemExtensions
{
    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabs(this ShellItemHandler handler, ShellItem item)
    {
        if (handler.VirtualView == null)
            return;

        handler._showTabs = handler.ShouldShowTabs(item);

        if (handler._tabControl == null)
        {
            handler.UpdateCurrentItem(item);
            return;
        }

        handler._isUpdatingTabs = true;
        try
        {
            handler._tabControl.Items.Clear();

            var sections = item is IShellItemController itemController
                ? itemController.GetItems()
                : item.Items;

            foreach (var section in sections)
            {
                if (!section.IsVisible)
                    continue;

                var tabItem = new TabItem
                {
                    Header = handler.CreateTabHeader(section),
                    Tag = section
                };

                handler._tabControl.Items.Add(tabItem);

                // Set selected if current
                if (section == item.CurrentItem)
                {
                    handler._tabControl.SelectedItem = tabItem;
                }
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

        if (handler._showTabs && handler._tabControl != null)
        {
            TabItem? targetTab = null;
            foreach (var tabItemObj in handler._tabControl.Items)
            {
                if (tabItemObj is TabItem tabItem && tabItem.Tag == item.CurrentItem)
                {
                    targetTab = tabItem;
                    break;
                }
            }

            if (targetTab != null)
            {
                if (targetTab.Content != control)
                {
                    if (control.Parent != null && control.Parent != targetTab)
                    {
                        control.DetachFromVisualTree();
                    }
                    targetTab.Content = control;
                }

                if (handler._tabControl.SelectedItem != targetTab)
                    handler._tabControl.SelectedItem = targetTab;
            }
        }
        else if (handler._contentControl != null)
        {
            int currentIndex = item.Items.IndexOf(item.CurrentItem);
            handler._previousSectionIndex = currentIndex;

            handler._contentControl.PageTransition = new CrossFade(TimeSpan.FromMilliseconds(250));
            handler._contentControl.Content = control;
        }
    }

    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabBarVisibility(this ShellItemHandler handler, ShellItem item)
    {
        if (handler._tabControl == null || item == null)
            return;

        var shell = item.Parent as Microsoft.Maui.Controls.Shell;
        if (shell == null)
            return;

        var currentPage = shell.CurrentPage;
        var isVisible = currentPage == null || Microsoft.Maui.Controls.Shell.GetTabBarIsVisible(currentPage);

        if (isVisible)
            handler._tabControl.Classes.Remove("hide-tabstrip");
        else
            handler._tabControl.Classes.Add("hide-tabstrip");
    }

    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabAppearance(this ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabBarBackgroundColor(item);
        handler.UpdateTabAppearanceInternal(item);
    }

    /// <param name="handler">The <see cref="ShellItemHandler"/> instance.</param>
    /// <param name="item">The <see cref="ShellItem"/> instance to update from.</param>
    public static void UpdateTabBarBackgroundColor(this ShellItemHandler handler, ShellItem item)
    {
        if (handler._tabControl == null || item == null)
            return;

        var color = handler.GetResolvedProperty<Color?>(Microsoft.Maui.Controls.Shell.TabBarBackgroundColorProperty, item);
        if (color == null)
        {
            var shell = item.Parent as Microsoft.Maui.Controls.Shell;
            color = shell?.BackgroundColor;
        }

        if (color != null)
            handler._tabControl.Background = color.ToPlatform();
        else
            handler._tabControl.ClearValue(TabControl.BackgroundProperty);
    }

    public static void UpdateTabBarForegroundColor(this ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabAppearance(item);
    }

    public static void UpdateTabBarTitleColor(this ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabAppearance(item);
    }

    public static void UpdateTabBarUnselectedColor(this ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabAppearance(item);
    }

    public static void UpdateTabBarDisabledColor(this ShellItemHandler handler, ShellItem item)
    {
        handler.UpdateTabAppearance(item);
    }

    internal static void UpdateTabAppearanceInternal(this ShellItemHandler handler, ShellItem item)
    {
        if (handler._tabControl == null || item == null)
            return;

        var foregroundColor = handler.GetResolvedProperty<Color?>(Microsoft.Maui.Controls.Shell.TabBarForegroundColorProperty, item);
        var titleColor = handler.GetResolvedProperty<Color?>(Microsoft.Maui.Controls.Shell.TabBarTitleColorProperty, item);
        var unselectedColor = handler.GetResolvedProperty<Color?>(Microsoft.Maui.Controls.Shell.TabBarUnselectedColorProperty, item);
        var disabledColor = handler.GetResolvedProperty<Color?>(Microsoft.Maui.Controls.Shell.TabBarDisabledColorProperty, item);

        var selectedIconBrush = (foregroundColor ?? titleColor)?.ToPlatform();
        var selectedTextBrush = (titleColor ?? foregroundColor)?.ToPlatform();
        var highlightBrush = (IBrush?)foregroundColor?.ToPlatform() ?? Brushes.Transparent;
        var unselectedBrush = unselectedColor?.ToPlatform();
        var disabledBrush = disabledColor?.ToPlatform();

        // Populate dynamic resources used by our custom Styles and the built-in themes
        handler._tabControl.Resources["ShellTabSelectedBackground"] = highlightBrush;
        handler._tabControl.Resources["ShellTabSelectedForeground"] = selectedTextBrush;
        handler._tabControl.Resources["ShellTabHoverBackground"] = Brushes.Transparent;
        handler._tabControl.Resources["ShellTabUnselectedForeground"] = unselectedBrush;

        // Correct Avalonia theme resource keys for selection/accent
        var resourceKeys = new[]
        {
            "TabItemHeaderSelectedBackground",
            "TabItemHeaderSelectedPointerOverBackground",
            "TabItemHeaderPointerOverBackground",
            "TabItemHeaderSelectedForeground",
            "TabItemHeaderSelectedPipeFill",          // Fluent selection line
            "ThemeAccentBrush",                       // Simple accent
            "ThemeAccentBrush2",
            "ThemeAccentBrush3",
            "ThemeAccentBrush4"                        // Simple selected background fallback
        };

        foreach (var tabItemObj in handler._tabControl.Items)
        {
            if (tabItemObj is TabItem tabItem)
            {
                foreach (var key in resourceKeys)
                {
                    if (key.Contains("Foreground")) tabItem.Resources[key] = selectedTextBrush;
                    else tabItem.Resources[key] = highlightBrush;
                }

                if (!tabItem.IsEnabled)
                    tabItem.ApplyColorsToTabItem(disabledBrush, disabledBrush);
                else if (tabItem.Tag == item.CurrentItem || tabItem.IsSelected)
                    tabItem.ApplyColorsToTabItem(selectedTextBrush, selectedIconBrush);
                else
                    tabItem.ApplyColorsToTabItem(unselectedBrush, unselectedBrush);
            }
        }
    }

    internal static void ApplyColorsToTabItem(this TabItem tabItem, IBrush? textBrush, IBrush? iconBrush)
    {
        if (tabItem.Header is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is TextBlock textBlock)
                {
                    if (textBrush != null) textBlock.Foreground = textBrush;
                    else textBlock.ClearValue(TextBlock.ForegroundProperty);
                }
                else if (child is Border border && border.Classes.Contains("shell-tab-header-icon"))
                {
                    if (iconBrush != null) border.Background = iconBrush;
                    else border.ClearValue(Border.BackgroundProperty);
                }
            }
        }
        else if (tabItem.Header is TextBlock tb)
        {
            if (textBrush != null) tb.Foreground = textBrush;
            else tb.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    internal static Page? GetCurrentPage(this ShellItemHandler handler, ShellItem item)
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
