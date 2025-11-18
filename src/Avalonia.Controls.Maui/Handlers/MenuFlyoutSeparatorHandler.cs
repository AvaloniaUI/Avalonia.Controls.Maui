using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Separator;

namespace Avalonia.Controls.Maui.Handlers;

public partial class MenuFlyoutSeparatorHandler : ElementHandler<IMenuFlyoutSeparator, PlatformView>, IMenuFlyoutSeparatorHandler
{
    public static IPropertyMapper<IMenuFlyoutSeparator, IMenuFlyoutSeparatorHandler> Mapper = new PropertyMapper<IMenuFlyoutSeparator, IMenuFlyoutSeparatorHandler>(ElementMapper)
    {
    };

    public static CommandMapper<IMenuFlyoutSeparator, IMenuFlyoutSeparatorHandler> CommandMapper = new(ElementCommandMapper)
    {
    };

    public MenuFlyoutSeparatorHandler() : this(Mapper, CommandMapper)
    {
    }

    public MenuFlyoutSeparatorHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
    {
    }

    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }

    IMenuFlyoutSeparator IMenuFlyoutSeparatorHandler.VirtualView => VirtualView;

    object IMenuFlyoutSeparatorHandler.PlatformView => PlatformView;
}
