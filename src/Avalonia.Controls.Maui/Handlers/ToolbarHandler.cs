using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Menu;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IToolbar"/>.</summary>
public partial class ToolbarHandler : ElementHandler<IToolbar, PlatformView>
{
    /// <summary>Property mapper for <see cref="ToolbarHandler"/>.</summary>
    public static IPropertyMapper<IToolbar, ToolbarHandler> Mapper =
        new PropertyMapper<IToolbar, ToolbarHandler>(ElementMapper)
        {
            [nameof(IToolbar.Title)] = MapTitle,
        };

    /// <summary>Command mapper for <see cref="ToolbarHandler"/>.</summary>
    public static CommandMapper<IToolbar, ToolbarHandler> CommandMapper = new();

    /// <summary>Initializes a new instance of <see cref="ToolbarHandler"/>.</summary>
    public ToolbarHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ToolbarHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public ToolbarHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ToolbarHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public ToolbarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }

    /// <summary>Maps the Title property to the platform view.</summary>
    /// <param name="handler">The handler for the toolbar.</param>
    /// <param name="toolbar">The virtual toolbar.</param>
    public static void MapTitle(ToolbarHandler handler, IToolbar toolbar)
    {
        // Avalonia Menu doesn't have a direct Title property
        // Title is typically handled by the window/page that contains the menu
        // This is a platform difference between Windows/MAUI and Avalonia
    }
}
