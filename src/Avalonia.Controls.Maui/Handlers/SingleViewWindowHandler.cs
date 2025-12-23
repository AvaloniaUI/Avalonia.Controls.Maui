using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Window handler for single-view application lifetimes (browser, mobile, etc.)
/// This handler creates a ContentControl instead of a Window to avoid windowing platform dependencies.
/// </summary>
public partial class SingleViewWindowHandler : ElementHandler<IWindow, object>
{
    static readonly AlertManager s_alertManager = new();

    static IPropertyMapper<IWindow, IWindowHandler> mapper = new PropertyMapper<IWindow, IWindowHandler>(ElementHandler.ElementMapper)
    {
        [nameof(IWindow.Title)] = mapTitle,
        [nameof(IWindow.Content)] = mapContent,
        // X, Y, Width, Height, Min/Max dimensions are not relevant for single-view platforms
    };

    public SingleViewWindowHandler()
        : base(mapper)
    {
    }

    protected override object CreatePlatformElement()
    {
        return new MauiAvaloniaContent();
    }

    protected override void ConnectHandler(object platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is Microsoft.Maui.Controls.Window window)
        {
            s_alertManager.Subscribe(window);
            window.ModalPushed += OnModalPushed;
            window.ModalPopped += OnModalPopped;
        }
    }

    protected override void DisconnectHandler(object platformView)
    {
        if (VirtualView is Microsoft.Maui.Controls.Window window)
        {
            s_alertManager.Unsubscribe(window);
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

    static void mapTitle(IWindowHandler handler, IWindow window)
    {
        // Title mapping is not relevant for single-view platforms
        // In browser, this could potentially update the document title
    }

    static void mapContent(IWindowHandler handler, IWindow window)
    {
        var avContent = GetMauiContent(handler);
        var content = window.Content?.ToPlatform(handler.MauiContext!);
        avContent.SetMainContent(content);
    }

    static MauiAvaloniaContent GetMauiContent(IWindowHandler handler)
    {
        _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
        return (MauiAvaloniaContent)handler.PlatformView;
    }
}
