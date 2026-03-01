using Avalonia.Controls;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IContentView"/>.</summary>
public partial class ContentViewHandler : ViewHandler<IContentView, Avalonia.Controls.Maui.Platform.ContentView>
{
    /// <summary>Property mapper for <see cref="ContentViewHandler"/>.</summary>
    public static IPropertyMapper<IContentView, ContentViewHandler> Mapper =
        new PropertyMapper<IContentView, ContentViewHandler>(ViewMapper)
        {
            [nameof(IContentView.Content)] = MapContent,
        };

    /// <summary>Command mapper for <see cref="ContentViewHandler"/>.</summary>
    public static CommandMapper<IContentView, ContentViewHandler> CommandMapper =
        new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="ContentViewHandler"/>.</summary>
    public ContentViewHandler() : base(Mapper, CommandMapper)
    {

    }

    /// <summary>Initializes a new instance of <see cref="ContentViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public ContentViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ContentViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public ContentViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Maps the Content property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="page">The virtual view.</param>
    public static void MapContent(ContentViewHandler handler, IContentView page)
    {
        if (handler.PlatformView is Avalonia.Controls.Maui.Platform.ContentView platformView)
        {
            platformView.UpdateContent(page, handler.MauiContext);
        }
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
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

    /// <inheritdoc/>
    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

        PlatformView.CrossPlatformLayout = VirtualView;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(Avalonia.Controls.Maui.Platform.ContentView platformView)
    {
        base.DisconnectHandler(platformView);

        // Null the cross-platform layout delegate so the Avalonia ContentView
        // does not hold a strong path back to the MAUI virtual view.
        // Note: We intentionally do NOT call platformView.Children.Clear() here
        // because removing children from the panel fires DetachedFromVisualTree
        // events on child controls, which cascades into premature MAUI Unloaded
        // lifecycle events on controls whose handlers are still connected.
        // The children will be collected along with the platform view once
        // the handler releases its reference.
        platformView.CrossPlatformLayout = null;
    }
}
