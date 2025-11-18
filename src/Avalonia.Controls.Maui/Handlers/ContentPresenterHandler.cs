using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ContentPresenterHandler : ViewHandler<IContentView, Avalonia.Controls.Maui.Platform.MauiContentPresenter>, IContentViewHandler
{
    public static IPropertyMapper<IContentView, IContentViewHandler> Mapper =
        new PropertyMapper<IContentView, IContentViewHandler>(ViewMapper)
        {
            [nameof(IContentView.Content)] = MapContent,
        };

    public static CommandMapper<IContentView, IContentViewHandler> CommandMapper =
        new(ViewCommandMapper);

    public ContentPresenterHandler() : base(Mapper, CommandMapper)
    {

    }

    public ContentPresenterHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ContentPresenterHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    IContentView IContentViewHandler.VirtualView => VirtualView;

    PlatformView IContentViewHandler.PlatformView => PlatformView;

    public static void MapContent(IContentViewHandler handler, IContentView page)
    {
        ((ContentPresenterHandler)handler).UpdateContent();
    }

    protected override Avalonia.Controls.Maui.Platform.MauiContentPresenter CreatePlatformView()
    {
        if (VirtualView == null)
        {
            throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a MauiContentPresenter");
        }

        var view = new Avalonia.Controls.Maui.Platform.MauiContentPresenter
        {
            CrossPlatformLayout = VirtualView
        };

        return view;
    }

    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

        PlatformView.CrossPlatformLayout = VirtualView;
    }

    void UpdateContent()
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        PlatformView.Children.Clear();

        if (VirtualView.PresentedContent is IView view)
            PlatformView.Children.Add((Control)view.ToPlatform(MauiContext));
    }
}
