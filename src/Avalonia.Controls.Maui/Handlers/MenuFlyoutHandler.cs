using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.ContextMenu;

namespace Avalonia.Controls.Maui.Handlers;

public partial class MenuFlyoutHandler : ElementHandler<IMenuFlyout, PlatformView>
{
    public static IPropertyMapper<IMenuFlyout, MenuFlyoutHandler> Mapper = new PropertyMapper<IMenuFlyout, MenuFlyoutHandler>(ElementMapper)
    {
    };

    public static CommandMapper<IMenuFlyout, MenuFlyoutHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(MenuFlyoutHandler.Add)] = MapAdd,
        [nameof(MenuFlyoutHandler.Remove)] = MapRemove,
        [nameof(MenuFlyoutHandler.Clear)] = MapClear,
        [nameof(MenuFlyoutHandler.Insert)] = MapInsert,
    };

    public MenuFlyoutHandler() : this(Mapper, CommandMapper)
    {
    }

    public MenuFlyoutHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
    {
    }

    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }

    public override void SetVirtualView(IElement view)
    {
        base.SetVirtualView(view);
        Clear();

        foreach (var item in (IMenuFlyout)view)
        {
            Add(item);
        }
    }

    public static void MapAdd(MenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
    {
        if (arg is ContextFlyoutItemHandlerUpdate args)
        {
            handler.Add(args.MenuElement);
        }
    }

    public static void MapRemove(MenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
    {
        if (arg is ContextFlyoutItemHandlerUpdate args)
        {
            handler.Remove(args.MenuElement);
        }
    }

    public static void MapInsert(MenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
    {
        if (arg is ContextFlyoutItemHandlerUpdate args)
        {
            handler.Insert(args.Index, args.MenuElement);
        }
    }

    public static void MapClear(MenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
    {
        handler.Clear();
    }

    public void Add(IMenuElement view)
    {
        var platformView = view.ToPlatform(MauiContext!);
        if (platformView is not null)
        {
            PlatformView.Items.Add(platformView);
        }
    }

    public void Remove(IMenuElement view)
    {
        if (view.Handler?.PlatformView is object platformView)
        {
            PlatformView.Items.Remove(platformView);
        }
    }

    public void Clear()
    {
        PlatformView.Items.Clear();
    }

    public void Insert(int index, IMenuElement view)
    {
        var platformView = view.ToPlatform(MauiContext!);
        if (platformView is not null)
        {
            PlatformView.Items.Insert(index, platformView);
        }
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        if (VirtualView is not null)
        {
            foreach (var item in VirtualView)
            {
                item.Handler?.DisconnectHandler();
            }
        }

        base.DisconnectHandler(platformView);
    }
}
