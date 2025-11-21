using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Avalonia.Controls;
using PlatformView = Avalonia.Controls.ContentControl;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI SwipeItemView to Avalonia ContentControl mapping
/// </summary>
internal partial class SwipeItemViewHandler : ContentViewHandler, ISwipeItemViewHandler
{
    public static new IPropertyMapper<ISwipeItemView, ISwipeItemViewHandler> Mapper =
        new PropertyMapper<ISwipeItemView, ISwipeItemViewHandler>(ContentViewHandler.Mapper)
        {
        };

    public static new CommandMapper<ISwipeItemView, ISwipeItemViewHandler> CommandMapper =
        new(ContentViewHandler.CommandMapper)
        {
        };

    public SwipeItemViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public SwipeItemViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public SwipeItemViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    ISwipeItemView ISwipeItemViewHandler.VirtualView => (ISwipeItemView)VirtualView;

    object ISwipeItemViewHandler.PlatformView => PlatformView!;
}
