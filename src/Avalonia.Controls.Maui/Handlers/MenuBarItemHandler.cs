using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.MenuItem;

namespace Avalonia.Controls.Maui.Handlers;

public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, PlatformView>
{
    public static IPropertyMapper<IMenuBarItem, MenuBarItemHandler> Mapper = new PropertyMapper<IMenuBarItem, MenuBarItemHandler>(ElementMapper)
    {
        [nameof(IMenuBarItem.Text)] = MapText,
        [nameof(IMenuBarItem.IsEnabled)] = MapIsEnabled,
    };

    public static CommandMapper<IMenuBarItem, MenuBarItemHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(MenuBarItemHandler.Add)] = MapAdd,
        [nameof(MenuBarItemHandler.Remove)] = MapRemove,
        [nameof(MenuBarItemHandler.Clear)] = MapClear,
        [nameof(MenuBarItemHandler.Insert)] = MapInsert,
    };

    public MenuBarItemHandler() : this(Mapper, CommandMapper)
    {
    }

    public MenuBarItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
    {
    }

    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }

    public static void MapText(MenuBarItemHandler handler, IMenuBarItem view)
    {
        if (handler is MenuBarItemHandler platformHandler)
            platformHandler.PlatformView.Header = view.Text;
    }

    public static void MapIsEnabled(MenuBarItemHandler handler, IMenuBarItem view)
    {
        if (handler is MenuBarItemHandler platformHandler)
            platformHandler.PlatformView.IsEnabled = view.IsEnabled;
    }

    public override void SetVirtualView(IElement view)
    {
        base.SetVirtualView(view);
        Clear();

        foreach (var item in ((IMenuBarItem)view))
        {
            Add(item);
        }
    }

    public static void MapAdd(MenuBarItemHandler handler, IMenuBarItem layout, object? arg)
    {
        if (arg is MenuBarItemHandlerUpdate args)
        {
            handler.Add(args.MenuElement);
        }
    }

    public static void MapRemove(MenuBarItemHandler handler, IMenuBarItem layout, object? arg)
    {
        if (arg is MenuBarItemHandlerUpdate args)
        {
            handler.Remove(args.MenuElement);
        }
    }

    public static void MapInsert(MenuBarItemHandler handler, IMenuBarItem layout, object? arg)
    {
        if (arg is MenuBarItemHandlerUpdate args)
        {
            handler.Insert(args.Index, args.MenuElement);
        }
    }

    public static void MapClear(MenuBarItemHandler handler, IMenuBarItem layout, object? arg)
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

    public override void OnDisconnectHandler(object platformView)
    {
        base.OnDisconnectHandler(platformView);
        foreach (var item in VirtualView)
            item?.Handler?.DisconnectHandler();
    }
}
