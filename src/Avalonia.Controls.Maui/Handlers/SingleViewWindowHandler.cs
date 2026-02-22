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

    static IPropertyMapper<IWindow, SingleViewWindowHandler> mapper = new PropertyMapper<IWindow, SingleViewWindowHandler>(ElementHandler.ElementMapper)
    {
        [nameof(IWindow.Title)] = mapTitle,
        [nameof(IWindow.Content)] = mapContent,
        // X, Y, Width, Height, Min/Max dimensions are not relevant for single-view platforms
    };

    static CommandMapper<IWindow, SingleViewWindowHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(IWindow.RequestDisplayDensity)] = MapRequestDisplayDensity,
    };

    private static void MapRequestDisplayDensity(SingleViewWindowHandler handler, IWindow window, object? arg3)
    {
        if (arg3 is DisplayDensityRequest request)
        {
            var renderingScale = handler.PlatformView.Presenter?.GetPresentationSource()?.RenderScaling;
            request.SetResult((float)(renderingScale ?? 1.0));
        }
    }

    public SingleViewWindowHandler()
        : base(mapper, CommandMapper)
    {
    }

    protected override Avalonia.Controls.ContentControl CreatePlatformElement()
    {
        return new MauiAvaloniaContent();
    }

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

    static void mapTitle(SingleViewWindowHandler handler, IWindow window)
    {
        // Title mapping is not relevant for single-view platforms
        // In browser, this could potentially update the document title
    }

    static void mapContent(SingleViewWindowHandler handler, IWindow window)
    {
        var avContent = GetMauiContent(handler);
        var content = window.Content?.ToPlatform(handler.MauiContext!);
        avContent.SetMainContent(content);
    }

    static MauiAvaloniaContent GetMauiContent(SingleViewWindowHandler handler)
    {
        _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
        return (MauiAvaloniaContent)handler.PlatformView;
    }
}
