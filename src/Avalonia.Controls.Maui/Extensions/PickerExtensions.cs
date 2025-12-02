using System.Collections;
using System.Collections.ObjectModel;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods to map <see cref="IPicker"/> properties onto the Avalonia
/// <see cref="ComboBox"/> platform control.
/// </summary>
public static class PickerExtensions
{
    /// <summary>
    /// Applies the items source backing the picker.
    /// </summary>
    /// <param name="platformView">The Avalonia ComboBox control.</param>
    /// <param name="picker">The .NET MAUI view supplying the items.</param>
    public static void UpdateItems(this ComboBox platformView, IPicker picker)
    {
        platformView.ItemsSource = picker.Items != null
            ? new ObservableCollection<string>(picker.Items)
            : new ObservableCollection<string>();
    }

    /// <summary>
    /// Applies vertical text alignment to the selection content.
    /// </summary>
    /// <param name="platformView">The Avalonia ComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing alignment values.</param>
    public static void UpdateVerticalTextAlignment(this ComboBox platformView, IPicker picker)
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
    /// <param name="platformView">The Avalonia ComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing alignment values.</param>
    public static void UpdateHorizontalTextAlignment(this ComboBox platformView, IPicker picker)
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
    /// Updates the picker title placeholder text.
    /// </summary>
    /// <param name="platformView">The Avalonia platform view.</param>
    /// <param name="picker">The .NET MAUI view providing the title.</param>
    [NotImplemented("Pending implementation in Avalonia Core. More information: https://github.com/AvaloniaUI/Avalonia/issues/20198")] 
    public static void UpdateTitle(this ComboBox platformView, IPicker picker)
    {
        platformView.PlaceholderText = picker.Title ?? string.Empty;
    }

    /// <summary>
    /// Updates the picker title placeholder color.
    /// </summary>
    /// <param name="platformView">The Avalonia platform view.</param>
    /// <param name="picker">The .NET MAUI view providing the color.</param>
    [NotImplemented("Pending implementation in Avalonia Core. More information: https://github.com/AvaloniaUI/Avalonia/issues/20198")] 
    public static void UpdateTitleColor(this ComboBox platformView, IPicker picker)
    {
        if (picker.TitleColor != null)
        {
            platformView.PlaceholderForeground = picker.TitleColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(ComboBox.PlaceholderForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the selected text color.
    /// </summary>
    /// <param name="platformView">The Avalonia ComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing the color.</param>
    public static void UpdateTextColor(this ComboBox platformView, IPicker picker)
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
    /// <param name="platformView">The Avalonia ComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing the index.</param>
    public static void UpdateSelectedIndex(this ComboBox platformView, IPicker picker)
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
    /// <param name="platformView">The Avalonia ComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing the font.</param>
    /// <param name="fontManager">The font manager service.</param>
    public static void UpdateFont(this ComboBox platformView, IPicker picker, IFontManager fontManager)
    {
        platformView.UpdateFont((ITextStyle)picker, fontManager);
    }

    /// <summary>
    /// Applies character spacing to the displayed items.
    /// </summary>
    /// <param name="platformView">The Avalonia ComboBox control.</param>
    /// <param name="picker">The .NET MAUI view providing the character spacing value.</param>
    public static void UpdateCharacterSpacing(this ComboBox platformView, IPicker picker)
    {
        var characterSpacing = picker.CharacterSpacing;

        if (characterSpacing == 0)
        {
            platformView.ItemTemplate = null;
            platformView.SelectionBoxItemTemplate = null;
            return;
        }
        
        var letterSpacing = characterSpacing;

        var dataTemplate = new FuncDataTemplate<string>((_, _) => new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(),
            [!TextBlock.FontSizeProperty] = new Binding
            {
                Source = platformView,
                Path = nameof(ComboBox.FontSize)
            },
            [!TextBlock.FontFamilyProperty] = new Binding
            {
                Source = platformView,
                Path = nameof(ComboBox.FontFamily)
            },
            [!TextBlock.FontStyleProperty] = new Binding
            {
                Source = platformView,
                Path = nameof(ComboBox.FontStyle)
            },
            [!TextBlock.FontWeightProperty] = new Binding
            {
                Source = platformView,
                Path = nameof(ComboBox.FontWeight)
            },
            LetterSpacing = letterSpacing
        });

        platformView.ItemTemplate = dataTemplate;
        platformView.SelectionBoxItemTemplate = dataTemplate;
    }
}
