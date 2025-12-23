using Avalonia.VisualTree;
using Microsoft.Maui;
using AvaloniaDatePicker = Avalonia.Controls.DatePicker;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods to map <see cref="IDatePicker"/> properties onto the
/// <see cref="AvaloniaDatePicker"/> platform control.
/// </summary>
public static class DatePickerExtensions
{
    /// <summary>
    /// Updates the selected date on the Avalonia DatePicker from the MAUI DatePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia DatePicker control.</param>
    /// <param name="datePicker">The .NET MAUI view providing the date value.</param>
    public static void UpdateDate(this AvaloniaDatePicker platformView, IDatePicker datePicker)
    {
        if (datePicker.Date is null)
        {
            platformView.SelectedDate = null;
        }
        else
        {
            var date = datePicker.Date.Value;
            
            // Validate against min/max if they are set
            var minDate = datePicker.MinimumDate;
            var maxDate = datePicker.MaximumDate;
            
            if (minDate.HasValue && date < minDate.Value)
            {
                date = minDate.Value;
            }
            
            if (maxDate.HasValue && date > maxDate.Value)
            {
                date = maxDate.Value;
            }
            
            platformView.SelectedDate = new DateTimeOffset(date);
        }
    }

    /// <summary>
    /// Updates the minimum selectable date constraint on the Avalonia DatePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia DatePicker control.</param>
    /// <param name="datePicker">The .NET MAUI view providing the minimum date value.</param>
    /// <remarks>
    /// Avalonia's DatePicker does not have a built-in MinimumDate property.
    /// This method validates the current date against the minimum and adjusts if needed.
    /// </remarks>
    public static void UpdateMinimumDate(this AvaloniaDatePicker platformView, IDatePicker datePicker)
    {
        // Avalonia DatePicker doesn't have built-in MinimumDate property
        // We validate and adjust the current date if it's before the minimum
        if (datePicker.MinimumDate.HasValue && datePicker.Date.HasValue)
        {
            if (datePicker.Date.Value < datePicker.MinimumDate.Value)
            {
                platformView.SelectedDate = new DateTimeOffset(datePicker.MinimumDate.Value);
            }
        }
    }

    /// <summary>
    /// Updates the maximum selectable date constraint on the Avalonia DatePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia DatePicker control.</param>
    /// <param name="datePicker">The .NET MAUI view providing the maximum date value.</param>
    /// <remarks>
    /// Avalonia's DatePicker does not have a built-in MaximumDate property.
    /// This method validates the current date against the maximum and adjusts if needed.
    /// </remarks>
    public static void UpdateMaximumDate(this AvaloniaDatePicker platformView, IDatePicker datePicker)
    {
        // Avalonia DatePicker doesn't have built-in MaximumDate property
        // We validate and adjust the current date if it's after the maximum
        if (datePicker.MaximumDate.HasValue && datePicker.Date.HasValue)
        {
            if (datePicker.Date.Value > datePicker.MaximumDate.Value)
            {
                platformView.SelectedDate = new DateTimeOffset(datePicker.MaximumDate.Value);
            }
        }
    }

    /// <summary>
    /// Updates the date format display on the Avalonia DatePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia DatePicker control.</param>
    /// <param name="datePicker">The .NET MAUI view providing the format string.</param>
    /// <remarks>
    /// Avalonia DatePicker uses separate DayFormat, MonthFormat, and YearFormat properties.
    /// Common MAUI format strings are mapped to appropriate Avalonia formats.
    /// </remarks>
    public static void UpdateFormat(this AvaloniaDatePicker platformView, IDatePicker datePicker)
    {
        var format = datePicker.Format;
        
        if (string.IsNullOrEmpty(format))
        {
            return;
        }

        // Map common .NET date format strings to Avalonia DatePicker formats
        // Avalonia DatePicker uses DayFormat, MonthFormat, YearFormat properties
        switch (format)
        {
            case "d": // Short date pattern (e.g., 6/15/2009)
                platformView.DayFormat = "d";
                platformView.MonthFormat = "M";
                platformView.YearFormat = "yyyy";
                break;
            case "D": // Long date pattern (e.g., Monday, June 15, 2009)
                platformView.DayFormat = "dd";
                platformView.MonthFormat = "MMMM";
                platformView.YearFormat = "yyyy";
                break;
            case "M": // Month/day pattern (e.g., June 15)
            case "m":
                platformView.DayFormat = "dd";
                platformView.MonthFormat = "MMMM";
                platformView.YearVisible = false;
                break;
            case "Y": // Year/month pattern (e.g., June 2009)
            case "y":
                platformView.DayVisible = false;
                platformView.MonthFormat = "MMMM";
                platformView.YearFormat = "yyyy";
                break;
            default:
                // For custom formats, try to parse and set individual components
                if (format.Contains("yyyy"))
                {
                    platformView.YearFormat = "yyyy";
                }
                else if (format.Contains("yy"))
                {
                    platformView.YearFormat = "yy";
                }
                
                if (format.Contains("MMMM"))
                {
                    platformView.MonthFormat = "MMMM";
                }
                else if (format.Contains("MMM"))
                {
                    platformView.MonthFormat = "MMM";
                }
                else if (format.Contains("MM"))
                {
                    platformView.MonthFormat = "MM";
                }
                else if (format.Contains("M"))
                {
                    platformView.MonthFormat = "M";
                }
                
                if (format.Contains("dd"))
                {
                    platformView.DayFormat = "dd";
                }
                else if (format.Contains("d"))
                {
                    platformView.DayFormat = "d";
                }
                break;
        }
    }

    /// <summary>
    /// Updates the text color of the Avalonia DatePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia DatePicker control.</param>
    /// <param name="datePicker">The .NET MAUI view providing the text color.</param>
    public static void UpdateTextColor(this AvaloniaDatePicker platformView, IDatePicker datePicker)
    {
        if (datePicker.TextColor != null)
        {
            platformView.Foreground = datePicker.TextColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(AvaloniaDatePicker.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the font properties of the Avalonia DatePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia DatePicker control.</param>
    /// <param name="datePicker">The .NET MAUI view providing the font information.</param>
    /// <param name="fontManager">The font manager service for font resolution.</param>
    public static void UpdateFont(this AvaloniaDatePicker platformView, IDatePicker datePicker, IFontManager fontManager)
    {
        var font = datePicker.Font;

        if (font.IsDefault)
        {
            return;
        }

        if (font.Size > 0)
        {
            platformView.FontSize = font.Size;
        }

        if (font.Family != null)
        {
            platformView.FontFamily = font.Family;
        }

        if (font.Weight != FontWeight.Regular)
        {
            platformView.FontWeight = FontManager.ToAvaloniaFontWeight(font.Weight);
        }

        if (font.Slant != FontSlant.Default)
        {
            platformView.FontStyle = FontManager.ToAvaloniaFontStyle(font.Slant);
        }
    }

    /// <summary>
    /// Updates the character spacing for the Avalonia DatePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia DatePicker control.</param>
    /// <param name="datePicker">The .NET MAUI view providing the character spacing value.</param>
    /// <remarks>
    /// Applies letter spacing to the DatePicker's template TextBlocks (PART_DayTextBlock, PART_MonthTextBlock, PART_YearTextBlock).
    /// </remarks>
    public static void UpdateCharacterSpacing(this AvaloniaDatePicker platformView, IDatePicker datePicker)
    {
        var characterSpacing = datePicker.CharacterSpacing;
        
        // Store the spacing value as a tag for later retrieval
        platformView.Tag = characterSpacing;
        
        // Subscribe to TemplateApplied to apply when template is ready
        platformView.TemplateApplied -= OnDatePickerTemplateApplied;
        platformView.TemplateApplied += OnDatePickerTemplateApplied;
        
        // Try to apply immediately if template parts exist
        ApplyLetterSpacingToTemplateParts(platformView, characterSpacing);
    }

    private static void OnDatePickerTemplateApplied(object? sender, Primitives.TemplateAppliedEventArgs e)
    {
        if (sender is AvaloniaDatePicker datePicker && datePicker.Tag is double spacing)
        {
            // Find the template parts using the NameScope from the event
            var dayTextBlock = e.NameScope.Find<TextBlock>("PART_DayTextBlock");
            var monthTextBlock = e.NameScope.Find<TextBlock>("PART_MonthTextBlock");
            var yearTextBlock = e.NameScope.Find<TextBlock>("PART_YearTextBlock");
            
            if (dayTextBlock != null)
                dayTextBlock.LetterSpacing = spacing;
            if (monthTextBlock != null)
                monthTextBlock.LetterSpacing = spacing;
            if (yearTextBlock != null)
                yearTextBlock.LetterSpacing = spacing;
        }
    }

    private static void ApplyLetterSpacingToTemplateParts(AvaloniaDatePicker datePicker, double letterSpacing)
    {
        // Try to find the specific template parts by traversing descendants
        foreach (var textBlock in datePicker.GetVisualDescendants().OfType<TextBlock>())
        {
            // Check if this is one of the date display TextBlocks by name
            if (textBlock.Name == "PART_DayTextBlock" || 
                textBlock.Name == "PART_MonthTextBlock" || 
                textBlock.Name == "PART_YearTextBlock")
            {
                textBlock.LetterSpacing = letterSpacing;
            }
        }
    }
}



