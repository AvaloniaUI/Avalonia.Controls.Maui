using Avalonia.Controls;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ContentPresenterHandler : ViewHandler<IContentView, Avalonia.Controls.Maui.Platform.MauiContentPresenter>
{
    public static IPropertyMapper<IContentView, ContentPresenterHandler> Mapper =
        new PropertyMapper<IContentView, ContentPresenterHandler>(ViewMapper)
        {
            [nameof(IContentView.Content)] = MapContent,
        };

    public static CommandMapper<IContentView, ContentPresenterHandler> CommandMapper =
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

    public static void MapContent(ContentPresenterHandler handler, IContentView page)
    {
        if (handler.PlatformView is MauiContentPresenter platformView)
        {
            platformView.UpdateContent(page, handler.MauiContext);
        }
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
}
