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

public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, PlatformView>, ISwipeItemMenuItemHandler
{
    public static IPropertyMapper<ISwipeItemMenuItem, ISwipeItemMenuItemHandler> Mapper =
        new PropertyMapper<ISwipeItemMenuItem, ISwipeItemMenuItemHandler>(ElementHandler.ElementMapper)
        {
            [nameof(ISwipeItemMenuItem.Visibility)] = MapVisibility,
            [nameof(IView.Background)] = MapBackground,
            [nameof(IMenuElement.Text)] = MapText,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(IMenuElement.Source)] = MapSource,
        };

    public static CommandMapper<ISwipeItemMenuItem, ISwipeItemMenuItemHandler> CommandMapper =
        new(ElementHandler.ElementCommandMapper)
        {
        };

    public SwipeItemMenuItemHandler() : base(Mapper, CommandMapper)
    {
    }

    protected SwipeItemMenuItemHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    protected SwipeItemMenuItemHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformElement()
    {
        return new PlatformView();
    }

    public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.IsVisible = view.Visibility == Microsoft.Maui.Visibility.Visible;
    }

    public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        // TODO: Map background to Avalonia control
        // Avalonia MenuItem background mapping
    }

    public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.Header = view.Text;
    }

    public static void MapTextColor(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        // TODO: Map text color to Avalonia MenuItem foreground
    }

    public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        // TODO: Map character spacing to Avalonia text rendering
    }

    public static void MapFont(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        // TODO: Map font to Avalonia MenuItem font properties
    }

    public static void MapSource(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
        MapSourceAsync(handler, view).FireAndForget(handler);

    public static async Task MapSourceAsync(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
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
                ?.CreateLogger<SwipeItemMenuItemHandler>()
                ?.LogError(ex, "Error loading menu icon source");
            platformView.Icon = null;
        }
    }

    ISwipeItemMenuItem ISwipeItemMenuItemHandler.VirtualView => VirtualView;

    object ISwipeItemMenuItemHandler.PlatformView => PlatformView;
}
