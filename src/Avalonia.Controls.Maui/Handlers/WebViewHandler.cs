using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="WebView"/>.</summary>
[Avalonia.Controls.Maui.Platform.NotImplemented("WebView is not yet implemented for the Avalonia backend")]
public class WebViewHandler : ViewHandler<WebView, PlaceholderControl>
{
    /// <summary>Property mapper for <see cref="WebViewHandler"/>.</summary>
    public static IPropertyMapper<WebView, WebViewHandler> Mapper = new PropertyMapper<WebView, WebViewHandler>(ViewHandler.ViewMapper);

    /// <summary>Command mapper for <see cref="WebViewHandler"/>.</summary>
    public static CommandMapper<WebView, WebViewHandler> CommandMapper = new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="WebViewHandler"/>.</summary>
    public WebViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="WebViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public WebViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="WebViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public WebViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlaceholderControl CreatePlatformView()
    {
        return new PlaceholderControl("WebView is not available in this version of Avalonia.Controls.Maui");
    }
}
