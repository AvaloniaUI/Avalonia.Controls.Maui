using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Maui.Handlers;

public partial class LayoutHandler : ViewHandler<ILayout, Panel>, ILayoutHandler
{
    public static IPropertyMapper<ILayout, ILayoutHandler> Mapper = new PropertyMapper<ILayout, ILayoutHandler>(ViewMapper)
    {
        [nameof(ILayout.Background)] = MapBackground,
        [nameof(ILayout.ClipsToBounds)] = MapClipsToBounds,
        [nameof(IView.InputTransparent)] = MapInputTransparent,
    };

    public static CommandMapper<ILayout, ILayoutHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(ILayoutHandler.Add)] = MapAdd,
        [nameof(ILayoutHandler.Remove)] = MapRemove,
        [nameof(ILayoutHandler.Clear)] = MapClear,
        [nameof(ILayoutHandler.Insert)] = MapInsert,
        [nameof(ILayoutHandler.Update)] = MapUpdate,
        [nameof(ILayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
    };

    public LayoutHandler() : base(Mapper, CommandMapper)
    {
    }

    public LayoutHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    ILayout ILayoutHandler.VirtualView => VirtualView;

    System.Object ILayoutHandler.PlatformView => PlatformView;

    protected override Panel CreatePlatformView()
    {
        return new LayoutPanel()
        {
            CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
            CrossPlatformArrange = VirtualView.CrossPlatformArrange,
        };
    }

    public static void MapBackground(ILayoutHandler handler, ILayout layout)
    {
        ((LayoutHandler)handler).PlatformView?.UpdateBackground(layout);
    }

    public static void MapInputTransparent(ILayoutHandler handler, ILayout layout)
    {
        // TODO: Implement InputTransparent
    }

    public static void MapClipsToBounds(ILayoutHandler handler, ILayout layout)
    {
        ((LayoutHandler)handler).PlatformView?.UpdateClipsToBounds(layout);
    }

    public static void MapAdd(ILayoutHandler handler, ILayout layout, object? arg)
    {
        if (arg is LayoutHandlerUpdate args)
        {
            handler.Add(args.View);
        }
    }

    public static void MapRemove(ILayoutHandler handler, ILayout layout, object? arg)
    {
        if (arg is LayoutHandlerUpdate args)
        {
            handler.Remove(args.View);
        }
    }

    public static void MapInsert(ILayoutHandler handler, ILayout layout, object? arg)
    {
        if (arg is LayoutHandlerUpdate args)
        {
            handler.Insert(args.Index, args.View);
        }
    }

    public static void MapClear(ILayoutHandler handler, ILayout layout, object? arg)
    {
        handler.Clear();
    }

    static void MapUpdate(ILayoutHandler handler, ILayout layout, object? arg)
    {
        if (arg is LayoutHandlerUpdate args)
        {
            handler.Update(args.Index, args.View);
        }
    }

    static void MapUpdateZIndex(ILayoutHandler handler, ILayout layout, object? arg)
    {
        if (arg is IView view)
        {
            handler.UpdateZIndex(view);
        }
    }

    public void Add(IView child)
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        if (child?.ToPlatform(MauiContext) is Control control)
        {
            PlatformView.Children.Add(control);
        }
    }

    public void Remove(IView child)
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

        if (child?.Handler?.PlatformView is Control control)
        {
            PlatformView.Children.Remove(control);
        }
    }

    public void Clear()
    {
        PlatformView?.Children.Clear();
    }

    public void Insert(int index, IView child)
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        if (child?.ToPlatform(MauiContext) is Control control)
        {
            PlatformView.Children.Insert(index, control);
        }
    }

    public void Update(int index, IView child)
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        PlatformView.Children.RemoveAt(index);
        if (child?.ToPlatform(MauiContext) is Control control)
        {
            PlatformView.Children.Insert(index, control);
        }
    }

    public void UpdateZIndex(IView child)
    {
        if (child?.Handler?.PlatformView is Control control)
        {
            control.ZIndex = child.ZIndex;
        }
    }

    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        ((LayoutPanel)PlatformView).CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
        ((LayoutPanel)PlatformView).CrossPlatformArrange = VirtualView.CrossPlatformArrange;

        PlatformView.Children.Clear();
        foreach (var child in VirtualView)
        {
            Add(child);
        }
    }

    protected override void ConnectHandler(Panel platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is Microsoft.Maui.Controls.VisualElement element)
        {
            element.MeasureInvalidated += OnMeasureInvalidated;
        }
    }

    protected override void DisconnectHandler(Panel platformView)
    {
        base.DisconnectHandler(platformView);

        if (VirtualView is Microsoft.Maui.Controls.VisualElement element)
        {
            element.MeasureInvalidated -= OnMeasureInvalidated;
        }
    }

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
        else
        {
            panel.ClearValue(Panel.BackgroundProperty);
        }
    }

    internal static void UpdateClipsToBounds(this Panel panel, ILayout layout)
    {
        panel.ClipToBounds = layout.ClipsToBounds;
    }
}