using System;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.Controls.MauiSearchBar;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI SearchBar to Avalonia MauiSearchBar mapping
/// </summary>
internal partial class SearchBarHandler : ViewHandler<ISearchBar, PlatformView>, ISearchBarHandler
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

    public object? QueryEditor => PlatformView?._textBox;

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.TextChanged += OnTextChanged;
        platformView.SearchButtonPressed += OnSearchButtonPressed;
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
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.Text = searchBar.Text ?? string.Empty;
    }

    public static void MapPlaceholder(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.Placeholder = searchBar.Placeholder ?? string.Empty;
    }

    public static void MapPlaceholderColor(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (searchBar.PlaceholderColor != null)
        {
            var avaloniaColor = global::Avalonia.Media.Color.FromArgb(
                (byte)(searchBar.PlaceholderColor.Alpha * 255),
                (byte)(searchBar.PlaceholderColor.Red * 255),
                (byte)(searchBar.PlaceholderColor.Green * 255),
                (byte)(searchBar.PlaceholderColor.Blue * 255));

            platformView.PlaceholderForeground = new global::Avalonia.Media.SolidColorBrush(avaloniaColor);
        }
    }

    public static void MapTextColor(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (searchBar.TextColor != null)
        {
            var avaloniaColor = global::Avalonia.Media.Color.FromArgb(
                (byte)(searchBar.TextColor.Alpha * 255),
                (byte)(searchBar.TextColor.Red * 255),
                (byte)(searchBar.TextColor.Green * 255),
                (byte)(searchBar.TextColor.Blue * 255));

            platformView.Foreground = new global::Avalonia.Media.SolidColorBrush(avaloniaColor);
        }
    }

    public static void MapCharacterSpacing(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.CharacterSpacing = searchBar.CharacterSpacing;
    }

    public static void MapFont(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        platformView.UpdateFont(searchBar, fontManager);
    }

    public static void MapHorizontalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.HorizontalTextAlignment = searchBar.HorizontalTextAlignment switch
        {
            Microsoft.Maui.TextAlignment.Start => global::Avalonia.Media.TextAlignment.Left,
            Microsoft.Maui.TextAlignment.Center => global::Avalonia.Media.TextAlignment.Center,
            Microsoft.Maui.TextAlignment.End => global::Avalonia.Media.TextAlignment.Right,
            _ => global::Avalonia.Media.TextAlignment.Left,
        };
    }

    public static void MapVerticalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
    {
        // Avalonia TextBox doesn't have direct vertical text alignment
        // This is typically handled by VerticalContentAlignment on the TextBox template
    }

    public static void MapIsReadOnly(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.IsReadOnly = searchBar.IsReadOnly;
    }

    public static void MapMaxLength(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.MaxLength = searchBar.MaxLength;
    }

    public static void MapCancelButtonColor(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (searchBar.CancelButtonColor != null)
        {
            var avaloniaColor = global::Avalonia.Media.Color.FromArgb(
                (byte)(searchBar.CancelButtonColor.Alpha * 255),
                (byte)(searchBar.CancelButtonColor.Red * 255),
                (byte)(searchBar.CancelButtonColor.Green * 255),
                (byte)(searchBar.CancelButtonColor.Blue * 255));

            platformView.CancelButtonColor = new global::Avalonia.Media.SolidColorBrush(avaloniaColor);
        }
    }

    public static void MapCursorPosition(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.CursorPosition = searchBar.CursorPosition;
    }

    public static void MapSelectionLength(ISearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.SelectionLength = searchBar.SelectionLength;
    }

    public static void MapFocus(ISearchBarHandler handler, ISearchBar searchBar, object? args)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (args is FocusRequest)
        {
            platformView.FocusSearchBar();
        }
    }

    ISearchBar ISearchBarHandler.VirtualView => VirtualView;

    object ISearchBarHandler.PlatformView => PlatformView;
}
