using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;
using System;
using System.Runtime.CompilerServices;
using Avalonia.VisualTree;
using PlatformView = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Generic base Avalonia handler for <see cref="IView"/>. Maps a MAUI virtual view of type
/// <typeparamref name="TVirtualView"/> to an Avalonia control of type <typeparamref name="TPlatformView"/>.
/// </summary>
/// <typeparam name="TVirtualView">The MAUI virtual view interface type.</typeparam>
/// <typeparam name="TPlatformView">The Avalonia platform view type.</typeparam>
public abstract partial class ViewHandler<TVirtualView, TPlatformView> : ViewHandler, IViewHandler
        where TVirtualView : class, IView
        where TPlatformView : PlatformView
{
    private Avalonia.Controls.Maui.Platform.GestureManager? _gestureManager;
    private bool _isLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewHandler{TVirtualView, TPlatformView}"/> class.
    /// </summary>
    /// <param name="mapper">The property mapper to use for this handler.</param>
    /// <param name="commandMapper">The command mapper to use for this handler.</param>
    protected ViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper)
    {
    }

    /// <summary>
    /// Gets the strongly-typed Avalonia platform view associated with this handler.
    /// </summary>
    public new TPlatformView PlatformView
    {
        get => (TPlatformView?)base.PlatformView ?? throw new InvalidOperationException($"PlatformView cannot be null here");
        private protected set => base.PlatformView = value;
    }

    /// <summary>
    /// Gets the strongly-typed MAUI virtual view associated with this handler.
    /// </summary>
    public new TVirtualView VirtualView
    {
        get => (TVirtualView?)base.VirtualView ?? throw new InvalidOperationException($"VirtualView cannot be null here");
        private protected set => base.VirtualView = value;
    }

    /// <inheritdoc/>
    IView? IViewHandler.VirtualView => base.VirtualView;

    /// <inheritdoc/>
    IElement? IElementHandler.VirtualView => base.VirtualView;

    /// <inheritdoc/>
    object? IElementHandler.PlatformView => base.PlatformView;

    /// <summary>
    /// Sets the MAUI virtual view for this handler.
    /// </summary>
    /// <param name="view">The <see cref="IView"/> to associate with this handler.</param>
    public virtual void SetVirtualView(IView view) =>
        base.SetVirtualView(view);

    /// <inheritdoc/>
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

        AttachPlatformViewEvents(platformView);
    }

    /// <summary>
    /// Disconnects handler from platform view.
    /// </summary>
    /// <param name="platformView">The platform view to disconnect from.</param>
    /// <remarks>This method is called when handler is disconnected from its platform view.
    /// It disposes the gesture manager and can be overridden to perform additional cleanup logic.</remarks>
    protected virtual void DisconnectHandler(TPlatformView platformView)
    {
        // Fire Unloaded before detaching events so MAUI's IsLoaded becomes false.
        // Without this, Window.OnPageChanged sees IsLoaded==true and calls OnUnloaded()
        // which is not implemented on non-platform targets, preventing DisconnectHandlers
        // from ever running and leaving the old page tree permanently rooted.
        if (_isLoaded)
        {
            _isLoaded = false;
            TrySendUnloaded();
        }

        DetachPlatformViewEvents(platformView);
        Extensions.ViewExtensions.DisposeClipSubscription(platformView);

        // Clear any TransformGroup created by UpdateTransformation() to release
        // ScaleTransform, RotateTransform, and TranslateTransform objects.
        platformView.RenderTransform = null;

        _gestureManager?.Dispose();
        _gestureManager = null;
    }

    /// <inheritdoc/>
    private protected override PlatformView OnCreatePlatformView()
    {
        return PlatformViewFactory?.Invoke(this) ?? CreatePlatformView();
    }

    /// <inheritdoc/>
    public override void OnConnectHandler(object platformView) =>
        ConnectHandler((TPlatformView)platformView);

    /// <inheritdoc/>
    public override void OnDisconnectHandler(object platformView) =>
        DisconnectHandler((TPlatformView)platformView);

    /// <inheritdoc/>
    protected override void SetupContainer()
    {
        if (PlatformView == null)
            return;

        // Remember PlatformView's current parent and position so we can swap in the container
        var parentPanel = PlatformView.Parent as Avalonia.Controls.Panel;
        int index = parentPanel?.Children.IndexOf(PlatformView) ?? -1;

        if (parentPanel != null && index >= 0)
            parentPanel.Children.RemoveAt(index);

        var containerView = new Avalonia.Controls.Maui.Platform.ContentView();
        containerView.Children.Add(PlatformView);
        ContainerView = containerView;

        if (parentPanel != null && index >= 0)
            parentPanel.Children.Insert(Math.Min(index, parentPanel.Children.Count), containerView);
    }

    /// <inheritdoc/>
    protected override void RemoveContainer()
    {
        if (ContainerView is Avalonia.Controls.Maui.Platform.ContentView container && PlatformView != null)
        {
            // Remember the container's parent and position so we can swap PlatformView back in
            var parentPanel = container.Parent as Avalonia.Controls.Panel;
            int index = parentPanel?.Children.IndexOf(container) ?? -1;

            if (parentPanel != null && index >= 0)
                parentPanel.Children.RemoveAt(index);

            container.Children.Remove(PlatformView);

            if (parentPanel != null && index >= 0)
                parentPanel.Children.Insert(Math.Min(index, parentPanel.Children.Count), PlatformView);
        }

        ContainerView = null;
    }

    /// <summary>
    /// Attaches event handlers to the platform view for lifecycle, focus, and bounds tracking.
    /// </summary>
    private void AttachPlatformViewEvents(TPlatformView platformView)
    {
        platformView.AttachedToVisualTree += OnPlatformViewAttachedToVisualTree;
        platformView.DetachedFromVisualTree += OnPlatformViewDetachedFromVisualTree;
        platformView.GotFocus += OnPlatformViewGotFocus;
        platformView.LostFocus += OnPlatformViewLostFocus;
        platformView.PropertyChanged += OnPlatformViewPropertyChanged;

        if (platformView.Parent != null)
        {
            _isLoaded = true;
            TrySendLoaded();
        }

        if (platformView.IsFocused)
        {
            SetFocused(true);
        }
    }

    /// <summary>
    /// Detaches event handlers from the platform view.
    /// </summary>
    private void DetachPlatformViewEvents(TPlatformView platformView)
    {
        platformView.AttachedToVisualTree -= OnPlatformViewAttachedToVisualTree;
        platformView.DetachedFromVisualTree -= OnPlatformViewDetachedFromVisualTree;
        platformView.GotFocus -= OnPlatformViewGotFocus;
        platformView.LostFocus -= OnPlatformViewLostFocus;
        platformView.PropertyChanged -= OnPlatformViewPropertyChanged;
    }

    private void OnPlatformViewAttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_isLoaded)
            return;

        _isLoaded = true;
        TrySendLoaded();
    }

    private void OnPlatformViewDetachedFromVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (!_isLoaded)
            return;

        _isLoaded = false;
        TrySendUnloaded();
        SetFocused(false);
    }

    private void OnPlatformViewGotFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetFocused(true);
    }

    private void OnPlatformViewLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetFocused(false);
    }

    private void OnPlatformViewPropertyChanged(object? sender, global::Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Avalonia.Visual.BoundsProperty && e.Property.Name != "Bounds")
            return;

        if (e.NewValue is not Avalonia.Rect newBounds)
            return;

        var oldBounds = e.OldValue is Avalonia.Rect oldRect ? oldRect : new Avalonia.Rect();
        if (newBounds.Width.Equals(oldBounds.Width) && newBounds.Height.Equals(oldBounds.Height))
            return;

        if (VirtualView is IVisualElementController controller)
        {
            controller.PlatformSizeChanged();
        }
    }

    private void SetFocused(bool isFocused)
    {
        if (VirtualView is null)
            return;

        if (VirtualView.IsFocused == isFocused)
            return;

        VirtualView.IsFocused = isFocused;
    }

    private void TrySendLoaded()
    {
        if (VirtualView is VisualElement visualElement)
        {
            VisualElementLifecycle.TrySendLoaded(visualElement);
        }
    }

    private void TrySendUnloaded()
    {
        if (VirtualView is VisualElement visualElement)
        {
            VisualElementLifecycle.TrySendUnloaded(visualElement);
        }
    }

    private static class VisualElementLifecycle
    {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SendLoaded")]
        private static extern void SendLoaded(VisualElement element);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SendUnloaded")]
        private static extern void SendUnloaded(VisualElement element);

        public static void TrySendLoaded(VisualElement element) =>
            SendLoaded(element);

        public static void TrySendUnloaded(VisualElement element) =>
            SendUnloaded(element);
    }
}
