using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using AvaloniaPanel = Avalonia.Controls.Panel;

namespace Avalonia.Controls.Maui.Handlers;

internal partial class PageHandler : ContentViewHandler, IPageHandler
{
    public static new IPropertyMapper<IContentView, IPageHandler> Mapper =
        new PropertyMapper<IContentView, IPageHandler>(ContentViewHandler.Mapper)
        {
            [nameof(IContentView.Background)] = MapBackground,
        };

    public static new CommandMapper<IContentView, IPageHandler> CommandMapper =
        new(ContentViewHandler.CommandMapper);

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

    public static void MapBackground(IPageHandler handler, IContentView page)
    {
        var platformView = (AvaloniaPanel)handler.PlatformView;
        if (platformView == null || page?.Background == null)
            return;
        platformView.Background = page.Background.ToPlatform();
    }
}