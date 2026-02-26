using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using System;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="ITitleBar"/>.</summary>
public partial class TitleBarHandler : ViewHandler<ITitleBar, TitleBarView>
{
    /// <summary>Property mapper for <see cref="TitleBarHandler"/>.</summary>
    public static IPropertyMapper<ITitleBar, TitleBarHandler> Mapper =
        new PropertyMapper<ITitleBar, TitleBarHandler>(ViewMapper)
        {
            [nameof(ITitleBar.Title)] = MapTitle,
            [nameof(ITitleBar.Subtitle)] = MapSubtitle,
            [nameof(IContentView.Content)] = MapContent,
            [nameof(ITitleBar.PassthroughElements)] = MapPassthroughElements,
        };

    /// <summary>Command mapper for <see cref="TitleBarHandler"/>.</summary>
    public static CommandMapper<ITitleBar, TitleBarHandler> CommandMapper =
        new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="TitleBarHandler"/>.</summary>
    public TitleBarHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="TitleBarHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public TitleBarHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="TitleBarHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public TitleBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
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

    /// <inheritdoc/>
    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

        PlatformView.TitleBar = VirtualView;
        PlatformView.CrossPlatformLayout = VirtualView;
    }

    /// <summary>Maps the Title property to the platform view.</summary>
    /// <param name="handler">The handler for the title bar.</param>
    /// <param name="titleBar">The virtual title bar.</param>
    public static void MapTitle(TitleBarHandler handler, ITitleBar titleBar)
    {
        handler.PlatformView.TitleBar = titleBar;
    }

    /// <summary>Maps the Subtitle property to the platform view.</summary>
    /// <param name="handler">The handler for the title bar.</param>
    /// <param name="titleBar">The virtual title bar.</param>
    public static void MapSubtitle(TitleBarHandler handler, ITitleBar titleBar)
    {
        handler.PlatformView.TitleBar = titleBar;
    }

    /// <summary>Maps the Content property to the platform view.</summary>
    /// <param name="handler">The handler for the title bar.</param>
    /// <param name="titleBar">The virtual title bar.</param>
    public static void MapContent(TitleBarHandler handler, ITitleBar titleBar)
    {
        handler.UpdateContent();
    }

    /// <summary>Maps the PassthroughElements property to the platform view.</summary>
    /// <param name="handler">The handler for the title bar.</param>
    /// <param name="titleBar">The virtual title bar.</param>
    public static void MapPassthroughElements(TitleBarHandler handler, ITitleBar titleBar)
    {
        handler.UpdatePassthroughElements();
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

        // Update passthrough elements after content is set
        UpdatePassthroughElements();
    }

    void UpdatePassthroughElements()
    {
        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        // Clear existing passthrough elements
        PlatformView.ClearPassthroughElements();

        // Convert each passthrough element to a platform control and register it
        foreach (var element in VirtualView.PassthroughElements)
        {
            // Handle IContentView containers (LeadingContent, Content, TrailingContent are typically ContentViews)
            IView? viewToConvert = element;
            if (element is IContentView container && container.PresentedContent is not null)
            {
                viewToConvert = container.PresentedContent;
            }

            // Get the platform view for this element
            var handler = viewToConvert?.Handler;
            if (handler?.PlatformView is Control platformControl)
            {
                PlatformView.AddPassthroughElement(platformControl);
            }
            else if (viewToConvert != null)
            {
                // Try to get or create the platform view
                try
                {
                    var platformView = viewToConvert.ToPlatform(MauiContext);
                    if (platformView is Control control)
                    {
                        PlatformView.AddPassthroughElement(control);
                    }
                }
                catch
                {
                    // Element may not be attached to visual tree yet
                }
            }
        }
    }
}
