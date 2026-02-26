using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Platform;
using Avalonia.VisualTree;
using Microsoft.Maui;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Window handler for single-view application lifetimes (browser, mobile, etc.)
/// This handler creates a ContentControl instead of a Window to avoid windowing platform dependencies.
/// </summary>
public partial class SingleViewWindowHandler : ElementHandler<IWindow, Avalonia.Controls.ContentControl>
{
    static readonly AlertManager s_alertManager = new();

    /// <summary>
    /// Property mapper for <see cref="SingleViewWindowHandler"/>.
    /// </summary>
    static IPropertyMapper<IWindow, SingleViewWindowHandler> mapper = new PropertyMapper<IWindow, SingleViewWindowHandler>(ElementHandler.ElementMapper)
    {
        [nameof(IWindow.Title)] = mapTitle,
        [nameof(IWindow.Content)] = mapContent,
        // X, Y, Width, Height, Min/Max dimensions are not relevant for single-view platforms
    };

    /// <summary>
    /// Command mapper for <see cref="SingleViewWindowHandler"/>.
    /// </summary>
    static CommandMapper<IWindow, SingleViewWindowHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(IWindow.RequestDisplayDensity)] = MapRequestDisplayDensity,
    };

    /// <summary>
    /// Maps the <see cref="IWindow.RequestDisplayDensity"/> command to return the current rendering scale.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="window">The associated <see cref="IWindow"/> instance.</param>
    /// <param name="arg3">The associated command arguments.</param>
    private static void MapRequestDisplayDensity(SingleViewWindowHandler handler, IWindow window, object? arg3)
    {
        if (arg3 is DisplayDensityRequest request)
        {
            var renderingScale = handler.PlatformView.Presenter?.GetPresentationSource()?.RenderScaling;
            request.SetResult((float)(renderingScale ?? 1.0));
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SingleViewWindowHandler"/>.
    /// </summary>
    public SingleViewWindowHandler()
        : base(mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform view for this handler.
    /// </summary>
    /// <returns>A new <see cref="MauiAvaloniaContent"/> instance.</returns>
    protected override Avalonia.Controls.ContentControl CreatePlatformElement()
    {
        return new MauiAvaloniaContent();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(Avalonia.Controls.ContentControl platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is Microsoft.Maui.Controls.Window window)
        {
            window.AlertManager.Subscribe();
            window.ModalPushed += OnModalPushed;
            window.ModalPopped += OnModalPopped;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(Avalonia.Controls.ContentControl platformView)
    {
        if (VirtualView is Microsoft.Maui.Controls.Window window)
        {
            window.AlertManager.Unsubscribe();
            window.ModalPushed -= OnModalPushed;
            window.ModalPopped -= OnModalPopped;
        }

        base.DisconnectHandler(platformView);
    }

    private void OnModalPushed(object? sender, Microsoft.Maui.Controls.ModalPushedEventArgs e)
    {
        // Modal support would need to be implemented differently for single-view platforms
    }

    private void OnModalPopped(object? sender, Microsoft.Maui.Controls.ModalPoppedEventArgs e)
    {
        // Modal support would need to be implemented differently for single-view platforms
    }

    /// <summary>
    /// Maps the Title property. Not relevant for single-view platforms.
    /// </summary>
    static void mapTitle(SingleViewWindowHandler handler, IWindow window)
    {
        // Title mapping is not relevant for single-view platforms
        // In browser, this could potentially update the document title
    }

    /// <summary>
    /// Maps the <see cref="IWindow.Content"/> property to the platform view.
    /// </summary>
    static void mapContent(SingleViewWindowHandler handler, IWindow window)
    {
        var avContent = GetMauiContent(handler);
        var content = window.Content?.ToPlatform(handler.MauiContext!);
        avContent.SetMainContent(content);
    }

    /// <summary>
    /// Gets the <see cref="MauiAvaloniaContent"/> from the handler, ensuring <see cref="IMauiContext"/> is set.
    /// </summary>
    static MauiAvaloniaContent GetMauiContent(SingleViewWindowHandler handler)
    {
        _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
        return (MauiAvaloniaContent)handler.PlatformView;
    }
}
