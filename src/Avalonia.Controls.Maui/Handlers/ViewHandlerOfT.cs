using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using System;
using PlatformView = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers;

public abstract partial class ViewHandler<TVirtualView, TPlatformView> : ViewHandler, IViewHandler
        where TVirtualView : class, IView
        where TPlatformView : PlatformView
{
    private Avalonia.Controls.Maui.Platform.GestureManager? _gestureManager;

    protected ViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper)
    {
    }

    public new TPlatformView PlatformView
    {
        get => (TPlatformView?)base.PlatformView ?? throw new InvalidOperationException($"PlatformView cannot be null here");
        private protected set => base.PlatformView = value;
    }

    public new TVirtualView VirtualView
    {
        get => (TVirtualView?)base.VirtualView ?? throw new InvalidOperationException($"VirtualView cannot be null here");
        private protected set => base.VirtualView = value;
    }

    IView? IViewHandler.VirtualView => base.VirtualView;

    IElement? IElementHandler.VirtualView => base.VirtualView;

    object? IElementHandler.PlatformView => base.PlatformView;

    public virtual void SetVirtualView(IView view) =>
        base.SetVirtualView(view);

    public sealed override void SetVirtualView(IElement view) =>
        SetVirtualView((IView)view);

    /// <summary>
    /// Gets or sets a factory function for creating platform views.
    /// </summary>
    /// <remarks>This property allows customization of platform view creation by providing a factory function
    /// that takes a handler instance and returns a platform view. If not set, the default <see cref="CreatePlatformView"/> method is used.</remarks>
    public static Func<ViewHandler<TVirtualView, TPlatformView>, TPlatformView>? PlatformViewFactory { get; set; }

    /// <summary>
    /// Creates a new platform view instance.
    /// </summary>
    /// <returns>A new instance of the platform-specific view.</returns>
    /// <remarks>This method is called when the handler needs to create a platform view for the virtual view.
    /// Derived classes must implement this method to provide the appropriate platform view creation logic.</remarks>
    protected abstract TPlatformView CreatePlatformView();

    /// <summary>
    /// Connects the handler to the platform view.
    /// </summary>
    /// <param name="platformView">The platform view to connect to.</param>
    /// <remarks>This method is called when the handler is connected to its platform view.
    /// It initializes gesture manager for controls views and can be overridden to perform additional connection logic.</remarks>
    protected virtual void ConnectHandler(TPlatformView platformView)
    {
        // Initialize gesture manager for IControlsView
        if (VirtualView is Microsoft.Maui.Controls.IControlsView controlsView && _gestureManager == null)
        {
            _gestureManager = new Avalonia.Controls.Maui.Platform.GestureManager(controlsView);
        }
    }

    /// <summary>
    /// Disconnects handler from platform view.
    /// </summary>
    /// <param name="platformView">The platform view to disconnect from.</param>
    /// <remarks>This method is called when handler is disconnected from its platform view.
    /// It disposes the gesture manager and can be overridden to perform additional cleanup logic.</remarks>
    protected virtual void DisconnectHandler(TPlatformView platformView)
    {
        _gestureManager?.Dispose();
        _gestureManager = null;
    }

    private protected override PlatformView OnCreatePlatformView()
    {
        return PlatformViewFactory?.Invoke(this) ?? CreatePlatformView();
    }

    public override void OnConnectHandler(object platformView) =>
        ConnectHandler((TPlatformView)platformView);

    public override void OnDisconnectHandler(object platformView) =>
        DisconnectHandler((TPlatformView)platformView);

    protected override void SetupContainer()
    {
        if (PlatformView == null)
            return;

        var containerView = new Avalonia.Controls.Maui.Platform.ContentView();
        containerView.Children.Add(PlatformView);
        ContainerView = containerView;
    }

    protected override void RemoveContainer()
    {
        if (ContainerView is Avalonia.Controls.Maui.Platform.ContentView container && PlatformView != null)
        {
            container.Children.Remove(PlatformView);
        }

        ContainerView = null;
    }
}