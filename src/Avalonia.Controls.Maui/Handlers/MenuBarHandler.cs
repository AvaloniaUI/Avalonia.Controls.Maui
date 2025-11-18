using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Menu;

namespace Avalonia.Controls.Maui.Handlers;

public partial class MenuBarHandler : ElementHandler<IMenuBar, PlatformView>, IMenuBarHandler
{
    public static IPropertyMapper<IMenuBar, IMenuBarHandler> Mapper = new PropertyMapper<IMenuBar, IMenuBarHandler>(ElementMapper)
    {
    };

    public static CommandMapper<IMenuBar, IMenuBarHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(IMenuBarHandler.Add)] = MapAdd,
        [nameof(IMenuBarHandler.Remove)] = MapRemove,
        [nameof(IMenuBarHandler.Clear)] = MapClear,
        [nameof(IMenuBarHandler.Insert)] = MapInsert,
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

    public static void MapAdd(IMenuBarHandler handler, IMenuBar layout, object? arg)
    {
        if (arg is MenuBarHandlerUpdate args)
        {
            handler.Add(args.MenuBarItem);
        }
    }

    public static void MapRemove(IMenuBarHandler handler, IMenuBar layout, object? arg)
    {
        if (arg is MenuBarHandlerUpdate args)
        {
            handler.Remove(args.MenuBarItem);
        }
    }

    public static void MapInsert(IMenuBarHandler handler, IMenuBar layout, object? arg)
    {
        if (arg is MenuBarHandlerUpdate args)
        {
            handler.Insert(args.Index, args.MenuBarItem);
        }
    }

    public static void MapClear(IMenuBarHandler handler, IMenuBar layout, object? arg)
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

    IMenuBar IMenuBarHandler.VirtualView => VirtualView;

    object IMenuBarHandler.PlatformView => PlatformView;

    public override void OnDisconnectHandler(object platformView)
    {
        base.OnDisconnectHandler(platformView);
        foreach (var item in VirtualView)
            item?.Handler?.DisconnectHandler();
    }
}
