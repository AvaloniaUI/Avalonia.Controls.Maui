using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Separator;

namespace Avalonia.Controls.Maui.Handlers;

public partial class MenuFlyoutSeparatorHandler : ElementHandler<IMenuFlyoutSeparator, PlatformView>
{
    public static IPropertyMapper<IMenuFlyoutSeparator, MenuFlyoutSeparatorHandler> Mapper = new PropertyMapper<IMenuFlyoutSeparator, MenuFlyoutSeparatorHandler>(ElementMapper)
    {
    };

    public static CommandMapper<IMenuFlyoutSeparator, MenuFlyoutSeparatorHandler> CommandMapper = new(ElementCommandMapper)
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
}
