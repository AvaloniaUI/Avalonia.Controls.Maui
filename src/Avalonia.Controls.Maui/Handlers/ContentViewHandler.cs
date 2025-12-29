using Avalonia.Controls;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ContentViewHandler : ViewHandler<IContentView, Avalonia.Controls.Maui.Platform.ContentView>
{
    public static IPropertyMapper<IContentView, ContentViewHandler> Mapper =
        new PropertyMapper<IContentView, ContentViewHandler>(ViewMapper)
        {
            [nameof(IContentView.Content)] = MapContent,
        };

    public static CommandMapper<IContentView, ContentViewHandler> CommandMapper =
        new(ViewCommandMapper);

    public ContentViewHandler() : base(Mapper, CommandMapper)
    {

    }

    public ContentViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ContentViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    public static void MapContent(ContentViewHandler handler, IContentView page)
    {
        if (handler.PlatformView is Avalonia.Controls.Maui.Platform.ContentView platformView)
        {
            platformView.UpdateContent(page, handler.MauiContext);
        }
    }

    protected override Avalonia.Controls.Maui.Platform.ContentView CreatePlatformView()
    {
        if (VirtualView == null)
        {
            throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a ContentView");
        }

        var view = new Avalonia.Controls.Maui.Platform.ContentView
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
