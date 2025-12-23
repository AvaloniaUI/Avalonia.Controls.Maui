using Avalonia.Controls.Maui.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AvaloniaImage = Avalonia.Controls.Image;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping <see cref="TabbedPage"/> properties to Avalonia <see cref="TabControl"/>.
/// </summary>
public static class TabbedPageExtensions
{
    // Resource keys for tab bar styling (used by TabbedPage.axaml DynamicResource bindings)
    private const string BarBackgroundResourceKey = "TabbedPageBarBackground";
    private const string BarTextColorResourceKey = "TabbedPageBarTextColor";
    private const string SelectedTabColorResourceKey = "TabbedPageSelectedTabColor";
    private const string UnselectedTabColorResourceKey = "TabbedPageUnselectedTabColor";

    /// <summary>
    /// Updates the tab bar background brush from the TabbedPage's BarBackground property.
    /// </summary>
    /// <param name="tabControl">The Avalonia TabControl to update.</param>
    /// <param name="tabbedPage">The MAUI TabbedPage containing the BarBackground value.</param>
    /// <remarks>
    /// This property affects the tab bar strip area via DynamicResource binding.
    /// </remarks>
    public static void UpdateBarBackground(this TabControl tabControl, TabbedPage tabbedPage)
    {
        if (tabbedPage.BarBackground != null)
        {
            var brush = tabbedPage.BarBackground.ToPlatform();
            if (brush != null)
            {
                tabControl.Resources[BarBackgroundResourceKey] = brush;
            }
        }
        else
        {
            tabControl.Resources.Remove(BarBackgroundResourceKey);
        }
    }

    /// <summary>
    /// Updates the tab bar background color from the TabbedPage's BarBackgroundColor property.
    /// </summary>
    /// <param name="tabControl">The Avalonia TabControl to update.</param>
    /// <param name="tabbedPage">The MAUI TabbedPage containing the BarBackgroundColor value.</param>
    /// <remarks>
    /// If BarBackground is set, it takes precedence over BarBackgroundColor.
    /// This property affects the tab bar strip area via DynamicResource binding.
    /// </remarks>
    public static void UpdateBarBackgroundColor(this TabControl tabControl, TabbedPage tabbedPage)
    {
        // BarBackground takes precedence
        if (tabbedPage.BarBackground != null)
            return;

        if (tabbedPage.BarBackgroundColor != null)
        {
            var brush = new Media.SolidColorBrush(tabbedPage.BarBackgroundColor.ToAvaloniaColor());
            tabControl.Resources[BarBackgroundResourceKey] = brush;
        }
        else
        {
            tabControl.Resources.Remove(BarBackgroundResourceKey);
        }
    }

    /// <summary>
    /// Updates the tab bar text color from the TabbedPage's BarTextColor property.
    /// </summary>
    /// <param name="tabControl">The Avalonia TabControl to update.</param>
    /// <param name="tabbedPage">The MAUI TabbedPage containing the BarTextColor value.</param>
    /// <remarks>
    /// This sets the text color for tab headers in the bar.
    /// </remarks>
    public static void UpdateBarTextColor(this TabControl tabControl, TabbedPage tabbedPage)
    {
        if (tabbedPage.BarTextColor != null)
        {
            var brush = new Media.SolidColorBrush(tabbedPage.BarTextColor.ToAvaloniaColor());
            tabControl.Resources[BarTextColorResourceKey] = brush;

            // Apply to all tab item headers
            foreach (var item in tabControl.Items)
            {
                if (item is TabItem tabItem)
                {
                    tabItem.Foreground = brush;
                }
            }
        }
        else
        {
            tabControl.Resources.Remove(BarTextColorResourceKey);

            foreach (var item in tabControl.Items)
            {
                if (item is TabItem tabItem)
                {
                    tabItem.ClearValue(TabItem.ForegroundProperty);
                }
            }
        }
    }

    /// <summary>
    /// Updates the selected tab color from the TabbedPage's SelectedTabColor property.
    /// </summary>
    /// <param name="tabControl">The Avalonia TabControl to update.</param>
    /// <param name="tabbedPage">The MAUI TabbedPage containing the SelectedTabColor value.</param>
    /// <remarks>
    /// This applies a background color to the currently selected tab header.
    /// </remarks>
    public static void UpdateSelectedTabColor(this TabControl tabControl, TabbedPage tabbedPage)
    {
        if (tabbedPage.SelectedTabColor != null)
        {
            var brush = new Media.SolidColorBrush(tabbedPage.SelectedTabColor.ToAvaloniaColor());
            tabControl.Resources[SelectedTabColorResourceKey] = brush;

            // Apply to currently selected tab item
            if (tabControl.SelectedItem is TabItem selectedTabItem)
            {
                selectedTabItem.Background = brush;
            }
        }
        else
        {
            tabControl.Resources.Remove(SelectedTabColorResourceKey);

            if (tabControl.SelectedItem is TabItem selectedTabItem)
            {
                selectedTabItem.ClearValue(TabItem.BackgroundProperty);
            }
        }
    }

    /// <summary>
    /// Updates the unselected tab color from the TabbedPage's UnselectedTabColor property.
    /// </summary>
    /// <param name="tabControl">The Avalonia TabControl to update.</param>
    /// <param name="tabbedPage">The MAUI TabbedPage containing the UnselectedTabColor value.</param>
    /// <remarks>
    /// This applies a background color to tabs that are not currently selected.
    /// </remarks>
    public static void UpdateUnselectedTabColor(this TabControl tabControl, TabbedPage tabbedPage)
    {
        if (tabbedPage.UnselectedTabColor != null)
        {
            var brush = new Media.SolidColorBrush(tabbedPage.UnselectedTabColor.ToAvaloniaColor());
            tabControl.Resources[UnselectedTabColorResourceKey] = brush;

            // Apply to all non-selected tab items
            foreach (var item in tabControl.Items)
            {
                if (item is TabItem tabItem && tabItem != tabControl.SelectedItem)
                {
                    tabItem.Background = brush;
                }
            }
        }
        else
        {
            tabControl.Resources.Remove(UnselectedTabColorResourceKey);

            foreach (var item in tabControl.Items)
            {
                if (item is TabItem tabItem && tabItem != tabControl.SelectedItem)
                {
                    tabItem.ClearValue(TabItem.BackgroundProperty);
                }
            }
        }
    }

    /// <summary>
    /// Updates the tab items to reflect the TabbedPage's Children collection.
    /// </summary>
    /// <param name="tabControl">The Avalonia TabControl to update.</param>
    /// <param name="tabbedPage">The MAUI TabbedPage containing the children pages.</param>
    /// <param name="mauiContext">The MAUI context for converting pages to platform views.</param>
    public static void UpdateChildren(this TabControl tabControl, TabbedPage tabbedPage, IMauiContext? mauiContext)
    {
        if (mauiContext == null)
            return;

        tabControl.Items.Clear();

        foreach (var page in tabbedPage.Children)
        {
            var tabItem = new TabItem
            {
                Header = CreateTabHeader(page, mauiContext),
                Content = page.ToPlatform(mauiContext)
            };

            tabControl.Items.Add(tabItem);
        }

        // Reapply bar and tab colors after rebuilding items
        ApplyAllTabColors(tabControl, tabbedPage);

        // Apply SelectedItem if set
        tabControl.UpdateSelectedItem(tabbedPage);
    }

    /// <summary>
    /// Creates a tab header from a page, including icon if available.
    /// </summary>
    private static object CreateTabHeader(Page page, IMauiContext mauiContext)
    {
        var title = page.Title ?? "Tab";
        
        // If no icon, just return the title
        if (page.IconImageSource == null)
            return title;

        // Create header with icon and text
        var headerPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 4
        };

        // Load icon asynchronously
        _ = LoadTabIconAsync(headerPanel, page.IconImageSource, mauiContext);

        // Add text
        headerPanel.Children.Add(new TextBlock
        {
            Text = title,
            VerticalAlignment = Layout.VerticalAlignment.Center
        });

        return headerPanel;
    }

    /// <summary>
    /// Loads the tab icon asynchronously.
    /// </summary>
    private static async Task LoadTabIconAsync(StackPanel headerPanel, ImageSource imageSource, IMauiContext mauiContext)
    {
        try
        {
            var image = new AvaloniaImage
            {
                Width = 16,
                Height = 16,
                VerticalAlignment = Layout.VerticalAlignment.Center
            };

            var services = mauiContext.Services;
            var imageSourceServiceProvider = services.GetRequiredService<IImageSourceServiceProvider>();
            var imageSourceService = imageSourceServiceProvider.GetRequiredImageSourceService(imageSource);

            var result = await imageSourceService.GetImageAsync(imageSource);

            if (result is IImageSourceServiceResult<Media.Imaging.Bitmap> bitmapResult)
            {
                image.Source = bitmapResult.Value;
                // Insert icon at the beginning
                if (headerPanel.Children.Count > 0)
                    headerPanel.Children.Insert(0, image);
                else
                    headerPanel.Children.Add(image);
            }
        }
        catch
        {
            // Silently ignore icon loading failures
        }
    }

    /// <summary>
    /// Updates the selected tab to match the TabbedPage's CurrentPage property.
    /// </summary>
    /// <param name="tabControl">The Avalonia TabControl to update.</param>
    /// <param name="tabbedPage">The MAUI TabbedPage containing the CurrentPage value.</param>
    public static void UpdateCurrentPage(this TabControl tabControl, TabbedPage tabbedPage)
    {
        if (tabbedPage.CurrentPage != null)
        {
            var index = tabbedPage.Children.IndexOf(tabbedPage.CurrentPage);
            if (index >= 0 && index != tabControl.SelectedIndex)
            {
                tabControl.SelectedIndex = index;
            }
        }
    }

    /// <summary>
    /// Updates the selected tab to match the TabbedPage's SelectedItem property.
    /// </summary>
    /// <param name="tabControl">The Avalonia TabControl to update.</param>
    /// <param name="tabbedPage">The MAUI TabbedPage containing the SelectedItem value.</param>
    /// <remarks>
    /// SelectedItem is inherited from MultiPage&lt;T&gt; and represents the data object
    /// when using ItemsSource, or the Page when using Children directly.
    /// </remarks>
    public static void UpdateSelectedItem(this TabControl tabControl, TabbedPage tabbedPage)
    {
        var selectedItem = tabbedPage.SelectedItem;
        if (selectedItem == null)
            return;

        // When using ItemsSource, SelectedItem is the data object
        // When using Children, SelectedItem is the Page itself
        if (tabbedPage.ItemsSource != null)
        {
            var itemsList = tabbedPage.ItemsSource.Cast<object>().ToList();
            var index = itemsList.IndexOf(selectedItem);
            if (index >= 0 && index != tabControl.SelectedIndex)
            {
                tabControl.SelectedIndex = index;
            }
        }
        else if (selectedItem is Page page)
        {
            var index = tabbedPage.Children.IndexOf(page);
            if (index >= 0 && index != tabControl.SelectedIndex)
            {
                tabControl.SelectedIndex = index;
            }
        }
    }

    /// <summary>
    /// Reapplies all tab colors from stored resources after tab items are rebuilt or selection changes.
    /// </summary>
    public static void ApplyAllTabColors(this TabControl tabControl, TabbedPage tabbedPage)
    {
        // Apply bar background resource (template will pick it up via DynamicResource)
        if (tabbedPage.BarBackground != null)
        {
            var brush = tabbedPage.BarBackground.ToPlatform();
            if (brush != null)
            {
                tabControl.Resources[BarBackgroundResourceKey] = brush;
            }
        }
        else if (tabbedPage.BarBackgroundColor != null)
        {
            var brush = new Media.SolidColorBrush(tabbedPage.BarBackgroundColor.ToAvaloniaColor());
            tabControl.Resources[BarBackgroundResourceKey] = brush;
        }

        // Apply selected tab color
        if (tabbedPage.SelectedTabColor != null)
        {
            tabControl.UpdateSelectedTabColor(tabbedPage);
        }

        // Apply unselected tab color
        if (tabbedPage.UnselectedTabColor != null)
        {
            tabControl.UpdateUnselectedTabColor(tabbedPage);
        }

        // Apply text color
        if (tabbedPage.BarTextColor != null)
        {
            var textBrush = new Media.SolidColorBrush(tabbedPage.BarTextColor.ToAvaloniaColor());
            foreach (var item in tabControl.Items)
            {
                if (item is TabItem tabItem)
                {
                    tabItem.Foreground = textBrush;
                }
            }
        }
    }
}
