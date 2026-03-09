using Avalonia.Controls;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IContentView"/>.</summary>
public partial class ContentPresenterHandler : ViewHandler<IContentView, Avalonia.Controls.Maui.Platform.MauiContentPresenter>
{
    /// <summary>Property mapper for <see cref="ContentPresenterHandler"/>.</summary>
    public static IPropertyMapper<IContentView, ContentPresenterHandler> Mapper =
        new PropertyMapper<IContentView, ContentPresenterHandler>(ViewMapper)
        {
            [nameof(IContentView.Content)] = MapContent,
        };

    /// <summary>Command mapper for <see cref="ContentPresenterHandler"/>.</summary>
    public static CommandMapper<IContentView, ContentPresenterHandler> CommandMapper =
        new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="ContentPresenterHandler"/>.</summary>
    public ContentPresenterHandler() : base(Mapper, CommandMapper)
    {

    }

    /// <summary>Initializes a new instance of <see cref="ContentPresenterHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public ContentPresenterHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ContentPresenterHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public ContentPresenterHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Maps the Content property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="page">The virtual view.</param>
    public static void MapContent(ContentPresenterHandler handler, IContentView page)
    {
        if (handler.PlatformView is MauiContentPresenter platformView)
        {
            platformView.UpdateContent(page, handler.MauiContext);
        }
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
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

    /// <inheritdoc/>
    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

        PlatformView.CrossPlatformLayout = VirtualView;
    }
}
