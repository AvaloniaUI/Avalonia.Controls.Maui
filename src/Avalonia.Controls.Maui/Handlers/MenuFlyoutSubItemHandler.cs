using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Avalonia.Controls.Maui.Services;
using PlatformView = Avalonia.Controls.MenuItem;

namespace Avalonia.Controls.Maui.Handlers;

public partial class MenuFlyoutSubItemHandler : ElementHandler<IMenuFlyoutSubItem, PlatformView>
{
    public static IPropertyMapper<IMenuFlyoutSubItem, MenuFlyoutSubItemHandler> Mapper = new PropertyMapper<IMenuFlyoutSubItem, MenuFlyoutSubItemHandler>(ElementMapper)
    {
        [nameof(IMenuFlyoutSubItem.Text)] = MapText,
        [nameof(IMenuFlyoutSubItem.KeyboardAccelerators)] = MapKeyboardAccelerators,
        [nameof(IMenuFlyoutSubItem.Source)] = MapSource,
        [nameof(IMenuFlyoutSubItem.IsEnabled)] = MapIsEnabled,
    };

    public static CommandMapper<IMenuFlyoutSubItem, MenuFlyoutSubItemHandler> CommandMapper = new(ElementCommandMapper)
    {
        [nameof(MenuFlyoutSubItemHandler.Add)] = MapAdd,
        [nameof(MenuFlyoutSubItemHandler.Remove)] = MapRemove,
        [nameof(MenuFlyoutSubItemHandler.Clear)] = MapClear,
        [nameof(MenuFlyoutSubItemHandler.Insert)] = MapInsert,
    };

    public MenuFlyoutSubItemHandler() : this(Mapper, CommandMapper)
    {
    }

    public MenuFlyoutSubItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
    {
    }

    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Click += OnClicked;
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
        platformView.Click -= OnClicked;
    }

    void OnClicked(object? sender, Interactivity.RoutedEventArgs e)
    {
        VirtualView.Clicked();
    }

    public static void MapText(MenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.Header = view.Text;
    }

    public static void MapSource(MenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view) =>
        MapSourceAsync(handler, view).FireAndForget(handler);

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

    public static void MapIsEnabled(MenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.IsEnabled = view.IsEnabled;
    }

    public override void SetVirtualView(IElement view)
    {
        base.SetVirtualView(view);
        PlatformView.Items.Clear();

        foreach (var item in ((IMenuFlyoutSubItem)view))
        {
            Add(item);
        }
    }

    public static void MapAdd(MenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
    {
        if (arg is MenuFlyoutSubItemHandlerUpdate args)
        {
            handler.Add(args.MenuElement);
        }
    }

    public static void MapRemove(MenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
    {
        if (arg is MenuFlyoutSubItemHandlerUpdate args)
        {
            handler.Remove(args.MenuElement);
        }
    }

    public static void MapInsert(MenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
    {
        if (arg is MenuFlyoutSubItemHandlerUpdate args)
        {
            handler.Insert(args.Index, args.MenuElement);
        }
    }

    public static void MapClear(MenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
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
}
