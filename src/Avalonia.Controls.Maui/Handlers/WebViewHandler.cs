using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

[Avalonia.Controls.Maui.Platform.NotImplemented("WebView is not yet implemented for the Avalonia backend")]
public class WebViewHandler : ViewHandler<WebView, PlaceholderControl>
{
    public static IPropertyMapper<WebView, WebViewHandler> Mapper = new PropertyMapper<WebView, WebViewHandler>(ViewHandler.ViewMapper);

    public static CommandMapper<WebView, WebViewHandler> CommandMapper = new(ViewCommandMapper);

    public WebViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public WebViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public WebViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlaceholderControl CreatePlatformView()
    {
        return new PlaceholderControl("WebView is not available in this version");
    }
}
