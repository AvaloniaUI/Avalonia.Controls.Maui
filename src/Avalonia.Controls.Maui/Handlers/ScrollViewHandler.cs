using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using System.Reflection;
using GraphicsSize = Microsoft.Maui.Graphics.Size;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ScrollViewHandler : ViewHandler<IScrollView, ScrollViewer>, IScrollViewHandler
{
    private EventHandler<ScrollChangedEventArgs>? _scrollChangedHandler;

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

    protected override ScrollViewer CreatePlatformView()
    {
        return new ScrollViewer();
    }

    protected override void ConnectHandler(ScrollViewer platformView)
    {
        base.ConnectHandler(platformView);
        _scrollChangedHandler = OnScrollChanged;
        platformView.ScrollChanged += _scrollChangedHandler;
    }

    protected override void DisconnectHandler(ScrollViewer platformView)
    {
        base.DisconnectHandler(platformView);
        if (_scrollChangedHandler != null)
        {
            platformView.ScrollChanged -= _scrollChangedHandler;
        }
        _scrollChangedHandler = null;
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs args)
    {
        if (VirtualView != null && sender is ScrollViewer scrollViewer)
        {
            scrollViewer.UpdateOffsets(VirtualView);
            UpdateScrollPosition(VirtualView, scrollViewer);
            UpdateContentSize(VirtualView, scrollViewer);
        }
    }

    public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
    {
        if (handler.PlatformView is ScrollViewer platformView)
        {
            platformView.UpdateContent(scrollView, handler.MauiContext);
            UpdateContentSize(scrollView, platformView);
        }
    }

    public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
    {
        if (handler.PlatformView is ScrollViewer platformView)
        {
            platformView.UpdateScrollBars(scrollView);
            UpdateContentSize(scrollView, platformView);
        }
    }

    public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
    {
        if (handler.PlatformView is ScrollViewer platformView)
        {
            platformView.UpdateScrollBars(scrollView);
            UpdateContentSize(scrollView, platformView);
        }
    }

    public static void MapOrientation(IScrollViewHandler handler, IScrollView scrollView)
    {
        if (handler.PlatformView is ScrollViewer platformView)
        {
            platformView.UpdateOrientation(scrollView);
            UpdateContentSize(scrollView, platformView);
        }
    }

    public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
    {
        if (args is not ScrollToRequest request)
            return;

        if (handler.PlatformView is ScrollViewer platformView)
        {
            platformView.ScrollTo(request);
            platformView.UpdateOffsetsFromRequest(scrollView, request);
            UpdateScrollPositionFromRequest(scrollView, request);
            UpdateContentSize(scrollView, platformView);
        }
    }

    private static void UpdateContentSize(IScrollView scrollView, ScrollViewer platformView)
    {
        // IScrollView.ContentSize is defined as a read-only property on the interface (no public setter), so we can't assign to it directly in the handler.
        // MAUI's concrete ScrollView exposes a protected/internal setter, which is what we need to update during mapping. Reflection
        // is used purely to invoke that non-public setter when it exists, allowing us to keep the virtual view's ContentSize in sync with the Avalonia ScrollViewer
        // extent despite the interface being getter-only.
        var extent = platformView.Extent;
        var avaloniaSize = new Size(
            extent.Width > 0 ? extent.Width : platformView.Bounds.Width,
            extent.Height > 0 ? extent.Height : platformView.Bounds.Height);

        var targetSize = new GraphicsSize(avaloniaSize.Width, avaloniaSize.Height);
        var contentSizeProperty = scrollView.GetType().GetProperty(
            "ContentSize",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        var setter = contentSizeProperty?.GetSetMethod(true);
        if (setter != null)
        {
            setter.Invoke(scrollView, [targetSize]);
        }
    }

    private static void UpdateScrollPosition(IScrollView scrollView, ScrollViewer platformView)
    {
        var offset = platformView.Offset;
        TrySetScrollPosition(scrollView, offset.X, offset.Y);
    }

    private static void UpdateScrollPositionFromRequest(IScrollView scrollView, ScrollToRequest request)
    {
        TrySetScrollPosition(scrollView, request.HorizontalOffset, request.VerticalOffset);
    }
    
    private static void TrySetScrollPosition(IScrollView scrollView, double scrollX, double scrollY)
    {
        // Updates the ScrollX and ScrollY properties on the ScrollView using reflection.
        // These properties are internal on MAUI's ScrollView implementation and not exposed through IScrollView,
        // but they're read by consumers of the ScrollView (e.g., for data binding or event handling).
        // This ensures the internal scroll position state stays synchronized with the platform control.
        var scrollViewType = scrollView.GetType();

        var scrollXProp = scrollViewType.GetProperty("ScrollX", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (scrollXProp?.CanWrite == true)
        {
            scrollXProp.SetValue(scrollView, scrollX);
        }

        var scrollYProp = scrollViewType.GetProperty("ScrollY", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (scrollYProp?.CanWrite == true)
        {
            scrollYProp.SetValue(scrollView, scrollY);
        }
    }
}
