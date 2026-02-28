using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Avalonia handler for <see cref="ILayout"/>.
/// </summary>
public partial class LayoutHandler : ViewHandler<ILayout, Panel>
{
    /// <summary>
    /// Property mapper for <see cref="LayoutHandler"/>.
    /// </summary>
    public static IPropertyMapper<ILayout, LayoutHandler> Mapper = new PropertyMapper<ILayout, LayoutHandler>(ViewMapper)
    {
        [nameof(ILayout.Background)] = MapBackground,
        [nameof(ILayout.ClipsToBounds)] = MapClipsToBounds,
        [nameof(IView.InputTransparent)] = MapInputTransparent,
    };

    /// <summary>
    /// Command mapper for <see cref="LayoutHandler"/>.
    /// </summary>
    public static CommandMapper<ILayout, LayoutHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(LayoutHandler.Add)] = MapAdd,
        [nameof(LayoutHandler.Remove)] = MapRemove,
        [nameof(LayoutHandler.Clear)] = MapClear,
        [nameof(LayoutHandler.Insert)] = MapInsert,
        [nameof(LayoutHandler.Update)] = MapUpdate,
        [nameof(LayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
    };

    /// <summary>
    /// Initializes a new instance of <see cref="LayoutHandler"/>.
    /// </summary>
    public LayoutHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="LayoutHandler"/>.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public LayoutHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Creates the Avalonia platform view for this handler.
    /// </summary>
    /// <returns>A new <see cref="Platform.LayoutPanel"/> instance configured with cross-platform layout delegates.</returns>
    protected override Panel CreatePlatformView()
    {
        return new Avalonia.Controls.Maui.Platform.LayoutPanel()
        {
            CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
            CrossPlatformArrange = VirtualView.CrossPlatformArrange,
        };
    }

    /// <summary>
    /// Maps the Background property to the platform view.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="layout">The associated <see cref="ILayout"/> instance.</param>
    public static void MapBackground(LayoutHandler handler, ILayout layout)
    {
        ((LayoutHandler)handler).PlatformView?.UpdateBackground(layout);
    }

    /// <summary>
    /// Maps the <see cref="IView.InputTransparent"/> property to the platform view.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="layout">The associated <see cref="ILayout"/> instance.</param>
    public static void MapInputTransparent(LayoutHandler handler, ILayout layout)
    {
        // TODO: Implement InputTransparent
    }

    /// <summary>
    /// Maps the <see cref="ILayout.ClipsToBounds"/> property to the platform view.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="layout">The associated <see cref="ILayout"/> instance.</param>
    public static void MapClipsToBounds(LayoutHandler handler, ILayout layout)
    {
        ((LayoutHandler)handler).PlatformView?.UpdateClipsToBounds(layout);
    }

    /// <summary>
    /// Maps the Add command to add a child view to the layout.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="layout">The associated <see cref="ILayout"/> instance.</param>
    /// <param name="arg">The <see cref="LayoutHandlerUpdate"/> containing the view to add.</param>
    public static void MapAdd(LayoutHandler handler, ILayout layout, object? arg)
    {
        if (arg is LayoutHandlerUpdate args)
        {
            handler.Add(args.View);
        }
    }

    /// <summary>
    /// Maps the Remove command to remove a child view from the layout.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="layout">The associated <see cref="ILayout"/> instance.</param>
    /// <param name="arg">The <see cref="LayoutHandlerUpdate"/> containing the view to remove.</param>
    public static void MapRemove(LayoutHandler handler, ILayout layout, object? arg)
    {
        if (arg is LayoutHandlerUpdate args)
        {
            handler.Remove(args.Index, args.View);
        }
    }

    /// <summary>
    /// Maps the Insert command to insert a child view at a specific index in the layout.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="layout">The associated <see cref="ILayout"/> instance.</param>
    /// <param name="arg">The <see cref="LayoutHandlerUpdate"/> containing the view and index.</param>
    public static void MapInsert(LayoutHandler handler, ILayout layout, object? arg)
    {
        if (arg is LayoutHandlerUpdate args)
        {
            handler.Insert(args.Index, args.View);
        }
    }

    /// <summary>
    /// Maps the Clear command to remove all child views from the layout.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="layout">The associated <see cref="ILayout"/> instance.</param>
    /// <param name="arg">The associated command arguments.</param>
    public static void MapClear(LayoutHandler handler, ILayout layout, object? arg)
    {
        handler.Clear();
    }

    /// <summary>
    /// Maps the Update command to replace a child view at a specific index in the layout.
    /// </summary>
    static void MapUpdate(LayoutHandler handler, ILayout layout, object? arg)
    {
        if (arg is LayoutHandlerUpdate args)
        {
            handler.Update(args.Index, args.View);
        }
    }

    /// <summary>
    /// Maps the UpdateZIndex command to update the Z-index of a child view.
    /// </summary>
    static void MapUpdateZIndex(LayoutHandler handler, ILayout layout, object? arg)
    {
        if (arg is IView view)
        {
            handler.UpdateZIndex(view);
        }
    }

    /// <summary>
    /// Adds a child view to the layout's platform panel.
    /// </summary>
    /// <param name="child">The child view to add.</param>
    public void Add(IView child)
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        if (child?.ToPlatform(MauiContext) is Control control)
        {
            control.ZIndex = child.ZIndex;
            PlatformView.Children.Add(control);
        }
    }

    /// <summary>
    /// Removes a child view from the layout's platform panel.
    /// </summary>
    /// <param name="index">The index of the child to remove.</param>
    /// <param name="child">The child view to remove.</param>
    public void Remove(int index, IView child)
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

        if (child?.Handler is IViewHandler viewHandler)
        {
            var control = viewHandler.ContainerView as Control ?? viewHandler.PlatformView as Control;
            if (control != null)
            {
                PlatformView.Children.Remove(control);
                return;
            }
        }

        if (index >= 0 && index < PlatformView.Children.Count)
        {
            PlatformView.Children.RemoveAt(index);
        }
    }

    /// <summary>
    /// Removes all child views from the layout's platform panel.
    /// </summary>
    public void Clear()
    {
        PlatformView?.Children.Clear();
    }

    /// <summary>
    /// Inserts a child view at the specified index in the layout's platform panel.
    /// </summary>
    /// <param name="index">The index at which to insert the child.</param>
    /// <param name="child">The child view to insert.</param>
    public void Insert(int index, IView child)
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        if (child?.ToPlatform(MauiContext) is Control control)
        {
            control.ZIndex = child.ZIndex;
            PlatformView.Children.Insert(index, control);
        }
    }

    /// <summary>
    /// Replaces the child view at the specified index in the layout's platform panel.
    /// </summary>
    /// <param name="index">The index of the child to replace.</param>
    /// <param name="child">The new child view.</param>
    public void Update(int index, IView child)
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        PlatformView.Children.RemoveAt(index);
        if (child?.ToPlatform(MauiContext) is Control control)
        {
            control.ZIndex = child.ZIndex;
            PlatformView.Children.Insert(index, control);
        }
    }

    /// <summary>
    /// Updates the Z-index of a child view's platform control.
    /// </summary>
    /// <param name="child">The child view whose Z-index should be updated.</param>
    public void UpdateZIndex(IView child)
    {
        if (child?.Handler is IViewHandler viewHandler)
        {
            var control = viewHandler.ContainerView as Control ?? viewHandler.PlatformView as Control;
            if (control != null)
            {
                control.ZIndex = child.ZIndex;
            }
        }
    }

    /// <inheritdoc/>
    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        ((Avalonia.Controls.Maui.Platform.LayoutPanel)PlatformView).CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
        ((Avalonia.Controls.Maui.Platform.LayoutPanel)PlatformView).CrossPlatformArrange = VirtualView.CrossPlatformArrange;

        PlatformView.Children.Clear();
        foreach (var child in VirtualView)
        {
            Add(child);
        }
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(Panel platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is Microsoft.Maui.Controls.VisualElement element)
        {
            element.MeasureInvalidated += OnMeasureInvalidated;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(Panel platformView)
    {
        base.DisconnectHandler(platformView);

        if (VirtualView is Microsoft.Maui.Controls.VisualElement element)
        {
            element.MeasureInvalidated -= OnMeasureInvalidated;
        }

        // Clear the delegate references that create strong paths from the Avalonia
        // LayoutPanel back to the MAUI layout control.  Without this, if the
        // Avalonia compositor briefly retains the LayoutPanel (e.g. during an
        // in-flight TransitioningContentControl animation), the delegate targets
        // keep the entire MAUI control subtree alive and prevent garbage collection.
        if (platformView is LayoutPanel layoutPanel)
        {
            layoutPanel.CrossPlatformMeasure = null;
            layoutPanel.CrossPlatformArrange = null;
        }
    }

    /// <summary>
    /// Gets the flow direction for the layout. Currently returns the input value unchanged.
    /// </summary>
    internal static FlowDirection GetLayoutFlowDirection(FlowDirection flowDirection)
    {
        return flowDirection;
    }

    private void OnMeasureInvalidated(object? sender, EventArgs e)
    {
        PlatformView?.InvalidateMeasure();
    }
}

internal static class LayoutPanelExtensions
{
    internal static void UpdateBackground(this Panel panel, ILayout layout)
    {
        if (layout.Background != null)
        {
            panel.Background = layout.Background.ToPlatform();
        }
        else if (layout is Microsoft.Maui.Controls.Layout mauiLayout && mauiLayout.BackgroundColor != null)
        {
            panel.Background = mauiLayout.BackgroundColor.ToPlatform();
        }
        else
        {
            panel.Background = Avalonia.Media.Brushes.Transparent;
        }
    }

    internal static void UpdateClipsToBounds(this Panel panel, ILayout layout)
    {
        panel.ClipToBounds = layout.ClipsToBounds;
    }
}