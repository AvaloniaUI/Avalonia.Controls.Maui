using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using AvaloniaPanel = Avalonia.Controls.Panel;

namespace Avalonia.Controls.Maui.Handlers;

public partial class PageHandler : ViewHandler<Microsoft.Maui.Controls.Page, Avalonia.Controls.Maui.Platform.ContentView>
{
    public static IPropertyMapper<Microsoft.Maui.Controls.Page, PageHandler> Mapper =
        new PropertyMapper<Microsoft.Maui.Controls.Page, PageHandler>(ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.Page.Background)] = MapBackground,
            [nameof(Microsoft.Maui.Controls.Page.BackgroundImageSource)] = MapBackgroundImageSource,

            [nameof(Microsoft.Maui.Controls.ContentPage.Content)] = MapContent,
        };

    public static CommandMapper<Microsoft.Maui.Controls.Page, PageHandler> CommandMapper =
        new(ViewCommandMapper);

    public PageHandler() : base(Mapper, CommandMapper)
    {
    }

    public PageHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public PageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override Platform.ContentView CreatePlatformView()
    {
        return new Platform.ContentView
        {
            CrossPlatformLayout = VirtualView as ICrossPlatformLayout
        };
    }

    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);
        
        if (PlatformView != null && VirtualView != null)
        {
            PlatformView.CrossPlatformLayout = VirtualView as ICrossPlatformLayout;
        }
    }

    public static void MapContent(PageHandler handler, Microsoft.Maui.Controls.Page page)
    {
        if (handler.PlatformView is Platform.ContentView platformView &&
            page is IContentView contentView)
        {
            platformView.UpdateContent(contentView, handler.MauiContext);
        }
    }

    public static void MapBackground(PageHandler handler, Microsoft.Maui.Controls.Page page)
    {
        var platformView = (AvaloniaPanel)handler.PlatformView;
        platformView?.UpdateBackground(page);
    }

    public static void MapBackgroundImageSource(PageHandler handler, Microsoft.Maui.Controls.Page page)
    {
        var platformView = (AvaloniaPanel)handler.PlatformView;
        platformView?.UpdateBackgroundImageSource(page, handler.MauiContext);
    }
}