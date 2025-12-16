using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Avalonia.Interactivity;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = Avalonia.Controls.Maui.Controls.MauiSearchBar;

namespace Avalonia.Controls.Maui.Handlers;

public partial class SearchBarHandler : ViewHandler<ISearchBar, PlatformView>, ISearchBarHandler
{
    public static IPropertyMapper<ISearchBar, ISearchBarHandler> Mapper =
        new PropertyMapper<ISearchBar, ISearchBarHandler>(ViewHandler.ViewMapper)
        {
            [nameof(ISearchBar.Text)] = MapText,
            [nameof(ISearchBar.Placeholder)] = MapPlaceholder,
            [nameof(ISearchBar.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(ISearchBar.TextColor)] = MapTextColor,
            [nameof(ISearchBar.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(ISearchBar.Font)] = MapFont,
            [nameof(ISearchBar.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(ISearchBar.VerticalTextAlignment)] = MapVerticalTextAlignment,
            [nameof(ISearchBar.IsReadOnly)] = MapIsReadOnly,
            [nameof(ISearchBar.MaxLength)] = MapMaxLength,
            [nameof(ISearchBar.CancelButtonColor)] = MapCancelButtonColor,
            [nameof(ISearchBar.CursorPosition)] = MapCursorPosition,
            [nameof(ISearchBar.SelectionLength)] = MapSelectionLength,
            ["SearchIconColor"] = MapSearchIconColor,
        };

    public static CommandMapper<ISearchBar, ISearchBarHandler> CommandMapper =
        new(ViewCommandMapper)
        {
            [nameof(ISearchBar.Focus)] = MapFocus,
        };

    public SearchBarHandler() : base(Mapper, CommandMapper)
    {
    }

    public SearchBarHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public SearchBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    public object? QueryEditor => PlatformView._textBox;

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.TextChanged += OnTextChanged;
        platformView.SearchButtonPressed += OnSearchButtonPressed;

        if (VirtualView != null)
            MapSearchIconColor(this, VirtualView);
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.TextChanged -= OnTextChanged;
        platformView.SearchButtonPressed -= OnSearchButtonPressed;
        base.DisconnectHandler(platformView);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        VirtualView.Text = PlatformView.Text;
    }

    private void OnSearchButtonPressed(object? sender, RoutedEventArgs e)
    {
        if (VirtualView == null)
            return;

        VirtualView.SearchButtonPressed();
    }
    
    public static void MapText(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateText(searchBar);
    }

    public static void MapSearchIconColor(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateSearchIconColor(searchBar);
    }

    public static void MapPlaceholder(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdatePlaceholder(searchBar);
    }

    public static void MapPlaceholderColor(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdatePlaceholderColor(searchBar);
    }
    
    public static void MapTextColor(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateTextColor(searchBar);
    }
    
    public static void MapCharacterSpacing(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateCharacterSpacing(searchBar);
    }

    public static void MapFont(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            var fontManager = handler.GetRequiredService<IFontManager>();
            platformView.UpdateFont(searchBar, fontManager);
        }
    }
    
    public static void MapHorizontalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateHorizontalTextAlignment(searchBar);
    }

    public static void MapVerticalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateVerticalTextAlignment(searchBar);
    }

    public static void MapIsReadOnly(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateIsReadOnly(searchBar);
    }

    public static void MapMaxLength(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateMaxLength(searchBar);
    }

    public static void MapCancelButtonColor(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateCancelButtonColor(searchBar);
    }

    public static void MapCursorPosition(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateCursorPosition(searchBar);
    }

    public static void MapSelectionLength(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateSelectionLength(searchBar);
    }

    public static void MapFocus(ISearchBarHandler handler, ISearchBar searchBar, object? args)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            if (args is FocusRequest focusRequest)
            {
                platformView.Focus();
                focusRequest.TrySetResult(true);
            }
        }
    }

    ISearchBar ISearchBarHandler.VirtualView => VirtualView;

    object ISearchBarHandler.PlatformView => PlatformView;
}