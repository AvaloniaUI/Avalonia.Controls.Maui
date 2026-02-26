using Microsoft.Maui.Handlers;
using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Avalonia.Controls.Maui.Services;
using PlatformView = Avalonia.Controls.MenuItem;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IMenuFlyoutSubItem"/>.</summary>
public partial class MenuFlyoutSubItemHandler : ElementHandler<IMenuFlyoutSubItem, PlatformView>
{
    /// <summary>Property mapper for <see cref="MenuFlyoutSubItemHandler"/>.</summary>
    public static IPropertyMapper<IMenuFlyoutSubItem, MenuFlyoutSubItemHandler> Mapper = new PropertyMapper<IMenuFlyoutSubItem, MenuFlyoutSubItemHandler>(ElementMapper)
    {
        [nameof(IMenuFlyoutSubItem.Text)] = MapText,
        [nameof(IMenuFlyoutSubItem.KeyboardAccelerators)] = MapKeyboardAccelerators,
        [nameof(IMenuFlyoutSubItem.Source)] = MapSource,
        [nameof(IMenuFlyoutSubItem.IsEnabled)] = MapIsEnabled,
    };

    /// <summary>Command mapper for <see cref="MenuFlyoutSubItemHandler"/>.</summary>
    public static CommandMapper<IMenuFlyoutSubItem, MenuFlyoutSubItemHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(MenuFlyoutSubItemHandler.Add)] = MapAdd,
        [nameof(MenuFlyoutSubItemHandler.Remove)] = MapRemove,
        [nameof(MenuFlyoutSubItemHandler.Clear)] = MapClear,
        [nameof(MenuFlyoutSubItemHandler.Insert)] = MapInsert,
    };

    /// <summary>Initializes a new instance of <see cref="MenuFlyoutSubItemHandler"/>.</summary>
    public MenuFlyoutSubItemHandler() : this(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="MenuFlyoutSubItemHandler"/>.</summary>
    /// <param name="mapper">The property mapper.</param>
    /// <param name="commandMapper">The command mapper.</param>
    public MenuFlyoutSubItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Click += OnClicked;
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
        platformView.Click -= OnClicked;
    }

    void OnClicked(object? sender, Interactivity.RoutedEventArgs e)
    {
        VirtualView.Clicked();
    }

    /// <summary>Maps the Text property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The menu flyout sub-item.</param>
    public static void MapText(MenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.Header = view.Text;
    }

    /// <summary>Maps the Source property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The menu flyout sub-item.</param>
    public static void MapSource(MenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view) =>
        MapSourceAsync(handler, view).FireAndForget(handler);

    /// <summary>Asynchronously maps the Source property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The menu flyout sub-item.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task MapSourceAsync(MenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (view.Source == null)
        {
            platformView.Icon = null;
            return;
        }

        try
        {
            var imageSourceServiceProvider = handler.GetRequiredService<IImageSourceServiceProvider>();
            var serviceSource = imageSourceServiceProvider?.GetImageSourceService(view.Source.GetType());

            if (serviceSource is IAvaloniaImageSourceService service)
            {
                var result = await service.GetImageAsync(view.Source, 1.0f);
                if (result?.Value is global::Avalonia.Media.Imaging.Bitmap bitmap)
                {
                    // Create an Image control to display as the icon
                    var iconImage = new global::Avalonia.Controls.Image
                    {
                        Source = bitmap,
                        Width = 16,
                        Height = 16
                    };
                    platformView.Icon = iconImage;
                }
                else
                {
                    platformView.Icon = null;
                }
            }
            else
            {
                platformView.Icon = null;
            }
        }
        catch (Exception ex)
        {
            handler.GetRequiredService<ILoggerFactory>()
                ?.CreateLogger<MenuFlyoutSubItemHandler>()
                ?.LogError(ex, "Error loading menu icon source");
            platformView.Icon = null;
        }
    }

    /// <summary>Maps the KeyboardAccelerators property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The menu flyout sub-item.</param>
    public static void MapKeyboardAccelerators(MenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (view.KeyboardAccelerators == null || view.KeyboardAccelerators.Count == 0)
        {
            platformView.InputGesture = null;
            return;
        }

        // Use the first keyboard accelerator
        var accelerator = view.KeyboardAccelerators[0];
        if (accelerator == null || string.IsNullOrEmpty(accelerator.Key))
            return;

        var keyModifiers = ConvertModifiers(accelerator.Modifiers);
        var key = ConvertKey(accelerator.Key);

        if (key != Avalonia.Input.Key.None)
        {
            platformView.InputGesture = new Avalonia.Input.KeyGesture(key, keyModifiers);
        }
    }

    private static Avalonia.Input.KeyModifiers ConvertModifiers(KeyboardAcceleratorModifiers modifiers)
    {
        var result = Avalonia.Input.KeyModifiers.None;

        if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Shift))
            result |= Avalonia.Input.KeyModifiers.Shift;

        if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Ctrl))
            result |= Avalonia.Input.KeyModifiers.Control;

        if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Alt))
            result |= Avalonia.Input.KeyModifiers.Alt;

        if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Cmd) || modifiers.HasFlag(KeyboardAcceleratorModifiers.Windows))
            result |= Avalonia.Input.KeyModifiers.Meta;

        return result;
    }

    private static Avalonia.Input.Key ConvertKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return Avalonia.Input.Key.None;

        // Try to parse as Avalonia Key enum
        if (Enum.TryParse<Avalonia.Input.Key>(key, ignoreCase: true, out var avaloniaKey))
            return avaloniaKey;

        // Handle single character keys (A-Z, 0-9)
        if (key.Length == 1)
        {
            var ch = char.ToUpper(key[0]);
            if (ch >= 'A' && ch <= 'Z')
            {
                return (Avalonia.Input.Key)((int)Avalonia.Input.Key.A + (ch - 'A'));
            }
            if (ch >= '0' && ch <= '9')
            {
                return (Avalonia.Input.Key)((int)Avalonia.Input.Key.D0 + (ch - '0'));
            }
        }

        return Avalonia.Input.Key.None;
    }

    /// <summary>Maps the IsEnabled property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The menu flyout sub-item.</param>
    public static void MapIsEnabled(MenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.IsEnabled = view.IsEnabled;
    }

    /// <inheritdoc/>
    public override void SetVirtualView(IElement view)
    {
        base.SetVirtualView(view);
        PlatformView.Items.Clear();

        foreach (var item in ((IMenuFlyoutSubItem)view))
        {
            Add(item);
        }
    }

    /// <summary>Maps the Add command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu element.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapAdd(MenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
    {
        if (arg is MenuFlyoutSubItemHandlerUpdate args)
        {
            handler.Add(args.MenuElement);
        }
    }

    /// <summary>Maps the Remove command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu element.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapRemove(MenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
    {
        if (arg is MenuFlyoutSubItemHandlerUpdate args)
        {
            handler.Remove(args.MenuElement);
        }
    }

    /// <summary>Maps the Insert command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu element.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapInsert(MenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
    {
        if (arg is MenuFlyoutSubItemHandlerUpdate args)
        {
            handler.Insert(args.Index, args.MenuElement);
        }
    }

    /// <summary>Maps the Clear command to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="layout">The menu element.</param>
    /// <param name="arg">The command argument.</param>
    public static void MapClear(MenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
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
}
