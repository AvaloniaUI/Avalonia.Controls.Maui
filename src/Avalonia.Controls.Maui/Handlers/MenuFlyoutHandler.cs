using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.ContextMenu;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IMenuFlyout"/>.</summary>
public partial class MenuFlyoutHandler : ElementHandler<IMenuFlyout, PlatformView>
{
    /// <summary>Property mapper for <see cref="MenuFlyoutHandler"/>.</summary>
    public static IPropertyMapper<IMenuFlyout, MenuFlyoutHandler> Mapper = new PropertyMapper<IMenuFlyout, MenuFlyoutHandler>(ElementMapper)
    {
    };

    /// <summary>Command mapper for <see cref="MenuFlyoutHandler"/>.</summary>
    public static CommandMapper<IMenuFlyout, MenuFlyoutHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(MenuFlyoutHandler.Add)] = MapAdd,
        [nameof(MenuFlyoutHandler.Remove)] = MapRemove,
        [nameof(MenuFlyoutHandler.Clear)] = MapClear,
        [nameof(MenuFlyoutHandler.Insert)] = MapInsert,
    };

    /// <summary>Initializes a new instance of <see cref="MenuFlyoutHandler"/>.</summary>
    public MenuFlyoutHandler() : this(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="MenuFlyoutHandler"/>.</summary>
    /// <param name="mapper">The property mapper.</param>
    /// <param name="commandMapper">The command mapper.</param>
    public MenuFlyoutHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
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

        foreach (var item in (IMenuFlyout)view)
        {
            Add(item);
        }
    }

    /// <summary>Maps the Add command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="menuElement">The menu flyout.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapAdd(MenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
    {
        if (arg is ContextFlyoutItemHandlerUpdate args)
        {
            handler.Add(args.MenuElement);
        }
    }

    /// <summary>Maps the Remove command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="menuElement">The menu flyout.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapRemove(MenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
    {
        if (arg is ContextFlyoutItemHandlerUpdate args)
        {
            handler.Remove(args.MenuElement);
        }
    }

    /// <summary>Maps the Insert command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="menuElement">The menu flyout.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapInsert(MenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
    {
        if (arg is ContextFlyoutItemHandlerUpdate args)
        {
            handler.Insert(args.Index, args.MenuElement);
        }
    }

    /// <summary>Maps the Clear command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="menuElement">The menu flyout.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapClear(MenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
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
