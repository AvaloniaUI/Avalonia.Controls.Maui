using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;
using System;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.VisualTree;
using PlatformView = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Generic base Avalonia handler for <see cref="IView"/>. Maps a MAUI virtual view of type
/// <typeparamref name="TVirtualView"/> to an Avalonia control of type <typeparamref name="TPlatformView"/>.
/// </summary>
/// <typeparam name="TVirtualView">The MAUI virtual view interface type.</typeparam>
/// <typeparam name="TPlatformView">The Avalonia platform view type.</typeparam>
public abstract partial class ViewHandler<TVirtualView, TPlatformView> : ElementHandler<TVirtualView, TPlatformView>, IViewHandler, IAvaloniaViewHandler, IHandlerStateExhibitor
        where TVirtualView : class, IView
        where TPlatformView : PlatformView
{
    bool _hasContainer;
    bool _isLoaded;
    bool _isConnectingHandler;
    bool _isMappingProperties;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewHandler{TVirtualView, TPlatformView}"/> class.
    /// </summary>
    /// <param name="mapper">The property mapper to use for this handler.</param>
    /// <param name="commandMapper">The command mapper to use for this handler.</param>
    protected ViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper ?? ViewHandler.ViewCommandMapper)
    {
    }

    private protected PlatformView? PlatformViewOrNull => (PlatformView?)((IElementHandler)this).PlatformView;

    IView? VirtualViewOrNull => (IView?)((IElementHandler)this).VirtualView;

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="ElementHandler{TVirtualView, TPlatformView}.PlatformView"/> is contained within a view.
    /// </summary>
    /// <remarks>When set to <see langword="true"/>, <see cref="SetupContainer"/> is called to setup the container view.
    /// When set to <see langword="false"/>, <see cref="RemoveContainer"/> is called to remove the current container view.</remarks>
    public bool HasContainer
    {
        get => _hasContainer;
        set
        {
            if (_hasContainer == value)
                return;

            _hasContainer = value;

            if (value)
                SetupContainer();
            else
                RemoveContainer();
        }
    }

    /// <summary>
    /// Gets a value that indicates whether or not the <see cref="ElementHandler{TVirtualView, TPlatformView}.VirtualView"/> needs a container view.
    /// </summary>
    public virtual bool NeedsContainer
    {
        get => VirtualViewOrNull?.NeedsContainer() ?? false;
    }

    /// <summary>
    /// Gets the view that acts as a container for the <see cref="ElementHandler{TVirtualView, TPlatformView}.PlatformView"/>.
    /// </summary>
    /// <remarks>Note that this can be <see langword="null"/>. Especially when <see cref="HasContainer"/> is set to <see langword="false"/> this value might not be set.</remarks>
    public PlatformView? ContainerView { get; private protected set; }

    object? IViewHandler.ContainerView => ContainerView;

    PlatformView? IAvaloniaViewHandler.ContainerView => ContainerView;

    PlatformView? IAvaloniaViewHandler.PlatformView => PlatformViewOrNull;

    IView? IViewHandler.VirtualView => VirtualViewOrNull;

    bool IHandlerStateExhibitor.IsConnectingHandler => _isConnectingHandler;

    bool IHandlerStateExhibitor.IsMappingProperties => _isMappingProperties;

    /// <summary>
    /// Sets the MAUI virtual view for this handler.
    /// </summary>
    /// <param name="view">The <see cref="IView"/> to associate with this handler.</param>
    public virtual void SetVirtualView(IView view)
    {
        // Track handler state so property mappers can skip default values during the
        // initial connection, mirroring MAUI's internal ElementHandlerState tracking.
        _isConnectingHandler = ((IElementHandler)this).PlatformView is null;
        _isMappingProperties = true;
        try
        {
            base.SetVirtualView(view);
        }
        finally
        {
            _isConnectingHandler = false;
            _isMappingProperties = false;
        }
    }

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

    /// <inheritdoc/>
    protected sealed override TPlatformView CreatePlatformElement() =>
        PlatformViewFactory?.Invoke(this) ?? CreatePlatformView();

    /// <summary>
    /// Connects the handler to the platform view.
    /// </summary>
    /// <param name="platformView">The platform view to connect to.</param>
    /// <remarks>This method is called when the handler is connected to its platform view.
    /// It can be overridden to perform additional connection logic.</remarks>
    protected override void ConnectHandler(TPlatformView platformView)
    {
        AttachPlatformViewEvents(platformView);
    }

    /// <summary>
    /// Disconnects handler from platform view.
    /// </summary>
    /// <param name="platformView">The platform view to disconnect from.</param>
    /// <remarks>This method is called when handler is disconnected from its platform view.
    /// It can be overridden to perform additional cleanup logic.</remarks>
    protected override void DisconnectHandler(TPlatformView platformView)
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
    }

    /// <inheritdoc/>
    public Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        var platformView = PlatformViewOrNull;
        if (platformView is null || VirtualViewOrNull is null)
            return Microsoft.Maui.Graphics.Size.Zero;

        // When there's a ContainerView, it's the control in the parent's visual tree
        // and carries the margin. Measure the outermost view for correct sizing.
        var viewToMeasure = ContainerView ?? platformView;

        if (viewToMeasure.Dispatcher.CheckAccess())
        {
            return MeasureCore(viewToMeasure, widthConstraint, heightConstraint);
        }
        else
        {
            return viewToMeasure.Dispatcher.InvokeAsync(() =>
            {
                return MeasureCore(viewToMeasure, widthConstraint, heightConstraint);
            }).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Performs the core measurement of the platform view against the given constraints.
    /// </summary>
    /// <param name="viewToMeasure">The Avalonia view to measure (ContainerView if present, otherwise PlatformView).</param>
    /// <param name="widthConstraint">The maximum width constraint.</param>
    /// <param name="heightConstraint">The maximum height constraint.</param>
    /// <returns>The desired size of the view.</returns>
    private static Microsoft.Maui.Graphics.Size MeasureCore(PlatformView viewToMeasure, double widthConstraint, double heightConstraint)
    {
        var avaloniaConstraint = new global::Avalonia.Size(
            double.IsNaN(widthConstraint) ? double.PositiveInfinity : widthConstraint,
            double.IsNaN(heightConstraint) ? double.PositiveInfinity : heightConstraint);

        viewToMeasure.Measure(avaloniaConstraint);

        // Avalonia's DesiredSize includes the control's Margin, but MAUI's layout system
        // adds margin separately when positioning children. Subtract it to avoid double-counting.
        var contentSize = viewToMeasure.DesiredSize.Deflate(viewToMeasure.Margin);
        return new Microsoft.Maui.Graphics.Size(contentSize.Width, contentSize.Height);
    }

    /// <inheritdoc/>
    public virtual void PlatformArrange(Microsoft.Maui.Graphics.Rect frame)
    {
        var dispatcher = PlatformViewOrNull?.Dispatcher ?? Avalonia.Threading.Dispatcher.UIThread;
        if (dispatcher.CheckAccess())
            Arrange(frame);
        else
            dispatcher.Invoke(() => Arrange(frame));
    }

    /// <summary>
    /// Arranges the platform view within the specified frame, compensating for MAUI/Avalonia margin differences.
    /// When a ContainerView exists (for Clip/Shadow), it is the control in the parent's visual tree
    /// and must be arranged instead, with the PlatformView filling the container.
    /// </summary>
    /// <param name="frame">The frame rectangle provided by the MAUI layout system.</param>
    private protected void Arrange(Microsoft.Maui.Graphics.Rect frame)
    {
        var platformView = PlatformViewOrNull;
        if (platformView is null)
            return;

        // Determine which view is in the parent's visual tree.
        var viewToArrange = ContainerView ?? platformView;

        // MAUI's frame already accounts for margin positioning. Avalonia's Arrange
        // further deflates by Margin internally, so inflate to compensate.
        var arrangeRect = new global::Avalonia.Rect(frame.X, frame.Y, frame.Width, frame.Height)
            .Inflate(viewToArrange.Margin);

        if (!viewToArrange.IsMeasureValid)
        {
            viewToArrange.Measure(arrangeRect.Size);
        }

        viewToArrange.Arrange(arrangeRect);
    }

    /// <summary>
    /// Constructs the <see cref="ContainerView"/> and adds the platform view to a container.
    /// </summary>
    /// <remarks>This method is called when <see cref="HasContainer"/> is set to <see langword="true"/>.</remarks>
    protected virtual void SetupContainer()
    {
        var platformView = PlatformViewOrNull;
        if (platformView == null)
            return;

        // Remember PlatformView's current parent and position so we can swap in the container
        var parentPanel = platformView.Parent as Avalonia.Controls.Panel;
        int index = parentPanel?.Children.IndexOf(platformView) ?? -1;

        if (parentPanel != null && index >= 0)
            parentPanel.Children.RemoveAt(index);

        var containerView = new Avalonia.Controls.Maui.Platform.ContentView();
        containerView.Children.Add(platformView);
        ContainerView = containerView;

        // Move margin from PlatformView to ContainerView. The ContainerView is the control
        // in the parent's visual tree, so it must carry the margin for correct layout.
        // Without this, the PlatformView's margin offsets it inside the ContainerView,
        // causing misalignment with Clip/Shadow applied to the ContainerView.
        if (platformView.Margin != default)
        {
            containerView.Margin = platformView.Margin;
            platformView.Margin = new Avalonia.Thickness(0);
        }

        if (parentPanel != null && index >= 0)
            parentPanel.Children.Insert(Math.Min(index, parentPanel.Children.Count), containerView);
    }

    /// <summary>
    /// Deconstructs the <see cref="ContainerView"/> and removes the platform view from its container.
    /// </summary>
    /// <remarks>This method is called when <see cref="HasContainer"/> is set to <see langword="false"/>.</remarks>
    protected virtual void RemoveContainer()
    {
        var platformView = PlatformViewOrNull;
        if (ContainerView is Avalonia.Controls.Maui.Platform.ContentView container && platformView != null)
        {
            // Move margin back from ContainerView to PlatformView
            if (container.Margin != default)
            {
                platformView.Margin = container.Margin;
                container.Margin = new Avalonia.Thickness(0);
            }

            // Remember the container's parent and position so we can swap PlatformView back in
            var parentPanel = container.Parent as Avalonia.Controls.Panel;
            int index = parentPanel?.Children.IndexOf(container) ?? -1;

            if (parentPanel != null && index >= 0)
                parentPanel.Children.RemoveAt(index);

            container.Children.Remove(platformView);

            if (parentPanel != null && index >= 0)
                parentPanel.Children.Insert(Math.Min(index, parentPanel.Children.Count), platformView);
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

        if (VirtualViewOrNull is IVisualElementController controller)
        {
            controller.PlatformSizeChanged();
        }
    }

    private void SetFocused(bool isFocused)
    {
        if (VirtualViewOrNull is not IView virtualView)
            return;

        if (virtualView.IsFocused == isFocused)
            return;

        virtualView.IsFocused = isFocused;
    }

    private void TrySendLoaded()
    {
        if (VirtualViewOrNull is VisualElement visualElement)
        {
            VisualElementLifecycle.TrySendLoaded(visualElement);
        }
    }

    private void TrySendUnloaded()
    {
        if (VirtualViewOrNull is VisualElement visualElement)
        {
            VisualElementLifecycle.TrySendUnloaded(visualElement);
        }
    }

    private static class VisualElementLifecycle
    {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SendLoaded")]
        private static extern void SendLoaded(VisualElement element, bool updateWiring);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SendUnloaded")]
        private static extern void SendUnloaded(VisualElement element, bool updateWiring);

        public static void TrySendLoaded(VisualElement element) =>
            SendLoaded(element, false);

        public static void TrySendUnloaded(VisualElement element) =>
            SendUnloaded(element, false);
    }
}
