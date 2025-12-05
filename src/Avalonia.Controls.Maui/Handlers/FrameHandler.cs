using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Border;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for the Frame control, which wraps content with a border, rounded corners, and optional shadow.
/// </summary>
/// <remarks>
/// <para>
/// Frame is implemented using Avalonia's Border control with shadow effects.
/// Note: Frame is obsolete in .NET MAUI 9+ and Microsoft recommends using Border instead.
/// </para>
/// <para>
/// Supported properties:
/// - BorderColor: Border stroke color (via IBorderElement)
/// - CornerRadius: Rounded corner radius
/// - HasShadow: Drop shadow effect
/// - Content: The wrapped view
/// - Background, Padding (inherited from ContentView)
/// </para>
/// </remarks>
public partial class FrameHandler : ViewHandler<IContentView, PlatformView>, IContentViewHandler
{
    public static IPropertyMapper<IContentView, IContentViewHandler> Mapper =
        new PropertyMapper<IContentView, IContentViewHandler>(ViewMapper)
        {
            ["BorderColor"] = MapBorderColor,
            [nameof(IContentView.Content)] = MapContent,
            ["CornerRadius"] = MapCornerRadius,
            ["HasShadow"] = MapHasShadow,
            [nameof(IContentView.Background)] = MapBackground,
            [nameof(IContentView.Padding)] = MapPadding,
        };
    
    public static CommandMapper<IContentView, IContentViewHandler> CommandMapper =
        new(ViewCommandMapper);
    
    public FrameHandler() : base(Mapper, CommandMapper)
    {
    }
    
    public FrameHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }
    
    public FrameHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }
    
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }
    
    IContentView IContentViewHandler.VirtualView => VirtualView;
    
    object IContentViewHandler.PlatformView => PlatformView;

    public static void MapBorderColor(IContentViewHandler handler, IContentView view)
    {
        if (handler is FrameHandler frameHandler &&
            frameHandler.PlatformView is PlatformView platformView &&
            view is Frame frame)
        {
            platformView.UpdateBorderColor(frame);
        }
    }
    
    public static void MapCornerRadius(IContentViewHandler handler, IContentView view)
    {
        if (handler is FrameHandler frameHandler &&
            frameHandler.PlatformView is PlatformView platformView &&
            view is Frame frame)
        {
            platformView.UpdateCornerRadius(frame);
        }
    }
    
    public static void MapHasShadow(IContentViewHandler handler, IContentView view)
    {
        if (handler is FrameHandler frameHandler &&
            frameHandler.PlatformView is PlatformView platformView &&
            view is Frame frame)
        {
            platformView.UpdateHasShadow(frame);
        }
    }

    public static void MapContent(IContentViewHandler handler, IContentView view)
    {
        if (handler is FrameHandler frameHandler)
        {
            frameHandler.UpdateContent();
        }
    }
    
    public static void MapBackground(IContentViewHandler handler, IContentView view)
    {
        if (handler is FrameHandler frameHandler &&
            frameHandler.PlatformView is PlatformView platformView &&
            view is Frame frame)
        {
            platformView.UpdateBackground(frame);
        }
    }
    
    public static void MapPadding(IContentViewHandler handler, IContentView view)
    {
        if (handler is FrameHandler frameHandler &&
            frameHandler.PlatformView is PlatformView platformView &&
            view is Frame frame)
        {
            platformView.UpdatePadding(frame);
        }
    }
    
    private void UpdateContent()
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        PlatformView.Child = null;

        if (VirtualView.PresentedContent is IView view)
        {
            PlatformView.Child = (Control)view.ToPlatform(MauiContext);
        }
    }
}