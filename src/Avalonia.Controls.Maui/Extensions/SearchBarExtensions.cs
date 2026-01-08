using Avalonia.Controls.Maui.Controls;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping ISearchBar properties to MauiSearchBar.
/// </summary>
public static class SearchBarExtensions
{
    /// <summary>
    /// Updates the Text property of the MauiSearchBar.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the text.</param>
    public static void UpdateText(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        platformView.Text = searchBar.Text;
    }

    /// <summary>
    /// Updates the Placeholder property of the MauiSearchBar.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the placeholder.</param>
    public static void UpdatePlaceholder(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        platformView.Placeholder = searchBar.Placeholder ?? string.Empty;
    }

    /// <summary>
    /// Updates the PlaceholderForeground brush of the MauiSearchBar based on the SearchBar's PlaceholderColor.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the color.</param>
    public static void UpdatePlaceholderColor(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        if (searchBar.PlaceholderColor != null)
        {
            platformView.PlaceholderForeground = searchBar.PlaceholderColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(MauiSearchBar.PlaceholderForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the Foreground brush of the MauiSearchBar based on the SearchBar's TextColor.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the color.</param>
    public static void UpdateTextColor(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        if (searchBar.TextColor != null)
        {
            platformView.Foreground = searchBar.TextColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(MauiSearchBar.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the CharacterSpacing property of the MauiSearchBar.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the character spacing.</param>
    public static void UpdateCharacterSpacing(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        platformView.CharacterSpacing = searchBar.CharacterSpacing;
    }

    /// <summary>
    /// Updates the font properties of the MauiSearchBar using the MAUI Font and FontManager.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the font.</param>
    /// <param name="fontManager">The FontManager to use for font resolution.</param>
    public static void UpdateFont(this MauiSearchBar platformView, ISearchBar searchBar, IFontManager fontManager)
    {
        if (fontManager == null)
            throw new ArgumentNullException(nameof(fontManager));

        var font = searchBar.Font;

        if (font.IsDefault)
            return;

        platformView.FontSize = fontManager.GetFontSize(font);
        platformView.FontFamily = fontManager.GetFontFamily(font);
        platformView.FontStyle = FontManager.ToAvaloniaFontStyle(font.Slant);
        platformView.FontWeight = FontManager.ToAvaloniaFontWeight(font.Weight);
    }

    /// <summary>
    /// Updates the HorizontalTextAlignment property of the MauiSearchBar.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the alignment.</param>
    public static void UpdateHorizontalTextAlignment(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        platformView.HorizontalTextAlignment = searchBar.HorizontalTextAlignment switch
        {
            TextAlignment.Start => Media.TextAlignment.Left,
            TextAlignment.Center => Media.TextAlignment.Center,
            TextAlignment.End => Media.TextAlignment.Right,
            _ => Media.TextAlignment.Left,
        };
    }

    /// <summary>
    /// Updates the VerticalTextAlignment property of the MauiSearchBar.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the alignment.</param>
    public static void UpdateVerticalTextAlignment(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        // TODO: Vertical Text Alignment is not directly supported in Avalonia TextBox yet.
    }

    /// <summary>
    /// Updates the IsReadOnly property of the MauiSearchBar.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the read-only state.</param>
    public static void UpdateIsReadOnly(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        platformView.IsReadOnly = searchBar.IsReadOnly;
    }

    /// <summary>
    /// Updates the MaxLength property of the MauiSearchBar.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the max length.</param>
    public static void UpdateMaxLength(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        platformView.MaxLength = searchBar.MaxLength;
    }

    /// <summary>
    /// Updates the CancelButtonColor brush of the MauiSearchBar based on the SearchBar's CancelButtonColor.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the color.</param>
    public static void UpdateCancelButtonColor(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        if (searchBar.CancelButtonColor != null)
        {
            platformView.CancelButtonColor = searchBar.CancelButtonColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(MauiSearchBar.CancelButtonColorProperty);
        }
    }

    /// <summary>
    /// Updates the CursorPosition property of the MauiSearchBar.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the cursor position.</param>
    public static void UpdateCursorPosition(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        platformView.CursorPosition = searchBar.CursorPosition;
    }

    /// <summary>
    /// Updates the SelectionLength property of the MauiSearchBar.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the selection length.</param>
    public static void UpdateSelectionLength(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        platformView.SelectionLength = searchBar.SelectionLength;
    }

    /// <summary>
    /// Updates the SearchIconColor brush of the MauiSearchBar.
    /// </summary>
    /// <param name="platformView">The MauiSearchBar control to update.</param>
    /// <param name="searchBar">The .NET MAUI SearchBar providing the selection length.</param>
    public static void UpdateSearchIconColor(this MauiSearchBar platformView, ISearchBar searchBar)
    {
        if (searchBar.SearchIconColor != null)
        {
            platformView.SearchIconColor = searchBar.SearchIconColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(MauiSearchBar.SearchIconColorProperty);
        }
    }
}
