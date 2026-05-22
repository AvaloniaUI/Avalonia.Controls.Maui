using Avalonia.Controls.Maui.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AvaloniaContentPage = Avalonia.Controls.ContentPage;
using AvaloniaImage = Avalonia.Controls.Image;
using AvaloniaPage = Avalonia.Controls.Page;
using AvaloniaTabbedPage = Avalonia.Controls.TabbedPage;
using MauiTabbedPage = Microsoft.Maui.Controls.TabbedPage;
using MauiPage = Microsoft.Maui.Controls.Page;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping <see cref="MauiTabbedPage"/> properties to Avalonia <see cref="AvaloniaTabbedPage"/>.
/// </summary>
public static class TabbedPageExtensions
{
    // Resource key for tab bar strip background (matches TabbedPage Fluent theme)
    private const string BarBackgroundResourceKey = "TabbedPageTabStripBackground";

    // TabbedPage Fluent theme resource keys we override for tab appearance.
    // The indicator binds to $parent[TabItem].Foreground, so setting the selected
    // foreground key controls both tab text and the selection indicator.
    private static readonly string[] FluentThemeKeys =
    [
        "TabbedPageTabItemHeaderForegroundSelected",
        "TabbedPageTabItemHeaderForegroundUnselected",
        "TabbedPageTabItemHeaderForegroundDisabled"
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

        tabbedPage.DisposeTabHeaderIcons();

        var pages = new List<Avalonia.Controls.Page>();
        var pagesToLoadIcons = new List<(AvaloniaPage PlatformPage, MauiPage MauiPage)>();

        foreach (var page in mauiTabbedPage.Children)
        {
            var wrappedPage = (AvaloniaContentPage)page.ToPlatform(mauiContext);
            wrappedPage.Header = CreateTabHeader(page);
            pages.Add(wrappedPage);
            pagesToLoadIcons.Add((wrappedPage, page));
        }

        tabbedPage.Pages = pages;

        foreach (var (platformPage, page) in pagesToLoadIcons)
        {
            LoadTabIconIfNeeded(tabbedPage, platformPage, page, mauiContext);
        }

        // Reapply bar and tab colors after rebuilding items
        UpdateTabColors(tabbedPage, mauiTabbedPage);

        // Reapply current selection after rebuilding items
        tabbedPage.UpdateCurrentPage(mauiTabbedPage);
        tabbedPage.UpdateSelectedItem(mauiTabbedPage);
    }

    /// <summary>
    /// Updates a child tab title without rebuilding the whole header, preserving any loaded tab icon.
    /// </summary>
    public static void UpdateChildTitle(
        this AvaloniaTabbedPage tabbedPage,
        MauiTabbedPage mauiTabbedPage,
        MauiPage page,
        IMauiContext? mauiContext,
        object? preservedHeader = null,
        ImageSource? preservedIconImageSource = null)
    {
        if (mauiContext == null || tabbedPage.Pages == null)
            return;

        var index = mauiTabbedPage.Children.IndexOf(page);
        if (index < 0)
            return;

        var platformPage = tabbedPage.Pages.ElementAtOrDefault(index);
        if (platformPage == null)
            return;

        var title = page.Title ?? "Tab";

        if (platformPage.Header is StackPanel panel)
        {
            var titleBlock = panel.Children.OfType<TextBlock>().FirstOrDefault();
            if (titleBlock != null)
            {
                titleBlock.Text = title;
                return;
            }
        }

        if (platformPage.Header is string)
        {
            if (page.IconImageSource != null &&
                ReferenceEquals(page.IconImageSource, preservedIconImageSource) &&
                preservedHeader is StackPanel preservedPanel)
            {
                var preservedTitleBlock = preservedPanel.Children.OfType<TextBlock>().FirstOrDefault();
                if (preservedTitleBlock != null)
                {
                    preservedTitleBlock.Text = title;
                    platformPage.Header = preservedPanel;
                    return;
                }
            }

            platformPage.Header = page.IconImageSource == null
                ? title
                : CreateTabHeader(page);
            LoadTabIconIfNeeded(tabbedPage, platformPage, page, mauiContext);
            return;
        }

        DisposeHeaderIconImages(platformPage.Header);
        platformPage.Header = CreateTabHeader(page);
        LoadTabIconIfNeeded(tabbedPage, platformPage, page, mauiContext);
    }

    /// <summary>
    /// Updates a child tab icon without rebuilding the tab page collection.
    /// </summary>
    public static void UpdateChildIconImageSource(
        this AvaloniaTabbedPage tabbedPage,
        MauiTabbedPage mauiTabbedPage,
        MauiPage page,
        IMauiContext? mauiContext)
    {
        if (mauiContext == null || tabbedPage.Pages == null)
            return;

        var index = mauiTabbedPage.Children.IndexOf(page);
        if (index < 0)
            return;

        var platformPage = tabbedPage.Pages.ElementAtOrDefault(index);
        if (platformPage == null)
            return;

        DisposeHeaderIconImages(platformPage.Header);
        platformPage.Header = CreateTabHeader(page);
        LoadTabIconIfNeeded(tabbedPage, platformPage, page, mauiContext);
    }

    /// <summary>
    /// Creates a tab header from a page, including icon if available.
    /// </summary>
    private static object CreateTabHeader(MauiPage page)
    {
        var title = page.Title ?? "Tab";

        // If no icon, just return the title
        if (page.IconImageSource == null)
            return title;

        var headerPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 4
        };

        headerPanel.Children.Add(new TextBlock
        {
            Text = title,
            VerticalAlignment = Layout.VerticalAlignment.Center
        });

        return headerPanel;
    }

    private static void LoadTabIconIfNeeded(AvaloniaTabbedPage tabbedPage, AvaloniaPage platformPage, MauiPage page, IMauiContext mauiContext)
    {
        if (page.IconImageSource == null || platformPage.Header is not StackPanel headerPanel)
            return;

        _ = LoadTabIconAsync(tabbedPage, platformPage, headerPanel, page.IconImageSource, mauiContext);
    }

    /// <summary>
    /// Loads the tab icon asynchronously.
    /// </summary>
    private static async Task LoadTabIconAsync(AvaloniaTabbedPage tabbedPage, AvaloniaPage platformPage, StackPanel headerPanel, ImageSource imageSource, IMauiContext mauiContext)
    {
        try
        {
            var services = mauiContext.Services;
            var imageSourceServiceProvider = services.GetRequiredService<IImageSourceServiceProvider>();
            var imageSourceService = imageSourceServiceProvider.GetImageSourceService(GetImageSourceInterfaceType(imageSource));

            if (imageSourceService is IAvaloniaImageSourceService avaloniaImageSourceService)
            {
                var result = await avaloniaImageSourceService.GetImageAsync(imageSource);
                if (result?.Value != null)
                {
                    if (!ReferenceEquals(platformPage.Header, headerPanel) ||
                        tabbedPage.Pages?.Contains(platformPage) != true)
                    {
                        (result as IDisposable)?.Dispose();
                        return;
                    }

                    var image = new AvaloniaImage
                    {
                        Source = result.Value,
                        Width = 16,
                        Height = 16,
                        VerticalAlignment = Layout.VerticalAlignment.Center
                    };

                    if (result is IDisposable disposable)
                    {
                        image.Tag = disposable;
                    }

                    if (headerPanel.Children.Count > 0)
                    {
                        headerPanel.Children.Insert(0, image);
                    }
                    else
                    {
                        headerPanel.Children.Add(image);
                    }
                }
            }
        }
        catch
        {
            // Silently ignore icon loading failures
        }
    }

    private static void DisposeHeaderIconImages(object? header)
    {
        if (header is not StackPanel panel)
            return;

        foreach (var image in panel.Children.OfType<AvaloniaImage>())
        {
            if (image.Tag is IDisposable disposable)
            {
                image.Tag = null;
                image.Source = null;
                disposable.Dispose();
            }
        }
    }

    internal static void DisposeTabHeaderIcons(this AvaloniaTabbedPage tabbedPage)
    {
        if (tabbedPage.Pages == null)
            return;

        foreach (var page in tabbedPage.Pages)
        {
            DisposeHeaderIconImages(page.Header);
        }
    }

    internal static void DetachAndDisposeTabHeaderIcons(this AvaloniaTabbedPage tabbedPage)
    {
        if (tabbedPage.Pages == null)
            return;

        var pages = tabbedPage.Pages.ToList();
        tabbedPage.Pages = Array.Empty<AvaloniaPage>();

        foreach (var page in pages)
        {
            DisposeHeaderIconImages(page.Header);
        }
    }

    private static Type GetImageSourceInterfaceType(ImageSource imageSource) => imageSource switch
    {
        IFileImageSource => typeof(IFileImageSource),
        IFontImageSource => typeof(IFontImageSource),
        IUriImageSource => typeof(IUriImageSource),
        IStreamImageSource => typeof(IStreamImageSource),
        _ => imageSource.GetType()
    };

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
            // SelectedTabColor controls the selected tab text + indicator (indicator binds to TabItem.Foreground)
            if (selectedTabColor != null)
            {
                var brush = new Media.SolidColorBrush(selectedTabColor.ToAvaloniaColor());
                tabbedPage.Resources["TabbedPageTabItemHeaderForegroundSelected"] = brush;
            }

            // BarTextColor overrides both selected and unselected foreground
            if (barTextColor != null)
            {
                var brush = new Media.SolidColorBrush(barTextColor.ToAvaloniaColor());
                tabbedPage.Resources["TabbedPageTabItemHeaderForegroundSelected"] = brush;
                tabbedPage.Resources["TabbedPageTabItemHeaderForegroundUnselected"] = brush;
            }

            if (unselectedTabColor != null)
            {
                var brush = new Media.SolidColorBrush(unselectedTabColor.ToAvaloniaColor());
                tabbedPage.Resources["TabbedPageTabItemHeaderForegroundUnselected"] = brush;
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
