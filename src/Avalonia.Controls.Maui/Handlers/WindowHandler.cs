using Microsoft.Maui.Handlers;
using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.VisualTree;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using MauiPage = Microsoft.Maui.Controls.Page;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Avalonia handler for <see cref="IWindow"/>. Creates and manages an Avalonia <see cref="Avalonia.Controls.Window"/>
/// for desktop application lifetimes.
/// </summary>
public partial class WindowHandler : ElementHandler<IWindow, Avalonia.Controls.Window>
{
    static readonly AlertManager s_alertManager = new();
    ModalAnimationTrackingNavigation? _modalTracker;

    /// <summary>
    /// Property mapper for <see cref="WindowHandler"/>.
    /// </summary>
    public static IPropertyMapper<IWindow, WindowHandler> Mapper = new PropertyMapper<IWindow, WindowHandler>(ElementHandler.ElementMapper)
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
        [nameof(IWindow.FlowDirection)] = mapFlowDirection,
    };

    /// <summary>
    /// Command mapper for <see cref="WindowHandler"/>.
    /// </summary>
    static CommandMapper<IWindow, WindowHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(IWindow.RequestDisplayDensity)] = MapRequestDisplayDensity,
    };

    /// <summary>
    /// Maps the <see cref="IWindow.RequestDisplayDensity"/> command to return the current rendering scale.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="window">The associated <see cref="IWindow"/> instance.</param>
    /// <param name="arg3">The associated command arguments.</param>
    private static void MapRequestDisplayDensity(WindowHandler handler, IWindow window, object? arg3)
    {
        if (arg3 is DisplayDensityRequest request)
        {
            var renderingScale = handler.PlatformView.Presenter?.GetPresentationSource()?.RenderScaling;
            request.SetResult((float)(renderingScale ?? 1.0));
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="WindowHandler"/>.
    /// </summary>
    public WindowHandler()
        : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform view for this handler.
    /// </summary>
    /// <returns>A new <see cref="MauiAvaloniaWindow"/> instance.</returns>
    protected override Avalonia.Controls.Window CreatePlatformElement()
    {
        return new MauiAvaloniaWindow();
    }

    /// <inheritdoc/>
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
            window.AlertManager.Subscribe();
            window.ModalPushed += OnModalPushed;
            window.ModalPopped += OnModalPopped;

            // Install modal animation tracker to capture the animated flag
            // that MAUI's ModalPushed/ModalPopped events don't carry.
            _modalTracker = new ModalAnimationTrackingNavigation(window.Navigation);
            InstallModalTracker(window.Page);
            window.PropertyChanged += OnWindowPropertyChanged;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(Avalonia.Controls.Window platformView)
    {
        var avWindow = (Window)platformView;

        if (VirtualView is Microsoft.Maui.Controls.Window window)
        {
            window.AlertManager.Unsubscribe();
            window.ModalPushed -= OnModalPushed;
            window.ModalPopped -= OnModalPopped;
            window.PropertyChanged -= OnWindowPropertyChanged;
        }

        _modalTracker = null;

        base.DisconnectHandler(platformView);
    }

    private void OnModalPushed(object? sender, Microsoft.Maui.Controls.ModalPushedEventArgs e)
    {
        bool animated = ModalAnimationTrackingNavigation.GetAnimated(e.Modal);
        PresentModalPage(e.Modal, animated);

        // ModalNavigationManager.PushModalAsync overwrites modal.NavigationProxy.Inner
        // with window.Navigation, bypassing our tracker. Re-install it so that
        // PopModalAsync called from within the modal page is also tracked.
        InstallModalTracker(e.Modal);
    }

    private void OnModalPopped(object? sender, Microsoft.Maui.Controls.ModalPoppedEventArgs e)
    {
        bool animated = ModalAnimationTrackingNavigation.GetAnimated(e.Modal);
        DismissModalPage(animated);
    }

    private void OnWindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Microsoft.Maui.Controls.Window.Page) &&
            sender is Microsoft.Maui.Controls.Window window &&
            window.Page is MauiPage page)
        {
            // Clear any stale modal overlays from the previous page.
            // MAUI's ModalNavigationManager.ClearModalPages clears its internal
            // stack without firing ModalPopped events, so our visual overlay
            // stack must be cleaned up explicitly.
            var mauiWindow = (MauiAvaloniaWindow)PlatformView;
            mauiWindow.ClearAllModals();

            InstallModalTracker(page);
        }
    }

    /// <summary>
    /// Installs the modal animation tracker on the given page's NavigationProxy,
    /// but only when the page is using the Window's navigation directly (non-Shell).
    /// </summary>
    private void InstallModalTracker(MauiPage? page)
    {
        if (_modalTracker is null || page is null)
            return;

        var currentInner = page.NavigationProxy.Inner;

        // Already installed
        if (currentInner is ModalAnimationTrackingNavigation)
            return;

        // Only install if the current Inner is the Window's own navigation.
        // Shell sets a different NavigationImplWrapper that we must not overwrite.
        if (VirtualView is Microsoft.Maui.Controls.Window window &&
            currentInner == window.Navigation)
        {
            page.NavigationProxy.Inner = _modalTracker;
        }
    }

    /// <summary>
    /// Maps the Title property to the platform view.
    /// </summary>
    static void mapTitle(WindowHandler handler, IWindow window) =>
        ((Window)handler.PlatformView).UpdateTitle(window);

    /// <summary>
    /// Maps the <see cref="IWindow.Content"/> property to the platform view.
    /// </summary>
    static void mapContent(WindowHandler handler, IWindow window)
    {
        var avWindow = GetMauiWindow(handler);
        var content = window.Content?.ToPlatform(handler.MauiContext!);
        avWindow.SetMainContent(content);
    }

    /// <summary>
    /// Maps the <see cref="Microsoft.Maui.Controls.Window.TitleBar"/> property to the platform view.
    /// </summary>
    static void mapTitleBar(WindowHandler handler, IWindow window)
    {
        var avWindow = GetMauiWindow(handler);
        // TitleBar is defined on Microsoft.Maui.Controls.Window, not IWindow
        var controlsWindow = window as Microsoft.Maui.Controls.Window;
        avWindow.SetTitleBar(controlsWindow?.TitleBar, handler.MauiContext);
    }

    /// <summary>
    /// Maps the <see cref="IWindow.X"/> property to the platform view.
    /// </summary>
    static void mapX(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        avWindow.Position = new PixelPoint((int)window.X, avWindow.Position.Y);
    }

    /// <summary>
    /// Maps the <see cref="IWindow.Y"/> property to the platform view.
    /// </summary>
    static void mapY(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        avWindow.Position = new PixelPoint(avWindow.Position.X, (int)window.Y);
    }

    /// <summary>
    /// Maps the <see cref="IWindow.Width"/> property to the platform view.
    /// </summary>
    static void mapWidth(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (!double.IsNaN(window.Width))
            avWindow.Width = window.Width;
        else
            avWindow.Width = avWindow.ClientSize.Width;
    }

    /// <summary>
    /// Maps the <see cref="IWindow.Height"/> property to the platform view.
    /// </summary>
    static void mapHeight(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (!double.IsNaN(window.Height))
            avWindow.Height = window.Height;
        else
            avWindow.Height = avWindow.ClientSize.Height;
    }

    /// <summary>
    /// Gets the Avalonia <see cref="Window"/> from the handler, ensuring <see cref="IMauiContext"/> is set.
    /// </summary>
    static Window GetWindow(WindowHandler handler, IWindow window)
    {
        _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
        var content = window.Content?.ToPlatform(handler.MauiContext);
        return (Window)handler.PlatformView;
    }

    /// <summary>
    /// Gets the <see cref="MauiAvaloniaWindow"/> from the handler, ensuring <see cref="IMauiContext"/> is set.
    /// </summary>
    static MauiAvaloniaWindow GetMauiWindow(WindowHandler handler)
    {
        _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
        return (MauiAvaloniaWindow)handler.PlatformView;
    }

    /// <summary>
    /// Presents a modal page as a fullscreen overlay on top of the current content.
    /// </summary>
    public void PresentModalPage(Microsoft.Maui.IView modalPage, bool animated = true)
    {
        var mauiWindow = GetMauiWindow(this);
        var modalControl = modalPage.ToPlatform(MauiContext!);

        if (modalControl is Control control)
        {
            mauiWindow.PresentModal(control, animated);
        }
    }

    /// <summary>
    /// Dismisses the top-most modal page.
    /// </summary>
    public void DismissModalPage(bool animated = true)
    {
        var mauiWindow = GetMauiWindow(this);
        mauiWindow.DismissModal(animated);
    }

    /// <summary>
    /// Maps the <see cref="IWindow.MaximumWidth"/> property to the platform view.
    /// </summary>
    static void mapMaximumWidth(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MinimumHeight))
            avWindow.MaxWidth = double.MaxValue;
        else
            avWindow.MaxWidth = window.MaximumWidth;
    }

    /// <summary>
    /// Maps the <see cref="IWindow.MaximumHeight"/> property to the platform view.
    /// </summary>
    static void mapMaximumHeight(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MaximumHeight))
            avWindow.MaxHeight = double.MaxValue;
        else
            avWindow.MaxHeight = window.MaximumHeight;
    }

    /// <summary>
    /// Maps the <see cref="IWindow.MinimumWidth"/> property to the platform view.
    /// </summary>
    static void mapMinimumWidth(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MinimumWidth))
            avWindow.MinWidth = 0;
        else
            avWindow.MinWidth = window.MinimumWidth;
    }

    /// <summary>
    /// Maps the <see cref="IWindow.MinimumHeight"/> property to the platform view.
    /// </summary>
    static void mapMinimumHeight(WindowHandler handler, IWindow window)
    {
        var avWindow = GetWindow(handler, window);
        if (double.IsNaN(window.MinimumHeight))
            avWindow.MinHeight = 0;
        else
            avWindow.MinHeight = window.MinimumHeight;
    }

    /// <summary>
    /// Maps the <see cref="IWindow.FlowDirection"/> property to the platform view.
    /// </summary>
    static void mapFlowDirection(WindowHandler handler, IWindow window)
    {
        var avWindow = (Window)handler.PlatformView;
        switch (window.FlowDirection)
        {
            case FlowDirection.MatchParent:
            case FlowDirection.LeftToRight:
                avWindow.FlowDirection = Avalonia.Media.FlowDirection.LeftToRight;
                break;
            case FlowDirection.RightToLeft:
                avWindow.FlowDirection = Avalonia.Media.FlowDirection.RightToLeft;
                break;
        }
    }
}
