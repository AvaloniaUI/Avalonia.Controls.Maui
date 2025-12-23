using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Avalonia.Controls;
using PlatformView = Avalonia.Controls.ContentControl;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for .NET MAUI SwipeItemView to Avalonia ContentControl mapping
/// </summary>
public partial class SwipeItemViewHandler : ContentViewHandler
{
    public static new IPropertyMapper<ISwipeItemView, SwipeItemViewHandler> Mapper =
        new PropertyMapper<ISwipeItemView, SwipeItemViewHandler>(ContentViewHandler.Mapper)
        {
        };

    public static new CommandMapper<ISwipeItemView, SwipeItemViewHandler> CommandMapper =
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
}
