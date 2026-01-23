using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Menu;

namespace Avalonia.Controls.Maui.Handlers;

public partial class MenuBarHandler : ElementHandler<IMenuBar, PlatformView>
{
    public static IPropertyMapper<IMenuBar, MenuBarHandler> Mapper = new PropertyMapper<IMenuBar, MenuBarHandler>(ElementMapper)
    {
    };

    public static CommandMapper<IMenuBar, MenuBarHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(MenuBarHandler.Add)] = MapAdd,
        [nameof(MenuBarHandler.Remove)] = MapRemove,
        [nameof(MenuBarHandler.Clear)] = MapClear,
        [nameof(MenuBarHandler.Insert)] = MapInsert,
    };

    public MenuBarHandler() : this(Mapper, CommandMapper)
    {
    }

    public MenuBarHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
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

        foreach (var item in ((IMenuBar)view))
        {
            Add(item);
        }
    }

    public static void MapAdd(MenuBarHandler handler, IMenuBar layout, object? arg)
    {
        if (arg is MenuBarHandlerUpdate args)
        {
            handler.Add(args.MenuBarItem);
        }
    }

    public static void MapRemove(MenuBarHandler handler, IMenuBar layout, object? arg)
    {
        if (arg is MenuBarHandlerUpdate args)
        {
            handler.Remove(args.MenuBarItem);
        }
    }

    public static void MapInsert(MenuBarHandler handler, IMenuBar layout, object? arg)
    {
        if (arg is MenuBarHandlerUpdate args)
        {
            handler.Insert(args.Index, args.MenuBarItem);
        }
    }

    public static void MapClear(MenuBarHandler handler, IMenuBar layout, object? arg)
    {
        handler.Clear();
    }

    public void Add(IMenuBarItem view)
    {
        var platformView = view.ToPlatform(MauiContext!);
        if (platformView is not null)
        {
            PlatformView.Items.Add(platformView);
        }
    }

    public void Remove(IMenuBarItem view)
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

    public void Insert(int index, IMenuBarItem view)
    {
        var platformView = view.ToPlatform(MauiContext!);
        if (platformView is not null)
        {
            PlatformView.Items.Insert(index, platformView);
        }
    }

    public override void OnDisconnectHandler(object platformView)
    {
        base.OnDisconnectHandler(platformView);
        foreach (var item in VirtualView)
            item?.Handler?.DisconnectHandler();
    }
}
