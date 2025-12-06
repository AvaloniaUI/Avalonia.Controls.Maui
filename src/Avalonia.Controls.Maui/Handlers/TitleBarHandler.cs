using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI TitleBar control on Avalonia.
/// </summary>
public partial class TitleBarHandler : ViewHandler<ITitleBar, TitleBarView>
{
    public static IPropertyMapper<ITitleBar, TitleBarHandler> Mapper =
        new PropertyMapper<ITitleBar, TitleBarHandler>(ViewMapper)
        {
            [nameof(ITitleBar.Title)] = MapTitle,
            [nameof(ITitleBar.Subtitle)] = MapSubtitle,
            [nameof(IContentView.Content)] = MapContent,
        };

    public static CommandMapper<ITitleBar, TitleBarHandler> CommandMapper =
        new(ViewCommandMapper);

    public TitleBarHandler() : base(Mapper, CommandMapper)
    {
    }

    public TitleBarHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public TitleBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override TitleBarView CreatePlatformView()
    {
        if (VirtualView == null)
        {
            throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a TitleBarView");
        }

        var view = new TitleBarView
        {
            MauiContext = MauiContext,
            TitleBar = VirtualView,
            CrossPlatformLayout = VirtualView
        };

        return view;
    }

    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

        PlatformView.TitleBar = VirtualView;
        PlatformView.CrossPlatformLayout = VirtualView;
    }

    public static void MapTitle(TitleBarHandler handler, ITitleBar titleBar)
    {
        handler.PlatformView.TitleBar = titleBar;
    }

    public static void MapSubtitle(TitleBarHandler handler, ITitleBar titleBar)
    {
        handler.PlatformView.TitleBar = titleBar;
    }

    public static void MapContent(TitleBarHandler handler, ITitleBar titleBar)
    {
        handler.UpdateContent();
    }

    void UpdateContent()
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        PlatformView.Children.Clear();

        // The TitleBar is a TemplatedView, so its PresentedContent is the actual rendered template
        if (VirtualView.PresentedContent is IView view)
        {
            var platformContent = view.ToPlatform(MauiContext);
            if (platformContent is Control control)
            {
                PlatformView.Children.Add(control);
            }
        }
    }
}
