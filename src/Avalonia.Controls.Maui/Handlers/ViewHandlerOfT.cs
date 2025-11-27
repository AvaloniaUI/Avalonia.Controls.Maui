using Microsoft.Maui;
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

    public static Func<ViewHandler<TVirtualView, TPlatformView>, TPlatformView>? PlatformViewFactory { get; set; }

    protected abstract TPlatformView CreatePlatformView();

    protected virtual void ConnectHandler(TPlatformView platformView)
    {
        platformView.LayoutUpdated += OnPlatformLayoutUpdated;

        // Initialize gesture manager for IControlsView
        if (VirtualView is Microsoft.Maui.Controls.IControlsView controlsView && _gestureManager == null)
        {
            _gestureManager = new Avalonia.Controls.Maui.Platform.GestureManager(controlsView);
        }
    }

    protected virtual void DisconnectHandler(TPlatformView platformView)
    {
        platformView.LayoutUpdated -= OnPlatformLayoutUpdated;

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

    void OnPlatformLayoutUpdated(object? sender, EventArgs e)
    {
        if (VirtualView == null)
            return;

        VirtualView.InvalidateMeasure();
    }
}