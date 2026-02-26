using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Menu;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IMenuBar"/>.</summary>
public partial class MenuBarHandler : ElementHandler<IMenuBar, PlatformView>
{
    /// <summary>Property mapper for <see cref="MenuBarHandler"/>.</summary>
    public static IPropertyMapper<IMenuBar, MenuBarHandler> Mapper = new PropertyMapper<IMenuBar, MenuBarHandler>(ElementMapper)
    {
    };

    /// <summary>Command mapper for <see cref="MenuBarHandler"/>.</summary>
    public static CommandMapper<IMenuBar, MenuBarHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(MenuBarHandler.Add)] = MapAdd,
        [nameof(MenuBarHandler.Remove)] = MapRemove,
        [nameof(MenuBarHandler.Clear)] = MapClear,
        [nameof(MenuBarHandler.Insert)] = MapInsert,
    };

    /// <summary>Initializes a new instance of <see cref="MenuBarHandler"/>.</summary>
    public MenuBarHandler() : this(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="MenuBarHandler"/>.</summary>
    /// <param name="mapper">The property mapper.</param>
    /// <param name="commandMapper">The command mapper.</param>
    public MenuBarHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }

    /// <inheritdoc/>
    public override void SetVirtualView(IElement view)
    {
        base.SetVirtualView(view);
        Clear();

        foreach (var item in ((IMenuBar)view))
        {
            Add(item);
        }
    }

    /// <summary>Maps the Add command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu bar.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapAdd(MenuBarHandler handler, IMenuBar layout, object? arg)
    {
        if (arg is MenuBarHandlerUpdate args)
        {
            handler.Add(args.MenuBarItem);
        }
    }

    /// <summary>Maps the Remove command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu bar.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapRemove(MenuBarHandler handler, IMenuBar layout, object? arg)
    {
        if (arg is MenuBarHandlerUpdate args)
        {
            handler.Remove(args.MenuBarItem);
        }
    }

    /// <summary>Maps the Insert command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu bar.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapInsert(MenuBarHandler handler, IMenuBar layout, object? arg)
    {
        if (arg is MenuBarHandlerUpdate args)
        {
            handler.Insert(args.Index, args.MenuBarItem);
        }
    }

    /// <summary>Maps the Clear command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu bar.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapClear(MenuBarHandler handler, IMenuBar layout, object? arg)
    {
        handler.Clear();
    }

    /// <summary>Adds a child element.</summary>
    /// <param name="view">The menu bar item to add.</param>
    public void Add(IMenuBarItem view)
    {
        var platformView = view.ToPlatform(MauiContext!);
        if (platformView is not null)
        {
            PlatformView.Items.Add(platformView);
        }
    }

    /// <summary>Removes a child element.</summary>
    /// <param name="view">The menu bar item to remove.</param>
    public void Remove(IMenuBarItem view)
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
    /// <param name="view">The menu bar item to insert.</param>
    public void Insert(int index, IMenuBarItem view)
    {
        var platformView = view.ToPlatform(MauiContext!);
        if (platformView is not null)
        {
            PlatformView.Items.Insert(index, platformView);
        }
    }

    /// <inheritdoc/>
    public override void OnDisconnectHandler(object platformView)
    {
        base.OnDisconnectHandler(platformView);
        foreach (var item in VirtualView)
            item?.Handler?.DisconnectHandler();
    }
}
