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
    // Resource key for tab bar strip background (used by TabbedPage.axaml DynamicResource binding)
    private const string BarBackgroundResourceKey = "TabbedPageBarBackground";

    // Fluent theme resource keys we override for tab appearance
    private static readonly string[] FluentThemeKeys =
    [
        "TabItemHeaderSelectedPipeFill",
        "TabItemHeaderForegroundSelected",
        "TabItemHeaderForegroundSelectedPointerOver",
        "TabItemHeaderForegroundSelectedPressed",
        "TabItemHeaderForegroundUnselected",
        "TabItemHeaderForegroundUnselectedPointerOver",
        "TabItemHeaderForegroundUnselectedPressed",
        "ThemeAccentBrush",
        "ThemeAccentBrush2",
        "ThemeAccentBrush3",
        "ThemeAccentBrush4"
    ];

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
    public static void UpdateBarTextColor(this TabControl tabControl, TabbedPage tabbedPage)
    {
        tabControl.UpdateTabColors(tabbedPage);
    }

    /// <summary>
    /// Updates the selected tab color from the TabbedPage's SelectedTabColor property.
    /// </summary>
    public static void UpdateSelectedTabColor(this TabControl tabControl, TabbedPage tabbedPage)
    {
        tabControl.UpdateTabColors(tabbedPage);
    }

    /// <summary>
    /// Updates the unselected tab color from the TabbedPage's UnselectedTabColor property.
    /// </summary>
    public static void UpdateUnselectedTabColor(this TabControl tabControl, TabbedPage tabbedPage)
    {
        tabControl.UpdateTabColors(tabbedPage);
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
        UpdateTabColors(tabControl, tabbedPage);

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
    /// Reapplies all tab colors after tab items are rebuilt or selection changes.
    /// Uses Fluent theme resource overrides instead of direct property setting so that
    /// all states (selected, hover, pressed) are handled correctly by the theme.
    /// </summary>
    public static void UpdateTabColors(this TabControl tabControl, TabbedPage tabbedPage)
    {
        // Apply bar background resource (template picks it up via DynamicResource)
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
            tabControl.Resources[BarBackgroundResourceKey] =
                new Media.SolidColorBrush(tabbedPage.BarBackgroundColor.ToAvaloniaColor());
        }

        var selectedTabColor = tabbedPage.SelectedTabColor;
        var unselectedTabColor = tabbedPage.UnselectedTabColor;
        var barTextColor = tabbedPage.BarTextColor;

        bool hasExplicitColors = selectedTabColor != null || unselectedTabColor != null || barTextColor != null;

        if (hasExplicitColors)
        {
            // SelectedTabColor → pipe fill (selection indicator) and accent.
            // On Android/iOS this is the indicator tint, not a full background.
            if (selectedTabColor != null)
            {
                var brush = new Media.SolidColorBrush(selectedTabColor.ToAvaloniaColor());
                tabControl.Resources["TabItemHeaderSelectedPipeFill"] = brush;
                tabControl.Resources["ThemeAccentBrush"] = brush;
                tabControl.Resources["ThemeAccentBrush2"] = brush;
                tabControl.Resources["ThemeAccentBrush3"] = brush;
                tabControl.Resources["ThemeAccentBrush4"] = brush;

                // Also use as selected text foreground when BarTextColor isn't set
                if (barTextColor == null)
                {
                    tabControl.Resources["TabItemHeaderForegroundSelected"] = brush;
                    tabControl.Resources["TabItemHeaderForegroundSelectedPointerOver"] = brush;
                    tabControl.Resources["TabItemHeaderForegroundSelectedPressed"] = brush;
                }
            }

            // BarTextColor → all text foreground states
            if (barTextColor != null)
            {
                var brush = new Media.SolidColorBrush(barTextColor.ToAvaloniaColor());
                tabControl.Resources["TabItemHeaderForegroundSelected"] = brush;
                tabControl.Resources["TabItemHeaderForegroundSelectedPointerOver"] = brush;
                tabControl.Resources["TabItemHeaderForegroundSelectedPressed"] = brush;
                tabControl.Resources["TabItemHeaderForegroundUnselected"] = brush;
                tabControl.Resources["TabItemHeaderForegroundUnselectedPointerOver"] = brush;
                tabControl.Resources["TabItemHeaderForegroundUnselectedPressed"] = brush;
            }

            // UnselectedTabColor → unselected foreground (overrides BarTextColor for unselected)
            if (unselectedTabColor != null)
            {
                var brush = new Media.SolidColorBrush(unselectedTabColor.ToAvaloniaColor());
                tabControl.Resources["TabItemHeaderForegroundUnselected"] = brush;
                tabControl.Resources["TabItemHeaderForegroundUnselectedPointerOver"] = brush;
                tabControl.Resources["TabItemHeaderForegroundUnselectedPressed"] = brush;
            }
        }
        else
        {
            // No explicit colors — clear overrides so Fluent theme defaults apply
            foreach (var key in FluentThemeKeys)
            {
                tabControl.Resources.Remove(key);
            }
        }

        // Clear any direct foreground/background overrides on TabItems so
        // Fluent theme resource inheritance handles all state transitions
        foreach (var item in tabControl.Items)
        {
            if (item is TabItem tabItem)
            {
                tabItem.ClearValue(TabItem.ForegroundProperty);
                tabItem.ClearValue(TabItem.BackgroundProperty);
            }
        }
    }
}
