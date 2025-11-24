using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Button;

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

    private void OnButtonClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        VirtualView?.OnInvoked();
        TryCloseParentSwipeView();
    }

    private void TryCloseParentSwipeView()
    {
        if (PlatformView?.Tag is ValueTuple<SwipeBehaviorOnInvoked, Avalonia.Controls.Maui.Swipe> tag)
        {
            var (behavior, swipe) = tag;
            if (behavior == SwipeBehaviorOnInvoked.Close)
            {
                swipe.Close(animated: true);
            }
        }
        else if (VirtualView is IElement element)
        {
            ISwipeItems? swipeItems = null;
            ISwipeView? swipeView = null;
            IElement? current = element;

            while (current != null && (swipeItems == null || swipeView == null))
            {
                if (swipeItems == null && current is ISwipeItems si)
                    swipeItems = si;

                if (swipeView == null && current is ISwipeView sv)
                    swipeView = sv;

                current = current.Parent;
            }

            if (swipeItems?.SwipeBehaviorOnInvoked == SwipeBehaviorOnInvoked.Close && swipeView != null)
            {
                swipeView.RequestClose(new SwipeViewCloseRequest(true));
            }
        }
    }

    public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateVisibility(view);
    }

    public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateBackground(view);
    }

    public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateText(view);
    }

    public static void MapTextColor(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateTextColor(view);
    }

    public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateCharacterSpacing(view);
    }

    public static void MapFont(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateFont(view, handler);
    }

    public static void MapSource(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
        MapSourceAsync(handler, view).FireAndForget(handler);

    public static async Task MapSourceAsync(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        await platformView.UpdateSourceAsync(view, handler);
    }

    ISwipeItemMenuItem ISwipeItemMenuItemHandler.VirtualView => VirtualView;

    object ISwipeItemMenuItemHandler.PlatformView => PlatformView;
}
