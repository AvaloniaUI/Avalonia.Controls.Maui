using Microsoft.Maui.Handlers;
using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.VisualTree;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using MauiPage = Microsoft.Maui.Controls.Page;
using System.ComponentModel;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Window handler for single-view application lifetimes (browser, mobile, etc.)
/// This handler creates a ContentControl instead of a Window to avoid windowing platform dependencies.
/// </summary>
public partial class SingleViewWindowHandler : ElementHandler<IWindow, Avalonia.Controls.ContentControl>
{
    static readonly AlertManager s_alertManager = new();
    ModalAnimationTrackingNavigation? _modalTracker;

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

            // Install modal animation tracker to capture the animated flag
            // that MAUI's ModalPushed/ModalPopped events don't carry.
            _modalTracker = new ModalAnimationTrackingNavigation(window.Navigation);
            InstallModalTracker(window.Page);
            window.PropertyChanged += OnWindowPropertyChanged;
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
            window.PropertyChanged -= OnWindowPropertyChanged;
        }

        _modalTracker = null;

        base.DisconnectHandler(platformView);
    }

    private void OnModalPushed(object? sender, Microsoft.Maui.Controls.ModalPushedEventArgs e)
    {
        bool animated = ModalAnimationTrackingNavigation.GetAnimated(e.Modal);
        var mauiContent = GetMauiContent(this);
        var modalControl = e.Modal.ToPlatform(MauiContext!);

        if (modalControl is Control control)
        {
            mauiContent.PresentModal(control, animated);
        }

        // ModalNavigationManager.PushModalAsync overwrites modal.NavigationProxy.Inner
        // with window.Navigation, bypassing our tracker. Re-install it so that
        // PopModalAsync called from within the modal page is also tracked.
        InstallModalTracker(e.Modal);
    }

    private void OnModalPopped(object? sender, Microsoft.Maui.Controls.ModalPoppedEventArgs e)
    {
        bool animated = ModalAnimationTrackingNavigation.GetAnimated(e.Modal);
        var mauiContent = GetMauiContent(this);
        mauiContent.DismissModal(animated);
    }

    private void OnWindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Microsoft.Maui.Controls.Window.Page) &&
            sender is Microsoft.Maui.Controls.Window window &&
            window.Page is MauiPage page)
        {
            // Clear any stale modal overlays from the previous page.
            var mauiContent = GetMauiContent(this);
            mauiContent.ClearAllModals();

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
