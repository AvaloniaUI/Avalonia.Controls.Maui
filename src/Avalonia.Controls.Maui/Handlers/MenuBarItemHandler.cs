using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.MenuItem;

namespace Avalonia.Controls.Maui.Handlers;

public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, PlatformView>, IMenuBarItemHandler
{
    public static IPropertyMapper<IMenuBarItem, IMenuBarItemHandler> Mapper = new PropertyMapper<IMenuBarItem, IMenuBarItemHandler>(ElementMapper)
    {
        [nameof(IMenuBarItem.Text)] = MapText,
        [nameof(IMenuBarItem.IsEnabled)] = MapIsEnabled,
    };

    public static CommandMapper<IMenuBarItem, IMenuBarItemHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(IMenuBarItemHandler.Add)] = MapAdd,
        [nameof(IMenuBarItemHandler.Remove)] = MapRemove,
        [nameof(IMenuBarItemHandler.Clear)] = MapClear,
        [nameof(IMenuBarItemHandler.Insert)] = MapInsert,
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

    public static void MapText(IMenuBarItemHandler handler, IMenuBarItem view)
    {
        if (handler is MenuBarItemHandler platformHandler)
            platformHandler.PlatformView.Header = view.Text;
    }

    public static void MapIsEnabled(IMenuBarItemHandler handler, IMenuBarItem view)
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

    public static void MapAdd(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
    {
        if (arg is MenuBarItemHandlerUpdate args)
        {
            handler.Add(args.MenuElement);
        }
    }

    public static void MapRemove(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
    {
        if (arg is MenuBarItemHandlerUpdate args)
        {
            handler.Remove(args.MenuElement);
        }
    }

    public static void MapInsert(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
    {
        if (arg is MenuBarItemHandlerUpdate args)
        {
            handler.Insert(args.Index, args.MenuElement);
        }
    }

    public static void MapClear(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
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

    IMenuBarItem IMenuBarItemHandler.VirtualView => VirtualView;

    object IMenuBarItemHandler.PlatformView => PlatformView;

    public override void OnDisconnectHandler(object platformView)
    {
        base.OnDisconnectHandler(platformView);
        foreach (var item in VirtualView)
            item?.Handler?.DisconnectHandler();
    }
}
