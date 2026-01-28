using Avalonia.VisualTree;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.TimePicker;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods to map <see cref="ITimePicker"/> properties onto the
/// Avalonia TimePicker platform control.
/// </summary>
public static class TimePickerExtensions
{
    /// <summary>
    /// Updates the selected time on the Avalonia TimePicker from the MAUI TimePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia TimePicker control.</param>
    /// <param name="timePicker">The .NET MAUI view providing the time value.</param>
    public static void UpdateTime(this PlatformView platformView, ITimePicker timePicker)
    {
        platformView.SelectedTime = timePicker.Time;
    }

    /// <summary>
    /// Updates the clock identifier (12h/24h) based on the format string.
    /// </summary>
    /// <param name="platformView">The Avalonia TimePicker control.</param>
    /// <param name="timePicker">The .NET MAUI view providing the format string.</param>
    /// <remarks>
    /// Avalonia TimePicker uses ClockIdentifier property for 12/24 hour format.
    /// MAUI uses a format string. Common patterns are detected:
    /// - "t" or formats with "h" = 12 hour clock
    /// - "T" or formats with "H" = 24 hour clock
    /// </remarks>
    public static void UpdateFormat(this PlatformView platformView, ITimePicker timePicker)
    {
        var format = timePicker.Format;
        
        if (string.IsNullOrEmpty(format))
        {
            return;
        }

        if (format.Contains("H") || format == "T")
        {
            platformView.ClockIdentifier = "24HourClock";
        }
        else if (format.Contains("h") || format == "t")
        {
            platformView.ClockIdentifier = "12HourClock";
        }
    }

    /// <summary>
    /// Updates the text color of the Avalonia TimePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia TimePicker control.</param>
    /// <param name="timePicker">The .NET MAUI view providing the text color.</param>
    public static void UpdateTextColor(this PlatformView platformView, ITimePicker timePicker)
    {
        if (timePicker.TextColor != null)
        {
            platformView.Foreground = timePicker.TextColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(PlatformView.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the font properties of the Avalonia TimePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia TimePicker control.</param>
    /// <param name="timePicker">The .NET MAUI view providing the font information.</param>
    /// <param name="fontManager">The font manager service for font resolution.</param>
    public static void UpdateFont(this PlatformView platformView, ITimePicker timePicker, IFontManager fontManager)
    {
        var font = timePicker.Font;

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
    /// Updates the character spacing for the Avalonia TimePicker.
    /// </summary>
    /// <param name="platformView">The Avalonia TimePicker control.</param>
    /// <param name="timePicker">The .NET MAUI view providing the character spacing value.</param>
    /// <remarks>
    /// Applies letter spacing to the TimePicker's template TextBlocks (PART_HourTextBlock, PART_MinuteTextBlock, PART_PeriodTextBlock).
    /// </remarks>
    public static void UpdateCharacterSpacing(this PlatformView platformView, ITimePicker timePicker)
    {
        var characterSpacing = timePicker.CharacterSpacing;
        
        // Store the spacing value as a tag for later retrieval
        platformView.Tag = characterSpacing;
        
        // Subscribe to TemplateApplied to apply when template is ready
        platformView.TemplateApplied -= OnTimePickerTemplateApplied;
        platformView.TemplateApplied += OnTimePickerTemplateApplied;
        
        // Try to apply immediately if template parts exist
        ApplyLetterSpacingToTemplateParts(platformView, characterSpacing);
    }

    private static void OnTimePickerTemplateApplied(object? sender, Primitives.TemplateAppliedEventArgs e)
    {
        if (sender is PlatformView timePicker && timePicker.Tag is double spacing)
        {
            // Find the template parts using the NameScope from the event
            var hourTextBlock = e.NameScope.Find<AvaloniaTextBlock>("PART_HourTextBlock");
            var minuteTextBlock = e.NameScope.Find<AvaloniaTextBlock>("PART_MinuteTextBlock");
            var periodTextBlock = e.NameScope.Find<AvaloniaTextBlock>("PART_PeriodTextBlock");
            
            if (hourTextBlock != null)
                hourTextBlock.LetterSpacing = spacing;
            if (minuteTextBlock != null)
                minuteTextBlock.LetterSpacing = spacing;
            if (periodTextBlock != null)
                periodTextBlock.LetterSpacing = spacing;
        }
    }

    private static void ApplyLetterSpacingToTemplateParts(PlatformView timePicker, double letterSpacing)
    {
        // Try to find the specific template parts by traversing descendants
        foreach (var textBlock in timePicker.GetVisualDescendants().OfType<AvaloniaTextBlock>())
        {
            // Check if this is one of the time display TextBlocks by name
            if (textBlock.Name == "PART_HourTextBlock" || 
                textBlock.Name == "PART_MinuteTextBlock" || 
                textBlock.Name == "PART_PeriodTextBlock")
            {
                textBlock.LetterSpacing = letterSpacing;
            }
        }
    }
}