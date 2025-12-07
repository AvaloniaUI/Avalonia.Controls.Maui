using System.Collections;
using System.Collections.ObjectModel;
using Avalonia.Controls.Templates;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods to map <see cref="IPicker"/> properties onto the Avalonia
/// <see cref="MauiComboBox"/> platform control.
/// </summary>
public static class PickerExtensions
{
    /// <summary>
    /// Applies the items source backing the picker.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiComboBox control.</param>
    /// <param name="picker">The .NET MAUI view supplying the items.</param>
    public static void UpdateItems(this MauiComboBox platformView, IPicker picker)
    {
        platformView.ItemsSource = picker.Items != null
            ? new ObservableCollection<string>(picker.Items)
            : new ObservableCollection<string>();
    }

    /// <summary>
    /// Applies vertical text alignment to the selection content.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing alignment values.</param>
    public static void UpdateVerticalTextAlignment(this MauiComboBox platformView, IPicker picker)
    {
        platformView.VerticalContentAlignment = picker.VerticalTextAlignment switch
        {
            TextAlignment.Start => global::Avalonia.Layout.VerticalAlignment.Top,
            TextAlignment.Center => global::Avalonia.Layout.VerticalAlignment.Center,
            TextAlignment.End => global::Avalonia.Layout.VerticalAlignment.Bottom,
            _ => platformView.VerticalContentAlignment
        };
    }

    /// <summary>
    /// Applies horizontal text alignment to the selection content.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing alignment values.</param>
    public static void UpdateHorizontalTextAlignment(this MauiComboBox platformView, IPicker picker)
    {
        platformView.HorizontalContentAlignment = picker.HorizontalTextAlignment switch
        {
            TextAlignment.Start => global::Avalonia.Layout.HorizontalAlignment.Left,
            TextAlignment.Center => global::Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment.End => global::Avalonia.Layout.HorizontalAlignment.Right,
            _ => platformView.HorizontalContentAlignment
        };
    }

    /// <summary>
    /// Updates the picker title using the Header property.
    /// </summary>
    /// <param name="platformView">The Avalonia platform view.</param>
    /// <param name="picker">The .NET MAUI view providing the title.</param>
    public static void UpdateTitle(this MauiComboBox platformView, IPicker picker)
    {
        var title = picker.Title ?? string.Empty;

        if (string.IsNullOrEmpty(title))
        {
            platformView.Header = null;
            platformView.HeaderTemplate = null;
            return;
        }

        // If we have a TitleColor set, we need to use a template
        if (picker.TitleColor != null)
        {
            platformView.Header = title;
            platformView.HeaderTemplate = CreateHeaderTemplate(picker.TitleColor);
        }
        else
        {
            // Just set the header as a string, let the default template handle it
            platformView.Header = title;
            platformView.HeaderTemplate = null;
        }
    }

    /// <summary>
    /// Updates the picker title color using the HeaderTemplate.
    /// </summary>
    /// <param name="platformView">The Avalonia platform view.</param>
    /// <param name="picker">The .NET MAUI view providing the color.</param>
    public static void UpdateTitleColor(this MauiComboBox platformView, IPicker picker)
    {
        // Update the title which will handle both title and color
        UpdateTitle(platformView, picker);
    }

    private static IDataTemplate? CreateHeaderTemplate(Color titleColor)
    {
        return new FuncDataTemplate<object?>((data, _) =>
        {
            var textBlock = new TextBlock
            {
                Text = data?.ToString() ?? string.Empty,
                Foreground = titleColor.ToPlatform()
            };
            return textBlock;
        });
    }

    /// <summary>
    /// Updates the selected text color.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing the color.</param>
    public static void UpdateTextColor(this MauiComboBox platformView, IPicker picker)
    {
        if (picker.TextColor != null)
        {
            platformView.Foreground = picker.TextColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(ComboBox.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the selected index ensuring bounds are respected.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing the index.</param>
    public static void UpdateSelectedIndex(this MauiComboBox platformView, IPicker picker)
    {
        var targetIndex = picker.SelectedIndex;

        if (targetIndex < 0 || targetIndex >= picker.GetCount())
        {
            platformView.SelectedIndex = -1;
            platformView.SelectedItem = null;
        }
        else
        {
            platformView.SelectedIndex = targetIndex;

            if (platformView.ItemsSource is IList list && targetIndex < list.Count)
            {
                platformView.SelectedItem = list[targetIndex];
            }
        }
    }

    /// <summary>
    /// Updates the font styling for the picker content.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing the font.</param>
    /// <param name="fontManager">The font manager service.</param>
    public static void UpdateFont(this MauiComboBox platformView, IPicker picker, IFontManager fontManager)
    {
        platformView.UpdateFont((ITextStyle)picker, fontManager);
    }

    /// <summary>
    /// Applies character spacing to the displayed items.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing the character spacing value.</param>
    public static void UpdateCharacterSpacing(this MauiComboBox platformView, IPicker picker)
    {
        var characterSpacing = picker.CharacterSpacing;

        if (characterSpacing == 0)
        {
            platformView.ItemTemplate = null;
            platformView.SelectionBoxItemTemplate = null;
            return;
        }
        
        var letterSpacing = characterSpacing;

        var dataTemplate = new FuncDataTemplate<string>((item, _) => 
        {
            var textBlock = new TextBlock
            {
                Text = item ?? string.Empty,
                FontSize = platformView.FontSize,
                FontFamily = platformView.FontFamily,
                FontStyle = platformView.FontStyle,
                FontWeight = platformView.FontWeight,
                LetterSpacing = letterSpacing
            };
            return textBlock;
        });

        platformView.ItemTemplate = dataTemplate;
        platformView.SelectionBoxItemTemplate = dataTemplate;
    }
}
