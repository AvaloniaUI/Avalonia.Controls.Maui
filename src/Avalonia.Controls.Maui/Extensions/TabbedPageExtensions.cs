using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AvaloniaContentPage = Avalonia.Controls.ContentPage;
using AvaloniaTabbedPage = Avalonia.Controls.TabbedPage;
using AvaloniaImage = Avalonia.Controls.Image;
using MauiTabbedPage = Microsoft.Maui.Controls.TabbedPage;
using MauiPage = Microsoft.Maui.Controls.Page;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping <see cref="MauiTabbedPage"/> properties to Avalonia <see cref="AvaloniaTabbedPage"/>.
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
    /// <param name="tabbedPage">The Avalonia TabbedPage to update.</param>
    /// <param name="mauiTabbedPage">The MAUI TabbedPage containing the BarBackground value.</param>
    public static void UpdateBarBackground(this AvaloniaTabbedPage tabbedPage, MauiTabbedPage mauiTabbedPage)
    {
        if (mauiTabbedPage.BarBackground != null)
        {
            var brush = mauiTabbedPage.BarBackground.ToPlatform();
            if (brush != null)
            {
                tabbedPage.Resources[BarBackgroundResourceKey] = brush;
            }
        }
        else
        {
            tabbedPage.Resources.Remove(BarBackgroundResourceKey);
        }
    }

    /// <summary>
    /// Updates the tab bar background color from the TabbedPage's BarBackgroundColor property.
    /// </summary>
    /// <param name="tabbedPage">The Avalonia TabbedPage to update.</param>
    /// <param name="mauiTabbedPage">The MAUI TabbedPage containing the BarBackgroundColor value.</param>
    public static void UpdateBarBackgroundColor(this AvaloniaTabbedPage tabbedPage, MauiTabbedPage mauiTabbedPage)
    {
        // BarBackground takes precedence
        if (mauiTabbedPage.BarBackground != null)
            return;

        if (mauiTabbedPage.BarBackgroundColor != null)
        {
            var brush = new Media.SolidColorBrush(mauiTabbedPage.BarBackgroundColor.ToAvaloniaColor());
            tabbedPage.Resources[BarBackgroundResourceKey] = brush;
        }
        else
        {
            tabbedPage.Resources.Remove(BarBackgroundResourceKey);
        }
    }

    /// <summary>
    /// Updates the tab bar text color from the TabbedPage's BarTextColor property.
    /// </summary>
    public static void UpdateBarTextColor(this AvaloniaTabbedPage tabbedPage, MauiTabbedPage mauiTabbedPage)
    {
        tabbedPage.UpdateTabColors(mauiTabbedPage);
    }

    /// <summary>
    /// Updates the selected tab color from the TabbedPage's SelectedTabColor property.
    /// </summary>
    public static void UpdateSelectedTabColor(this AvaloniaTabbedPage tabbedPage, MauiTabbedPage mauiTabbedPage)
    {
        tabbedPage.UpdateTabColors(mauiTabbedPage);
    }

    /// <summary>
    /// Updates the unselected tab color from the TabbedPage's UnselectedTabColor property.
    /// </summary>
    public static void UpdateUnselectedTabColor(this AvaloniaTabbedPage tabbedPage, MauiTabbedPage mauiTabbedPage)
    {
        tabbedPage.UpdateTabColors(mauiTabbedPage);
    }

    /// <summary>
    /// Updates the tab items to reflect the TabbedPage's Children collection.
    /// </summary>
    /// <param name="tabbedPage">The Avalonia TabbedPage to update.</param>
    /// <param name="mauiTabbedPage">The MAUI TabbedPage containing the children pages.</param>
    /// <param name="mauiContext">The MAUI context for converting pages to platform views.</param>
    public static void UpdateChildren(this AvaloniaTabbedPage tabbedPage, MauiTabbedPage mauiTabbedPage, IMauiContext? mauiContext)
    {
        if (mauiContext == null)
            return;

        var pages = new List<Avalonia.Controls.Page>();

        foreach (var page in mauiTabbedPage.Children)
        {
            var wrappedPage = MauiPageWrapper.Wrap(page, mauiContext);
            wrappedPage.Header = CreateTabHeader(page, mauiContext);
            pages.Add(wrappedPage);
        }

        tabbedPage.Pages = pages;

        // Reapply bar and tab colors after rebuilding items
        UpdateTabColors(tabbedPage, mauiTabbedPage);

        // Apply SelectedItem if set
        tabbedPage.UpdateSelectedItem(mauiTabbedPage);
    }

    /// <summary>
    /// Creates a tab header from a page, including icon if available.
    /// </summary>
    private static object CreateTabHeader(MauiPage page, IMauiContext mauiContext)
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
    /// <param name="tabbedPage">The Avalonia TabbedPage to update.</param>
    /// <param name="mauiTabbedPage">The MAUI TabbedPage containing the CurrentPage value.</param>
    public static void UpdateCurrentPage(this AvaloniaTabbedPage tabbedPage, MauiTabbedPage mauiTabbedPage)
    {
        if (mauiTabbedPage.CurrentPage != null)
        {
            var index = mauiTabbedPage.Children.IndexOf(mauiTabbedPage.CurrentPage);
            if (index >= 0 && index != tabbedPage.SelectedIndex)
            {
                tabbedPage.SelectedIndex = index;
            }
        }
    }

    /// <summary>
    /// Updates the selected tab to match the TabbedPage's SelectedItem property.
    /// </summary>
    /// <param name="tabbedPage">The Avalonia TabbedPage to update.</param>
    /// <param name="mauiTabbedPage">The MAUI TabbedPage containing the SelectedItem value.</param>
    public static void UpdateSelectedItem(this AvaloniaTabbedPage tabbedPage, MauiTabbedPage mauiTabbedPage)
    {
        var selectedItem = mauiTabbedPage.SelectedItem;
        if (selectedItem == null)
            return;

        if (mauiTabbedPage.ItemsSource != null)
        {
            var itemsList = mauiTabbedPage.ItemsSource.Cast<object>().ToList();
            var index = itemsList.IndexOf(selectedItem);
            if (index >= 0 && index != tabbedPage.SelectedIndex)
            {
                tabbedPage.SelectedIndex = index;
            }
        }
        else if (selectedItem is Page page)
        {
            var index = mauiTabbedPage.Children.IndexOf(page);
            if (index >= 0 && index != tabbedPage.SelectedIndex)
            {
                tabbedPage.SelectedIndex = index;
            }
        }
    }

    /// <summary>
    /// Reapplies all tab colors after tab items are rebuilt or selection changes.
    /// Uses Fluent theme resource overrides instead of direct property setting so that
    /// all states (selected, hover, pressed) are handled correctly by the theme.
    /// </summary>
    public static void UpdateTabColors(this AvaloniaTabbedPage tabbedPage, MauiTabbedPage mauiTabbedPage)
    {
        // Apply bar background resource
        if (mauiTabbedPage.BarBackground != null)
        {
            var brush = mauiTabbedPage.BarBackground.ToPlatform();
            if (brush != null)
            {
                tabbedPage.Resources[BarBackgroundResourceKey] = brush;
            }
        }
        else if (mauiTabbedPage.BarBackgroundColor != null)
        {
            tabbedPage.Resources[BarBackgroundResourceKey] =
                new Media.SolidColorBrush(mauiTabbedPage.BarBackgroundColor.ToAvaloniaColor());
        }

        var selectedTabColor = mauiTabbedPage.SelectedTabColor;
        var unselectedTabColor = mauiTabbedPage.UnselectedTabColor;
        var barTextColor = mauiTabbedPage.BarTextColor;

        bool hasExplicitColors = selectedTabColor != null || unselectedTabColor != null || barTextColor != null;

        if (hasExplicitColors)
        {
            if (selectedTabColor != null)
            {
                var brush = new Media.SolidColorBrush(selectedTabColor.ToAvaloniaColor());
                tabbedPage.Resources["TabItemHeaderSelectedPipeFill"] = brush;
                tabbedPage.Resources["ThemeAccentBrush"] = brush;
                tabbedPage.Resources["ThemeAccentBrush2"] = brush;
                tabbedPage.Resources["ThemeAccentBrush3"] = brush;
                tabbedPage.Resources["ThemeAccentBrush4"] = brush;

                if (barTextColor == null)
                {
                    tabbedPage.Resources["TabItemHeaderForegroundSelected"] = brush;
                    tabbedPage.Resources["TabItemHeaderForegroundSelectedPointerOver"] = brush;
                    tabbedPage.Resources["TabItemHeaderForegroundSelectedPressed"] = brush;
                }
            }

            if (barTextColor != null)
            {
                var brush = new Media.SolidColorBrush(barTextColor.ToAvaloniaColor());
                tabbedPage.Resources["TabItemHeaderForegroundSelected"] = brush;
                tabbedPage.Resources["TabItemHeaderForegroundSelectedPointerOver"] = brush;
                tabbedPage.Resources["TabItemHeaderForegroundSelectedPressed"] = brush;
                tabbedPage.Resources["TabItemHeaderForegroundUnselected"] = brush;
                tabbedPage.Resources["TabItemHeaderForegroundUnselectedPointerOver"] = brush;
                tabbedPage.Resources["TabItemHeaderForegroundUnselectedPressed"] = brush;
            }

            if (unselectedTabColor != null)
            {
                var brush = new Media.SolidColorBrush(unselectedTabColor.ToAvaloniaColor());
                tabbedPage.Resources["TabItemHeaderForegroundUnselected"] = brush;
                tabbedPage.Resources["TabItemHeaderForegroundUnselectedPointerOver"] = brush;
                tabbedPage.Resources["TabItemHeaderForegroundUnselectedPressed"] = brush;
            }
        }
        else
        {
            foreach (var key in FluentThemeKeys)
            {
                tabbedPage.Resources.Remove(key);
            }
        }
    }
}
