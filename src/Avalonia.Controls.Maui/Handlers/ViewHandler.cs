using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System.Diagnostics;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers;

public abstract partial class ViewHandler : ElementHandler, IViewHandler
{
    /// <summary>
    /// A dictionary that maps the virtual view properties to their platform view counterparts.
    /// </summary>
    public static IPropertyMapper<IView, IViewHandler> ViewMapper =
        new PropertyMapper<IView, IViewHandler>(ElementHandler.ElementMapper)
        {
            [nameof(IViewHandler.ContainerView)] = MapContainerView,
            [nameof(IView.AutomationId)] = MapAutomationId,
            [nameof(IView.Clip)] = MapClip,
            [nameof(IView.Shadow)] = MapShadow,
            [nameof(IView.Visibility)] = MapVisibility,
            [nameof(IView.Background)] = MapBackground,
            ["BackgroundColor"] = MapBackgroundColor,
            [nameof(IView.FlowDirection)] = MapFlowDirection,
            [nameof(IView.Width)] = MapWidth,
            [nameof(IView.Height)] = MapHeight,
            [nameof(IView.MinimumHeight)] = MapMinimumHeight,
            [nameof(IView.MaximumHeight)] = MapMaximumHeight,
            [nameof(IView.MinimumWidth)] = MapMinimumWidth,
            [nameof(IView.MaximumWidth)] = MapMaximumWidth,
            [nameof(IView.IsEnabled)] = MapIsEnabled,
            [nameof(IView.Opacity)] = MapOpacity,
            [nameof(IView.Semantics)] = MapSemantics,
            [nameof(IView.TranslationX)] = MapTranslationX,
            [nameof(IView.TranslationY)] = MapTranslationY,
            [nameof(IView.Scale)] = MapScale,
            [nameof(IView.ScaleX)] = MapScaleX,
            [nameof(IView.ScaleY)] = MapScaleY,
            [nameof(IView.Rotation)] = MapRotation,
            [nameof(IView.RotationX)] = MapRotationX,
            [nameof(IView.RotationY)] = MapRotationY,
            [nameof(IView.AnchorX)] = MapAnchorX,
            [nameof(IView.AnchorY)] = MapAnchorY,
            [nameof(IView.InputTransparent)] = MapInputTransparent,
            [nameof(IToolTipElement.ToolTip)] = MapToolTip,
            [nameof(IContextFlyoutElement.ContextFlyout)] = MapContextFlyout,
            [nameof(IView.HorizontalLayoutAlignment)] = MapHorizontalLayoutAlignment,
            [nameof(IView.VerticalLayoutAlignment)] = MapVerticalLayoutAlignment,
        };

    /// <summary>
    /// A dictionary that maps the virtual view commands to their platform view counterparts.
    /// </summary>
    /// <remarks>The concept or a command mapper is very similar to the property mapper with
    /// the addition that you can provide extra data in the form of arguments with the command mapper.</remarks>
    public static CommandMapper<IView, IViewHandler> ViewCommandMapper = new()
    {
        [nameof(IView.InvalidateMeasure)] = MapInvalidateMeasure,
        [nameof(IView.Frame)] = MapFrame,
        [nameof(IView.ZIndex)] = MapZIndex,
        [nameof(IView.Focus)] = MapFocus,
        [nameof(IView.Unfocus)] = MapUnfocus,
    };

    bool _hasContainer;

    internal DataFlowDirection DataFlowDirection { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewHandler"/> class.
    /// </summary>
    /// <param name="mapper">The default mapper to use for this handler.</param>
    /// <param name="commandMapper">The command mapper to use for this handler.</param>
    protected ViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper ?? ViewCommandMapper)
    {
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="PlatformView"/> is contained within a view.
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
    /// Gets a value that indicates whether or not the <see cref="VirtualView"/> needs a container view.
    /// </summary>
    public virtual bool NeedsContainer
    {
        get => VirtualView.NeedsContainer();
    }

    /// <summary>
    /// Constructs the <see cref="ContainerView"/> and adds <see cref="PlatformView"/> to a container.
    /// </summary>
    /// <remarks>This method is called when <see cref="HasContainer"/> is set to <see langword="true"/>.</remarks>
    protected abstract void SetupContainer();

    /// <summary>
    /// Deconstructs the <see cref="ContainerView"/> and removes <see cref="PlatformView"/> from its container. 
    /// </summary>
    /// <remarks>This method is called when <see cref="HasContainer"/> is set to <see langword="false"/>.</remarks>
    protected abstract void RemoveContainer();

    /// <summary>
    /// Gets the view that acts as a container for the <see cref="PlatformView"/>.
    /// </summary>
    /// <remarks>Note that this can be <see langword="null"/>. Especially when <see cref="HasContainer"/> is set to <see langword="false"/> this value might not be set.</remarks>
    public PlatformView? ContainerView { get; private protected set; }

    object? IViewHandler.ContainerView => ContainerView;

    /// <summary>
    /// Gets or sets the platform representation of the view associated to this handler.
    /// </summary>
    /// <remarks>This property holds the reference to platform layer view, e.g. the iOS/macOS, Android or Windows view.
    /// The abstract (.NET MAUI) view is found in <see cref="VirtualView"/>.</remarks>
    public new PlatformView? PlatformView
    {
        get => (PlatformView?)base.PlatformView;
        private protected set => base.PlatformView = value;
    }

    /// <summary>
    /// Gets or sets the .NET MAUI repesentation of the view associated to this handler.
    /// </summary>
    /// <remarks>This property holds the reference to the abstract (.NET MAUI) view.
    /// The platform view is found in <see cref="PlatformView"/>.</remarks>
    public new IView? VirtualView
    {
        get => (IView?)base.VirtualView;
        private protected set => base.VirtualView = value;
    }

    /// <inheritdoc/>
    public Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        var platformView = PlatformView;
        if (platformView is null || VirtualView is null)
            return Microsoft.Maui.Graphics.Size.Zero;

        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            // Already on UI thread, execute directly
            platformView.Measure(new global::Avalonia.Size(widthConstraint, heightConstraint));
            var avaloniaSize = platformView.DesiredSize;
            return new Microsoft.Maui.Graphics.Size(avaloniaSize.Width, avaloniaSize.Height);
        }
        else
        {
            // Not on UI thread, invoke synchronously on UI thread
            return Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                platformView.Measure(new global::Avalonia.Size(widthConstraint, heightConstraint));
                var avaloniaSize = platformView.DesiredSize;
                return new Microsoft.Maui.Graphics.Size(avaloniaSize.Width, avaloniaSize.Height);
            }).GetAwaiter().GetResult();
        }
    }

    /// <inheritdoc/>
    public virtual void PlatformArrange(Microsoft.Maui.Graphics.Rect frame)
    {
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            Arrange(frame);
        else
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => Arrange(frame));
    }

    private protected void Arrange(Microsoft.Maui.Graphics.Rect frame)
    {
        if (PlatformView is null)
            return;
            
        PlatformView.Measure(new global::Avalonia.Size(frame.Width, frame.Height));
        PlatformView.Arrange(new global::Avalonia.Rect(frame.X, frame.Y, frame.Width, frame.Height));
    }


    private protected abstract PlatformView OnCreatePlatformView();

    public sealed override object OnCreatePlatformElement() =>
        OnCreatePlatformView();


    /// <summary>
    /// Maps the abstract <see cref="IView.Width"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapWidth(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateWidth(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.Height"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapHeight(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateHeight(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.MinimumHeight"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapMinimumHeight(IViewHandler handler, IView view)
    {
        if (handler.IsConnectingHandler() && double.IsNaN(view.MinimumHeight))
        {
            return;
        }

        ((PlatformView?)handler.PlatformView)?.UpdateMinimumHeight(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.MaximumHeight"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapMaximumHeight(IViewHandler handler, IView view)
    {
        if (handler.IsConnectingHandler() && double.IsPositiveInfinity(view.MaximumHeight))
        {
            return;
        }

        ((PlatformView?)handler.PlatformView)?.UpdateMaximumHeight(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.MinimumWidth"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapMinimumWidth(IViewHandler handler, IView view)
    {
        if (handler.IsConnectingHandler() && double.IsNaN(view.MinimumWidth))
        {
            return;
        }

        ((PlatformView?)handler.PlatformView)?.UpdateMinimumWidth(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.MaximumWidth"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapMaximumWidth(IViewHandler handler, IView view)
    {
        if (handler.IsConnectingHandler() && double.IsPositiveInfinity(view.MaximumWidth))
        {
            return;
        }

        ((PlatformView?)handler.PlatformView)?.UpdateMaximumWidth(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.IsEnabled"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapIsEnabled(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateIsEnabled(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.Visibility"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapVisibility(IViewHandler handler, IView view)
    {
        var isConnectingHandler = handler.IsConnectingHandler();

        if (isConnectingHandler && view.Visibility == Visibility.Visible)
        {
            // Views are visible by default, so we don't need to map this property
            return;
        }

        if (handler.HasContainer)
        {
            ((PlatformView?)handler.ContainerView)?.UpdateVisibility(view);
        }


        ((PlatformView?)handler.PlatformView)?.UpdateVisibility(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.Background"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapBackground(IViewHandler handler, IView view)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (view.Background is ImageSourcePaint image)
        {
            var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

            platformView.UpdateBackgroundImageSourceAsync(image.ImageSource, provider)
                .FireAndForget(handler);
        }
        else
        {
            platformView.UpdateBackground(view);
        }
    }

    /// <summary>
    /// Maps the BackgroundColor property to trigger a Background update.
    /// This mirrors MAUI's VisualElement.MapBackgroundColor behavior where BackgroundColor
    /// changes are forwarded to the Background property mapper.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapBackgroundColor(IViewHandler handler, IView view)
    {
        handler.UpdateValue(nameof(IView.Background));
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.FlowDirection"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapFlowDirection(IViewHandler handler, IView view)
    {
        // All platforms match parent's flow direction by default, except for iOS/macOS where we have to manually set it.
        if (handler.IsConnectingHandler() && view.FlowDirection == FlowDirection.MatchParent)
        {
            return;
        }

        ((PlatformView?)handler.PlatformView)?.UpdateFlowDirection(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.Opacity"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapOpacity(IViewHandler handler, IView view)
    {
        if (handler.IsConnectingHandler() && view.Opacity == 1)
            return;

        if (handler.HasContainer)
        {
            ((PlatformView?)handler.ContainerView)?.UpdateOpacity(view);
            //We don't want the control opacity to be reduced by the container one, so we always set 100% to the control if it has a container
            ((PlatformView?)handler.PlatformView)?.UpdateOpacity(1);
        }
        else
            ((PlatformView?)handler.PlatformView)?.UpdateOpacity(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.AutomationId"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapAutomationId(IViewHandler handler, IView view)
    {
        if (handler.IsConnectingHandler() && view.AutomationId is null)
            return;

        ((PlatformView?)handler.PlatformView)?.UpdateAutomationId(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.Clip"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapClip(IViewHandler handler, IView view)
    {
        if (handler.IsConnectingHandler() && view.Clip is null)
            return;

        if (!handler.IsMappingProperties())
        {
            // Ensure container state is up to date before applying the clip
            handler.UpdateValue(nameof(IViewHandler.ContainerView));
        }

        var target = (PlatformView?)handler.ContainerView ?? (PlatformView?)handler.PlatformView;
        target?.UpdateClip(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.Shadow"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapShadow(IViewHandler handler, IView view)
    {
        if (handler.IsConnectingHandler() && view.Shadow is null)
            return;

        if (!handler.IsMappingProperties())
        {
            // ContainerView is already being mapped
            handler.UpdateValue(nameof(IViewHandler.ContainerView));
        }

        ((PlatformView?)handler.ContainerView)?.UpdateShadow(view);
    }

    static partial void MappingSemantics(IViewHandler handler, IView view);

    /// <summary>
    /// Maps the abstract <see cref="IView.Semantics"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapSemantics(IViewHandler handler, IView view)
    {
        MappingSemantics(handler, view);
        ((PlatformView?)handler.PlatformView)?.UpdateSemantics(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.InvalidateMeasure"/> method to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    /// <param name="args">The arguments passed associated to this event.</param>
    public static void MapInvalidateMeasure(IViewHandler handler, IView view, object? args)
    {
        (handler.PlatformView as PlatformView)?.InvalidateMeasure(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IViewHandler.ContainerView"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapContainerView(IViewHandler handler, IView view)
    {
        bool hasContainerOldValue = handler.HasContainer;

        if (handler is ViewHandler viewHandler)
            handler.HasContainer = viewHandler.NeedsContainer;
        else
            handler.HasContainer = view.NeedsContainer();

        if (hasContainerOldValue != handler.HasContainer)
        {
            handler.UpdateValue(nameof(IView.Visibility));
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    /// <summary>
    /// Maps the abstract <see cref="IBorder.Border"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapBorderView(IViewHandler handler, IView view)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));

        ((PlatformView?)handler.ContainerView)?.UpdateBorder(view);
    }
#pragma warning restore CS0618 // Type or member is obsolete

    static partial void MappingFrame(IViewHandler handler, IView view);

    /// <summary>
    /// Maps the abstract <see cref="IView.Frame"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    /// <param name="args">The arguments passed associated to this event.</param>
    public static void MapFrame(IViewHandler handler, IView view, object? args)
    {
        MappingFrame(handler, view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.ZIndex"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    /// <param name="args">The arguments passed associated to this event.</param>
    public static void MapZIndex(IViewHandler handler, IView view, object? args)
    {
        if (view.Parent is ILayout layout)
        {
            layout.Handler?.Invoke(nameof(ILayoutHandler.UpdateZIndex), view);
        }
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.Focus"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    /// <param name="args">The arguments passed associated to this event.</param>
    public static void MapFocus(IViewHandler handler, IView view, object? args)
    {
        if (args is not FocusRequest request)
            return;

        ((PlatformView?)handler.PlatformView)?.Focus(request);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.InputTransparent"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapInputTransparent(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateInputTransparent(handler, view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.Unfocus"/> method to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    /// <param name="args">The arguments passed associated to this event.</param>
    public static void MapUnfocus(IViewHandler handler, IView view, object? args)
    {
        ((PlatformView?)handler.PlatformView)?.Unfocus(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.TranslationX"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapTranslationX(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateTransformation(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.TranslationY"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapTranslationY(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateTransformation(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.Scale"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapScale(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateTransformation(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.ScaleX"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapScaleX(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateTransformation(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.ScaleY"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapScaleY(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateTransformation(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.Rotation"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapRotation(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateTransformation(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.RotationX"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapRotationX(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateTransformation(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.RotationY"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapRotationY(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateTransformation(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.AnchorX"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapAnchorX(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateTransformation(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.AnchorY"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapAnchorY(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateTransformation(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IToolTipElement.ToolTip"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapToolTip(IViewHandler handler, IView view)
    {
        if (handler.IsConnectingHandler() && view is not IToolTipElement)
            return;

        if (((IToolTipElement)view).ToolTip?.Content is not string toolTip)
            return;

        ((PlatformView?)handler.PlatformView)?.UpdateToolTip(toolTip);
    }

    /// <summary>
    /// Maps the abstract <see cref="IContextFlyoutElement.ContextFlyout"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapContextFlyout(IViewHandler handler, IView view)
    {
        if (view is IContextFlyoutElement contextFlyoutContainer)
        {
            if (handler.IsConnectingHandler() && contextFlyoutContainer.ContextFlyout is null)
            {
                return;
            }

            MapContextFlyout(handler, contextFlyoutContainer);
        }
    }

    internal static void MapContextFlyout(IElementHandler handler, IContextFlyoutElement contextFlyoutContainer)
    {
        _ = handler.MauiContext ?? throw new InvalidOperationException($"The handler's {nameof(handler.MauiContext)} cannot be null.");

        if (handler.PlatformView is PlatformView control)
        {
            if (contextFlyoutContainer.ContextFlyout != null)
            {

                var contextFlyoutHandler = contextFlyoutContainer.ContextFlyout.ToHandler(handler.MauiContext);
                var contextFlyoutPlatformView = contextFlyoutHandler.PlatformView;

                // MenuFlyout inherits from ContextMenu, so this cast will work
                if (contextFlyoutPlatformView is global::Avalonia.Controls.ContextMenu contextMenu)
                {
                    control.ContextMenu = contextMenu;
                }
            }
            else
            {
                control.ContextMenu = null;
            }
        }
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.HorizontalLayoutAlignment"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapHorizontalLayoutAlignment(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateHorizontalLayoutAlignment(view);
    }

    /// <summary>
    /// Maps the abstract <see cref="IView.VerticalLayoutAlignment"/> property to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="view">The associated <see cref="IView"/> instance.</param>
    public static void MapVerticalLayoutAlignment(IViewHandler handler, IView view)
    {
        ((PlatformView?)handler.PlatformView)?.UpdateVerticalLayoutAlignment(view);
    }

    /// <summary>
    /// Provides a string representation of the current object for debugging purposes.
    /// </summary>
    /// <remarks>
    /// This method is used by the <see cref="DebuggerDisplayAttribute"/> to display
    /// a concise and informative string representation of the <see cref="ViewHandler"/> instance
    /// during debugging sessions.
    /// </remarks>
    /// <returns>A string containing the type name and key properties of the object.</returns>
    private protected virtual string GetDebuggerDisplay()
    {
        var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(VirtualView), VirtualView, nameof(PlatformView), PlatformView);
        return $"{GetType().FullName}: {debugText}";
    }
}
