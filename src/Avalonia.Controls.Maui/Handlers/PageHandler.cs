using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Extensions;
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
using AvaloniaPanel = Avalonia.Controls.Panel;
using MauiPage = Microsoft.Maui.Controls.Page;
using MauiElement = Microsoft.Maui.Controls.Element;
using MauiNavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Avalonia handler for <see cref="MauiPage"/>.
/// </summary>
public partial class PageHandler : ViewHandler<MauiPage, AvaloniaContentPage>
{
    /// <summary>
    /// Property mapper for <see cref="PageHandler"/>.
    /// </summary>
    public static IPropertyMapper<MauiPage, PageHandler> Mapper =
        new PropertyMapper<MauiPage, PageHandler>(ViewMapper)
        {
            [nameof(MauiPage.Background)] = MapBackground,
            [nameof(MauiPage.BackgroundImageSource)] = MapBackgroundImageSource,
            [nameof(MauiPage.Title)] = MapTitle,

            [nameof(ContentPage.Content)] = MapContent,
        };

    /// <summary>
    /// Command mapper for <see cref="PageHandler"/>.
    /// </summary>
    public static CommandMapper<MauiPage, PageHandler> CommandMapper =
        new(ViewCommandMapper);

    /// <summary>
    /// Initializes a new instance of <see cref="PageHandler"/>.
    /// </summary>
    public PageHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PageHandler"/>.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public PageHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PageHandler"/>.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public PageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Gets the inner <see cref="Platform.ContentView"/> that hosts the MAUI cross-platform layout.
    /// </summary>
    internal Platform.ContentView? InnerContentView => PlatformView?.Content as Platform.ContentView;

    /// <summary>
    /// Creates the Avalonia platform view for this handler.
    /// </summary>
    /// <returns>A new <see cref="AvaloniaContentPage"/> instance wrapping a <see cref="Platform.ContentView"/> with cross-platform layout.</returns>
    protected override AvaloniaContentPage CreatePlatformView()
    {
        return new AvaloniaContentPage
        {
            Content = new Platform.ContentView
            {
                CrossPlatformLayout = VirtualView as ICrossPlatformLayout
            },
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
    }

    /// <inheritdoc/>
    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        if (InnerContentView != null && VirtualView != null)
        {
            InnerContentView.CrossPlatformLayout = VirtualView as ICrossPlatformLayout;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(AvaloniaContentPage platformView)
    {
        base.DisconnectHandler(platformView);

        // Null the cross-platform layout delegate so the Avalonia ContentView
        // does not hold a strong path back to the MAUI page.
        if (platformView.Content is Platform.ContentView contentView)
        {
            contentView.CrossPlatformLayout = null;
        }
    }

    /// <summary>
    /// Maps the <see cref="ContentPage.Content"/> property to the platform view.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="page">The associated <see cref="MauiPage"/> instance.</param>
    public static void MapContent(PageHandler handler, MauiPage page)
    {
        if (handler.InnerContentView is Platform.ContentView contentView &&
            page is IContentView contentViewInterface)
        {
            contentView.UpdateContent(contentViewInterface, handler.MauiContext);
        }
    }

    /// <summary>
    /// Maps the Background property to the platform view.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="page">The associated <see cref="MauiPage"/> instance.</param>
    public static void MapBackground(PageHandler handler, MauiPage page)
    {
        if (handler.InnerContentView is { } contentView)
        {
            if (!page.Background?.IsEmpty ?? false)
            {
                contentView.Background = page.Background.ToPlatform();
            }
            else if (page.BackgroundColor?.IsDefault() == false)
            {
                contentView.Background = page.BackgroundColor.ToPlatform();
            }
            else
            {
                contentView.ClearValue(Avalonia.Controls.Primitives.TemplatedControl.BackgroundProperty);
            }
        }
    }

    /// <summary>
    /// Maps the <see cref="MauiPage.BackgroundImageSource"/> property to the platform view.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="page">The associated <see cref="MauiPage"/> instance.</param>
    public static void MapBackgroundImageSource(PageHandler handler, MauiPage page)
    {
        if (handler.InnerContentView is { } contentView)
        {
            ((AvaloniaPanel)contentView).UpdateBackgroundImageSource(page, handler.MauiContext);
        }
    }

    /// <summary>
    /// Maps the <see cref="MauiPage.Title"/> property to the platform view.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="page">The associated <see cref="MauiPage"/> instance.</param>
    public static void MapTitle(PageHandler handler, MauiPage page)
    {
        handler.PlatformView.Header = page.Title;
    }

    /// <summary>
    /// Updates the navigation-related properties on the Avalonia <see cref="AvaloniaContentPage"/>
    /// from the MAUI page. Called by navigation hosts (NavigationPage, TabbedPage) to sync
    /// title, toolbar items, back button, and title view/icon.
    /// </summary>
    /// <param name="contentPage">The Avalonia ContentPage to update.</param>
    /// <param name="mauiPage">The MAUI page view.</param>
    internal static void UpdateNavigationProperties(AvaloniaContentPage contentPage, IView mauiPage)
    {
        if (mauiPage is MauiPage page)
        {
            contentPage.Header = page.Title;

            var hasNavigationBar = MauiNavigationPage.GetHasNavigationBar(page);
            AvaloniaNavigationPage.SetHasNavigationBar(contentPage, hasNavigationBar);

            var hasBackButton = MauiNavigationPage.GetHasBackButton(page);
            AvaloniaNavigationPage.SetHasBackButton(contentPage, hasBackButton);

            if (page.ToolbarItems.Count > 0)
            {
                AvaloniaNavigationPage.SetTopCommandBar(contentPage, new ToolbarCommandBar(page.ToolbarItems));
            }
            else
            {
                AvaloniaNavigationPage.SetTopCommandBar(contentPage, null);
            }

            // Set TitleView as Header if available (takes highest precedence)
            var titleView = MauiNavigationPage.GetTitleView(page);
            if (titleView != null)
            {
                contentPage.Header = titleView.ToPlatform(
                    page.Handler?.MauiContext ?? FindMauiContext(page));
            }
            else
            {
                // Check for TitleIconImageSource — compose icon + title
                var titleIconSource = MauiNavigationPage.GetTitleIconImageSource(page);
                if (titleIconSource != null && !titleIconSource.IsEmpty)
                {
                    var mauiContext = page.Handler?.MauiContext ?? FindMauiContext(page);
                    _ = LoadTitleIconAsync(contentPage, page.Title, titleIconSource, mauiContext);
                }
            }
        }
    }

    private static async Task LoadTitleIconAsync(
        AvaloniaContentPage contentPage,
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
                    Orientation = Orientation.Horizontal,
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

                contentPage.Header = panel;
            }
        }
        catch
        {
            // Silently ignore icon loading failures
        }
    }

    private static IMauiContext FindMauiContext(MauiPage page)
    {
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
