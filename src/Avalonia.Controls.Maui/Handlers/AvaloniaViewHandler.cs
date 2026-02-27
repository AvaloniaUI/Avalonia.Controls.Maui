using AvaloniaControl = Avalonia.Controls.Control;
using Avalonia.Controls.Maui.Controls;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// A handler that allows wrapping existing Avalonia content in an AvaloniaView.
/// </summary>
public class AvaloniaViewHandler : ViewHandler<AvaloniaView, MauiAvaloniaView>
{
    /// <summary>
    /// Defines the property mapper that maps properties from the AvaloniaView.
    /// </summary>
    public static IPropertyMapper<AvaloniaView, AvaloniaViewHandler> PropertyMapper = new PropertyMapper<AvaloniaView, AvaloniaViewHandler>(Microsoft.Maui.Handlers.ViewHandler.ViewMapper)
    {
        [nameof(AvaloniaView.Content)] = MapContent
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaViewHandler"/> class.
    /// </summary>
    public AvaloniaViewHandler() : base(PropertyMapper)
    {
    }

    /// <summary>
    /// Creates the <see cref="MauiAvaloniaView"/>.
    /// </summary>
    /// <returns>The view.</returns>
    protected override MauiAvaloniaView CreatePlatformView()
    {
        return new MauiAvaloniaView(VirtualView);
    }

    /// <summary>
    /// Connects the handler to <see cref="MauiAvaloniaView"/>.
    /// </summary>
    /// <param name="platformView">The view.</param>
    protected override void ConnectHandler(MauiAvaloniaView platformView)
    {
        base.ConnectHandler(platformView);

        platformView.Content = VirtualView.Content as AvaloniaControl;
    }

    /// <summary>
    /// Disconnects the handler from <see cref="MauiAvaloniaView"/>.
    /// </summary>
    /// <param name="handler">The handler that manages the connection between the AvaloniaView and the MauiAvaloniaView.</param>
    /// <param name="view">The AvaloniaView instance whose content is being mapped.</param>
    public static void MapContent(AvaloniaViewHandler handler, AvaloniaView view)
    {
        handler.PlatformView?.UpdateContent();
    }
}