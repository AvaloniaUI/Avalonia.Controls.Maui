using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using AvaloniaPanel = Avalonia.Controls.Panel;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Avalonia handler for <see cref="Microsoft.Maui.Controls.Page"/>.
/// </summary>
public partial class PageHandler : ViewHandler<Microsoft.Maui.Controls.Page, Avalonia.Controls.Maui.Platform.ContentView>
{
    /// <summary>
    /// Property mapper for <see cref="PageHandler"/>.
    /// </summary>
    public static IPropertyMapper<Microsoft.Maui.Controls.Page, PageHandler> Mapper =
        new PropertyMapper<Microsoft.Maui.Controls.Page, PageHandler>(ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.Page.Background)] = MapBackground,
            [nameof(Microsoft.Maui.Controls.Page.BackgroundImageSource)] = MapBackgroundImageSource,

            [nameof(Microsoft.Maui.Controls.ContentPage.Content)] = MapContent,
        };

    /// <summary>
    /// Command mapper for <see cref="PageHandler"/>.
    /// </summary>
    public static CommandMapper<Microsoft.Maui.Controls.Page, PageHandler> CommandMapper =
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
    /// Creates the Avalonia platform view for this handler.
    /// </summary>
    /// <returns>A new <see cref="Platform.ContentView"/> instance configured with cross-platform layout.</returns>
    protected override Platform.ContentView CreatePlatformView()
    {
        return new Platform.ContentView
        {
            CrossPlatformLayout = VirtualView as ICrossPlatformLayout
        };
    }

    /// <inheritdoc/>
    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);
        
        if (PlatformView != null && VirtualView != null)
        {
            PlatformView.CrossPlatformLayout = VirtualView as ICrossPlatformLayout;
        }
    }

    /// <summary>
    /// Maps the <see cref="Microsoft.Maui.Controls.ContentPage.Content"/> property to the platform view.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="page">The associated <see cref="Microsoft.Maui.Controls.Page"/> instance.</param>
    public static void MapContent(PageHandler handler, Microsoft.Maui.Controls.Page page)
    {
        if (handler.PlatformView is Platform.ContentView platformView &&
            page is IContentView contentView)
        {
            platformView.UpdateContent(contentView, handler.MauiContext);
        }
    }

    /// <summary>
    /// Maps the Background property to the platform view.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="page">The associated <see cref="Microsoft.Maui.Controls.Page"/> instance.</param>
    public static void MapBackground(PageHandler handler, Microsoft.Maui.Controls.Page page)
    {
        var platformView = (AvaloniaPanel)handler.PlatformView;
        platformView?.UpdateBackground(page);
    }

    /// <summary>
    /// Maps the <see cref="Microsoft.Maui.Controls.Page.BackgroundImageSource"/> property to the platform view.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="page">The associated <see cref="Microsoft.Maui.Controls.Page"/> instance.</param>
    public static void MapBackgroundImageSource(PageHandler handler, Microsoft.Maui.Controls.Page page)
    {
        var platformView = (AvaloniaPanel)handler.PlatformView;
        platformView?.UpdateBackgroundImageSource(page, handler.MauiContext);
    }
}