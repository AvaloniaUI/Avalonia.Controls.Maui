using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.MenuItem;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IMenuBarItem"/>.</summary>
public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, PlatformView>
{
    /// <summary>Property mapper for <see cref="MenuBarItemHandler"/>.</summary>
    public static IPropertyMapper<IMenuBarItem, MenuBarItemHandler> Mapper = new PropertyMapper<IMenuBarItem, MenuBarItemHandler>(ElementMapper)
    {
        [nameof(IMenuBarItem.Text)] = MapText,
        [nameof(IMenuBarItem.IsEnabled)] = MapIsEnabled,
    };

    /// <summary>Command mapper for <see cref="MenuBarItemHandler"/>.</summary>
    public static CommandMapper<IMenuBarItem, MenuBarItemHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(MenuBarItemHandler.Add)] = MapAdd,
        [nameof(MenuBarItemHandler.Remove)] = MapRemove,
        [nameof(MenuBarItemHandler.Clear)] = MapClear,
        [nameof(MenuBarItemHandler.Insert)] = MapInsert,
    };

    /// <summary>Initializes a new instance of <see cref="MenuBarItemHandler"/>.</summary>
    public MenuBarItemHandler() : this(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="MenuBarItemHandler"/>.</summary>
    /// <param name="mapper">The property mapper.</param>
    /// <param name="commandMapper">The command mapper.</param>
    public MenuBarItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }

    /// <summary>Maps the Text property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The menu bar item.</param>
    public static void MapText(MenuBarItemHandler handler, IMenuBarItem view)
    {
        if (handler is MenuBarItemHandler platformHandler)
            platformHandler.PlatformView.Header = view.Text;
    }

    /// <summary>Maps the IsEnabled property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The menu bar item.</param>
    public static void MapIsEnabled(MenuBarItemHandler handler, IMenuBarItem view)
    {
        if (handler is MenuBarItemHandler platformHandler)
            platformHandler.PlatformView.IsEnabled = view.IsEnabled;
    }

    /// <inheritdoc/>
    public override void SetVirtualView(IElement view)
    {
        base.SetVirtualView(view);
        Clear();

        foreach (var item in ((IMenuBarItem)view))
        {
            Add(item);
        }
    }

    /// <summary>Maps the Add command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu bar item.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapAdd(MenuBarItemHandler handler, IMenuBarItem layout, object? arg)
    {
        if (arg is MenuBarItemHandlerUpdate args)
        {
            handler.Add(args.MenuElement);
        }
    }

    /// <summary>Maps the Remove command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu bar item.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapRemove(MenuBarItemHandler handler, IMenuBarItem layout, object? arg)
    {
        if (arg is MenuBarItemHandlerUpdate args)
        {
            handler.Remove(args.MenuElement);
        }
    }

    /// <summary>Maps the Insert command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu bar item.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapInsert(MenuBarItemHandler handler, IMenuBarItem layout, object? arg)
    {
        if (arg is MenuBarItemHandlerUpdate args)
        {
            handler.Insert(args.Index, args.MenuElement);
        }
    }

    /// <summary>Maps the Clear command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu bar item.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapClear(MenuBarItemHandler handler, IMenuBarItem layout, object? arg)
    {
        handler.Clear();
    }

    /// <summary>Adds a child element.</summary>
    /// <param name="view">The menu element to add.</param>
    public void Add(IMenuElement view)
    {
        var platformView = view.ToPlatform(MauiContext!);
        if (platformView is not null)
        {
            PlatformView.Items.Add(platformView);
        }
    }

    /// <summary>Removes a child element.</summary>
    /// <param name="view">The menu element to remove.</param>
    public void Remove(IMenuElement view)
    {
        if (view.Handler?.PlatformView is object platformView)
        {
            PlatformView.Items.Remove(platformView);
        }
    }

    /// <summary>Clears all child elements.</summary>
    public void Clear()
    {
        PlatformView.Items.Clear();
    }

    /// <summary>Inserts a child element at the specified index.</summary>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="view">The menu element to insert.</param>
    public void Insert(int index, IMenuElement view)
    {
        var platformView = view.ToPlatform(MauiContext!);
        if (platformView is not null)
        {
            PlatformView.Items.Insert(index, platformView);
        }
    }

    /// <inheritdoc/>
#if MAUI_SOURCE_BUILD
    private protected override void OnDisconnectHandler(object platformView)
#else
    public override void OnDisconnectHandler(object platformView)
#endif
    {
        base.OnDisconnectHandler(platformView);
        foreach (var item in VirtualView)
            item?.Handler?.DisconnectHandler();
    }
}
