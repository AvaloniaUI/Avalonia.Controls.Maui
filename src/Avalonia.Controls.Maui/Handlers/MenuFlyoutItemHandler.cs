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

public partial class MenuFlyoutItemHandler : ElementHandler<IMenuFlyoutItem, PlatformView>
{
    public static IPropertyMapper<IMenuFlyoutItem, MenuFlyoutItemHandler> Mapper = new PropertyMapper<IMenuFlyoutItem, MenuFlyoutItemHandler>(ElementMapper)
    {
        [nameof(IMenuFlyoutSubItem.Text)] = MapText,
        [nameof(IMenuFlyoutItem.KeyboardAccelerators)] = MapKeyboardAccelerators,
        [nameof(IMenuElement.Source)] = MapSource,
        [nameof(IMenuElement.IsEnabled)] = MapIsEnabled
    };

    public static CommandMapper<IMenuFlyoutItem, MenuFlyoutItemHandler> CommandMapper = new(ElementCommandMapper)
    {
    };

    public MenuFlyoutItemHandler() : base(Mapper, CommandMapper)
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
        base.DisconnectHandler(platformView);
        platformView.Click -= OnClicked;
    }

    void OnClicked(object? sender, Interactivity.RoutedEventArgs e)
    {
        VirtualView.Clicked();
    }

    public static void MapText(MenuFlyoutItemHandler handler, IMenuFlyoutItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.Header = view.Text;
    }

    public static void MapSource(MenuFlyoutItemHandler handler, IMenuFlyoutItem view) =>
        MapSourceAsync(handler, view).FireAndForget(handler);

    public static async Task MapSourceAsync(MenuFlyoutItemHandler handler, IMenuFlyoutItem view)
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
                ?.CreateLogger<MenuFlyoutItemHandler>()
                ?.LogError(ex, "Error loading menu icon source");
            platformView.Icon = null;
        }
    }

    public static void MapKeyboardAccelerators(MenuFlyoutItemHandler handler, IMenuFlyoutItem view)
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

    public static void MapIsEnabled(MenuFlyoutItemHandler handler, IMenuFlyoutItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.IsEnabled = view.IsEnabled;
    }
}
