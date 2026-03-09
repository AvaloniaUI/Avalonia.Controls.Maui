using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Separator;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IMenuFlyoutSeparator"/>.</summary>
public partial class MenuFlyoutSeparatorHandler : ElementHandler<IMenuFlyoutSeparator, PlatformView>
{
    /// <summary>Property mapper for <see cref="MenuFlyoutSeparatorHandler"/>.</summary>
    public static IPropertyMapper<IMenuFlyoutSeparator, MenuFlyoutSeparatorHandler> Mapper = new PropertyMapper<IMenuFlyoutSeparator, MenuFlyoutSeparatorHandler>(ElementMapper)
    {
    };

    /// <summary>Command mapper for <see cref="MenuFlyoutSeparatorHandler"/>.</summary>
    public static CommandMapper<IMenuFlyoutSeparator, MenuFlyoutSeparatorHandler> CommandMapper = new(ElementCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="MenuFlyoutSeparatorHandler"/>.</summary>
    public MenuFlyoutSeparatorHandler() : this(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="MenuFlyoutSeparatorHandler"/>.</summary>
    /// <param name="mapper">The property mapper.</param>
    /// <param name="commandMapper">The command mapper.</param>
    public MenuFlyoutSeparatorHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }
}
