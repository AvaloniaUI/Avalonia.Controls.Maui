using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.VisualTree;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Maui.Handlers;

public partial class WindowHandler : ElementHandler<IWindow, Avalonia.Controls.Window>
{
    static readonly AlertManager s_alertManager = new();

    static IPropertyMapper<IWindow, WindowHandler> mapper = new PropertyMapper<IWindow, WindowHandler>(ElementHandler.ElementMapper)
    {
        [nameof(IWindow.Title)] = mapTitle,
        [nameof(IWindow.Content)] = mapContent,
        [nameof(Microsoft.Maui.Controls.Window.TitleBar)] = mapTitleBar,
        [nameof(IWindow.X)] = mapX,
        [nameof(IWindow.Y)] = mapY,
        [nameof(IWindow.Width)] = mapWidth,
        [nameof(IWindow.Height)] = mapHeight,
        [nameof(IWindow.MaximumWidth)] = mapMaximumWidth,
        [nameof(IWindow.MaximumHeight)] = mapMaximumHeight,
        [nameof(IWindow.MinimumWidth)] = mapMinimumWidth,
        [nameof(IWindow.MinimumHeight)] = mapMinimumHeight,
    };

    static CommandMapper<IWindow, WindowHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(IWindow.RequestDisplayDensity)] = MapRequestDisplayDensity,
    };

    private static void MapRequestDisplayDensity(WindowHandler handler, IWindow window, object? arg3)
    {
        if (arg3 is DisplayDensityRequest request)
        {
            var toplevel = handler.PlatformView.GetVisualRoot() as Avalonia.Controls.TopLevel;
            request.SetResult((float)(toplevel?.RenderScaling ?? 1.0));
        }
    }

    public WindowHandler()
        : base(mapper, CommandMapper)
    {
    }

    protected override Avalonia.Controls.Window CreatePlatformElement()
    {
        return new MauiAvaloniaWindow();
    }

    protected override void ConnectHandler(Avalonia.Controls.Window platformView)
    {
        base.ConnectHandler(platformView);

        var avWindow = (Window)platformView;

        // Set MauiContext on MauiAvaloniaWindow for TitleBar support
        if (avWindow is MauiAvaloniaWindow mauiWindow)
        {
            mauiWindow.SetMauiContext(MauiContext);
        }

        if (VirtualView is Microsoft.Maui.Controls.Window window)
        {
            s_alertManager.Subscribe(window);
            window.ModalPushed += OnModalPushed;
            window.ModalPopped += OnModalPopped;
        }
    }

    protected override void DisconnectHandler(Avalonia.Controls.Window platformView)
    {
        var avWindow = (Window)platformView;

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

    static void mapTitle(WindowHandler handler, IWindow window) =>
        ((Window)handler.PlatformView).UpdateTitle(window);

    static void mapContent(WindowHandler handler, IWindow window)
    {
        var avWindow = GetMauiWindow(handler);
        var content = window.Content?.ToPlatform(handler.MauiContext!);
        avWindow.SetMainContent(content);
    }

    static void mapTitleBar(WindowHandler handler, IWindow window)
    {
        var avWindow = GetMauiWindow(handler);
        // TitleBar is defined on Microsoft.Maui.Controls.Window, not IWindow
        var controlsWindow = window as Microsoft.Maui.Controls.Window;
        avWindow.SetTitleBar(controlsWindow?.TitleBar, handler.MauiContext);
    }

    static void mapX(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        avWindow.Position = new PixelPoint((int)window.X, avWindow.Position.Y);
    }

    static void mapY(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        avWindow.Position = new PixelPoint(avWindow.Position.X, (int)window.Y);
    }

    static void mapWidth(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (!double.IsNaN(window.Width))
            avWindow.Width = window.Width;
        else
            avWindow.Width = avWindow.ClientSize.Width;
    }

    static void mapHeight(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (!double.IsNaN(window.Height))
            avWindow.Height = window.Height;
        else
            avWindow.Height = avWindow.ClientSize.Height;
    }

    static Window GetWindow(WindowHandler handler, IWindow window)
    {
        _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
        var content = window.Content?.ToPlatform(handler.MauiContext);
        return (Window)handler.PlatformView;
    }

    static MauiAvaloniaWindow GetMauiWindow(WindowHandler handler)
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

    static void mapMaximumWidth(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MinimumHeight))
            avWindow.MaxWidth = double.MaxValue;
        else
            avWindow.MaxWidth = window.MaximumWidth;
    }

    static void mapMaximumHeight(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MaximumHeight))
            avWindow.MaxHeight = double.MaxValue;
        else
            avWindow.MaxHeight = window.MaximumHeight;
    }

    static void mapMinimumWidth(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MinimumWidth))
            avWindow.MinWidth = 0;
        else
            avWindow.MinWidth = window.MinimumWidth;
    }

    static void mapMinimumHeight(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MinimumHeight))
            avWindow.MinHeight = 0;
        else
            avWindow.MinHeight = window.MinimumHeight;
    }
}