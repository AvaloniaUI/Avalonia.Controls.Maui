using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ScrollViewHandler : ViewHandler<IScrollView, ScrollViewer>, IScrollViewHandler
{
    public static IPropertyMapper<IScrollView, IScrollViewHandler> Mapper = new PropertyMapper<IScrollView, IScrollViewHandler>(ViewMapper)
    {
        [nameof(IScrollView.Content)] = MapContent,
        [nameof(IScrollView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
        [nameof(IScrollView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
        [nameof(IScrollView.Orientation)] = MapOrientation,
    };

    public static CommandMapper<IScrollView, IScrollViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(IScrollView.RequestScrollTo)] = MapRequestScrollTo,
    };

    public ScrollViewHandler() : base(Mapper, CommandMapper)
    {

    }

    public ScrollViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ScrollViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    IScrollView IScrollViewHandler.VirtualView => VirtualView;

    PlatformView IScrollViewHandler.PlatformView => PlatformView;

    protected override ScrollViewer CreatePlatformView()
    {
        return new ScrollViewer();
    }

    public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
    {
        ScrollViewer platformScrollView = (ScrollViewer)handler.PlatformView;
        if (platformScrollView == null || handler.MauiContext == null)
            return;

        var content = ((IView)scrollView.Content!).ToPlatform(handler.MauiContext) as global::Avalonia.Controls.Control;
        platformScrollView.Content = content;
    }

    public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
    {
        ScrollViewer platformScrollView = (ScrollViewer)handler.PlatformView;
        if (platformScrollView == null)
            return;
        platformScrollView.HorizontalScrollBarVisibility = scrollView.HorizontalScrollBarVisibility.ToAvaloniaScrollBarVisibility();
    }

    public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
    {
        ScrollViewer platformScrollView = (ScrollViewer)handler.PlatformView;
        if (platformScrollView == null)
            return;
        platformScrollView.VerticalScrollBarVisibility = scrollView.VerticalScrollBarVisibility.ToAvaloniaScrollBarVisibility();
    }

    public static void MapOrientation(IScrollViewHandler handler, IScrollView scrollView)
    {
        ScrollViewer platformScrollView = (ScrollViewer)handler.PlatformView;
        if (platformScrollView == null)
            return;

        // Map MAUI Orientation to Avalonia ScrollBarVisibility
        // When scrolling is not enabled in a direction, we set it to Disabled
        // which constrains the content to the viewport size in that direction
        switch (scrollView.Orientation)
        {
            case ScrollOrientation.Horizontal:
                platformScrollView.HorizontalScrollBarVisibility = scrollView.HorizontalScrollBarVisibility.ToAvaloniaScrollBarVisibility();
                platformScrollView.VerticalScrollBarVisibility = Primitives.ScrollBarVisibility.Disabled;
                break;
            case ScrollOrientation.Vertical:
                platformScrollView.HorizontalScrollBarVisibility = Primitives.ScrollBarVisibility.Disabled;
                platformScrollView.VerticalScrollBarVisibility = scrollView.VerticalScrollBarVisibility.ToAvaloniaScrollBarVisibility();
                break;
            case ScrollOrientation.Both:
                platformScrollView.HorizontalScrollBarVisibility = scrollView.HorizontalScrollBarVisibility.ToAvaloniaScrollBarVisibility();
                platformScrollView.VerticalScrollBarVisibility = scrollView.VerticalScrollBarVisibility.ToAvaloniaScrollBarVisibility();
                break;
            case ScrollOrientation.Neither:
                platformScrollView.HorizontalScrollBarVisibility = Primitives.ScrollBarVisibility.Disabled;
                platformScrollView.VerticalScrollBarVisibility = Primitives.ScrollBarVisibility.Disabled;
                break;
        }
    }

    public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
    {
        if (args is not ScrollToRequest request)
            return;
        ScrollViewer platformScrollView = (ScrollViewer)handler.PlatformView;
        if (platformScrollView == null)
            return;

        platformScrollView.Offset = new global::Avalonia.Vector(request.HorizontalOffset, request.VerticalOffset);
    }
}