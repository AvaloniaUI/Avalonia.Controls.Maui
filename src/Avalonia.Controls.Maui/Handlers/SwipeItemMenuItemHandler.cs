using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Button;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="ISwipeItemMenuItem"/>.</summary>
public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, PlatformView>
{
    /// <summary>Property mapper for <see cref="SwipeItemMenuItemHandler"/>.</summary>
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

    /// <summary>Command mapper for <see cref="SwipeItemMenuItemHandler"/>.</summary>
    public static CommandMapper<ISwipeItemMenuItem, SwipeItemMenuItemHandler> CommandMapper =
        new(ElementHandler.ElementCommandMapper)
        {
        };

    /// <summary>Initializes a new instance of <see cref="SwipeItemMenuItemHandler"/>.</summary>
    public SwipeItemMenuItemHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SwipeItemMenuItemHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    protected SwipeItemMenuItemHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SwipeItemMenuItemHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    protected SwipeItemMenuItemHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform element for this handler.</summary>
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

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Click += OnButtonClick;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.Click -= OnButtonClick;
        base.DisconnectHandler(platformView);
    }

    /// <summary>Maps the Visibility property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapVisibility(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateVisibility(view);
    }

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapBackground(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateBackground(view);
    }

    /// <summary>Maps the Text property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapText(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateText(view);
    }

    /// <summary>Maps the TextColor property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapTextColor(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateTextColor(view);
    }

    /// <summary>Maps the CharacterSpacing property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapCharacterSpacing(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateCharacterSpacing(view);
    }

    /// <summary>Maps the Font property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapFont(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateFont(view, handler);
    }

    /// <summary>Maps the Source property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapSource(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
        MapSourceAsync(handler, view).FireAndForget(handler.MauiContext?.Services?.CreateLogger<SwipeItemMenuItemHandler>());

    /// <summary>Maps the Source property to the platform view asynchronously.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The virtual view.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
