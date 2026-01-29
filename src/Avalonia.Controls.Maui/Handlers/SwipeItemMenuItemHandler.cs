using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Button;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for .NET MAUI SwipeItemMenuItem to Avalonia platform-native menu item mapping.
/// Maps ISwipeItemMenuItem cross-platform interface to platform-specific menu item implementations.
/// </summary>
public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, PlatformView>
{
    public static IPropertyMapper<ISwipeItemMenuItem, SwipeItemMenuItemHandler> Mapper =
        new PropertyMapper<ISwipeItemMenuItem, SwipeItemMenuItemHandler>(ElementHandler.ElementMapper)
        {
            [nameof(ISwipeItemMenuItem.Visibility)] = MapVisibility,
            [nameof(IView.Background)] = MapBackground,
            [nameof(IMenuElement.Text)] = MapText,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(IMenuElement.Source)] = MapSource,
        };

    public static CommandMapper<ISwipeItemMenuItem, SwipeItemMenuItemHandler> CommandMapper =
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
        var button = new PlatformView
        {
            HorizontalAlignment = Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Layout.VerticalAlignment.Stretch,
            HorizontalContentAlignment = Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Layout.VerticalAlignment.Center,
            Padding = new Thickness(16, 8)
        };

        return button;
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Click += OnButtonClick;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.Click -= OnButtonClick;
        base.DisconnectHandler(platformView);
    }
    
    public static void MapVisibility(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateVisibility(view);
    }

    public static void MapBackground(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateBackground(view);
    }

    public static void MapText(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateText(view);
    }

    public static void MapTextColor(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateTextColor(view);
    }

    public static void MapCharacterSpacing(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateCharacterSpacing(view);
    }

    public static void MapFont(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateFont(view, handler);
    }

    public static void MapSource(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
        MapSourceAsync(handler, view).FireAndForget(handler);

    public static async Task MapSourceAsync(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        await platformView.UpdateSourceAsync(view, handler);
    }
    
    private void OnButtonClick(object? sender, Interactivity.RoutedEventArgs e)
    {
        VirtualView?.OnInvoked();
        TryCloseParentSwipeView();
    }

    private void TryCloseParentSwipeView()
    {
        // Use Tag is set when building swipe items for auto-close behavior.
        if (PlatformView?.Tag is ValueTuple<SwipeBehaviorOnInvoked, Swipe> tag)
        {
            var (behavior, swipe) = tag;
            if (behavior == SwipeBehaviorOnInvoked.Close)
            {
                swipe.SetSwipeState(SwipeState.Hidden, animated: true);
            }
            return;
        }

        // Walk up the element tree to find owning swipe items and swipe view.
        if (VirtualView is not IElement element)
            return;

        ISwipeItems? swipeItems = null;
        ISwipeView? swipeView = null;

        for (var current = element; current != null && (swipeItems == null || swipeView == null); current = current.Parent)
        {
            if (swipeItems == null && current is ISwipeItems si)
                swipeItems = si;

            if (swipeView == null && current is ISwipeView sv)
                swipeView = sv;
        }

        if (swipeItems?.SwipeBehaviorOnInvoked == SwipeBehaviorOnInvoked.Close && swipeView != null)
        {
            swipeView.RequestClose(new SwipeViewCloseRequest(true));
        }
    }
}
