using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Menu;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ToolbarHandler : ElementHandler<IToolbar, PlatformView>
{
    public static IPropertyMapper<IToolbar, ToolbarHandler> Mapper =
        new PropertyMapper<IToolbar, ToolbarHandler>(ElementMapper)
        {
            [nameof(IToolbar.Title)] = MapTitle,
        };

    public static CommandMapper<IToolbar, ToolbarHandler> CommandMapper = new();

    public ToolbarHandler() : base(Mapper, CommandMapper)
    {
    }

    public ToolbarHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ToolbarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }

    public static void MapTitle(ToolbarHandler handler, IToolbar toolbar)
    {
        // Avalonia Menu doesn't have a direct Title property
        // Title is typically handled by the window/page that contains the menu
        // This is a platform difference between Windows/MAUI and Avalonia
    }
}
