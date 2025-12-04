using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Maui.Handlers;

public partial class WindowHandler : Microsoft.Maui.Handlers.WindowHandler
{
    static readonly AlertManager s_alertManager = new();

    static IPropertyMapper<IWindow, IWindowHandler> mapper = new PropertyMapper<IWindow, IWindowHandler>(ElementHandler.ElementMapper)
    {
        [nameof(IWindow.Title)] = mapTitle,
        [nameof(IWindow.Content)] = mapContent,
        [nameof(IWindow.X)] = mapX,
        [nameof(IWindow.Y)] = mapY,
        [nameof(IWindow.Width)] = mapWidth,
        [nameof(IWindow.Height)] = mapHeight,
        [nameof(IWindow.MaximumWidth)] = mapMaximumWidth,
        [nameof(IWindow.MaximumHeight)] = mapMaximumHeight,
        [nameof(IWindow.MinimumWidth)] = mapMinimumWidth,
        [nameof(IWindow.MinimumHeight)] = mapMinimumHeight,
    };

    public WindowHandler()
        : base(mapper)
    {
    }

    protected override object CreatePlatformElement()
    {
        return new MauiAvaloniaWindow();
    }

    protected override void ConnectHandler(object platformView)
    {
        base.ConnectHandler(platformView);

        var avWindow = (Window)platformView;

        avWindow.Activated += OnWindowActivatedForBounds;

        if (VirtualView is Microsoft.Maui.Controls.Window window)
        {
            s_alertManager.Subscribe(window);
            window.ModalPushed += OnModalPushed;
            window.ModalPopped += OnModalPopped;
        }
    }

    protected override void DisconnectHandler(object platformView)
    {
        var avWindow = (Window)platformView;
        avWindow.Activated -= OnWindowActivatedForBounds;

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
        PresentModalPage(e.Modal);
    }

    private void OnModalPopped(object? sender, Microsoft.Maui.Controls.ModalPoppedEventArgs e)
    {
        DismissModalPage();
    }


    /// <summary>
    /// Sets explicit bounds on first window activation to prevent layout cycles.
    /// Without explicit Width/Height, the layout system can enter a feedback loop
    /// where content size depends on constraints and constraints depend on content size.
    /// Setting explicit bounds from ClientSize gives the windowing system's stable bounds.
    /// </summary>
    private void OnWindowActivatedForBounds(object? sender, EventArgs e)
    {
        if (sender is Window avWindow)
        {
            // Only set bounds if they haven't been explicitly set yet
            if (double.IsNaN(avWindow.Width) || double.IsNaN(avWindow.Height))
            {
                var clientSize = avWindow.ClientSize;
                if (clientSize.Width > 0 && clientSize.Height > 0)
                {
                    avWindow.Width = clientSize.Width;
                    avWindow.Height = clientSize.Height;
                }
            }

            // Unsubscribe after first activation since we only need to set bounds once
            avWindow.Activated -= OnWindowActivatedForBounds;
        }
    }

    static void mapTitle(IWindowHandler handler, IWindow window) =>
        ((Window)handler.PlatformView).UpdateTitle(window);

    static void mapContent(IWindowHandler handler, IWindow window)
    {
        var avWindow = GetMauiWindow(handler);
        var content = window.Content?.ToPlatform(handler.MauiContext!);
        avWindow.SetMainContent(content);
    }

    static void mapX(IWindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        avWindow.Position = new PixelPoint((int)window.X, avWindow.Position.Y);
    }

    static void mapY(IWindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        avWindow.Position = new PixelPoint(avWindow.Position.X, (int)window.Y);
    }

    static void mapWidth(IWindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        avWindow.Width = window.Width;
    }

    static void mapHeight(IWindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        avWindow.Height = window.Height;
    }

    static Window GetWindow(IWindowHandler handler, IWindow window)
    {
        _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
        var content = window.Content?.ToPlatform(handler.MauiContext);
        return (Window)handler.PlatformView;
    }

    static MauiAvaloniaWindow GetMauiWindow(IWindowHandler handler)
    {
        _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
        return (MauiAvaloniaWindow)handler.PlatformView;
    }

    /// <summary>
    /// Presents a modal page on top of the current content.
    /// </summary>
    public void PresentModalPage(Microsoft.Maui.IView modalPage)
    {
        var mauiWindow = GetMauiWindow(this);
        var modalControl = modalPage.ToPlatform(MauiContext!);

        if (modalControl is Control control)
        {
            mauiWindow.PresentModal(control);
        }
    }

    /// <summary>
    /// Dismisses the top-most modal page.
    /// </summary>
    public void DismissModalPage()
    {
        var mauiWindow = GetMauiWindow(this);
        mauiWindow.DismissModal();
    }

    static void mapMaximumWidth(IWindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MinimumHeight))
            avWindow.MaxWidth = double.MaxValue;
        else
            avWindow.MaxWidth = window.MaximumWidth;
    }

    static void mapMaximumHeight(IWindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MaximumHeight))
            avWindow.MaxHeight = double.MaxValue;
        else
            avWindow.MaxHeight = window.MaximumHeight;
    }

    static void mapMinimumWidth(IWindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MinimumWidth))
            avWindow.MinWidth = 0;
        else
            avWindow.MinWidth = window.MinimumWidth;
    }

    static void mapMinimumHeight(IWindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MinimumHeight))
            avWindow.MinHeight = 0;
        else
            avWindow.MinHeight = window.MinimumHeight;
    }
}