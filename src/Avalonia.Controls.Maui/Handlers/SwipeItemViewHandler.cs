using Microsoft.Maui;
using Microsoft.Maui.Platform;
using Avalonia.Controls;
using PlatformView = Avalonia.Controls.ContentControl;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="ISwipeItemView"/>.</summary>
public partial class SwipeItemViewHandler : ContentViewHandler
{
    /// <summary>Property mapper for <see cref="SwipeItemViewHandler"/>.</summary>
    public static new IPropertyMapper<ISwipeItemView, SwipeItemViewHandler> Mapper =
        new PropertyMapper<ISwipeItemView, SwipeItemViewHandler>(ContentViewHandler.Mapper)
        {
        };

    /// <summary>Command mapper for <see cref="SwipeItemViewHandler"/>.</summary>
    public static new CommandMapper<ISwipeItemView, SwipeItemViewHandler> CommandMapper =
        new(ContentViewHandler.CommandMapper)
        {
        };

    /// <summary>Initializes a new instance of <see cref="SwipeItemViewHandler"/>.</summary>
    public SwipeItemViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SwipeItemViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public SwipeItemViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SwipeItemViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public SwipeItemViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }
}
