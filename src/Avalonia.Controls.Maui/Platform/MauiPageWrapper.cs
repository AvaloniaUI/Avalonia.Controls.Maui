using System.Runtime.CompilerServices;
using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Services;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AvaloniaContentPage = Avalonia.Controls.ContentPage;
using AvaloniaNavigationPage = Avalonia.Controls.NavigationPage;
using AvaloniaImage = Avalonia.Controls.Image;
using MauiPage = Microsoft.Maui.Controls.Page;
using MauiElement = Microsoft.Maui.Controls.Element;
using MauiNavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Wraps a MAUI page's platform view inside an <see cref="AvaloniaContentPage"/>
/// so that it can be hosted in an <see cref="AvaloniaNavigationPage"/> or <see cref="Avalonia.Controls.TabbedPage"/>.
/// </summary>
public static class MauiPageWrapper
{
    private static readonly ConditionalWeakTable<IView, AvaloniaContentPage> _cache = new();

    /// <summary>
    /// Gets or creates an <see cref="AvaloniaContentPage"/> wrapper for the given MAUI page.
    /// </summary>
    /// <param name="mauiPage">The MAUI page view to wrap.</param>
    /// <param name="mauiContext">The MAUI context for converting the page to a platform view.</param>
    /// <returns>An Avalonia ContentPage wrapping the MAUI page's platform view.</returns>
    public static AvaloniaContentPage Wrap(IView mauiPage, IMauiContext mauiContext)
    {
        if (_cache.TryGetValue(mauiPage, out var existing))
        {
            // Update properties in case they changed
            UpdateProperties(existing, mauiPage);
            return existing;
        }

        var platformView = mauiPage.ToPlatform(mauiContext);

        var wrapper = new AvaloniaContentPage
        {
            Content = platformView,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        UpdateProperties(wrapper, mauiPage);

        _cache.AddOrUpdate(mauiPage, wrapper);
        return wrapper;
    }

    /// <summary>
    /// Updates the wrapper's navigation properties from the MAUI page.
    /// </summary>
    /// <param name="wrapper">The Avalonia ContentPage wrapper.</param>
    /// <param name="mauiPage">The MAUI page view.</param>
    public static void UpdateProperties(AvaloniaContentPage wrapper, IView mauiPage)
    {
        if (mauiPage is MauiPage page)
        {
            wrapper.Header = page.Title;

            var hasNavigationBar = MauiNavigationPage.GetHasNavigationBar(page);
            AvaloniaNavigationPage.SetHasNavigationBar(wrapper, hasNavigationBar);

            var hasBackButton = MauiNavigationPage.GetHasBackButton(page);
            AvaloniaNavigationPage.SetHasBackButton(wrapper, hasBackButton);

            // Set toolbar items as TopCommandBar
            if (page.ToolbarItems.Count > 0)
            {
                wrapper.TopCommandBar = new ToolbarCommandBar(page.ToolbarItems);
            }
            else
            {
                wrapper.TopCommandBar = null;
            }

            // Set TitleView as Header if available (takes highest precedence)
            var titleView = MauiNavigationPage.GetTitleView(page);
            if (titleView != null)
            {
                wrapper.Header = titleView.ToPlatform(
                    page.Handler?.MauiContext ?? FindMauiContext(page));
            }
            else
            {
                // Check for TitleIconImageSource — compose icon + title
                var titleIconSource = MauiNavigationPage.GetTitleIconImageSource(page);
                if (titleIconSource != null && !titleIconSource.IsEmpty)
                {
                    var mauiContext = page.Handler?.MauiContext ?? FindMauiContext(page);
                    _ = LoadTitleIconAsync(wrapper, page.Title, titleIconSource, mauiContext);
                }
            }
        }
    }

    private static async Task LoadTitleIconAsync(
        AvaloniaContentPage wrapper,
        string? title,
        ImageSource imageSource,
        IMauiContext mauiContext)
    {
        try
        {
            var services = mauiContext.Services;
            var imageSourceServiceProvider = services.GetRequiredService<IImageSourceServiceProvider>();
            var imageSourceService = imageSourceServiceProvider.GetRequiredImageSourceService(imageSource);

            if (imageSourceService is not IAvaloniaImageSourceService avaloniaService)
                return;

            var result = await avaloniaService.GetImageAsync(imageSource);

            if (result is IImageSourceServiceResult<Bitmap> bitmapResult)
            {
                var panel = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    Spacing = 8
                };

                panel.Children.Add(new AvaloniaImage
                {
                    Source = bitmapResult.Value,
                    Width = 20,
                    Height = 20,
                    VerticalAlignment = VerticalAlignment.Center
                });

                panel.Children.Add(new Avalonia.Controls.TextBlock
                {
                    Text = title ?? string.Empty,
                    VerticalAlignment = VerticalAlignment.Center
                });

                wrapper.Header = panel;
            }
        }
        catch
        {
            // Silently ignore icon loading failures
        }
    }

    /// <summary>
    /// Removes a cached wrapper for the given MAUI page.
    /// </summary>
    /// <param name="mauiPage">The MAUI page view whose wrapper should be removed.</param>
    public static void Remove(IView mauiPage)
    {
        _cache.Remove(mauiPage);
    }

    /// <summary>
    /// Tries to get the cached wrapper for a MAUI page.
    /// </summary>
    /// <param name="mauiPage">The MAUI page view.</param>
    /// <param name="wrapper">The cached wrapper, if found.</param>
    /// <returns>True if a wrapper was found; otherwise false.</returns>
    public static bool TryGetWrapper(IView mauiPage, out AvaloniaContentPage? wrapper)
    {
        return _cache.TryGetValue(mauiPage, out wrapper);
    }

    private static IMauiContext FindMauiContext(MauiPage page)
    {
        // Walk up the parent chain to find a MauiContext
        MauiElement? current = page;
        while (current != null)
        {
            if (current.Handler?.MauiContext is IMauiContext ctx)
                return ctx;
            current = current.Parent as MauiElement;
        }

        throw new InvalidOperationException("Could not find MauiContext for page");
    }
}
