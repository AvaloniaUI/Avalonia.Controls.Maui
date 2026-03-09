using Microsoft.Maui;
using Avalonia.Interactivity;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = Avalonia.Controls.Maui.Controls.MauiSearchBar;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="ISearchBar"/>.</summary>
public partial class SearchBarHandler : ViewHandler<ISearchBar, PlatformView>
{
    /// <summary>Property mapper for <see cref="SearchBarHandler"/>.</summary>
    public static IPropertyMapper<ISearchBar, SearchBarHandler> Mapper =
        new PropertyMapper<ISearchBar, SearchBarHandler>(ViewHandler.ViewMapper)
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

    /// <summary>Command mapper for <see cref="SearchBarHandler"/>.</summary>
    public static CommandMapper<ISearchBar, SearchBarHandler> CommandMapper =
        new(ViewCommandMapper)
        {
            [nameof(ISearchBar.Focus)] = MapFocus,
        };

    /// <summary>Initializes a new instance of <see cref="SearchBarHandler"/>.</summary>
    public SearchBarHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SearchBarHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public SearchBarHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SearchBarHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public SearchBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Gets the underlying text box editor of the search bar.</summary>
    public object? QueryEditor => PlatformView._textBox;

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.TextChanged += OnTextChanged;
        platformView.SearchButtonPressed += OnSearchButtonPressed;

        if (VirtualView != null)
            MapSearchIconColor(this, VirtualView);
    }

    /// <inheritdoc/>
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
    
    /// <summary>Maps the Text property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateText(searchBar);
    }

    /// <summary>Maps the SearchIconColor property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapSearchIconColor(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateSearchIconColor(searchBar);
    }

    /// <summary>Maps the Placeholder property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdatePlaceholder(searchBar);
    }

    /// <summary>Maps the PlaceholderColor property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapPlaceholderColor(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdatePlaceholderColor(searchBar);
    }

    /// <summary>Maps the TextColor property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapTextColor(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateTextColor(searchBar);
    }

    /// <summary>Maps the CharacterSpacing property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateCharacterSpacing(searchBar);
    }

    /// <summary>Maps the Font property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapFont(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            var fontManager = handler.GetRequiredService<IFontManager>();
            platformView.UpdateFont(searchBar, fontManager);
        }
    }

    /// <summary>Maps the HorizontalTextAlignment property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapHorizontalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateHorizontalTextAlignment(searchBar);
    }

    /// <summary>Maps the VerticalTextAlignment property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapVerticalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateVerticalTextAlignment(searchBar);
    }

    /// <summary>Maps the IsReadOnly property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapIsReadOnly(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateIsReadOnly(searchBar);
    }

    /// <summary>Maps the MaxLength property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapMaxLength(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateMaxLength(searchBar);
    }

    /// <summary>Maps the CancelButtonColor property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapCancelButtonColor(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateCancelButtonColor(searchBar);
    }

    /// <summary>Maps the CursorPosition property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapCursorPosition(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateCursorPosition(searchBar);
    }

    /// <summary>Maps the SelectionLength property to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    public static void MapSelectionLength(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateSelectionLength(searchBar);
    }

    /// <summary>Maps the Focus command to the platform view.</summary>
    /// <param name="handler">The handler for the search bar.</param>
    /// <param name="searchBar">The virtual view.</param>
    /// <param name="args">The focus request arguments.</param>
    public static void MapFocus(SearchBarHandler handler, ISearchBar searchBar, object? args)
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
}